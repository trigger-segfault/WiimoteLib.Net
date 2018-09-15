using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.DataTypes;
using WiimoteLib.Geometry;
using WiimoteLib.Util;

namespace WiimoteLib {
	/// <summary>
	/// Implementation of Wiimote
	/// </summary>
	public partial class WiimoteOld : IDisposable {


		private byte[] interleavedBufferA;
		private DataReportAttribute interleavedReportA;

		/// <summary>
		/// Parse a report sent by the Wiimote
		/// </summary>
		/// <param name="buff">Data buffer to parse</param>
		/// <returns>Returns a boolean noting whether an event needs to be posted</returns>
		private bool ParseInputReport(byte[] buff) {
			try {
			InputReport type = (InputReport) buff[0];
			//{
			byte[] buff2 = new byte[buff.Length - 1];
			Buffer.BlockCopy(buff, 1, buff2, 0, buff2.Length);
			//Buffer.BlockCopy(buff, 1, buff, 0, buff.Length - 1);
			//	buff = buff2;
			//}

			DataReportAttribute dataReport =
				EnumInfo<InputReport>.TryGetAttribute<DataReportAttribute>(type);

			if (dataReport != null) {
				// Buttons are ALWAYS parsed
				if (dataReport.HasButtons)
					ParseButtons2(buff2, dataReport.ButtonsOffset);

				switch (dataReport.Interleave) {
				case Interleave.None:
					if (dataReport.HasAccel)
						ParseAccel2(buff2, dataReport.AccelOffset);

					if (dataReport.HasIR)
						ParseIR2(buff2, dataReport.IROffset, dataReport.IRSize);

					if (dataReport.HasExt)
						ParseExtension2(buff2, dataReport.ExtOffset, dataReport.ExtSize);
					break;
				case Interleave.A:
					interleavedBufferA = buff2;
					interleavedReportA = dataReport;
					break;
				case Interleave.B:
					byte[] buffA = interleavedBufferA;
					byte[] buffB = buff2;
					DataReportAttribute reportA = interleavedReportA;
					DataReportAttribute reportB = dataReport;
					ParseAccelInterleaved2(buffA, buffB, reportA.IROffset, reportB.IROffset);
					ParseIRInterleaved2(buffA, buffB, reportA.IROffset, reportB.IROffset);
					break;
				}

				return true;
			}
			else {
				switch (type) {
				case InputReport.Status:
					Debug.WriteLine("******** STATUS ********");

					bool extensionLast = mWiimoteState.Status.Extension;

					ParseButtons2(buff2, 0);
					ParseStatus2(buff2, 2);

					BeginAsyncRead();
					byte[] extensionType = ReadData(Registers.ExtensionType2, 1);
					byte[] extensionType2 = ReadData(Registers.ExtensionType1, 6);
						Debug.WriteLine(string.Join("", extensionType2.Select(b => b.ToString("x2"))));
						Debug.WriteLine("Extension byte=" + extensionType[0].ToString("X2"));

					// extension connected?
					//bool extension = (buff2[2] & 0x02) != 0;
					Debug.WriteLine("Extension, Old: " + extensionLast + ", New: " + mWiimoteState.Extension);

					if (mWiimoteState.Extension != extensionLast || extensionType[0] == 0x04 || extensionType[0] == 0x5) {

						if (mWiimoteState.Extension) {
							BeginAsyncRead();
							InitializeExtension(extensionType[0]);
						}
						else
							mWiimoteState.ExtensionType = ExtensionType.None;

						// only fire the extension changed event if we have a real extension (i.e. not a balance board)
						//if (WiimoteExtensionChanged != null && mWiimoteState.ExtensionType != ExtensionType.BalanceBoard)
						//	WiimoteExtensionChanged(this, new WiimoteExtensionChangedEventArgs(mWiimoteState.ExtensionType, mWiimoteState.Extension));
					}
					mStatusDone.Set();
					break;
				case InputReport.ReadData:
					ParseButtons2(buff2, 0);
					ParseReadData2(buff2);
					break;
				case InputReport.AcknowledgeOutputReport:
					mWriteDone.Set();
					break;
				}
			}
			}
			catch (TimeoutException) { }
			catch (Exception ex) {
				Debug.WriteLine("ParseInputReport: " + ex.Message);
			}
			/*switch (type) {
			case InputReport.Buttons:
				ParseButtons(buff);
				//mWiimoteState.ButtonState.Parse(buff2, 0);
				//mWiimoteState.AccelState.ParseWiimote(buff2, 2, mWiimoteState.AccelCalibrationInfo);
				ParseButtons2(buff, 0);
				break;
			case InputReport.ButtonsAccel:
				ParseButtons(buff);
				ParseAccel(buff);
				//mWiimoteState.ButtonState.Parse(buff2, 0);
				//mWiimoteState.AccelState.ParseWiimote(buff2, 2, mWiimoteState.AccelCalibrationInfo);
				ParseButtons2(buff, 0);
				ParseAccel2(buff, 0);
				break;
			case InputReport.ButtonsAccelIR12:
				//ParseButtons(buff);
				//ParseAccel(buff);
				//ParseIR(buff);
				ParseButtons2(buff, 0);
				ParseAccel2(buff, 0);
				ParseIR2(buff2, 5, 12);
				break;
			case InputReport.ButtonsExt19:
				//ParseButtons(buff);
				//ParseExtension(buff, 3);
				ParseButtons2(buff2, 0);
				ParseExtension2(buff2, 2, 19);
				break;
			case InputReport.ButtonsAccelExt16:
				//ParseButtons(buff);
				//ParseAccel(buff);
				//ParseExtension(buff, 6);
				ParseButtons2(buff2, 0);
				ParseAccel2(buff2, 0);
				ParseExtension2(buff2, 5, 16);
				break;
			case InputReport.ButtonsAccelIR10Ext6:
				ParseButtons(buff);
				ParseAccel(buff);
				ParseIR(buff);
				ParseExtension(buff, 16);
				mWiimoteState.Buttons.Parse(buff2, 0);
				mWiimoteState.Accel.ParseWiimote(buff2, 2, mWiimoteState.AccelCalibrationInfo);
				ParseButtons2(buff2, 0);
				ParseAccel2(buff2, 0);
				ParseIR2(buff2, 5, 10);
				ParseExtension2(buff2, 15, 6);
				break;
			case InputReport.Status:
				Debug.WriteLine("******** STATUS ********");
				ParseButtons(buff);
				mWiimoteState.BatteryRaw = buff[6];
				mWiimoteState.Battery = (((100.0f * 48.0f * (float) ((int) buff[6] / 48.0f))) / 192.0f);

				// get the real LED values in case the values from SetLEDs() somehow becomes out of sync, which really shouldn't be possible
				mWiimoteState.LED.LED1 = (buff[3] & 0x10) != 0;
				mWiimoteState.LED.LED2 = (buff[3] & 0x20) != 0;
				mWiimoteState.LED.LED3 = (buff[3] & 0x40) != 0;
				mWiimoteState.LED.LED4 = (buff[3] & 0x80) != 0;

				mWiimoteState.Buttons.Parse(buff2, 0);
				mWiimoteState.Status.Parse(buff2, 2);
				ParseButtons2(buff2, 0);
				ParseStatus2(buff2, 2);

				BeginAsyncRead();
				byte[] extensionType = ReadData(Registers.ExtensionType2, 1);
				Debug.WriteLine("Extension byte=" + extensionType[0].ToString("x2"));

				// extension connected?
				bool extension = (buff[3] & 0x02) != 0;
				Debug.WriteLine("Extension, Old: " + mWiimoteState.Extension + ", New: " + extension);

				if (mWiimoteState.Extension != extension || extensionType[0] == 0x04) {
					mWiimoteState.Extension = extension;

					if (extension) {
						BeginAsyncRead();
						InitializeExtension(extensionType[0]);
					}
					else
						mWiimoteState.ExtensionType = ExtensionType.None;

					// only fire the extension changed event if we have a real extension (i.e. not a balance board)
					if (WiimoteExtensionChanged != null && mWiimoteState.ExtensionType != ExtensionType.BalanceBoard)
						WiimoteExtensionChanged(this, new WiimoteExtensionChangedEventArgs(mWiimoteState.ExtensionType, mWiimoteState.Extension));
				}
				mStatusDone.Set();
				break;
			case InputReport.ReadData:
				ParseButtons(buff);
				ParseReadData(buff);
				break;
			case InputReport.AcknowledgeOutputReport:
				//					Debug.WriteLine("ack: " + buff[0] + " " +  buff[1] + " " +buff[2] + " " +buff[3] + " " +buff[4]);
				mWriteDone.Set();
				break;
			default:
				Debug.WriteLine("Unknown report type: " + type.ToString("x"));
				return false;
			}*/

			return true;
		}

		/// <summary>
		/// Handles setting up an extension when plugged in
		/// </summary>
		private void InitializeExtension(byte extensionType) {
			Debug.WriteLine("InitExtension");

			// only initialize if it's not a MotionPlus
			if (extensionType != 0x04 && extensionType != 0x05) {
				WriteData(Registers.ExtensionInit1, 0x55);
				WriteData(Registers.ExtensionInit2, 0x00);
			}

			// start reading again
			BeginAsyncRead();

			byte[] buff = ReadData(Registers.ExtensionType1, 6);
			long type = ((long) buff[0] << 40) | ((long) buff[1] << 32) | ((long) buff[2]) << 24 | ((long) buff[3]) << 16 | ((long) buff[4]) << 8 | buff[5];

			switch ((ExtensionType) type) {
			case ExtensionType.None:
			case ExtensionType.ParitallyInserted:
				mWiimoteState.Extension = false;
				mWiimoteState.ExtensionType = ExtensionType.None;
				return;
			case ExtensionType.Nunchuk:
			case ExtensionType.ClassicController:
			//case ExtensionType.Guitar:
			//case ExtensionType.BalanceBoard:
			//case ExtensionType.Drums:
			//case ExtensionType.TaikoDrum:
			case ExtensionType.MotionPlus:
			case ExtensionType.MotionPlusNunchuk:
				mWiimoteState.ExtensionType = (ExtensionType) type;
				//this.SetReportType(InputReport.ButtonsExt19, true);
				break;
			default:
				throw new WiimoteException("Unknown extension controller found: " + type.ToString("x"));
			}

			switch (mWiimoteState.ExtensionType) {
			case ExtensionType.Nunchuk:
				buff = ReadData(Registers.ExtensionCalibration, 16);

				mWiimoteState.Nunchuk.CalibrationInfo.Parse(buff, 0);

				/*mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.X0 = buff[0];
				mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.Y0 = buff[1];
				mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.Z0 = buff[2];
				mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.XG = buff[4];
				mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.YG = buff[5];
				mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.ZG = buff[6];
				mWiimoteState.Nunchuk.CalibrationInfo.Max.X = buff[8];
				mWiimoteState.Nunchuk.CalibrationInfo.Min.X = buff[9];
				mWiimoteState.Nunchuk.CalibrationInfo.Mid.X = buff[10];
				mWiimoteState.Nunchuk.CalibrationInfo.Max.Y = buff[11];
				mWiimoteState.Nunchuk.CalibrationInfo.Min.Y = buff[12];
				mWiimoteState.Nunchuk.CalibrationInfo.Mid.Y = buff[13];
				mWiimoteState.Nunchuk.CalibrationInfo.Parse(buff, 0);*/
				break;
			case ExtensionType.ClassicController:
				buff = ReadData(Registers.ExtensionCalibration, 16);

				mWiimoteState.ClassicController.CalibrationInfo.MaxXL = (byte) (buff[0] >> 2);
				mWiimoteState.ClassicController.CalibrationInfo.MinXL = (byte) (buff[1] >> 2);
				mWiimoteState.ClassicController.CalibrationInfo.MidXL = (byte) (buff[2] >> 2);
				mWiimoteState.ClassicController.CalibrationInfo.MaxYL = (byte) (buff[3] >> 2);
				mWiimoteState.ClassicController.CalibrationInfo.MinYL = (byte) (buff[4] >> 2);
				mWiimoteState.ClassicController.CalibrationInfo.MidYL = (byte) (buff[5] >> 2);

				mWiimoteState.ClassicController.CalibrationInfo.MaxXR = (byte) (buff[6] >> 3);
				mWiimoteState.ClassicController.CalibrationInfo.MinXR = (byte) (buff[7] >> 3);
				mWiimoteState.ClassicController.CalibrationInfo.MidXR = (byte) (buff[8] >> 3);
				mWiimoteState.ClassicController.CalibrationInfo.MaxYR = (byte) (buff[9] >> 3);
				mWiimoteState.ClassicController.CalibrationInfo.MinYR = (byte) (buff[10] >> 3);
				mWiimoteState.ClassicController.CalibrationInfo.MidYR = (byte) (buff[11] >> 3);

				// this doesn't seem right...
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MinTriggerL = (byte)(buff[12] >> 3);
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MaxTriggerL = (byte)(buff[14] >> 3);
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MinTriggerR = (byte)(buff[13] >> 3);
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MaxTriggerR = (byte)(buff[15] >> 3);
				mWiimoteState.ClassicController.CalibrationInfo.MinTriggerL = 0;
				mWiimoteState.ClassicController.CalibrationInfo.MaxTriggerL = 31;
				mWiimoteState.ClassicController.CalibrationInfo.MinTriggerR = 0;
				mWiimoteState.ClassicController.CalibrationInfo.MaxTriggerR = 31;
				break;
			/*case ExtensionType.BalanceBoard:
				buff = ReadData(Registers.ExtensionCalibration, 32);

				mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.TopRight = (short) ((short) buff[4] << 8 | buff[5]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.BottomRight = (short) ((short) buff[6] << 8 | buff[7]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.TopLeft = (short) ((short) buff[8] << 8 | buff[9]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.BottomLeft = (short) ((short) buff[10] << 8 | buff[11]);

				mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.TopRight = (short) ((short) buff[12] << 8 | buff[13]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.BottomRight = (short) ((short) buff[14] << 8 | buff[15]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.TopLeft = (short) ((short) buff[16] << 8 | buff[17]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.BottomLeft = (short) ((short) buff[18] << 8 | buff[19]);

				mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.TopRight = (short) ((short) buff[20] << 8 | buff[21]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.BottomRight = (short) ((short) buff[22] << 8 | buff[23]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.TopLeft = (short) ((short) buff[24] << 8 | buff[25]);
				mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.BottomLeft = (short) ((short) buff[26] << 8 | buff[27]);
				break;*/
			case ExtensionType.MotionPlus:
				// someday...
				break;
			case ExtensionType.MotionPlusNunchuk:
				buff = ReadData(Registers.PassthroughCalibration, 16);
				mWiimoteState.Nunchuk.CalibrationInfo.Parse(buff, 0);
				//buff = ReadData(Registers.Extension, 256);

				// Doesn't do anything yet
				buff = ReadData(Registers.MotionPlusCalibration, 32);
				mWiimoteState.MotionPlus.CalibrationInfo.Parse(buff, 0);
				break;
			//case ExtensionType.Guitar:
			//case ExtensionType.Drums:
			//case ExtensionType.TaikoDrum:
				// there appears to be no calibration for these controllers
				//break;
			}
		}

		/// <summary>
		/// Parses a standard button report into the ButtonState struct
		/// </summary>
		/// <param name="buff">Data buffer</param>
		/*private void ParseButtons(byte[] buff) {
			mWiimoteState.Buttons.A = (buff[2] & 0x08) != 0;
			mWiimoteState.Buttons.B = (buff[2] & 0x04) != 0;
			mWiimoteState.Buttons.Minus = (buff[2] & 0x10) != 0;
			mWiimoteState.Buttons.Home = (buff[2] & 0x80) != 0;
			mWiimoteState.Buttons.Plus = (buff[1] & 0x10) != 0;
			mWiimoteState.Buttons.One = (buff[2] & 0x02) != 0;
			mWiimoteState.Buttons.Two = (buff[2] & 0x01) != 0;
			mWiimoteState.Buttons.Up = (buff[1] & 0x08) != 0;
			mWiimoteState.Buttons.Down = (buff[1] & 0x04) != 0;
			mWiimoteState.Buttons.Left = (buff[1] & 0x01) != 0;
			mWiimoteState.Buttons.Right = (buff[1] & 0x02) != 0;
		}*/

		/// <summary>
		/// Parse accelerometer data
		/// </summary>
		/// <param name="buff">Data buffer</param>
		/*private void ParseAccel(byte[] buff) {
			mWiimoteState.Accel.RawValues.X = buff[3];
			mWiimoteState.Accel.RawValues.Y = buff[4];
			mWiimoteState.Accel.RawValues.Z = buff[5];

			mWiimoteState.Accel.Values.X = (float) ((float) mWiimoteState.Accel.RawValues.X - ((int) mWiimoteState.AccelCalibrationInfo.X0)) /
											((float) mWiimoteState.AccelCalibrationInfo.XG - ((int) mWiimoteState.AccelCalibrationInfo.X0));
			mWiimoteState.Accel.Values.Y = (float) ((float) mWiimoteState.Accel.RawValues.Y - mWiimoteState.AccelCalibrationInfo.Y0) /
											((float) mWiimoteState.AccelCalibrationInfo.YG - mWiimoteState.AccelCalibrationInfo.Y0);
			mWiimoteState.Accel.Values.Z = (float) ((float) mWiimoteState.Accel.RawValues.Z - mWiimoteState.AccelCalibrationInfo.Z0) /
											((float) mWiimoteState.AccelCalibrationInfo.ZG - mWiimoteState.AccelCalibrationInfo.Z0);
		}*/

		private void ParseStatus2(byte[] buff, int off) {
			mWiimoteState.Status.Parse(buff, off);
		}

		private void ParseButtons2(byte[] buff, int off) {
			mWiimoteState.Buttons.Parse(buff, off);
		}

		private void ParseAccel2(byte[] buff, int off) {
			mWiimoteState.Accel.ParseWiimote(buff, off, mWiimoteState.AccelCalibrationInfo);
		}

		private void ParseAccelInterleaved2(byte[] buffA, byte[] buffB, int offA, int offB) {
			mWiimoteState.Accel.ParseWiimoteInterleaved(
				buffA, buffB, offA, offB, mWiimoteState.AccelCalibrationInfo);
		}

		private void ParseIR2(byte[] buff, int off, int size) {

		}

		private void ParseNunchuk(byte[] buff, int off) {
			mWiimoteState.Nunchuk.Parse(buff, off, false);
		}

		private void ParseClassicController(byte[] buff, int off) {
			//mWiimoteState.ClassicControllerState.Parse(buff, off, false);
		}

		private void ParseMotionPlus(byte[] buff, int off) {
			bool passthrough = !buff.GetBit(off + 5, 1);

			if (!passthrough) {
				mWiimoteState.MotionPlus.Parse(buff, off);
				//WriteData(Registers.Extension, 0);
			}
			else {
				mWiimoteState.Nunchuk.Parse(buff, off, true);
				//WriteData(Registers.Extension, 0);
			}
		}

		private void ParseExtension2(byte[] buff, int off, int size) {
			switch (mWiimoteState.ExtensionType) {
			case ExtensionType.Nunchuk:
				ParseNunchuk(buff, off);
				break;
			case ExtensionType.ClassicController:
				ParseClassicController(buff, off);
				break;
			case ExtensionType.MotionPlus:
			case ExtensionType.MotionPlusNunchuk:
				ParseMotionPlus(buff, off);
				break;
			}
		}

		private void ParseIRInterleaved2(byte[] buffA, byte[] buffB, int offA, int offB) {

		}

		/// <summary>
		/// Parse IR data from report
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseIR(byte[] buff) {
			mWiimoteState.IRState.IRSensor0.RawPosition.X = buff[6] | ((buff[8] >> 4) & 0x03) << 8;
			mWiimoteState.IRState.IRSensor0.RawPosition.Y = buff[7] | ((buff[8] >> 6) & 0x03) << 8;

			switch (mWiimoteState.IRState.Mode) {
			case IRMode.Basic:
				mWiimoteState.IRState.IRSensor1.RawPosition.X = buff[9] | ((buff[8] >> 0) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor1.RawPosition.Y = buff[10] | ((buff[8] >> 2) & 0x03) << 8;

				mWiimoteState.IRState.IRSensor2.RawPosition.X = buff[11] | ((buff[13] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor2.RawPosition.Y = buff[12] | ((buff[13] >> 6) & 0x03) << 8;

				mWiimoteState.IRState.IRSensor3.RawPosition.X = buff[14] | ((buff[13] >> 0) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor3.RawPosition.Y = buff[15] | ((buff[13] >> 2) & 0x03) << 8;

				mWiimoteState.IRState.IRSensor0.Size = 0x00;
				mWiimoteState.IRState.IRSensor1.Size = 0x00;
				mWiimoteState.IRState.IRSensor2.Size = 0x00;
				mWiimoteState.IRState.IRSensor3.Size = 0x00;

				mWiimoteState.IRState.IRSensor0.Found = !(buff[6] == 0xff && buff[7] == 0xff);
				mWiimoteState.IRState.IRSensor1.Found = !(buff[9] == 0xff && buff[10] == 0xff);
				mWiimoteState.IRState.IRSensor2.Found = !(buff[11] == 0xff && buff[12] == 0xff);
				mWiimoteState.IRState.IRSensor3.Found = !(buff[14] == 0xff && buff[15] == 0xff);
				break;
			case IRMode.Extended:
				mWiimoteState.IRState.IRSensor1.RawPosition.X = buff[9] | ((buff[11] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor1.RawPosition.Y = buff[10] | ((buff[11] >> 6) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor2.RawPosition.X = buff[12] | ((buff[14] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor2.RawPosition.Y = buff[13] | ((buff[14] >> 6) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor3.RawPosition.X = buff[15] | ((buff[17] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensor3.RawPosition.Y = buff[16] | ((buff[17] >> 6) & 0x03) << 8;

				mWiimoteState.IRState.IRSensor0.Size = buff[8] & 0x0f;
				mWiimoteState.IRState.IRSensor1.Size = buff[11] & 0x0f;
				mWiimoteState.IRState.IRSensor2.Size = buff[14] & 0x0f;
				mWiimoteState.IRState.IRSensor3.Size = buff[17] & 0x0f;

				mWiimoteState.IRState.IRSensor0.Found = !(buff[6] == 0xff && buff[7] == 0xff && buff[8] == 0xff);
				mWiimoteState.IRState.IRSensor1.Found = !(buff[9] == 0xff && buff[10] == 0xff && buff[11] == 0xff);
				mWiimoteState.IRState.IRSensor2.Found = !(buff[12] == 0xff && buff[13] == 0xff && buff[14] == 0xff);
				mWiimoteState.IRState.IRSensor3.Found = !(buff[15] == 0xff && buff[16] == 0xff && buff[17] == 0xff);
				break;
			}

			mWiimoteState.IRState.IRSensor0.Position.X = (float) (mWiimoteState.IRState.IRSensor0.RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensor1.Position.X = (float) (mWiimoteState.IRState.IRSensor1.RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensor2.Position.X = (float) (mWiimoteState.IRState.IRSensor2.RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensor3.Position.X = (float) (mWiimoteState.IRState.IRSensor3.RawPosition.X / 1023.5f);

			mWiimoteState.IRState.IRSensor0.Position.Y = (float) (mWiimoteState.IRState.IRSensor0.RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensor1.Position.Y = (float) (mWiimoteState.IRState.IRSensor1.RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensor2.Position.Y = (float) (mWiimoteState.IRState.IRSensor2.RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensor3.Position.Y = (float) (mWiimoteState.IRState.IRSensor3.RawPosition.Y / 767.5f);

			if (mWiimoteState.IRState.IRSensor0.Found && mWiimoteState.IRState.IRSensor1.Found) {
				mWiimoteState.IRState.RawMidpoint.X = (mWiimoteState.IRState.IRSensor1.RawPosition.X + mWiimoteState.IRState.IRSensor0.RawPosition.X) / 2;
				mWiimoteState.IRState.RawMidpoint.Y = (mWiimoteState.IRState.IRSensor1.RawPosition.Y + mWiimoteState.IRState.IRSensor0.RawPosition.Y) / 2;

				mWiimoteState.IRState.Midpoint.X = (mWiimoteState.IRState.IRSensor1.Position.X + mWiimoteState.IRState.IRSensor0.Position.X) / 2.0f;
				mWiimoteState.IRState.Midpoint.Y = (mWiimoteState.IRState.IRSensor1.Position.Y + mWiimoteState.IRState.IRSensor0.Position.Y) / 2.0f;
			}
			else
				mWiimoteState.IRState.Midpoint.X = mWiimoteState.IRState.Midpoint.Y = 0.0f;
			/*mWiimoteState.IRState.IRSensors[0].RawPosition.X = buff[6] | ((buff[8] >> 4) & 0x03) << 8;
			mWiimoteState.IRState.IRSensors[0].RawPosition.Y = buff[7] | ((buff[8] >> 6) & 0x03) << 8;

			switch (mWiimoteState.IRState.Mode) {
			case IRMode.Basic:
				mWiimoteState.IRState.IRSensors[1].RawPosition.X = buff[9] | ((buff[8] >> 0) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[1].RawPosition.Y = buff[10] | ((buff[8] >> 2) & 0x03) << 8;

				mWiimoteState.IRState.IRSensors[2].RawPosition.X = buff[11] | ((buff[13] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[2].RawPosition.Y = buff[12] | ((buff[13] >> 6) & 0x03) << 8;

				mWiimoteState.IRState.IRSensors[3].RawPosition.X = buff[14] | ((buff[13] >> 0) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[3].RawPosition.Y = buff[15] | ((buff[13] >> 2) & 0x03) << 8;

				mWiimoteState.IRState.IRSensors[0].Size = 0x00;
				mWiimoteState.IRState.IRSensors[1].Size = 0x00;
				mWiimoteState.IRState.IRSensors[2].Size = 0x00;
				mWiimoteState.IRState.IRSensors[3].Size = 0x00;

				mWiimoteState.IRState.IRSensors[0].Found = !(buff[6] == 0xff && buff[7] == 0xff);
				mWiimoteState.IRState.IRSensors[1].Found = !(buff[9] == 0xff && buff[10] == 0xff);
				mWiimoteState.IRState.IRSensors[2].Found = !(buff[11] == 0xff && buff[12] == 0xff);
				mWiimoteState.IRState.IRSensors[3].Found = !(buff[14] == 0xff && buff[15] == 0xff);
				break;
			case IRMode.Extended:
				mWiimoteState.IRState.IRSensors[1].RawPosition.X = buff[9] | ((buff[11] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[1].RawPosition.Y = buff[10] | ((buff[11] >> 6) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[2].RawPosition.X = buff[12] | ((buff[14] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[2].RawPosition.Y = buff[13] | ((buff[14] >> 6) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[3].RawPosition.X = buff[15] | ((buff[17] >> 4) & 0x03) << 8;
				mWiimoteState.IRState.IRSensors[3].RawPosition.Y = buff[16] | ((buff[17] >> 6) & 0x03) << 8;

				mWiimoteState.IRState.IRSensors[0].Size = buff[8] & 0x0f;
				mWiimoteState.IRState.IRSensors[1].Size = buff[11] & 0x0f;
				mWiimoteState.IRState.IRSensors[2].Size = buff[14] & 0x0f;
				mWiimoteState.IRState.IRSensors[3].Size = buff[17] & 0x0f;

				mWiimoteState.IRState.IRSensors[0].Found = !(buff[6] == 0xff && buff[7] == 0xff && buff[8] == 0xff);
				mWiimoteState.IRState.IRSensors[1].Found = !(buff[9] == 0xff && buff[10] == 0xff && buff[11] == 0xff);
				mWiimoteState.IRState.IRSensors[2].Found = !(buff[12] == 0xff && buff[13] == 0xff && buff[14] == 0xff);
				mWiimoteState.IRState.IRSensors[3].Found = !(buff[15] == 0xff && buff[16] == 0xff && buff[17] == 0xff);
				break;
			}

			mWiimoteState.IRState.IRSensors[0].Position.X = (float) (mWiimoteState.IRState.IRSensors[0].RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensors[1].Position.X = (float) (mWiimoteState.IRState.IRSensors[1].RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensors[2].Position.X = (float) (mWiimoteState.IRState.IRSensors[2].RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensors[3].Position.X = (float) (mWiimoteState.IRState.IRSensors[3].RawPosition.X / 1023.5f);

			mWiimoteState.IRState.IRSensors[0].Position.Y = (float) (mWiimoteState.IRState.IRSensors[0].RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensors[1].Position.Y = (float) (mWiimoteState.IRState.IRSensors[1].RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensors[2].Position.Y = (float) (mWiimoteState.IRState.IRSensors[2].RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensors[3].Position.Y = (float) (mWiimoteState.IRState.IRSensors[3].RawPosition.Y / 767.5f);

			if (mWiimoteState.IRState.IRSensors[0].Found && mWiimoteState.IRState.IRSensors[1].Found) {
				mWiimoteState.IRState.RawMidpoint.X = (mWiimoteState.IRState.IRSensors[1].RawPosition.X + mWiimoteState.IRState.IRSensors[0].RawPosition.X) / 2;
				mWiimoteState.IRState.RawMidpoint.Y = (mWiimoteState.IRState.IRSensors[1].RawPosition.Y + mWiimoteState.IRState.IRSensors[0].RawPosition.Y) / 2;

				mWiimoteState.IRState.Midpoint.X = (mWiimoteState.IRState.IRSensors[1].Position.X + mWiimoteState.IRState.IRSensors[0].Position.X) / 2.0f;
				mWiimoteState.IRState.Midpoint.Y = (mWiimoteState.IRState.IRSensors[1].Position.Y + mWiimoteState.IRState.IRSensors[0].Position.Y) / 2.0f;
			}
			else
				mWiimoteState.IRState.Midpoint.X = mWiimoteState.IRState.Midpoint.Y = 0.0f;*/
		}

		/// <summary>
		/// Parse data from an extension controller
		/// </summary>
		/// <param name="buff">Data buffer</param>
		/// <param name="offset">Offset into data buffer</param>
		/*private void ParseExtension(byte[] buff, int offset) {
			switch (mWiimoteState.ExtensionType) {
			case ExtensionType.Nunchuk:
				mWiimoteState.Nunchuk.RawJoystick.X = buff[offset];
				mWiimoteState.Nunchuk.RawJoystick.Y = buff[offset + 1];
				mWiimoteState.Nunchuk.Accel.RawValues.X = buff[offset + 2];
				mWiimoteState.Nunchuk.Accel.RawValues.Y = buff[offset + 3];
				mWiimoteState.Nunchuk.Accel.RawValues.Z = buff[offset + 4];

				mWiimoteState.Nunchuk.C = (buff[offset + 5] & 0x02) == 0;
				mWiimoteState.Nunchuk.Z = (buff[offset + 5] & 0x01) == 0;

				mWiimoteState.Nunchuk.Accel.Values.X = (float) ((float) mWiimoteState.Nunchuk.Accel.RawValues.X - mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.X0) /
												((float) mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.XG - mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.X0);
				mWiimoteState.Nunchuk.Accel.Values.Y = (float) ((float) mWiimoteState.Nunchuk.Accel.RawValues.Y - mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.Y0) /
												((float) mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.YG - mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.Y0);
				mWiimoteState.Nunchuk.Accel.Values.Z = (float) ((float) mWiimoteState.Nunchuk.Accel.RawValues.Z - mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.Z0) /
												((float) mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.ZG - mWiimoteState.Nunchuk.CalibrationInfo.AccelCalibration.Z0);

				if (mWiimoteState.Nunchuk.CalibrationInfo.Max.X != 0x00)
					mWiimoteState.Nunchuk.Joystick.X = (float) ((float) mWiimoteState.Nunchuk.RawJoystick.X - mWiimoteState.Nunchuk.CalibrationInfo.Mid.X) /
											((float) mWiimoteState.Nunchuk.CalibrationInfo.Max.X - mWiimoteState.Nunchuk.CalibrationInfo.Min.X);

				if (mWiimoteState.Nunchuk.CalibrationInfo.Max.Y != 0x00)
					mWiimoteState.Nunchuk.Joystick.Y = (float) ((float) mWiimoteState.Nunchuk.RawJoystick.Y - mWiimoteState.Nunchuk.CalibrationInfo.Mid.Y) /
											((float) mWiimoteState.Nunchuk.CalibrationInfo.Max.Y - mWiimoteState.Nunchuk.CalibrationInfo.Min.Y);

				mWiimoteState.Nunchuk.Parse(buff, offset);

				break;

			case ExtensionType.ClassicController:
				mWiimoteState.ClassicController.RawJoystickL.X = (byte) (buff[offset] & 0x3f);
				mWiimoteState.ClassicController.RawJoystickL.Y = (byte) (buff[offset + 1] & 0x3f);
				mWiimoteState.ClassicController.RawJoystickR.X = (byte) ((buff[offset + 2] >> 7) | (buff[offset + 1] & 0xc0) >> 5 | (buff[offset] & 0xc0) >> 3);
				mWiimoteState.ClassicController.RawJoystickR.Y = (byte) (buff[offset + 2] & 0x1f);

				mWiimoteState.ClassicController.RawTriggerL = (byte) (((buff[offset + 2] & 0x60) >> 2) | (buff[offset + 3] >> 5));
				mWiimoteState.ClassicController.RawTriggerR = (byte) (buff[offset + 3] & 0x1f);

				mWiimoteState.ClassicController.ButtonState.TriggerR = (buff[offset + 4] & 0x02) == 0;
				mWiimoteState.ClassicController.ButtonState.Plus = (buff[offset + 4] & 0x04) == 0;
				mWiimoteState.ClassicController.ButtonState.Home = (buff[offset + 4] & 0x08) == 0;
				mWiimoteState.ClassicController.ButtonState.Minus = (buff[offset + 4] & 0x10) == 0;
				mWiimoteState.ClassicController.ButtonState.TriggerL = (buff[offset + 4] & 0x20) == 0;
				mWiimoteState.ClassicController.ButtonState.Down = (buff[offset + 4] & 0x40) == 0;
				mWiimoteState.ClassicController.ButtonState.Right = (buff[offset + 4] & 0x80) == 0;

				mWiimoteState.ClassicController.ButtonState.Up = (buff[offset + 5] & 0x01) == 0;
				mWiimoteState.ClassicController.ButtonState.Left = (buff[offset + 5] & 0x02) == 0;
				mWiimoteState.ClassicController.ButtonState.ZR = (buff[offset + 5] & 0x04) == 0;
				mWiimoteState.ClassicController.ButtonState.X = (buff[offset + 5] & 0x08) == 0;
				mWiimoteState.ClassicController.ButtonState.A = (buff[offset + 5] & 0x10) == 0;
				mWiimoteState.ClassicController.ButtonState.Y = (buff[offset + 5] & 0x20) == 0;
				mWiimoteState.ClassicController.ButtonState.B = (buff[offset + 5] & 0x40) == 0;
				mWiimoteState.ClassicController.ButtonState.ZL = (buff[offset + 5] & 0x80) == 0;

				if (mWiimoteState.ClassicController.CalibrationInfo.MaxXL != 0x00)
					mWiimoteState.ClassicController.JoystickL.X = (float) ((float) mWiimoteState.ClassicController.RawJoystickL.X - mWiimoteState.ClassicController.CalibrationInfo.MidXL) /
					(float) (mWiimoteState.ClassicController.CalibrationInfo.MaxXL - mWiimoteState.ClassicController.CalibrationInfo.MinXL);

				if (mWiimoteState.ClassicController.CalibrationInfo.MaxYL != 0x00)
					mWiimoteState.ClassicController.JoystickL.Y = (float) ((float) mWiimoteState.ClassicController.RawJoystickL.Y - mWiimoteState.ClassicController.CalibrationInfo.MidYL) /
					(float) (mWiimoteState.ClassicController.CalibrationInfo.MaxYL - mWiimoteState.ClassicController.CalibrationInfo.MinYL);

				if (mWiimoteState.ClassicController.CalibrationInfo.MaxXR != 0x00)
					mWiimoteState.ClassicController.JoystickR.X = (float) ((float) mWiimoteState.ClassicController.RawJoystickR.X - mWiimoteState.ClassicController.CalibrationInfo.MidXR) /
					(float) (mWiimoteState.ClassicController.CalibrationInfo.MaxXR - mWiimoteState.ClassicController.CalibrationInfo.MinXR);

				if (mWiimoteState.ClassicController.CalibrationInfo.MaxYR != 0x00)
					mWiimoteState.ClassicController.JoystickR.Y = (float) ((float) mWiimoteState.ClassicController.RawJoystickR.Y - mWiimoteState.ClassicController.CalibrationInfo.MidYR) /
					(float) (mWiimoteState.ClassicController.CalibrationInfo.MaxYR - mWiimoteState.ClassicController.CalibrationInfo.MinYR);

				if (mWiimoteState.ClassicController.CalibrationInfo.MaxTriggerL != 0x00)
					mWiimoteState.ClassicController.TriggerL = (mWiimoteState.ClassicController.RawTriggerL) /
					(float) (mWiimoteState.ClassicController.CalibrationInfo.MaxTriggerL - mWiimoteState.ClassicController.CalibrationInfo.MinTriggerL);

				if (mWiimoteState.ClassicController.CalibrationInfo.MaxTriggerR != 0x00)
					mWiimoteState.ClassicController.TriggerR = (mWiimoteState.ClassicController.RawTriggerR) /
					(float) (mWiimoteState.ClassicController.CalibrationInfo.MaxTriggerR - mWiimoteState.ClassicController.CalibrationInfo.MinTriggerR);
				break;

			case ExtensionType.Guitar:
				mWiimoteState.Guitar.GuitarType = ((buff[offset] & 0x80) == 0) ? GuitarType.GuitarHeroWorldTour : GuitarType.GuitarHero3;

				mWiimoteState.Guitar.ButtonState.Plus = (buff[offset + 4] & 0x04) == 0;
				mWiimoteState.Guitar.ButtonState.Minus = (buff[offset + 4] & 0x10) == 0;
				mWiimoteState.Guitar.ButtonState.StrumDown = (buff[offset + 4] & 0x40) == 0;

				mWiimoteState.Guitar.ButtonState.StrumUp = (buff[offset + 5] & 0x01) == 0;
				mWiimoteState.Guitar.FretButtonState.Yellow = (buff[offset + 5] & 0x08) == 0;
				mWiimoteState.Guitar.FretButtonState.Green = (buff[offset + 5] & 0x10) == 0;
				mWiimoteState.Guitar.FretButtonState.Blue = (buff[offset + 5] & 0x20) == 0;
				mWiimoteState.Guitar.FretButtonState.Red = (buff[offset + 5] & 0x40) == 0;
				mWiimoteState.Guitar.FretButtonState.Orange = (buff[offset + 5] & 0x80) == 0;

				// it appears the joystick values are only 6 bits
				mWiimoteState.Guitar.RawJoystick.X = (buff[offset + 0] & 0x3f);
				mWiimoteState.Guitar.RawJoystick.Y = (buff[offset + 1] & 0x3f);

				// and the whammy bar is only 5 bits
				mWiimoteState.Guitar.RawWhammyBar = (byte) (buff[offset + 3] & 0x1f);

				mWiimoteState.Guitar.Joystick.X = (float) (mWiimoteState.Guitar.RawJoystick.X - 0x1f) / 0x3f; // not fully accurate, but close
				mWiimoteState.Guitar.Joystick.Y = (float) (mWiimoteState.Guitar.RawJoystick.Y - 0x1f) / 0x3f; // not fully accurate, but close
				mWiimoteState.Guitar.WhammyBar = (float) (mWiimoteState.Guitar.RawWhammyBar) / 0x0a;  // seems like there are 10 positions?

				mWiimoteState.Guitar.TouchbarState.Yellow = false;
				mWiimoteState.Guitar.TouchbarState.Green = false;
				mWiimoteState.Guitar.TouchbarState.Blue = false;
				mWiimoteState.Guitar.TouchbarState.Red = false;
				mWiimoteState.Guitar.TouchbarState.Orange = false;

				switch (buff[offset + 2] & 0x1f) {
				case 0x04:
					mWiimoteState.Guitar.TouchbarState.Green = true;
					break;
				case 0x07:
					mWiimoteState.Guitar.TouchbarState.Green = true;
					mWiimoteState.Guitar.TouchbarState.Red = true;
					break;
				case 0x0a:
					mWiimoteState.Guitar.TouchbarState.Red = true;
					break;
				case 0x0c:
				case 0x0d:
					mWiimoteState.Guitar.TouchbarState.Red = true;
					mWiimoteState.Guitar.TouchbarState.Yellow = true;
					break;
				case 0x12:
				case 0x13:
					mWiimoteState.Guitar.TouchbarState.Yellow = true;
					break;
				case 0x14:
				case 0x15:
					mWiimoteState.Guitar.TouchbarState.Yellow = true;
					mWiimoteState.Guitar.TouchbarState.Blue = true;
					break;
				case 0x17:
				case 0x18:
					mWiimoteState.Guitar.TouchbarState.Blue = true;
					break;
				case 0x1a:
					mWiimoteState.Guitar.TouchbarState.Blue = true;
					mWiimoteState.Guitar.TouchbarState.Orange = true;
					break;
				case 0x1f:
					mWiimoteState.Guitar.TouchbarState.Orange = true;
					break;
				}
				break;

			case ExtensionType.Drums:
				// it appears the joystick values are only 6 bits
				mWiimoteState.Drums.RawJoystick.X = (buff[offset + 0] & 0x3f);
				mWiimoteState.Drums.RawJoystick.Y = (buff[offset + 1] & 0x3f);

				mWiimoteState.Drums.Plus = (buff[offset + 4] & 0x04) == 0;
				mWiimoteState.Drums.Minus = (buff[offset + 4] & 0x10) == 0;

				mWiimoteState.Drums.Pedal = (buff[offset + 5] & 0x04) == 0;
				mWiimoteState.Drums.Blue = (buff[offset + 5] & 0x08) == 0;
				mWiimoteState.Drums.Green = (buff[offset + 5] & 0x10) == 0;
				mWiimoteState.Drums.Yellow = (buff[offset + 5] & 0x20) == 0;
				mWiimoteState.Drums.Red = (buff[offset + 5] & 0x40) == 0;
				mWiimoteState.Drums.Orange = (buff[offset + 5] & 0x80) == 0;

				mWiimoteState.Drums.Joystick.X = (float) (mWiimoteState.Drums.RawJoystick.X - 0x1f) / 0x3f;   // not fully accurate, but close
				mWiimoteState.Drums.Joystick.Y = (float) (mWiimoteState.Drums.RawJoystick.Y - 0x1f) / 0x3f;   // not fully accurate, but close

				if ((buff[offset + 2] & 0x40) == 0) {
					int pad = (buff[offset + 2] >> 1) & 0x1f;
					int velocity = (buff[offset + 3] >> 5);

					if (velocity != 7) {
						switch (pad) {
						case 0x1b:
							mWiimoteState.Drums.PedalVelocity = velocity;
							break;
						case 0x19:
							mWiimoteState.Drums.RedVelocity = velocity;
							break;
						case 0x11:
							mWiimoteState.Drums.YellowVelocity = velocity;
							break;
						case 0x0f:
							mWiimoteState.Drums.BlueVelocity = velocity;
							break;
						case 0x0e:
							mWiimoteState.Drums.OrangeVelocity = velocity;
							break;
						case 0x12:
							mWiimoteState.Drums.GreenVelocity = velocity;
							break;
						}
					}
				}

				break;

			case ExtensionType.BalanceBoard:
				mWiimoteState.BalanceBoard.SensorValuesRaw.TopRight = (short) ((short) buff[offset + 0] << 8 | buff[offset + 1]);
				mWiimoteState.BalanceBoard.SensorValuesRaw.BottomRight = (short) ((short) buff[offset + 2] << 8 | buff[offset + 3]);
				mWiimoteState.BalanceBoard.SensorValuesRaw.TopLeft = (short) ((short) buff[offset + 4] << 8 | buff[offset + 5]);
				mWiimoteState.BalanceBoard.SensorValuesRaw.BottomLeft = (short) ((short) buff[offset + 6] << 8 | buff[offset + 7]);

				mWiimoteState.BalanceBoard.SensorValuesKg.TopLeft = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoard.SensorValuesRaw.TopLeft, mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.TopLeft, mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.TopLeft, mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.TopLeft);
				mWiimoteState.BalanceBoard.SensorValuesKg.TopRight = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoard.SensorValuesRaw.TopRight, mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.TopRight, mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.TopRight, mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.TopRight);
				mWiimoteState.BalanceBoard.SensorValuesKg.BottomLeft = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoard.SensorValuesRaw.BottomLeft, mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.BottomLeft, mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.BottomLeft, mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.BottomLeft);
				mWiimoteState.BalanceBoard.SensorValuesKg.BottomRight = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoard.SensorValuesRaw.BottomRight, mWiimoteState.BalanceBoard.CalibrationInfo.Kg0.BottomRight, mWiimoteState.BalanceBoard.CalibrationInfo.Kg17.BottomRight, mWiimoteState.BalanceBoard.CalibrationInfo.Kg34.BottomRight);

				mWiimoteState.BalanceBoard.SensorValuesLb.TopLeft = (mWiimoteState.BalanceBoard.SensorValuesKg.TopLeft * KG2LB);
				mWiimoteState.BalanceBoard.SensorValuesLb.TopRight = (mWiimoteState.BalanceBoard.SensorValuesKg.TopRight * KG2LB);
				mWiimoteState.BalanceBoard.SensorValuesLb.BottomLeft = (mWiimoteState.BalanceBoard.SensorValuesKg.BottomLeft * KG2LB);
				mWiimoteState.BalanceBoard.SensorValuesLb.BottomRight = (mWiimoteState.BalanceBoard.SensorValuesKg.BottomRight * KG2LB);

				mWiimoteState.BalanceBoard.WeightKg = (mWiimoteState.BalanceBoard.SensorValuesKg.TopLeft + mWiimoteState.BalanceBoard.SensorValuesKg.TopRight + mWiimoteState.BalanceBoard.SensorValuesKg.BottomLeft + mWiimoteState.BalanceBoard.SensorValuesKg.BottomRight) / 4.0f;
				mWiimoteState.BalanceBoard.WeightLb = (mWiimoteState.BalanceBoard.SensorValuesLb.TopLeft + mWiimoteState.BalanceBoard.SensorValuesLb.TopRight + mWiimoteState.BalanceBoard.SensorValuesLb.BottomLeft + mWiimoteState.BalanceBoard.SensorValuesLb.BottomRight) / 4.0f;

				float Kx = (mWiimoteState.BalanceBoard.SensorValuesKg.TopLeft + mWiimoteState.BalanceBoard.SensorValuesKg.BottomLeft) / (mWiimoteState.BalanceBoard.SensorValuesKg.TopRight + mWiimoteState.BalanceBoard.SensorValuesKg.BottomRight);
				float Ky = (mWiimoteState.BalanceBoard.SensorValuesKg.TopLeft + mWiimoteState.BalanceBoard.SensorValuesKg.TopRight) / (mWiimoteState.BalanceBoard.SensorValuesKg.BottomLeft + mWiimoteState.BalanceBoard.SensorValuesKg.BottomRight);

				mWiimoteState.BalanceBoard.CenterOfGravity.X = ((float) (Kx - 1) / (float) (Kx + 1)) * (float) (-BSL / 2);
				mWiimoteState.BalanceBoard.CenterOfGravity.Y = ((float) (Ky - 1) / (float) (Ky + 1)) * (float) (-BSW / 2);
				break;

			case ExtensionType.TaikoDrum:
				mWiimoteState.TaikoDrum.OuterLeft = (buff[offset + 5] & 0x20) == 0;
				mWiimoteState.TaikoDrum.InnerLeft = (buff[offset + 5] & 0x40) == 0;
				mWiimoteState.TaikoDrum.InnerRight = (buff[offset + 5] & 0x10) == 0;
				mWiimoteState.TaikoDrum.OuterRight = (buff[offset + 5] & 0x08) == 0;
				break;
			case ExtensionType.MotionPlus:
				mWiimoteState.MotionPlus.YawFast = ((buff[offset + 3] & 0x02) >> 1) == 0;
				mWiimoteState.MotionPlus.PitchFast = ((buff[offset + 3] & 0x01) >> 0) == 0;
				mWiimoteState.MotionPlus.RollFast = ((buff[offset + 4] & 0x02) >> 1) == 0;

				mWiimoteState.MotionPlus.RawValues.X = (buff[offset + 0] | (buff[offset + 3] & 0xf8) << 6);
				mWiimoteState.MotionPlus.RawValues.Y = (buff[offset + 1] | (buff[offset + 4] & 0xf8) << 6);
				mWiimoteState.MotionPlus.RawValues.Z = (buff[offset + 2] | (buff[offset + 5] & 0xf8) << 6);

				mWiimoteState.MotionPlus.Values.X = (float) mWiimoteState.MotionPlus.RawValues.X / GyroConst.X - 1;
				mWiimoteState.MotionPlus.Values.Y = (float) mWiimoteState.MotionPlus.RawValues.Y / GyroConst.Y - 1;
				mWiimoteState.MotionPlus.Values.Z = (float) mWiimoteState.MotionPlus.RawValues.Z / GyroConst.Z - 1;

				break;
			}
		}

		static readonly Point3I GyroConst = new Point3I {
			X = 7918,
			Y = 7791,
			Z = 7840,
		};

		private float GetBalanceBoardSensorValue(short sensor, short min, short mid, short max) {
			if (max == mid || mid == min)
				return 0;

			if (sensor < mid)
				return 68.0f * ((float) (sensor - min) / (mid - min));
			else
				return 68.0f * ((float) (sensor - mid) / (max - mid)) + 68.0f;
		}*/
	}
}

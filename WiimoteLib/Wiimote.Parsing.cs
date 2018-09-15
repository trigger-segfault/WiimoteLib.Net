using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.DataTypes;
using WiimoteLib.Events;
using WiimoteLib.Util;

namespace WiimoteLib {
	public partial class Wiimote : IDisposable {
		private byte[] interleavedBufferA;
		private DataReportAttribute interleavedReportA;

		/// <summary>
		/// Parse a report sent by the Wiimote
		/// </summary>
		/// <param name="buff">Data buffer to parse</param>
		/// <returns>Returns a boolean noting whether an event needs to be posted</returns>
		private bool ParseInputReport(byte[] buff) {
			//try {
			InputReport type = (InputReport) buff[0];
			DataReportAttribute dataReport =
				EnumInfo<InputReport>.TryGetAttribute<DataReportAttribute>(type);

			if (dataReport != null) {
				// Buttons are ALWAYS parsed
				if (dataReport.HasButtons)
					ParseButtons2(buff, dataReport.ButtonsOffset + 1);

				switch (dataReport.Interleave) {
				case Interleave.None:
					if (dataReport.HasAccel)
						ParseAccel2(buff, dataReport.AccelOffset + 1);

					if (dataReport.HasIR)
						ParseIR2(buff, dataReport.IROffset + 1, dataReport.IRSize);

					if (dataReport.HasExt)
						ParseExtension2(buff, dataReport.ExtOffset + 1, dataReport.ExtSize);
					break;
				case Interleave.A:
					interleavedBufferA = buff;
					interleavedReportA = dataReport;
					break;
				case Interleave.B:
					byte[] buffA = interleavedBufferA;
					byte[] buffB = buff;
					DataReportAttribute reportA = interleavedReportA;
					DataReportAttribute reportB = dataReport;
					ParseAccelInterleaved2(buffA, buffB, reportA.AccelOffset + 1, reportB.AccelOffset + 1);
					ParseIRInterleaved2(buffA, buffB, reportA.IROffset + 1, reportB.IROffset + 1);
					break;
				}

				return true;
			}
			else {
				switch (type) {
				case InputReport.Status:
					Debug.WriteLine("******** STATUS ********");

					ExtensionType extensionTypeLast = wiimoteState.ExtensionType;
					bool extensionLast = wiimoteState.Status.Extension;
					ParseButtons2(buff, 1);
					ParseStatus2(buff, 3);
					bool extensionNew = WiimoteState.Status.Extension;

					using (AsyncReadState state = BeginAsyncRead()) {
						byte extensionType = 0;
						if (extensionNew)
							ReadByte(Registers.ExtensionType2);

						Debug.WriteLine($"Extension byte={extensionType:X2}");

						// extension connected?
						Debug.WriteLine($"Extension, Old: {extensionLast}, New: {extensionNew}");

						if (extensionNew != extensionLast || extensionType == 0x04 || extensionType == 0x5) {

							if (wiimoteState.Extension) {
								InitializeExtension(extensionType);
								SetReportType(wiimoteState.ReportType,
									wiimoteState.IRState.Sensitivity,
									wiimoteState.ContinuousReport);
							}
							else {
								wiimoteState.ExtensionType = ExtensionType.None;
								wiimoteState.Nunchuk = new NunchukState();
								wiimoteState.ClassicController = new ClassicControllerState();
								RaiseExtensionChanged(extensionTypeLast, false);
								SetReportType(wiimoteState.ReportType,
									wiimoteState.IRState.Sensitivity,
									wiimoteState.ContinuousReport);
							}
						}
					}
					statusDone.Set();
					//Respond(OutputReport.Status, true);
					break;
				case InputReport.ReadData:
					Debug.WriteLine("******** READ DATA ********");
					ParseButtons2(buff, 1);
					ParseReadData(buff);
					break;
				case InputReport.AcknowledgeOutputReport:
					Debug.WriteLine("******** ACKNOWLEDGE ********");
					ParseButtons2(buff, 1);
					OutputReport outputType = (OutputReport) buff[3];
					WriteResult result = (WriteResult) buff[4];
					if (outputType == OutputReport.WriteMemory) {
						writeDone.Set();
						Debug.WriteLine("Write done");
					}
					//Acknowledge(outputType, result);
					break;
				default:
					Debug.WriteLine($"Unknown input report: {type}");
					break;
				}
			}
			//}
			//catch (TimeoutException) { }
			return true;
		}

		/// <summary>
		/// Handles setting up an extension when plugged in
		/// </summary>
		private void InitializeExtension(byte extensionType) {
			Debug.WriteLine("InitExtension");

			// only initialize if it's not a MotionPlus
			if (extensionType != 0x04 && extensionType != 0x05) {
				WriteByte(Registers.ExtensionInit1, 0x55);
				WriteByte(Registers.ExtensionInit2, 0x00);
			}

			// start reading again
			byte[] buff = ReadData(Registers.ExtensionType1, 6);
			long type = ((long) buff[0] << 40) | ((long) buff[1] << 32) | ((long) buff[2]) << 24 | ((long) buff[3]) << 16 | ((long) buff[4]) << 8 | buff[5];

			switch ((ExtensionType) type) {
			case ExtensionType.None:
			case ExtensionType.ParitallyInserted:
				wiimoteState.Extension = false;
				wiimoteState.ExtensionType = ExtensionType.None;
				return;
			case ExtensionType.Nunchuk:
			case ExtensionType.ClassicController:
			case ExtensionType.MotionPlus:
			case ExtensionType.MotionPlusNunchuk:
				wiimoteState.ExtensionType = (ExtensionType) type;
				//this.SetReportType(InputReport.ButtonsExt19, true);
				break;
			default:
				throw new WiimoteException(this, $"Unknown extension controller found: {type:x12}");
			}

			switch (wiimoteState.ExtensionType) {
			case ExtensionType.Nunchuk:
				buff = ReadData(Registers.ExtensionCalibration, 16);

				wiimoteState.Nunchuk.CalibrationInfo.Parse(buff, 0);

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
				Debug.WriteLine("Nunchuk Calibration:");
				var calib = wiimoteState.Nunchuk.CalibrationInfo;
				Debug.WriteLine($"Max={calib.Max} Min={calib.Min} Mid={calib.Mid}");
				break;
			case ExtensionType.ClassicController:
				buff = ReadData(Registers.ExtensionCalibration, 16);

				wiimoteState.ClassicController.CalibrationInfo.MaxXL = (byte) (buff[0] >> 2);
				wiimoteState.ClassicController.CalibrationInfo.MinXL = (byte) (buff[1] >> 2);
				wiimoteState.ClassicController.CalibrationInfo.MidXL = (byte) (buff[2] >> 2);
				wiimoteState.ClassicController.CalibrationInfo.MaxYL = (byte) (buff[3] >> 2);
				wiimoteState.ClassicController.CalibrationInfo.MinYL = (byte) (buff[4] >> 2);
				wiimoteState.ClassicController.CalibrationInfo.MidYL = (byte) (buff[5] >> 2);

				wiimoteState.ClassicController.CalibrationInfo.MaxXR = (byte) (buff[6] >> 3);
				wiimoteState.ClassicController.CalibrationInfo.MinXR = (byte) (buff[7] >> 3);
				wiimoteState.ClassicController.CalibrationInfo.MidXR = (byte) (buff[8] >> 3);
				wiimoteState.ClassicController.CalibrationInfo.MaxYR = (byte) (buff[9] >> 3);
				wiimoteState.ClassicController.CalibrationInfo.MinYR = (byte) (buff[10] >> 3);
				wiimoteState.ClassicController.CalibrationInfo.MidYR = (byte) (buff[11] >> 3);

				// this doesn't seem right...
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MinTriggerL = (byte)(buff[12] >> 3);
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MaxTriggerL = (byte)(buff[14] >> 3);
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MinTriggerR = (byte)(buff[13] >> 3);
				//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MaxTriggerR = (byte)(buff[15] >> 3);
				wiimoteState.ClassicController.CalibrationInfo.MinTriggerL = 0;
				wiimoteState.ClassicController.CalibrationInfo.MaxTriggerL = 31;
				wiimoteState.ClassicController.CalibrationInfo.MinTriggerR = 0;
				wiimoteState.ClassicController.CalibrationInfo.MaxTriggerR = 31;
				break;
			case ExtensionType.MotionPlusOther:
				//buff = ReadData(Registers.PassthroughCalibration, 16);
				//mWiimoteState.Nunchuk.CalibrationInfo.Parse(buff, 0);

				goto case ExtensionType.MotionPlus;
			case ExtensionType.MotionPlusNunchuk:
				buff = ReadData(Registers.PassthroughCalibration, 16);
				wiimoteState.Nunchuk.CalibrationInfo.Parse(buff, 0);

				goto case ExtensionType.MotionPlus;
			case ExtensionType.MotionPlus:
				// Doesn't do anything yet
				buff = ReadData(Registers.ExtensionCalibration, 32);
				wiimoteState.MotionPlus.CalibrationInfo.Parse(buff, 0);
				break;
			}
			Debug.WriteLine(wiimoteState.ExtensionType);
			RaiseExtensionChanged(wiimoteState.ExtensionType, true);
		}
		private void ParseStatus2(byte[] buff, int off) {
			wiimoteState.Status.Parse(buff, off);
		}

		private void ParseButtons2(byte[] buff, int off) {
			wiimoteState.Buttons.Parse(buff, off);
		}

		private void ParseAccel2(byte[] buff, int off) {
			wiimoteState.Accel.ParseWiimote(buff, off, wiimoteState.AccelCalibrationInfo);
		}

		private void ParseAccelInterleaved2(byte[] buffA, byte[] buffB, int offA, int offB) {
			wiimoteState.Accel.ParseWiimoteInterleaved(
				buffA, buffB, offA, offB, wiimoteState.AccelCalibrationInfo);
		}

		private void ParseIR2(byte[] buff, int off, int size) {
			wiimoteState.IRState.IRSensor0.RawPosition.X = buff[off + 0] | ((buff[off + 2] >> 4) & 0x03) << 8;
			wiimoteState.IRState.IRSensor0.RawPosition.Y = buff[off + 1] | ((buff[off + 2] >> 6) & 0x03) << 8;

			switch (wiimoteState.IRState.Mode) {
			case IRMode.Basic:
				wiimoteState.IRState.IRSensor1.RawPosition.X = buff[off + 3] | ((buff[off + 2] >> 0) & 0x03) << 8;
				wiimoteState.IRState.IRSensor1.RawPosition.Y = buff[off + 4] | ((buff[off + 2] >> 2) & 0x03) << 8;

				wiimoteState.IRState.IRSensor2.RawPosition.X = buff[off + 5] | ((buff[off + 7] >> 4) & 0x03) << 8;
				wiimoteState.IRState.IRSensor2.RawPosition.Y = buff[off + 6] | ((buff[off + 7] >> 6) & 0x03) << 8;

				wiimoteState.IRState.IRSensor3.RawPosition.X = buff[off + 8] | ((buff[off + 7] >> 0) & 0x03) << 8;
				wiimoteState.IRState.IRSensor3.RawPosition.Y = buff[off + 9] | ((buff[off + 7] >> 2) & 0x03) << 8;

				wiimoteState.IRState.IRSensor0.Size = 0x00;
				wiimoteState.IRState.IRSensor1.Size = 0x00;
				wiimoteState.IRState.IRSensor2.Size = 0x00;
				wiimoteState.IRState.IRSensor3.Size = 0x00;

				wiimoteState.IRState.IRSensor0.Found = !(buff[off + 0] == 0xff && buff[off + 1] == 0xff);
				wiimoteState.IRState.IRSensor1.Found = !(buff[off + 3] == 0xff && buff[off + 4] == 0xff);
				wiimoteState.IRState.IRSensor2.Found = !(buff[off + 5] == 0xff && buff[off + 6] == 0xff);
				wiimoteState.IRState.IRSensor3.Found = !(buff[off + 8] == 0xff && buff[off + 9] == 0xff);
				break;
			case IRMode.Extended:
				wiimoteState.IRState.IRSensor1.RawPosition.X = buff[off + 3] | ((buff[off + 5] >> 4) & 0x03) << 8;
				wiimoteState.IRState.IRSensor1.RawPosition.Y = buff[off + 4] | ((buff[off + 5] >> 6) & 0x03) << 8;
				wiimoteState.IRState.IRSensor2.RawPosition.X = buff[off + 6] | ((buff[off + 8] >> 4) & 0x03) << 8;
				wiimoteState.IRState.IRSensor2.RawPosition.Y = buff[off + 7] | ((buff[off + 8] >> 6) & 0x03) << 8;
				wiimoteState.IRState.IRSensor3.RawPosition.X = buff[off + 9] | ((buff[off + 11] >> 4) & 0x03) << 8;
				wiimoteState.IRState.IRSensor3.RawPosition.Y = buff[off + 10] | ((buff[off + 11] >> 6) & 0x03) << 8;

				wiimoteState.IRState.IRSensor0.Size = buff[off + 2] & 0x0f;
				wiimoteState.IRState.IRSensor1.Size = buff[off + 5] & 0x0f;
				wiimoteState.IRState.IRSensor2.Size = buff[off + 8] & 0x0f;
				wiimoteState.IRState.IRSensor3.Size = buff[off + 11] & 0x0f;

				wiimoteState.IRState.IRSensor0.Found = !(buff[off + 0] == 0xff && buff[off + 1] == 0xff && buff[off + 2] == 0xff);
				wiimoteState.IRState.IRSensor1.Found = !(buff[off + 3] == 0xff && buff[off + 4] == 0xff && buff[off + 5] == 0xff);
				wiimoteState.IRState.IRSensor2.Found = !(buff[off + 6] == 0xff && buff[off + 7] == 0xff && buff[off + 8] == 0xff);
				wiimoteState.IRState.IRSensor3.Found = !(buff[off + 9] == 0xff && buff[off + 10] == 0xff && buff[off + 11] == 0xff);
				break;
			}

			wiimoteState.IRState.IRSensor0.Position.X = (float) (wiimoteState.IRState.IRSensor0.RawPosition.X / 1023.5f);
			wiimoteState.IRState.IRSensor1.Position.X = (float) (wiimoteState.IRState.IRSensor1.RawPosition.X / 1023.5f);
			wiimoteState.IRState.IRSensor2.Position.X = (float) (wiimoteState.IRState.IRSensor2.RawPosition.X / 1023.5f);
			wiimoteState.IRState.IRSensor3.Position.X = (float) (wiimoteState.IRState.IRSensor3.RawPosition.X / 1023.5f);

			wiimoteState.IRState.IRSensor0.Position.Y = (float) (wiimoteState.IRState.IRSensor0.RawPosition.Y / 767.5f);
			wiimoteState.IRState.IRSensor1.Position.Y = (float) (wiimoteState.IRState.IRSensor1.RawPosition.Y / 767.5f);
			wiimoteState.IRState.IRSensor2.Position.Y = (float) (wiimoteState.IRState.IRSensor2.RawPosition.Y / 767.5f);
			wiimoteState.IRState.IRSensor3.Position.Y = (float) (wiimoteState.IRState.IRSensor3.RawPosition.Y / 767.5f);

			if (wiimoteState.IRState.IRSensor0.Found && wiimoteState.IRState.IRSensor1.Found) {
				wiimoteState.IRState.RawMidpoint.X = (wiimoteState.IRState.IRSensor1.RawPosition.X + wiimoteState.IRState.IRSensor0.RawPosition.X) / 2;
				wiimoteState.IRState.RawMidpoint.Y = (wiimoteState.IRState.IRSensor1.RawPosition.Y + wiimoteState.IRState.IRSensor0.RawPosition.Y) / 2;

				wiimoteState.IRState.Midpoint.X = (wiimoteState.IRState.IRSensor1.Position.X + wiimoteState.IRState.IRSensor0.Position.X) / 2.0f;
				wiimoteState.IRState.Midpoint.Y = (wiimoteState.IRState.IRSensor1.Position.Y + wiimoteState.IRState.IRSensor0.Position.Y) / 2.0f;
			}
			else
				wiimoteState.IRState.Midpoint.X = wiimoteState.IRState.Midpoint.Y = 0.0f;
		}

		private void ParseNunchuk(byte[] buff, int off) {
			wiimoteState.Nunchuk.Parse(buff, off, false);
		}

		private void ParseClassicController(byte[] buff, int off) {
			//mWiimoteState.ClassicControllerState.Parse(buff, off, false);
		}

		private void ParseMotionPlus(byte[] buff, int off) {
			bool passthrough = !buff.GetBit(off + 5, 1);

			if (!passthrough) {
				wiimoteState.MotionPlus.Parse(buff, off);
			}
			else {
				wiimoteState.Nunchuk.Parse(buff, off, true);
			}
		}

		private void ParseExtension2(byte[] buff, int off, int size) {
			switch (wiimoteState.ExtensionType) {
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
	}
}

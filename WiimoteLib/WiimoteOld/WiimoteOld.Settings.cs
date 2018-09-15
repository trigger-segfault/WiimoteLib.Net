using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.DataTypes;
using WiimoteLib.Util;

namespace WiimoteLib {
	/// <summary>
	/// Implementation of Wiimote
	/// </summary>
	public partial class WiimoteOld : IDisposable {

		/// <summary>
		/// Initialize the MotionPlus extension
		/// </summary>
		public void EnableMotionPlus(MotionPlusExtensionType extension = MotionPlusExtensionType.NoExtension) {
			Debug.WriteLine("InitializeMotionPlus");
			WriteData(Registers.ExtensionInit1, 0x55);
			/*WriteData(Registers.ExtensionInit2, 0x00);
			WriteData(Registers.ExtensionInit1, 0x55);*/
			WriteData(Registers.MotionPlusEnable, (byte) extension);
			mWiimoteState.MotionPlus.ExtensionType = extension;
		}

		public void DisableMotionPlus() {
			//if (mWiimoteState.MotionPlus.ExtensionType != MotionPlusExtensionType.NoExtension) {
				WriteData(Registers.MotionPlusDisable, 0x55);
				mWiimoteState.MotionPlus.ExtensionType = MotionPlusExtensionType.NoExtension;
			//}
		}

		/// <summary>
		/// Set Wiimote reporting mode (if using an IR report type, IR sensitivity is set to WiiLevel3)
		/// </summary>
		/// <param name="type">Report type</param>
		/// <param name="continuous">Continuous data</param>
		public void SetReportType(InputReport type, bool continuous) {
			Debug.WriteLine("SetReportType: " + type);
			SetReportType(type, IRSensitivity.Maximum, continuous);
		}

		/// <summary>
		/// Set Wiimote reporting mode
		/// </summary>
		/// <param name="type">Report type</param>
		/// <param name="irSensitivity">IR sensitivity</param>
		/// <param name="continuous">Continuous data</param>
		public void SetReportType(InputReport type, IRSensitivity irSensitivity, bool continuous) {
			// only 1 report type allowed for the BB
			//if (mWiimoteState.ExtensionType == ExtensionType.BalanceBoard)
			//	type = InputReport.ButtonsExt19;


			DataReportAttribute dataReport =
				EnumInfo<InputReport>.TryGetAttribute<DataReportAttribute>(type);

			if (dataReport == null)
				throw new WiimoteException($"{type} is not a valid report type!");

			int irSize = dataReport.IRSize;
			if (dataReport.IsInterleaved)
				irSize *= 2;

			switch (dataReport.IRSize) {
			case 10:
				EnableIR(IRMode.Basic, irSensitivity);
				break;
			case 12:
				EnableIR(IRMode.Extended, irSensitivity);
				break;
			case 36:
				EnableIR(IRMode.Full, irSensitivity);
				break;
			default:
				DisableIR();
				break;
			}

			/*switch (type) {
			case InputReport.ButtonsAccelIR12:
				EnableIR(IRMode.Extended, irSensitivity);
				break;
			case InputReport.ButtonsAccelIR10Ext6:
				EnableIR(IRMode.Basic, irSensitivity);
				break;
			default:
				DisableIR();
				break;
			}*/

			/*byte[] buff = CreateReport();
			buff[0] = (byte) OutputReport.DataReportType;
			buff[1] = (byte) ((continuous ? 0x04 : 0x00) | GetRumbleBit());
			buff[2] = (byte) type;*/

			byte[] buff = CreateReport();
			buff[0] = (byte) ((continuous ? 0x04 : 0x00) | GetRumbleBit());
			buff[1] = (byte) type;

			WriteReport2(OutputReport.DataReportType, buff);
		}

		/// <summary>
		/// Set the LEDs on the Wiimote
		/// </summary>
		/// <param name="led1">LED 1</param>
		/// <param name="led2">LED 2</param>
		/// <param name="led3">LED 3</param>
		/// <param name="led4">LED 4</param>
		public void SetLEDs(bool led1, bool led2, bool led3, bool led4) {
			mWiimoteState.Status.LED1 = led1;
			mWiimoteState.Status.LED2 = led2;
			mWiimoteState.Status.LED3 = led3;
			mWiimoteState.Status.LED4 = led4;
			LEDs leds = mWiimoteState.Status.LEDs;

			/*byte[] buff = CreateReport();

			buff[0] = (byte) OutputReport.LEDs;
			buff[1] = (byte) (
						(led1 ? 0x10 : 0x00) |
						(led2 ? 0x20 : 0x00) |
						(led3 ? 0x40 : 0x00) |
						(led4 ? 0x80 : 0x00) |
						GetRumbleBit());

			WriteReport(buff);*/

			byte[] buff = CreateReport2();
			buff[0] = (byte) (((byte) leds << 4) | GetRumbleBit());

			WriteReport2(OutputReport.LEDs, buff);
		}

		public void SetLEDs(LEDs leds) {
			mWiimoteState.Status.LEDs = leds;

			/*byte[] buff = CreateReport();

			buff[0] = (byte) OutputReport.LEDs;
			buff[1] = (byte) (
						(led1 ? 0x10 : 0x00) |
						(led2 ? 0x20 : 0x00) |
						(led3 ? 0x40 : 0x00) |
						(led4 ? 0x80 : 0x00) |
						GetRumbleBit());*/

			byte[] buff = CreateReport2();
			buff[0] = (byte) (((byte) leds << 4) | GetRumbleBit());

			WriteReport2(OutputReport.LEDs, buff);
		}

		/// <summary>
		/// Set the LEDs on the Wiimote
		/// </summary>
		/// <param name="leds">The value to be lit up in base2 on the Wiimote</param>
		/*public void SetLEDs(int leds) {
			mWiimoteState.LED.LED1 = (leds & 0x01) > 0;
			mWiimoteState.LED.LED2 = (leds & 0x02) > 0;
			mWiimoteState.LED.LED3 = (leds & 0x04) > 0;
			mWiimoteState.LED.LED4 = (leds & 0x08) > 0;

			byte[] buff = CreateReport();

			buff[0] = (byte) OutputReport.LEDs;
			buff[1] = (byte) (
						((leds & 0x01) > 0 ? 0x10 : 0x00) |
						((leds & 0x02) > 0 ? 0x20 : 0x00) |
						((leds & 0x04) > 0 ? 0x40 : 0x00) |
						((leds & 0x08) > 0 ? 0x80 : 0x00) |
						GetRumbleBit());

			WriteReport(buff);
		}*/

		/// <summary>
		/// Toggle rumble
		/// </summary>
		/// <param name="on">On or off</param>
		public void SetRumble(bool on) {
			mWiimoteState.Status.Rumble = on;

			// the LED report also handles rumble
			SetLEDs(mWiimoteState.Status.LEDs);
		}
	}
}

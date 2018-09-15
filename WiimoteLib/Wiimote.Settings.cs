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
		/// <summary>Initialize the MotionPlus extension.</summary>
		public void EnableMotionPlus(MotionPlusExtensionType extension = MotionPlusExtensionType.NoExtension) {
			Debug.WriteLine("InitializeMotionPlus");
			WriteByte(Registers.ExtensionInit1, 0x55);
			/*WriteData(Registers.ExtensionInit2, 0x00);
			WriteData(Registers.ExtensionInit1, 0x55);*/
			WriteByte(Registers.MotionPlusEnable, (byte) extension);
			wiimoteState.MotionPlus.ExtensionType = extension;
		}

		/// <summary>Turns off the MotionPlus extension.</summary>
		public void DisableMotionPlus() {
			Debug.WriteLine("DisableMotionPlus");
			//if (mWiimoteState.MotionPlus.ExtensionType != MotionPlusExtensionType.NoExtension) {
			WriteByte(Registers.MotionPlusDisable, 0x55);
			wiimoteState.MotionPlus.ExtensionType = MotionPlusExtensionType.NoExtension;
			//}
		}

		/// <summary>Set Wiimote reporting mode (if using an IR report type, IR
		/// sensitivity is set to WiiLevel3).</summary>
		/// <param name="type">Report type</param>
		/// <param name="continuous">Continuous data</param>
		public void SetReportType(ReportType type, bool continuous) {
			Debug.WriteLine("SetReportType: " + type);
			SetReportType(type, IRSensitivity.Maximum, continuous);
		}

		/// <summary>Set Wiimote reporting mode.</summary>
		/// <param name="reportType">Report type</param>
		/// <param name="irSensitivity">IR sensitivity</param>
		/// <param name="continuous">Continuous data</param>
		public void SetReportType(ReportType reportType, IRSensitivity irSensitivity, bool continuous) {
			InputReport type = (InputReport) reportType;
			DataReportAttribute dataReport =
				EnumInfo<InputReport>.TryGetAttribute<DataReportAttribute>(type);

			if (dataReport == null)
				throw new WiimoteException(this, $"{type} is not a valid report type!");

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
			
			byte[] buff = CreateReport(OutputReport.InputReportType);

			buff[1] = (byte) (continuous ? 0x04 : 0x00);
			buff[2] = (byte) type;

			WriteReport(buff);
			wiimoteState.ReportType = reportType;
			wiimoteState.ContinuousReport = continuous;
		}

		/// <summary>Set the LEDs on the Wiimote.</summary>
		/// <param name="led1">LED 1</param>
		/// <param name="led2">LED 2</param>
		/// <param name="led3">LED 3</param>
		/// <param name="led4">LED 4</param>
		public void SetLEDs(bool led1, bool led2, bool led3, bool led4) {
			LEDs leds = LEDs.None;
			if (led1) leds |= LEDs.LED1;
			if (led2) leds |= LEDs.LED2;
			if (led3) leds |= LEDs.LED3;
			if (led4) leds |= LEDs.LED4;
			SetLEDs(leds);
		}

		/// <summary>Set the LEDs on the Wiimote.</summary>
		public void SetLEDs(LEDs leds) {
			wiimoteState.Status.LEDs = leds;

			byte[] buff = CreateReport(OutputReport.LEDs);
			buff[1] = (byte) ((byte) leds << 4);
			
			WriteReport(buff);
		}

		/// <summary>Set 1-indexed player LED.</summary>
		public void SetPlayerLED(int player) {
			LEDs leds = LEDs.None;
			switch (player) {
			case 1: leds = LEDs.LED1; break;
			case 2: leds = LEDs.LED2; break;
			case 3: leds = LEDs.LED3; break;
			case 4: leds = LEDs.LED4; break;
			case 5: leds = LEDs.LED1 | LEDs.LED2; break;
			case 6: leds = LEDs.LED1 | LEDs.LED3; break;
			case 7: leds = LEDs.LED1 | LEDs.LED4; break;
			case 8: leds = LEDs.LED1 | LEDs.LED2 | LEDs.LED3; break;
			case 9: leds = LEDs.LED1 | LEDs.LED2 | LEDs.LED4; break;
			}
			if (player > 9)
				leds = LEDs.LED1 | LEDs.LED2 | LEDs.LED3 | LEDs.LED4;
			SetLEDs(leds);
		}

		/// <summary>Toggle rumble.</summary>
		/// <param name="on">On or off</param>
		public void SetRumble(bool on) {
			wiimoteState.Status.Rumble = on;

			byte[] buff = CreateReport(OutputReport.Rumble);
			WriteReport(buff);
		}
	}
}

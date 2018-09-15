using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Geometry;
using WiimoteLib.Util;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current state of the Nunchuk extension
	/// </summary>
	[Serializable]
	public struct NunchukState {
		/// <summary>
		/// Calibration data for Nunchuk extension
		/// </summary>
		public NunchukCalibrationInfo CalibrationInfo;
		/// <summary>
		/// State of accelerometers
		/// </summary>
		public AccelState Accel;
		/// <summary>
		/// Raw joystick position before normalization.  Values range between 0 and 255.
		/// </summary>
		public Point2I RawJoystick;
		/// <summary>
		/// Normalized joystick position.  Values range between -0.5 and 0.5
		/// </summary>
		public Point2F Joystick;
		/// <summary>
		/// Digital button on Nunchuk extension
		/// </summary>
		public bool C, Z;

		const int AnalogStickCenter = 128;
		static readonly Point2I JoystickMin = new Point2I(35, 27);
		static readonly Point2I JoystickNax = new Point2I(228, 220);

		internal void Parse(byte[] buff, int off, bool passthrough) {
			Accel.ParseNunchuk(buff, off, passthrough, CalibrationInfo.AccelCalibration);

			// pressed == 0... huh
			if (!passthrough) {
				/*Z = (buff[off + 5] & 0x1) == 0;
				C = (buff[off + 5] & 0x2) == 0;*/
				Z = !buff.GetBit(off + 5, 0);
				C = !buff.GetBit(off + 5, 1);
			}
			else {
				/*Z = (buff[off + 5] & 0x4) == 0;
				C = (buff[off + 5] & 0x8) == 0;*/
				Z = !buff.GetBit(off + 5, 2);
				C = !buff.GetBit(off + 5, 3);
			}

			RawJoystick.X = buff[off + 0];
			RawJoystick.Y = buff[off + 1];

			if (CalibrationInfo.Max.X != 0)
				Joystick.X = (float) (RawJoystick.X - CalibrationInfo.Mid.X) /
									 (CalibrationInfo.Max.X - CalibrationInfo.Min.X);
			else
				Joystick.X = 0f;

			if (CalibrationInfo.Max.Y != 0)
				Joystick.Y = (float) (RawJoystick.Y - CalibrationInfo.Mid.Y) /
									 (CalibrationInfo.Max.Y - CalibrationInfo.Min.Y);
			else
				Joystick.Y = 0f;
		}

		/*internal void ParsePassthrough(byte[] buff, int off) {
			Accel.ParseNunchukPassThrough(buff, off, CalibrationInfo.AccelCalibration);

			Z = (buff[off + 5] & 0x4) != 0;
			C = (buff[off + 5] & 0x8) != 0;

			Z = buff.GetBit(off + 5, 2);
			C = buff.GetBit(off + 5, 3);

			ParseJoystick(buff, off);
		}

		private void ParseJoystick(byte[] buff, int off) {
			RawJoystick.X = buff[off + 0];
			RawJoystick.Y = buff[off + 1];

			if (CalibrationInfo.Max.X != 0x00)
				Joystick.X = (float) (RawJoystick.X - CalibrationInfo.Mid.X) /
									 (CalibrationInfo.Max.X - CalibrationInfo.Min.X);

			if (CalibrationInfo.Max.Y != 0x00)
				Joystick.Y = (float) (RawJoystick.Y - CalibrationInfo.Mid.Y) /
									 (CalibrationInfo.Max.Y - CalibrationInfo.Min.Y);
		}*/
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Geometry;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Calibration information stored on the Nunchuk
	/// </summary>
	[Serializable]
	public struct NunchukCalibrationInfo {
		/// <summary>
		/// Accelerometer calibration data
		/// </summary>
		public AccelCalibrationInfo AccelCalibration;
		
		/// <summary>
		/// Joystick axis min calibration
		/// </summary>
		public Point2I Min;
		/// <summary>
		/// Joystick axis mid calibration
		/// </summary>
		public Point2I Mid;
		/// <summary>
		/// Joystick axis max calibration
		/// </summary>
		public Point2I Max;

		internal void Parse(byte[] buff, int off) {
			// 0:2,4:6
			AccelCalibration.Parse(buff, 0);

			Max.X = buff[off +  8];
			Min.X = buff[off +  9];
			Mid.X = buff[off + 10];
			Max.Y = buff[off + 11];
			Min.Y = buff[off + 12];
			Mid.Y = buff[off + 13];
		}
	}
}

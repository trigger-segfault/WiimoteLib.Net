using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Geometry;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Accelerometer calibration information
	/// </summary>
	[Serializable]
	public struct AccelCalibrationInfo {
		/// <summary>
		/// Zero point of accelerometer
		/// </summary>
		public Point3I Zero;
		/// <summary>
		/// Gravity at rest of accelerometer
		/// </summary>
		public Point3I Gravity;

		internal void Parse(byte[] buff, int off) {
			Zero.X = buff[off + 0];
			Zero.Y = buff[off + 1];
			Zero.Z = buff[off + 2];
			Gravity.X = buff[off + 4];
			Gravity.Y = buff[off + 5];
			Gravity.Z = buff[off + 6];
		}
	}
}

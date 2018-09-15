using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Geometry;
using WiimoteLib.Util;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current state of the accelerometers
	/// </summary>
	[Serializable]
	public struct AccelState {
		/// <summary>
		/// Raw accelerometer data.
		/// <remarks>Values range between 0 - 255</remarks>
		/// </summary>
		public Point3I RawValues;
		/// <summary>
		/// Normalized accelerometer data.
		/// <remarks>Values range between 0 - ?, but values > 3 and &lt; -3 are inaccurate.</remarks>
		/// </summary>
		public Point3F Values;


		internal void ParseWiimote(byte[] buff, int off, AccelCalibrationInfo calib) {
			// X = (byte[2] bits[7:0] 9:2) | (byte[0] bits[6:5] 1:0) out[9:0]
			// Y = (byte[3] bits[7:0] 9:2) | (byte[1] bit[5] 1) out[9:1]
			// Z = (byte[4] bits[7:0] 9:2) | (byte[1] bit[6] 1) out[9:1]
			RawValues.X = (buff[off + 2] << 2) | ((buff[0] >> 5) & 0x3);
			RawValues.Y = (buff[off + 3] << 2) | ((buff[1] >> 4) & 0x2);
			RawValues.Z = (buff[off + 4] << 2) | ((buff[1] >> 5) & 0x2);
			ParseRaw(calib);
		}

		internal void ParseWiimoteInterleaved(byte[] buffA, byte[] buffB, int offA, int offB, AccelCalibrationInfo calib) {
			// X = (byteA[2] bits[7:0] 9:2) out[9:2]
			// Y = (byteB[2] bits[7:0] 9:2) out[9:2]
			// Z = (byteA[1] bits[6:5] 9:8) | (byteA[0] bits[6:5] 7:6) |
			//     (byteB[1] bits[6:5] 5:4) | (byteB[0] bits[6:5] 3:2) out[9:2]
			RawValues.X = (buffA[offA + 2] << 2);
			RawValues.Y = (buffB[offB + 2] << 2);
			RawValues.Z = ((buffA[offA + 1] & 0x60) << 3) | ((buffA[offA + 0] & 0x60) << 1) |
						  ((buffB[offB + 1] & 0x60) >> 1) | ((buffB[offB + 0] & 0x60) >> 3);
			
			ParseRaw(calib);
		}

		internal void ParseNunchuk(byte[] buff, int off, bool passthrough, AccelCalibrationInfo calib) {
			if (!passthrough) {
				// X = (byte[2] bits[7:0] 9:2) | (byte[5] bits[3:2] 1:0) out[9:0]
				// Y = (byte[3] bits[7:0] 9:2) | (byte[5] bits[5:4] 1:0) out[9:0]
				// Z = (byte[4] bits[7:0] 9:2) | (byte[5] bits[7:6] 1:0) out[9:0]
				RawValues.X = (buff[off + 2] << 2) | ((buff[5] >> 2) & 0x3);
				RawValues.Y = (buff[off + 3] << 2) | ((buff[5] >> 4) & 0x3);
				RawValues.Z = (buff[off + 4] << 2) | ((buff[5] >> 6) & 0x3);
			}
			else {
				// X = (byte[2] bits[7:0] 9:2) | (byte[5] bit[4] 1) out[9:1]
				// Y = (byte[3] bits[7:0] 9:2) | (byte[5] bit[5] 1) out[9:1]
				// Z = (byte[4] bits[7:1] 9:3) | (byte[5] bits[7:6] 2:1) out[9:1]
				RawValues.X = (buff[off + 2] << 2) | ((buff[5] >> 3) & 0x2);
				RawValues.Y = (buff[off + 3] << 2) | ((buff[5] >> 4) & 0x2);
				RawValues.Z = ((buff[off + 4] & 0xFE) << 2) | ((buff[5] >> 5) & 0x6);
			}
			ParseRaw(calib);
		}

		private void ParseRaw(AccelCalibrationInfo calib) {
			Values.X = (float) (RawValues.X - calib.Zero.X) / (calib.Gravity.X - calib.Zero.X);
			Values.Y = (float) (RawValues.Y - calib.Zero.Y) / (calib.Gravity.Y - calib.Zero.Y);
			Values.Z = (float) (RawValues.Z - calib.Zero.Z) / (calib.Gravity.Z - calib.Zero.Z);
		}
	}
}

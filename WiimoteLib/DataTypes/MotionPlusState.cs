using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Geometry;
using WiimoteLib.Util;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current state of the MotionPlus controller
	/// </summary>
	[Serializable]
	public struct MotionPlusState {

		private const int Zero = 8063;
		private const float UnitsToDegreesPerSecond = 8192f / 595f;
		private const float HighSpeed = 2000f / 440f;

		/// <summary>The calibration info for the Wii Motion Plus.</summary>
		public MotionPlusCalibrationInfo CalibrationInfo;

		/// <summary>
		/// Raw speed data
		/// <remarks>Values range between 0 - 16384</remarks>
		/// </summary>
		public PitchYawRollI RawValues;

		/// <summary>
		/// Values in degrees per second.
		/// </summary>
		public PitchYawRollF Values;

		/// <summary>
		/// Yaw/Pitch/Roll rotating "slowly"
		/// </summary>
		public bool YawSlow, RollSlow, PitchSlow;
		
		/// <summary>An extension is connected to the Wiimotion Plus</summary>
		public bool HasExtension;

		public MotionPlusExtensionType ExtensionType;

		public bool IsDetected;

		internal void Parse(byte[] buff, int off) {

			/*YawSlow   = (buff[off + 3] & 0x02) != 0;
			RollSlow  = (buff[off + 4] & 0x02) != 0;
			PitchSlow = (buff[off + 3] & 0x01) != 0;

			ExtensionConnected = (buff[off + 4] & 0x01) != 0;*/

			YawSlow   = buff.GetBit(off + 3, 1);
			RollSlow  = buff.GetBit(off + 4, 1);
			PitchSlow = buff.GetBit(off + 3, 0);

			HasExtension = buff.GetBit(off + 4, 0);

			// Get raw
			RawValues.Yaw   = ((buff[off + 3] & 0xFC) << 6) | buff[off + 0];
			RawValues.Roll  = ((buff[off + 4] & 0xFC) << 6) | buff[off + 1];
			RawValues.Pitch = ((buff[off + 5] & 0xFC) << 6) | buff[off + 2];

			// Zero raw
			Values.Yaw   = RawValues.Yaw   - Zero;
			Values.Roll  = RawValues.Roll  - Zero;
			Values.Pitch = RawValues.Pitch - Zero;
			
			// Multiply when high speed
			Values.Yaw   *= (YawSlow   ? 1f : HighSpeed);
			Values.Roll  *= (RollSlow  ? 1f : HighSpeed);
			Values.Pitch *= (PitchSlow ? 1f : HighSpeed);

			// Convert units
			Values.Yaw   /= UnitsToDegreesPerSecond;
			Values.Roll  /= UnitsToDegreesPerSecond;
			Values.Pitch /= UnitsToDegreesPerSecond;
		}
	}
}

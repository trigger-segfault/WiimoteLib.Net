using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Calibration information stored on the Classic Controller
	/// </summary>
	[Serializable]
	public struct ClassicControllerCalibrationInfo {
		/// <summary>
		/// Left joystick X-axis 
		/// </summary>
		public byte MinXL, MidXL, MaxXL;
		/// <summary>
		/// Left joystick Y-axis
		/// </summary>
		public byte MinYL, MidYL, MaxYL;
		/// <summary>
		/// Right joystick X-axis
		/// </summary>
		public byte MinXR, MidXR, MaxXR;
		/// <summary>
		/// Right joystick Y-axis
		/// </summary>
		public byte MinYR, MidYR, MaxYR;
		/// <summary>
		/// Left analog trigger
		/// </summary>
		public byte MinTriggerL, MaxTriggerL;
		/// <summary>
		/// Right analog trigger
		/// </summary>
		public byte MinTriggerR, MaxTriggerR;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Geometry;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current state of the IR camera
	/// </summary>
	[Serializable]
	public struct IRState {
		/// <summary>
		/// Current mode of IR sensor data
		/// </summary>
		public IRMode Mode;
		/// <summary>
		/// Current state of IR sensors
		/// </summary>
		//public IRSensor[] IRSensors;
		/// <summary>
		/// Raw midpoint of IR sensors 1 and 2 only.  Values range between 0 - 1023, 0 - 767
		/// </summary>
		public Point2I RawMidpoint;
		/// <summary>
		/// Normalized midpoint of IR sensors 1 and 2 only.  Values range between 0.0 - 1.0
		/// </summary>
		public Point2F Midpoint;

		public IRSensitivity Sensitivity;

		
		public IRSensor IRSensor0;
		public IRSensor IRSensor1;
		public IRSensor IRSensor2;
		public IRSensor IRSensor3;

		public IRSensor this[int index] {
			get {
				switch (index) {
				case 0: return IRSensor0;
				case 1: return IRSensor1;
				case 2: return IRSensor2;
				case 3: return IRSensor3;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Geometry;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current state of a single IR sensor
	/// </summary>
	[Serializable]
	public struct IRSensor {
		/// <summary>
		/// Raw values of individual sensor.  Values range between 0 - 1023 on the X axis and 0 - 767 on the Y axis.
		/// </summary>
		public Point2I RawPosition;
		/// <summary>
		/// Normalized values of the sensor position.  Values range between 0.0 - 1.0.
		/// </summary>
		public Point2F Position;
		/// <summary>
		/// Size of IR Sensor.  Values range from 0 - 15
		/// </summary>
		public int Size;
		/// <summary>
		/// IR sensor seen
		/// </summary>
		public bool Found;
		/// <summary>
		/// Convert to human-readable string
		/// </summary>
		/// <returns>A string that represents the point.</returns>
		public override string ToString() {
			return string.Format("{{{0}, Size={1}, Found={2}}}", Position, Size, Found);
		}
	}
}

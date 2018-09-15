using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Util;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current state of LEDs
	/// </summary>
	[Serializable]
	public struct LEDState {
		/// <summary>
		/// LED on the Wiimote
		/// </summary>
		public bool LED1, LED2, LED3, LED4;

		internal void Parse(byte[] buff, int off) {
			LED1 = buff.GetBit(0, 4);
			LED2 = buff.GetBit(0, 5);
			LED3 = buff.GetBit(0, 6);
			LED4 = buff.GetBit(0, 7);
		}
	}
}

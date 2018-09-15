using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Util;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current button state
	/// </summary>
	[Serializable]
	public struct ButtonState {
		/// <summary>
		/// Digital button on the Wiimote
		/// </summary>
		public bool A, B, Plus, Home, Minus, One, Two, Up, Down, Left, Right;

		internal void Parse(byte[] buff, int off) {
			/*Left	= (buff[off + 0] & 0x01) != 0;
			Right	= (buff[off + 0] & 0x02) != 0;
			Down	= (buff[off + 0] & 0x04) != 0;
			Up		= (buff[off + 0] & 0x08) != 0;
			Plus	= (buff[off + 0] & 0x10) != 0;
			
			Two		= (buff[off + 1] & 0x01) != 0;
			One		= (buff[off + 1] & 0x02) != 0;
			B		= (buff[off + 1] & 0x04) != 0;
			A		= (buff[off + 1] & 0x08) != 0;
			Minus	= (buff[off + 1] & 0x10) != 0;

			Home	= (buff[off + 1] & 0x80) != 0;*/

			Left	= buff.GetBit(off + 0, 0);
			Right	= buff.GetBit(off + 0, 1);
			Down	= buff.GetBit(off + 0, 2);
			Up		= buff.GetBit(off + 0, 3);
			Plus	= buff.GetBit(off + 0, 4);
			
			Two		= buff.GetBit(off + 1, 0);
			One		= buff.GetBit(off + 1, 1);
			B		= buff.GetBit(off + 1, 2);
			A		= buff.GetBit(off + 1, 3);
			Minus	= buff.GetBit(off + 1, 4);

			Home	= buff.GetBit(off + 1, 7);
		}
	}
}

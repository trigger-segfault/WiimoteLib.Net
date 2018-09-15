using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Util;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Current Wiimote status state
	/// </summary>
	[Serializable]
	public struct StatusState {
		/// <summary>Current state of rumble.</summary>
		public bool Rumble;
		/// <summary>Is an extension controller inserted?</summary>
		public bool Extension;
		/// <summary>Current state of speaker.</summary>
		public bool Speaker;
		/// <summary>Is the IR Camera enabled?</summary>
		public bool IREnabled;
		/// <summary>LEDs on the Wiimote.</summary>
		//public bool LED1, LED2, LED3, LED4;
		/// <summary>The battery level is low.</summary>
		public bool BatteryLow;
		/// <summary>Raw byte value of current battery level.</summary>
		public byte BatteryRaw;
		/// <summary>Calculated current battery level.</summary>
		public float Battery;
		/// <summary>LEDs on the Wiimote as flags.</summary>
		public LEDs LEDs;
		/*public LEDs LEDs {
			get {
				LEDs leds = LEDs.None;
				if (LED1) leds |= LEDs.LED1;
				if (LED2) leds |= LEDs.LED2;
				if (LED3) leds |= LEDs.LED3;
				if (LED4) leds |= LEDs.LED4;
				return leds;
			}
			set {
				LED1 = value.HasFlag(LEDs.LED1);
				LED2 = value.HasFlag(LEDs.LED2);
				LED3 = value.HasFlag(LEDs.LED3);
				LED4 = value.HasFlag(LEDs.LED4);
			}
		}*/

		internal void Parse(byte[] buff, int off) {
			/*BatteryLow     = (buff[off + 0] & 0x01) != 0;
			Extension  = (buff[off + 0] & 0x02) != 0;
			Speaker    = (buff[off + 0] & 0x04) != 0;
			IREnabled  = (buff[off + 0] & 0x08) != 0;

			LEDs = (LEDs) ((buff[off + 0] >> 4) & 0x0F);

			LED1 = (buff[off + 0] & 0x10) != 0;
			LED2 = (buff[off + 0] & 0x20) != 0;
			LED3 = (buff[off + 0] & 0x40) != 0;
			LED4 = (buff[off + 0] & 0x80) != 0;*/

			BatteryLow = buff.GetBit(off + 0, 0);
			Extension  = buff.GetBit(off + 0, 1);
			Speaker    = buff.GetBit(off + 0, 2);
			IREnabled  = buff.GetBit(off + 0, 3);

			LEDs = (LEDs) ((buff[off + 0] >> 4) & 0x0F);

			/*LED1 = buff.GetBit(off + 0, 4);
			LED2 = buff.GetBit(off + 0, 5);
			LED3 = buff.GetBit(off + 0, 6);
			LED4 = buff.GetBit(off + 0, 7);*/

			BatteryRaw = buff[off + 3];
			Battery = ((100f * 48f * (float)((int) BatteryRaw / 48f))) / 176f;
			//Battery = BatteryRaw / 2.55f;// ((100f * 48f * (float)((int)BatteryRaw / 48f))) / 192f;
		}
	}
}

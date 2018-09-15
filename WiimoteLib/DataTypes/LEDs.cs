using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.DataTypes {
	public enum LEDs {
		None = 0,
		LED1 = (1 << 0),
		LED2 = (1 << 1),
		LED3 = (1 << 2),
		LED4 = (1 << 3),
	}
}

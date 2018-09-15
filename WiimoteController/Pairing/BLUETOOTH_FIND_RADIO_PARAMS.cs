using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	[StructLayout(LayoutKind.Sequential)]
	internal struct BLUETOOTH_FIND_RADIO_PARAMS {
		public int dwSize;
	}
}

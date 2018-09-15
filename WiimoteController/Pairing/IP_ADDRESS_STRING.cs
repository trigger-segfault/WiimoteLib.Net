using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct IP_ADDRESS_STRING {
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		public string Address;
	}
}

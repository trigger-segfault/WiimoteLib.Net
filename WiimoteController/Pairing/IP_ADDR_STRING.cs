using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct IP_ADDR_STRING {
		public IntPtr pNext;
		public IP_ADDRESS_STRING IpAddress;
		public IP_ADDRESS_STRING Mask;
		public int Context;

		public IP_ADDRESS_STRING Next {
			get => Marshal.PtrToStructure<IP_ADDRESS_STRING>(pNext);
		}
	}
}

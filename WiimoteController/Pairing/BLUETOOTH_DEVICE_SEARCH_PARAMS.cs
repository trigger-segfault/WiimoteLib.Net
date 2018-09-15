using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct BLUETOOTH_DEVICE_SEARCH_PARAMS {
		public int dwSize;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fReturnAuthenticated;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fReturnRemembered;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fReturnUnknown;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fReturnConnected;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fIssueInquiry;
		[MarshalAs(UnmanagedType.U1)]
		public byte cTimeoutMultiplier;

		public IntPtr hRadio;
	}
}

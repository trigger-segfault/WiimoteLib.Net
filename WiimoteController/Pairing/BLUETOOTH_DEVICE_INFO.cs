using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct BLUETOOTH_DEVICE_INFO {
		private const int BLUETOOTH_MAX_NAME_SIZE = 248;

		public int dwSize;
		//[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public long AddressLong;
		//public byte Address[8];
		//public byte[] Address;
		//public long Address;
		public uint ulClassofDevice;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fConnected;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fRemembered;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fAuthenticated;

		public SYSTEMTIME stLastSeen;
		public SYSTEMTIME stLastUsed;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = BLUETOOTH_MAX_NAME_SIZE)]
		public string szName;

		public byte[] Address => BitConverter.GetBytes(AddressLong);
	}
}

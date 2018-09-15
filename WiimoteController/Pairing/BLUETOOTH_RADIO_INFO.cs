using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct BLUETOOTH_RADIO_INFO {
		private const int BLUETOOTH_MAX_NAME_SIZE = 248;

		public int dwSize;
		//[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
		//public byte[] address;
		private long address;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = BLUETOOTH_MAX_NAME_SIZE)]
		public string szName;
		public uint ulClassofDevice;
		public ushort lmpSubversion;
		[MarshalAs(UnmanagedType.U2)]
		public Manufacturer manufacturer;

		public byte[] Address => BitConverter.GetBytes(address);
	}
}

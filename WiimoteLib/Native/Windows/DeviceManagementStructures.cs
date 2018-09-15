using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Native.Windows {
	internal enum DeviceType : uint {
		OEM = 0x00000000,
		Volume = 0x00000002,
		Port = 0x00000003,
		DeviceInterface = 0x00000005,
		Handle = 0x00000006,
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct DEV_BROADCAST_HDR {
		public int size;
		public DeviceType deviceType;
		public int reserved;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct DEV_BROADCAST_DEVICEINTERFACE {
		public int size;
		public DeviceType deviceType;
		public int reserved;
		public Guid classguid;
		public char name;
	}
}

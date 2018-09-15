using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Native {
	[StructLayout(LayoutKind.Sequential, Size = 60)]
	internal struct WSAQUERYSET {
		public int dwSize;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpszServiceInstanceName;
		public IntPtr lpServiceClassId;
		IntPtr lpVersion;
		IntPtr lpszComment;
		public int dwNameSpace;
		IntPtr lpNSProviderId;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpszContext;
		int dwNumberOfProtocols;
		IntPtr lpafpProtocols;
		IntPtr lpszQueryString;
		public int dwNumberOfCsAddrs;
		public IntPtr lpcsaBuffer;
		int dwOutputFlags;
		public IntPtr lpBlob;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SP_DEVINFO_DATA {
		public int cbSize;
		public Guid ClassGuid;
		public int DevInst;
		public IntPtr Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SP_DEVICE_INTERFACE_DATA {
		public int cbSize;
		public Guid InterfaceClassGuid;
		public int Flags;
		public IntPtr RESERVED;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct SP_DEVICE_INTERFACE_DETAIL_DATA {
		public int cbSize;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string DevicePath;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct HIDD_ATTRIBUTES {
		public int Size;
		public ushort VendorID;
		public ushort ProductID;
		public ushort VersionNumber;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct BLUETOOTH_DEVICE_INFO {
		private const int BLUETOOTH_MAX_NAME_SIZE = 248;

		public int dwSize;
		public long Address;
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

		public BLUETOOTH_DEVICE_INFO(long address) {
			dwSize = 560;
			Address = address;
			ulClassofDevice = 0;
			fConnected = false;
			fRemembered = false;
			fAuthenticated = false;
			stLastSeen = new SYSTEMTIME();
			stLastUsed = new SYSTEMTIME();
			szName = "";
		}

		public BLUETOOTH_DEVICE_INFO(ulong address)
			: this(unchecked((long) address))
		{
		}
	}

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
	[StructLayout(LayoutKind.Sequential)]
	internal struct BLUETOOTH_FIND_RADIO_PARAMS {
		public int dwSize;
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct BLUETOOTH_RADIO_INFO {
		private const int BLUETOOTH_MAX_NAME_SIZE = 248;

		public int dwSize;
		private long address;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = BLUETOOTH_MAX_NAME_SIZE)]
		public string szName;
		public uint ulClassofDevice;
		public ushort lmpSubversion;
		[MarshalAs(UnmanagedType.U2)]
		public Manufacturer manufacturer;

		public byte[] Address => BitConverter.GetBytes(address);
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SYSTEMTIME {
		private ushort year;
		private short month;
		private short dayOfWeek;
		private short day;
		private short hour;
		private short minute;
		private short second;
		private short millisecond;

		public static SYSTEMTIME FromByteArray(Byte[] array, int offset) {
			SYSTEMTIME st = new SYSTEMTIME();
			st.year = BitConverter.ToUInt16(array, offset);
			st.month = BitConverter.ToInt16(array, offset + 2);
			st.day = BitConverter.ToInt16(array, offset + 6);
			st.hour = BitConverter.ToInt16(array, offset + 8);
			st.minute = BitConverter.ToInt16(array, offset + 10);
			st.second = BitConverter.ToInt16(array, offset + 12);

			return st;
		}
		public static SYSTEMTIME FromDateTime(DateTime dt) {
			SYSTEMTIME st = new SYSTEMTIME();
			st.year = (ushort) dt.Year;
			st.month = (short) dt.Month;
			st.dayOfWeek = (short) dt.DayOfWeek;
			st.day = (short) dt.Day;
			st.hour = (short) dt.Hour;
			st.minute = (short) dt.Minute;
			st.second = (short) dt.Second;
			st.millisecond = (short) dt.Millisecond;

			return st;
		}

		public DateTime ToDateTime(DateTimeKind kind = DateTimeKind.Utc) {
			if (year == 0 && month == 0 && day == 0 && hour == 0 && minute == 0 && second == 0) {
				return DateTime.MinValue;
			}
			return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
		}

		public DateTime DateTime => ToDateTime().ToLocalTime();
		public DateTime DateTimeUtc => ToDateTime();

		public override string ToString() => DateTime.ToString();
	}
}

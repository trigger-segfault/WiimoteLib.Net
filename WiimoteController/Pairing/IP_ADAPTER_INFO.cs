using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 648)]
	internal struct IP_ADAPTER_INFO {
		private const int MAX_ADAPTER_NAME_LENGTH = 256;
		private const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
		private const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

		public IntPtr pNext;
		public int ComboNext;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
		public string AdapterName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
		public string Description;

		public int AddressLength;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
		public byte[] Address;
		public int Index;
		public int Type;
		public int DhcpEnabled;
		public IntPtr CurrentIpAddress;
		public IP_ADDR_STRING IpAddressList;
		public IP_ADDR_STRING GatewayList;
		public IP_ADDR_STRING DhcpServer;
		[MarshalAs(UnmanagedType.Bool)]
		public bool HaveWins;
		public IP_ADDR_STRING PrimaryWinsServer;
		public IP_ADDR_STRING SecondaryWinsServer;
		public int LeaseObtained;
		public int LeaseExpires;

		public IP_ADAPTER_INFO Next {
			get => Marshal.PtrToStructure<IP_ADAPTER_INFO>(pNext);
		}
	}
}

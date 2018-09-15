using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteController.Pairing {
	internal static class Uuids {
		public static readonly Guid HumanInterfaceDeviceServiceClass_UUID = new Guid(0x00001124, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
	}
	internal static class NativeMethods {
		private const string irpropsDll = "Irprops.cpl";

		public const int BLUETOOTH_SERVICE_DISABLE = 0x00;
		public const int BLUETOOTH_SERVICE_ENABLE = 0x01;

		[DllImport(irpropsDll, SetLastError = true)]
		public static extern IntPtr BluetoothFindFirstRadio(ref BLUETOOTH_FIND_RADIO_PARAMS pbtfrp, out IntPtr phRadio);

		[DllImport(irpropsDll, SetLastError = true)]
		public static extern IntPtr BluetoothFindFirstDevice(ref BLUETOOTH_DEVICE_SEARCH_PARAMS pbtsp, ref BLUETOOTH_DEVICE_INFO pbtdi);

		[DllImport(irpropsDll, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BluetoothFindNextRadio(IntPtr hFind, out IntPtr phRadio);

		[DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BluetoothFindNextDevice(IntPtr hFind, ref BLUETOOTH_DEVICE_INFO pbtdi);

		[DllImport(irpropsDll, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BluetoothFindDeviceClose(IntPtr hFind);

		[DllImport(irpropsDll, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BluetoothFindRadioClose(IntPtr hFind);

		[DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int BluetoothAuthenticateDevice(IntPtr hwndParent, IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, string pszPasskey, int ulPasskeyLength);

		[DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int BluetoothRemoveDevice(byte[] pAddress);

		[DllImport(irpropsDll, SetLastError = true)]
		public static extern int BluetoothEnumerateInstalledServices(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref int pcServices, Guid[] pGuidServices);

		[DllImport(irpropsDll, SetLastError = true)]
		public static extern int BluetoothSetServiceState(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref Guid pGuidService, int dwServiceFlags);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport(irpropsDll, SetLastError = true)]
		public static extern int BluetoothGetRadioInfo(IntPtr hRadio, ref BLUETOOTH_RADIO_INFO pRadioInfo);

		[DllImport("Iphlpapi.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		//public static extern int GetAdaptersInfo(IntPtr AdapterInfo, ref int SizePointer);
		public static extern int GetAdaptersInfo(IntPtr AdapterInfo, ref int SizePointer);
	}
	public static class AdaptersHelper {
		const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
		const int ERROR_BUFFER_OVERFLOW = 111;
		const int MAX_ADAPTER_NAME_LENGTH = 256;
		const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
		const int MIB_IF_TYPE_OTHER = 1;
		const int MIB_IF_TYPE_ETHERNET = 6;
		const int MIB_IF_TYPE_TOKENRING = 9;
		const int MIB_IF_TYPE_FDDI = 15;
		const int MIB_IF_TYPE_PPP = 23;
		const int MIB_IF_TYPE_LOOPBACK = 24;
		const int MIB_IF_TYPE_SLIP = 28;

		[DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
		public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

		public static List<AdapterInfo> GetAdapters() {
			var adapters = new List<AdapterInfo>();

			long structSize = Marshal.SizeOf<IP_ADAPTER_INFO>();
			IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

			int ret = GetAdaptersInfo(pArray, ref structSize);

			if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
			{
				// Buffer was too small, reallocate the correct size for the buffer.
				pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

				ret = GetAdaptersInfo(pArray, ref structSize);
			}

			if (ret == 0) {
				// Call Succeeded
				IntPtr pEntry = pArray;

				do {
					var adapter = new AdapterInfo();

					// Retrieve the adapter info from the memory address
					var entry = Marshal.PtrToStructure<IP_ADAPTER_INFO>(pEntry);

					// Adapter Type
					switch (entry.Type) {
					case MIB_IF_TYPE_ETHERNET:
						adapter.Type = "Ethernet";
						break;
					case MIB_IF_TYPE_TOKENRING:
						adapter.Type = "Token Ring";
						break;
					case MIB_IF_TYPE_FDDI:
						adapter.Type = "FDDI";
						break;
					case MIB_IF_TYPE_PPP:
						adapter.Type = "PPP";
						break;
					case MIB_IF_TYPE_LOOPBACK:
						adapter.Type = "Loopback";
						break;
					case MIB_IF_TYPE_SLIP:
						adapter.Type = "Slip";
						break;
					default:
						adapter.Type = "Other/Unknown";
						break;
					} // switch

					adapter.Name = entry.AdapterName;
					adapter.Description = entry.Description;

					// MAC Address (data is in a byte[])
					adapter.MAC = string.Join("-", Enumerable.Range(0, (int) entry.AddressLength).Select(s => string.Format("{0:X2}", entry.Address[s])));

					// Get next adapter (if any)

					adapters.Add(adapter);

					pEntry = entry.pNext;
				}
				while (pEntry != IntPtr.Zero);

				Marshal.FreeHGlobal(pArray);
			}
			else {
				Marshal.FreeHGlobal(pArray);
				throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
			}

			return adapters;
		}
	}
	public class AdapterInfo {
		public string Type { get; set; }

		public string Description { get; set; }

		public string Name { get; set; }

		public string MAC { get; set; }
	}
}

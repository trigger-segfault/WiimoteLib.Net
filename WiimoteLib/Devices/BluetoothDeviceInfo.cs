using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteLib.Native;
using WiimoteLib.Util;

namespace WiimoteLib.Devices {
	public class BluetoothDeviceInfo {
		internal BLUETOOTH_DEVICE_INFO DeviceInfo;

		//private DateTime pairingStarted;

		public BluetoothAddress Address => new BluetoothAddress(DeviceInfo.Address);
		public bool Connected => DeviceInfo.fConnected;
		public bool Remembered => DeviceInfo.fRemembered;
		public bool Authenticated => DeviceInfo.fAuthenticated;
		public string Name => DeviceInfo.szName;
		public DateTime LastSeen => DeviceInfo.stLastSeen.DateTime;
		public DateTime LastUsed => DeviceInfo.stLastUsed.DateTime;
		public DateTime LastSeenUtc => DeviceInfo.stLastSeen.DateTimeUtc;
		public DateTime LastUsedUtc => DeviceInfo.stLastUsed.DateTimeUtc;

		public bool IsInvalid => DeviceInfo.Address == 0;

		//internal bool IsPairing

		internal BluetoothDeviceInfo() {

		}

		internal BluetoothDeviceInfo(BLUETOOTH_DEVICE_INFO deviceInfo) {
			DeviceInfo = deviceInfo;
		}

		public BluetoothDeviceInfo(long address) {
			DeviceInfo = new BLUETOOTH_DEVICE_INFO(address);
			Refresh();
		}

		public BluetoothDeviceInfo(ulong address)
			: this(unchecked((long) address))
		{
		}

		public BluetoothDeviceInfo(string address)
			: this(BluetoothAddress.Parse(address).Int64)
		{
		}

		public BluetoothDeviceInfo(BluetoothAddress address)
			: this(address.Int64)
		{
		}

		public override string ToString() => $"{Name} ({Address})";

		public bool Refresh() {
			DeviceInfo.ulClassofDevice = 0;
			DeviceInfo.szName = "";
			return NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref DeviceInfo) == 0;
		}

		internal bool PairDevice(CancellationToken token = default(CancellationToken)) {
			if (!Connected) {
				if (Remembered && !RemoveDevice(token))
					return false;
				if (token.IsCancellationRequested)
					return false;

				Guid[] services = new Guid[16];
				int serviceCount = services.Length;
				Guid uuid = Uuids.HumanInterfaceDeviceServiceClass;
				NativeMethods.BluetoothEnumerateInstalledServices(IntPtr.Zero, ref DeviceInfo, ref serviceCount, services);
				int res = NativeMethods.BluetoothSetServiceState(IntPtr.Zero, ref DeviceInfo, ref uuid, BluetoothServiceFlags.Enable);
				serviceCount = services.Length;
				NativeMethods.BluetoothEnumerateInstalledServices(IntPtr.Zero, ref DeviceInfo, ref serviceCount, services);
				if (res == 0) {
					Debug.WriteLine($"{this} Paired");
					return true;
				}
				return false;
			}
			return true;
		}

		internal bool RemoveDevice(CancellationToken token = default(CancellationToken)) {
			byte[] data = BitConverter.GetBytes(DeviceInfo.Address);
			if (NativeMethods.BluetoothRemoveDevice(data) == 0) {
				DeviceInfo.fConnected = false;
				Debug.WriteLine($"{this} Removed");
				return true;
			}
			return false;
		}

		public bool IsDiscoverable() {
			Stopwatch watch = Stopwatch.StartNew();
			Guid service = new Guid("{F13F471D-47CB-41d6-9609-BAD0690BF891}");
			//Guid service = Uuids.HumanInterfaceDeviceServiceClass;

			const AddressFamily Bluetooth = (AddressFamily) 32;
			const ProtocolType RFComm = (ProtocolType) 0x0003;
			Socket s = new Socket(Bluetooth, SocketType.Stream, RFComm);


			WSAQUERYSET wqs = new WSAQUERYSET();
			wqs.dwSize = 60;
			wqs.dwNameSpace = 16;

            GCHandle hservice = GCHandle.Alloc(service.ToByteArray(), GCHandleType.Pinned);
            wqs.lpServiceClassId = hservice.AddrOfPinnedObject();
            wqs.lpszContext = $"({Address.MacAddress})";

			IntPtr hLookup;
			int result;
			LookupFlags flags = LookupFlags.FlushCache | LookupFlags.ReturnName | LookupFlags.ReturnBlob;
			//flags = LookupFlags.FlushCache | LookupFlags.ReturnName;

			// Start looking for Bluetooth services

			result = NativeMethods.WSALookupServiceBegin(ref wqs, flags, out hLookup);
			int err = NativeMethods.WSAGetLastError();
			hservice.Free();
			Debug.WriteLine($"IsDiscoverable: {watch.ElapsedMilliseconds}ms");
			if (result != 0)
				return false;
			NativeMethods.WSALookupServiceEnd(hLookup);
			return true;
		}

		internal static BluetoothDeviceInfo GetDevice(string hidPath) {
			return WiimoteRegistry.GetBluetoothDevice(hidPath);
		}

		internal static BluetoothDeviceInfo[] GetDevices() {
			return EnumerateDevices(new CancellationToken()).ToArray();
		}

		internal static BluetoothDeviceInfo[] GetDevices(CancellationToken token) {
			return EnumerateDevices(token).ToArray();
		}

		internal static BluetoothDeviceInfo[] GetDevices(Predicate<BluetoothDeviceInfo> match) {
			return EnumerateDevices(new CancellationToken(), match).ToArray();
		}

		internal static BluetoothDeviceInfo[] GetDevices(CancellationToken token, Predicate<BluetoothDeviceInfo> match) {
			return EnumerateDevices(token, match).ToArray();
		}

		internal static IEnumerable<BluetoothDeviceInfo> EnumerateDevices() {
			return EnumerateDevices(new CancellationToken());
		}

		internal static IEnumerable<BluetoothDeviceInfo> EnumerateDevices(CancellationToken token) {
			return EnumerateDevices(token, null);
		}

		internal static IEnumerable<BluetoothDeviceInfo> EnumerateDevices(Predicate<BluetoothDeviceInfo> match) {
			return EnumerateDevices(new CancellationToken(), match);
		}

		internal static IEnumerable<BluetoothDeviceInfo> EnumerateDevices(CancellationToken token, Predicate<BluetoothDeviceInfo> match) {
			IntPtr hFind = IntPtr.Zero;
			try {
				BLUETOOTH_DEVICE_INFO btdi = new BLUETOOTH_DEVICE_INFO();
				BLUETOOTH_DEVICE_SEARCH_PARAMS srch = new BLUETOOTH_DEVICE_SEARCH_PARAMS();
				
				btdi.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>();
				srch.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_SEARCH_PARAMS>();

				srch.fReturnAuthenticated = true;
				srch.fReturnRemembered = true;
				srch.fReturnConnected = true;
				srch.fReturnUnknown = true;
				srch.fIssueInquiry = true;
				srch.cTimeoutMultiplier = 1;
				srch.hRadio = IntPtr.Zero;
				//srch.hRadio = InTheHand.Net.Bluetooth.BluetoothRadio.PrimaryRadio.Handle;

				hFind = NativeMethods.BluetoothFindFirstDevice(ref srch, ref btdi);
				do {
					BluetoothDeviceInfo device = new BluetoothDeviceInfo(btdi);
					if (match?.Invoke(device) ?? true)
						yield return device;
					if (token.IsCancellationRequested)
						break;
				}
				while (NativeMethods.BluetoothFindNextDevice(hFind, ref btdi));
			}
			finally {
				if (hFind != IntPtr.Zero)
					NativeMethods.BluetoothFindDeviceClose(hFind);
			}
		}
	}
}

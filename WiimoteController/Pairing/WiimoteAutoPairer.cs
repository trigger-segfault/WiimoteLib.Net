using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WiimoteController.Pairing.NativeMethods;

namespace WiimoteController.Pairing {
	public class WiimoteAutoPairer {

		private CancellationTokenSource token;
		private Task findTask;
		private Task pairTask;

		private HashSet<long> knownAddresses;

		public WiimoteAutoPairer() {
			knownAddresses = new HashSet<long>();
			knownAddresses.Add(0x001CBEF2D864);
		}


		public void Start() {
			Stop();
			IsAnyDeviceAvailable = false;
			lock (connected)
				connected.Clear();
			token = new CancellationTokenSource();
			findTask = Task.Run(() => FindTask(token.Token), token.Token);
			pairTask = Task.Run(() => PairTask(token.Token), token.Token);
		}

		public void Stop() {
			token?.Cancel();
			token = null;
			findTask = null;
			pairTask = null;
		}

		public IEnumerable<BluetoothAddress> KnownAddresses {
			get {
				foreach (long address in knownAddresses)
					yield return new BluetoothAddress(address);
			}
		}

		private BluetoothAddress[] FindDevices(CancellationToken token) {
			IntPtr hRadio = BluetoothRadio.PrimaryRadio.Handle;
			BLUETOOTH_RADIO_INFO radioInfo = new BLUETOOTH_RADIO_INFO();
			IntPtr hFind;
			BLUETOOTH_DEVICE_INFO btdi = new BLUETOOTH_DEVICE_INFO();
			BLUETOOTH_DEVICE_SEARCH_PARAMS srch = new BLUETOOTH_DEVICE_SEARCH_PARAMS();

			radioInfo.dwSize = Marshal.SizeOf<BLUETOOTH_RADIO_INFO>();
			btdi.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>();
			srch.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_SEARCH_PARAMS>();

			srch.fReturnAuthenticated = true;
			srch.fReturnRemembered = true;
			srch.fReturnConnected = true;
			srch.fReturnUnknown = true;
			srch.fIssueInquiry = true;
			srch.cTimeoutMultiplier = 2;
			srch.hRadio = BluetoothRadio.PrimaryRadio.Handle;

			hFind = BluetoothFindFirstDevice(ref srch, ref btdi);

			List<BluetoothAddress> devices = new List<BluetoothAddress>();
			do {
				if (btdi.szName != "Nintendo RVL-WBC-01" && btdi.szName != "Nintendo RVL-CNT-01")
					continue;
				if (token.IsCancellationRequested)
					break;
				devices.Add(new BluetoothAddress(btdi.AddressLong));
			}
			while (BluetoothFindNextDevice(hFind, ref btdi));

			BluetoothFindDeviceClose(hFind);

			return devices.ToArray();
		}

		private bool FindLoop(CancellationToken token) {
			bool found = false;

			IntPtr hRadio = BluetoothRadio.PrimaryRadio.Handle;
			BLUETOOTH_RADIO_INFO radioInfo = new BLUETOOTH_RADIO_INFO();
			IntPtr hFind;
			BLUETOOTH_DEVICE_INFO btdi = new BLUETOOTH_DEVICE_INFO();
			BLUETOOTH_DEVICE_SEARCH_PARAMS srch = new BLUETOOTH_DEVICE_SEARCH_PARAMS();

			radioInfo.dwSize = Marshal.SizeOf<BLUETOOTH_RADIO_INFO>();
			btdi.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>();
			srch.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_SEARCH_PARAMS>();

			srch.fReturnAuthenticated = true;
			srch.fReturnRemembered = true;
			srch.fReturnConnected = true;
			srch.fReturnUnknown = true;
			srch.fIssueInquiry = true;
			srch.cTimeoutMultiplier = 2;
			srch.hRadio = InTheHand.Net.Bluetooth.BluetoothRadio.PrimaryRadio.Handle;

			hFind = BluetoothFindFirstDevice(ref srch, ref btdi);

			do {
				if (btdi.szName != "Nintendo RVL-WBC-01" && btdi.szName != "Nintendo RVL-CNT-01")
					continue;
				if (token.IsCancellationRequested)
					break;
				lock (knownAddresses) {
					if (knownAddresses.Add(btdi.AddressLong)) {
						Trace.WriteLine($"Wiimote found: {btdi.AddressLong.ToMacAddress()}");
						found = true;
					}
				}
			}
			while (BluetoothFindNextDevice(hFind, ref btdi));

			BluetoothFindDeviceClose(hFind);

			return found;
		}

		private void FindTask(CancellationToken token) {
			while (!token.IsCancellationRequested) {
				FindLoop(token);
				token.WaitHandle.WaitOne(500);
			}
		}


		// Once this is true, the Wiimote will
		// attempt to connect to an HID device.
		public bool IsAnyDeviceAvailable { get; private set; }
		
		private void PairTask(CancellationToken token) {
			// Setup automatic authentication
			BluetoothWin32Authentication auth = new BluetoothWin32Authentication(OnHandleRequests);

			while (!token.IsCancellationRequested)
				PairLoop(token);
		}

		private void PairLoop(CancellationToken token) {
			// Get a copy of known addresses since
			// these are added to in another task.
			BluetoothAddress[] addresses = FindDevices(token);
			/*lock (knownAddresses)
				addresses = KnownAddresses.ToArray();*/

			bool available = false;
			bool connectedChanged = false;
			List<long> newConnected = new List<long>();
			foreach (BluetoothAddress address in addresses) {
				if (token.IsCancellationRequested)
					return;
				BluetoothDeviceInfo device = new BluetoothDeviceInfo(address);

				bool isDiscoverable = device.IsDiscoverable();
				bool installed = device.InstalledServices.Any();
				
				if (device.Connected) {// || isDiscoverable)) {
					if (!connected.Contains(address.ToInt64()))
						connectedChanged = true;
					newConnected.Add(address.ToInt64());
					if (!available && !IsAnyDeviceAvailable) {
						lock (knownAddresses)
							IsAnyDeviceAvailable = true;
					}
					available = true;
					continue;
				}
				else if (isDiscoverable && !device.Connected/*device.IsDiscoverable()*/) {
					if (device.Remembered)
						RemoveDevice(device, token);
					if (PairDevice(device, token, available)) {
						if (!connected.Contains(address.ToInt64()))
							connectedChanged = true;
						newConnected.Add(address.ToInt64());
						if (!available && !IsAnyDeviceAvailable) {
							Trace.WriteLine("First device has been connected");
							lock (knownAddresses)
								IsAnyDeviceAvailable = true;
						}
						available = true;
					}
				}
			}
			if (connectedChanged) {
				lock (connected) {
					connected.Clear();
					foreach (long addr in newConnected)
						connected.Add(addr);
				}
			}
			if (!available && IsAnyDeviceAvailable) {
				Trace.WriteLine("No more devices connected");
				lock (knownAddresses)
					IsAnyDeviceAvailable = false;
			}
		}

		private void RemoveDevice(BluetoothDeviceInfo device, CancellationToken token) {
			token.WaitHandle.WaitOne(1000);
			if (BluetoothSecurity.RemoveDevice(device.DeviceAddress)) {
				Trace.WriteLine($"Wiimote removed: {device.DeviceAddress.ToMacAddress()}");
				token.WaitHandle.WaitOne(2000);
			}
		}

		private bool PairDevice(BluetoothDeviceInfo device, CancellationToken token,
			bool available)
		{
			try {
				Guid[] services = device.InstalledServices;
				device.SetServiceState(Uuids.HumanInterfaceDeviceServiceClass_UUID, true, true);
				services = device.InstalledServices;
				foreach (Guid service in services)
					Console.WriteLine($"Service: {service}");
				Trace.WriteLine($"Wiimote added: {device.DeviceAddress.ToMacAddress()}");

				token.WaitHandle.WaitOne(8000);

				return true;
				/*string pin = device.DeviceAddress.ToPin();
				if (BluetoothSecurity.PairRequest(device.DeviceAddress, pin)) {
					Trace.WriteLine($"Wiimote authenticated: {device.DeviceAddress.ToMacAddress()}");
					token.WaitHandle.WaitOne(1000);
					// Calling this before and after seems to help unsure
					// the device works when paired programmatically.
				}
				else {
					Trace.WriteLine($"Wiimote authentication failed: {device.DeviceAddress.ToMacAddress()}");
				}*/
			}
			catch {
				Trace.WriteLine($"Wiimote add failed: {device.DeviceAddress.ToMacAddress()}");
			}
			return false;
		}

		private void OnHandleRequests(object sender, BluetoothWin32AuthenticationEventArgs e) {
			e.Confirm = true;
		}

		private HashSet<long> connected = new HashSet<long>();

		public BluetoothAddress[] AvailableDevices {
			get {
				lock (connected)
					return connected.Select(l => new BluetoothAddress(l)).ToArray();
			}
		}

		const string WiimoteKey = @"SYSTEM\CurrentControlSet\Enum\BTHENUM\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002057e_PID&";

		const string PID		= "0306";
		const string PIDPlus	= "0330";
	}

	public struct WiimoteHID {
		public BluetoothAddress Address { get; }
		public string DevicePath { get; }

		public WiimoteHID(BluetoothAddress address, string devicePath) {
			Address = address;
			DevicePath = devicePath;
		}
	}

	internal static class BluetoothExtensions {

		public static bool IsDiscoverable(this BluetoothDeviceInfo device) {
			// // A specially created value, so no matches.
			Stopwatch watch = Stopwatch.StartNew();
			Guid fakeUuid = new Guid("{F13F471D-47CB-41d6-9609-BAD0690BF891}");
			try {
				ServiceRecord[] records = device.GetServiceRecords(fakeUuid);
				//Console.WriteLine($"IsDiscoverable true: {watch.ElapsedMilliseconds}");
				return true;
			}
			catch (SocketException) {
				Console.WriteLine($"IsDiscoverable false: {watch.ElapsedMilliseconds}");
				return false;
			}
		}

		public static string ToPin(this BluetoothAddress address) {
			// Pin is Mac address
			byte[] mac = address.ToByteArray();
			string pin = "";
			for (int i = 0; i < 6; i++)
				pin += (char) mac[i];
			return pin;
		}

		public static string ToMacAddress(this long address) {
			var parts = BitConverter.GetBytes(address).Take(6).Reverse().Select(b => b.ToString("X2"));
			return "{" + string.Join(":", parts) + "}";
		}

		public static string ToMacAddress(this BluetoothAddress address) {
			var parts = address.ToByteArray().Take(6).Reverse().Select(b => b.ToString("X2"));
			return "{" + string.Join(":", parts) + "}";
		}
	}
}

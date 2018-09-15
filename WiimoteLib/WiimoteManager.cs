using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteLib.DataTypes;
using WiimoteLib.Devices;
using WiimoteLib.Events;
using WiimoteLib.Native;
using WiimoteLib.Util;

namespace WiimoteLib {
	public static partial class WiimoteManager {
		// Settings
		private static int autoDiscoveryCount;
		private static bool unpairOnDisconnect;
		private static bool autoConnect;
		//private static TimeSpan disconnectTimeout;
		private static WiimoteType allowedTypes;
		private static string[] allowedDeviceNames;
		private static int[] allowedProductIDs;
		private static int maxWriteFrequency;
		private static int driverInstallDelay;

		// Management
		private static List<Wiimote> wiimotes;
		private static Queue<WriteRequest> writeQueue;
		private static DebugLockTracker locker = new DebugLockTracker("wiimotes", false);

		// Tasks
		private static readonly object taskLock;
		private static CancellationTokenSource discoverToken;
		private static Task discoverTask;
		private static CancellationTokenSource idleToken;
		private static Task idleTask;
		private static CancellationTokenSource writeToken;
		private static Task writeTask;
		private static readonly AutoResetEvent writeReady = new AutoResetEvent(false);

		public static bool DolphinBarMode { get; set; }

		/// <summary>Initializes the Wiimote manager.</summary>
		static WiimoteManager() {
			wiimotes = new List<Wiimote>();
			//disconnectTimeout = TimeSpan.FromMinutes(1);
			allowedDeviceNames = new string[0];
			allowedProductIDs = new int[0];
			autoDiscoveryCount = 0;
			autoConnect = false;
			unpairOnDisconnect = true;
			maxWriteFrequency = 15;//20;
			driverInstallDelay = 3000;
			writeQueue = new Queue<WriteRequest>();
			taskLock = new object();
			
			AllowedTypes = WiimoteType.Wiimote | WiimoteType.WiimotePlus;
			//StartWrite();
		}

		/// <summary>Cleans up the Wiimote manager and disconnects all Wiimotes.</summary>
		public static void Cleanup() {
			StopAllTasks();
			Wiimote[] wiimoteList;
			lock (wiimotes) {
				wiimoteList = ConnectedWiimotes;
				wiimotes.Clear();
			}
			foreach (Wiimote wiimote in wiimoteList) {
				wiimote.Dispose();
			}
			if (unpairOnDisconnect) {
				foreach (Wiimote wiimote in wiimoteList) {
					wiimote.Device.Bluetooth.RemoveDevice();
				}
			}
		}

		/// <summary>Cleans up the Wiimote manager and disconnects all Wiimotes
		/// asynchronously.</summary>
		public static void CleanupAsync() {
			Task.Run(() => Cleanup());
		}

		/// <summary>Disconnects all connected Wiimotes.</summary>
		public static void DisconnectAll() {
			lock (wiimotes) {
				foreach (Wiimote wiimote in ConnectedWiimotes) {
					wiimote.Dispose();
					wiimotes.Remove(wiimote);
					RaiseDisconnected(wiimote, DisconnectReason.User);
				}
			}
		}

		/// <summary>Disconnects the Wiimote with the specified address.</summary>
		/// <param name="address">The address of the Wiimote.</param>
		/// <param name="removeDevice">Should the device be removed. If null, the default
		/// action will be performed.</param>
		public static void Disconnect(BluetoothAddress address, bool? removeDevice = null) {
			lock (wiimotes) {
				Wiimote wiimote = wiimotes.Find(wm => wm.Address == address);
				if (wiimote == null) {
					throw new ArgumentException($"Wiimote {{{address}}} is not connected!", nameof(address));
				}
				wiimote.Dispose();
				wiimotes.Remove(wiimote);
				RaiseDisconnected(wiimote, DisconnectReason.User, removeDevice);
			}
		}

		/// <summary>Disconnects the Wiimote with the specified device path.</summary>
		/// <param name="devicePath">The HID device path of the Wiimote.</param>
		/// <param name="removeDevice">Should the device be removed. If null, the default
		/// action will be performed.</param>
		public static void Disconnect(string devicePath, bool? removeDevice = null) {
			lock (wiimotes) {
				Wiimote wiimote = wiimotes.Find(wm => wm.DevicePath == devicePath);
				if (wiimote == null) {
					throw new ArgumentException($"Wiimote ({devicePath}) is not connected!", nameof(devicePath));
				}
				wiimote.Dispose();
				wiimotes.Remove(wiimote);
				RaiseDisconnected(wiimote, DisconnectReason.User, removeDevice);
			}
		}

		/// <summary>Disconnects the Wiimote.</summary>
		/// <param name="wiimote">The Wiimote to disconnect.</param>
		/// <param name="removeDevice">Should the device be removed. If null, the default
		/// action will be performed.</param>
		public static void Disconnect(Wiimote wiimote, bool? removeDevice = null) {
			lock (wiimotes) {
				if (!wiimotes.Contains(wiimote)) {
					throw new ArgumentException($"{wiimote} is not connected!", nameof(wiimote));
				}
				wiimote.Dispose();
				wiimotes.Remove(wiimote);
				RaiseDisconnected(wiimote, DisconnectReason.User, removeDevice);
			}
		}

		/// <summary>Connects the Wiimote with the specified address.</summary>
		/// <param name="address">The address of the Wiimote.</param>
		/// <returns>The created Wiimote.</returns>
		public static Wiimote Connect(BluetoothAddress address) {
			lock (wiimotes) {
				Wiimote connected = wiimotes.Find(wm => wm.Address == address);
				if (connected != null)
					throw new WiimoteAlreadyConnectedException(connected, address);
				return Connect(new WiimoteDeviceInfo(address));
			}
		}

		/// <summary>Connects the Wiimote with the specified address.</summary>
		/// <param name="address">The address of the Wiimote.</param>
		/// <returns>The created Wiimote.</returns>
		public static Wiimote Connect(string devicePath) {
			lock (wiimotes) {
				Wiimote connected = wiimotes.Find(wm => wm.DevicePath == devicePath);
				if (connected != null)
					throw new WiimoteAlreadyConnectedException(connected, devicePath);
				return Connect(new WiimoteDeviceInfo(devicePath, DolphinBarMode));
			}
		}

		/// <summary>Connects the Wiimote device.</summary>
		/// <param name="device">The Wiimote device to connect.</param>
		/// <returns>The created Wiimote.</returns>
		private static Wiimote Connect(WiimoteDeviceInfo device) {
			lock (wiimotes) {
				if (device.IsOpen)
					throw new ArgumentException("Cannot connect to device that is already in use!",
						nameof(device));
				Wiimote connected = wiimotes.Find(wm => wm.Address == device.Address);
				if (connected != null)
					throw new WiimoteAlreadyConnectedException(connected);

				Wiimote wiimote = new Wiimote(device);
				wiimotes.Add(wiimote);
				wiimote.SetPlayerLED(wiimotes.Count);
				RaiseConnected(wiimote);
				return wiimote;
			}
		}
		
		/// <summary>Matches the Bluetooth device to the allowed Wiimote types.</summary>
		private static bool MatchBluetooth(BluetoothDeviceInfo device) {
			lock (allowedDeviceNames) {
				return allowedDeviceNames.Any(n => n == device.Name);
			}
		}

		/// <summary>Matches the HID device to the allowed Wiimote types.</summary>
		private static bool MatchHID(HIDDeviceInfo device) {
			lock (allowedDeviceNames) {
				return device.VendorID == WiimoteConstants.VendorID &&
					allowedProductIDs.Any(p => p == device.ProductID);
			}
		}

		/// <summary>Adds a write request to the write queue.</summary>
		/// <param name="request">The request to write.</param>
		/// <returns>The position in the queue.</returns>
		/*internal static int QueueWriteRequest(WriteRequest request) {
			lock (writeQueue) {
				writeQueue.Enqueue(request);
				return writeQueue.Count - 1;
			}
		}*/

		/// <summary>Adds a write request to the write queue.</summary>
		/// <param name="request">The request to write.</param>
		/// <returns>The position in the queue.</returns>
		internal static int QueueWriteRequest(Wiimote wiimote, byte[] buff) {
			WriteRequest request = new WriteRequest(wiimote, buff);
			if (!request.Send()) {
				//Debug.WriteLine($"Failed to send: {request}");
			}
			return 0;
			/*lock (writeQueue) {
				writeQueue.Enqueue(new WriteRequest(wiimote, buff));
				writeReady.Set();
				return writeQueue.Count - 1;
			}*/
		}

		/// <summary>The Wiimote types that are allowed to be discovered.</summary>
		public static WiimoteType AllowedTypes {
			get => allowedTypes;
			set {
				allowedTypes = value;
				var attrs = EnumInfo<WiimoteType>.GetAttributes<DeviceInfoAttribute>(value).ToArray();
				lock (allowedDeviceNames) {
					allowedDeviceNames = new string[attrs.Length];
					allowedProductIDs = new int[attrs.Length];
					for (int i = 0; i < attrs.Length; i++) {
						allowedDeviceNames[i] = attrs[i].Name;
						allowedProductIDs[i] = attrs[i].ProductID;
					}
				}
			}
		}

		/// <summary>Discovery will automatically turn on or off depending on if enough
		/// Wiimotes have been connected. Set to zero to disable.</summary>
		public static int AutoDiscoveryCount {
			get => autoDiscoveryCount;
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(AutoDiscoveryCount));
				autoDiscoveryCount = value;
				UpdateTaskMode();
			}
		}

		/// <summary>The timeout before a Wiimote should disconnect.
		/// Set to <see cref="TimeSpan.Zero"/> to disable the timeout.</summary>
		/*public static TimeSpan DisconnectTimeout {
			get => disconnectTimeout;
			set => disconnectTimeout = value;
		}*/

		/// <summary>Disconnected Wiimotes will automatically unpair from the system to
		/// facilitate and speed up re-paring later.</summary>
		public static bool UnpairOnDisconnect {
			get => unpairOnDisconnect;
			set => unpairOnDisconnect = value;
		}

		/// <summary>Discovered devices will automatically connect instead of raising
		/// <see cref="Discovered"/> to ask for what to do.</summary>
		public static bool AutoConnect {
			get => autoConnect;
			set => autoConnect = value;
		}

		/// <summary>
		/// The maximum time, in milliseconds, between data report writes.  This prevents
		/// WiimoteLib from attempting to write faster than most bluetooth drivers can handle.
		/// <para/>
		/// If you attempt to write at a rate faster than this, the extra write requests will
		/// be queued up and written to the Wii Remote after the delay is up.
		/// </summary>
		public static int MaxWriteFrequency {
			get => maxWriteFrequency;
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(MaxWriteFrequency));
				lock (writeQueue)
					maxWriteFrequency = value;
			}
		}

		/// <summary>The time to wait after pairing a Wiimote device before trying to
		/// connect (in milliseconds). Raise this value if your Wiimotes have trouble
		/// connecting.</summary>
		public static int DriverInstallDelay {
			get => driverInstallDelay;
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(DriverInstallDelay));
				driverInstallDelay = value;
			}
		}

		/// <summary>True if an async task is being run to discover Wiimote devices.</summary>
		public static bool IsInDiscoveryMode => discoverTask != null;

		/// <summary>True if an async task is being run to handle connected Wiimote states.</summary>
		public static bool IsInIdleMode => idleTask != null;

		/// <summary>True if an async task is being run to write data to connected Wiimotes.</summary>
		public static bool IsInWriteMode => writeTask != null;

		/// <summary>The size of the write queue for writing data to connected Wiimotes.</summary>
		public static int WriteQueueSize => writeQueue.Count;
		
		/// <summary>The number of connected Wiimotes.</summary>
		public static int WiimoteCount => wiimotes.Count;

		/// <summary>An array of the connected Wiimote addresses.</summary>
		public static BluetoothAddress[] ConnectedAddresses {
			get {
				lock (wiimotes)
					return wiimotes.Select(wm => wm.Address).ToArray();
			}
		}

		/// <summary>An array of the connected Wiimote device paths.</summary>
		public static string[] ConnectedDevices {
			get {
				lock (wiimotes)
					return wiimotes.Select(wm => wm.DevicePath).ToArray();
			}
		}

		/// <summary>An array of the connected Wiimotes.</summary>
		public static Wiimote[] ConnectedWiimotes {
			get => wiimotes.ToArray();
		}
	}
}

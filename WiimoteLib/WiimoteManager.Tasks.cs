using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteLib.Devices;
using WiimoteLib.Util;

namespace WiimoteLib {
	public static partial class WiimoteManager {

		public static void StartDiscovery() {
			// Discovery also handles idle functionality
			if (IsInIdleMode)
				StopIdle();

			lock (taskLock) {
				if (!IsInDiscoveryMode) {
					Debug.WriteLine("Discovery Mode: Start");
					discoverToken = new CancellationTokenSource();
					CancellationToken token = discoverToken.Token;
					discoverTask = Task.Run(() => DiscoverTask(token), token);
				}
			}
		}

		public static void StopDiscovery() {
			lock (taskLock) {
				if (IsInDiscoveryMode) {
					Debug.WriteLine("Discovery Mode: Stop");
					discoverToken?.Cancel();
					discoverToken = null;
					discoverTask = null;
				}
			}

			// Start idle if Wiimotes are connected
			if (wiimotes.Any() && !IsInIdleMode)
				StartIdle();
		}

		private static void StartIdle() {
			if (IsInDiscoveryMode)
				StopDiscovery();

			lock (taskLock) {
				// Only idle if Wiimotes are connected
				if (wiimotes.Any() && !IsInIdleMode) {
					Debug.WriteLine("Idle Mode: Start");
					idleToken = new CancellationTokenSource();
					CancellationToken token = idleToken.Token;
					idleTask = Task.Run(() => IdleTask(token), token);
				}
			}
		}

		private static void StopIdle() {
			lock (taskLock) {
				if (IsInIdleMode) {
					Debug.WriteLine("Idle Mode: Stop");
					idleToken?.Cancel();
					idleToken = null;
					idleTask = null;
				}
			}
		}

		private static void StartWrite() {
			lock (taskLock) {
				if (!IsInWriteMode) {
					Debug.WriteLine("Write Mode: Start");
					writeToken = new CancellationTokenSource();
					CancellationToken token = writeToken.Token;
					writeTask = Task.Run(() => WriteTask(token), token);
				}
			}
		}
		private static void StopWrite() {
			lock (taskLock) {
				if (IsInWriteMode) {
					Debug.WriteLine("Write Mode: Stop");
					writeToken?.Cancel();
					writeToken = null;
					writeTask = null;
				}
			}
		}

		private static void StopAllTasks() {
			lock (taskLock) {
				if (IsInDiscoveryMode) {
					Debug.WriteLine("Discovery Mode: Stop");
					discoverToken?.Cancel();
					discoverToken = null;
					discoverTask = null;
				}
				else if (IsInIdleMode) {
					Debug.WriteLine("Idle Mode: Stop");
					idleToken?.Cancel();
					idleToken = null;
					idleTask = null;
				}
				if (IsInWriteMode) {
					Debug.WriteLine("Write Mode: Stop");
					writeToken?.Cancel();
					writeToken = null;
					writeTask = null;
				}
			}
		}

		private static void UpdateTaskMode() {
			if (WiimoteCount < autoDiscoveryCount) {
				if (!IsInDiscoveryMode)
					StartDiscovery();
			}
			else if (autoDiscoveryCount > 0) {
				if (IsInDiscoveryMode)
					StopDiscovery();
			}
			else if (WiimoteCount > 0) {
				StartIdle();
			}
			else {
				StopIdle();
			}
		}


		private static void DiscoverTask(CancellationToken token) {
			while (!token.IsCancellationRequested) {
				if (!DiscoverLoop(token))
					break;
			}
			Debug.WriteLine("Discover Mode: End");
		}

		private static bool DiscoverLoop(CancellationToken token) {
			//FIXME: Handle BOTH Bluetooth and DolphinBarMode more gracefully.
			//if (DolphinBarMode)
			//	return HIDDiscoverLoop(token);
			//else
			//	return BluetoothDiscoverLoop(token);
			bool result = true;
			if (DolphinBarMode)
				result &= HIDDiscoverLoop(token);
			if (BluetoothMode)
				result &= BluetoothDiscoverLoop(token);
			return result;
		}

		private static bool HIDDiscoverLoop(CancellationToken token) {
			var hids = HIDDeviceInfo.EnumerateDevices(token, MatchHID);
			foreach (HIDDeviceInfo hid in hids) {
				if (token.IsCancellationRequested)
					return false;
				Wiimote wiimote = null;
				lock (wiimotes) {
					wiimote = wiimotes.Find(wm => wm.DevicePath == hid.DevicePath);
				}

				if (wiimote == null) {
					if (autoConnect) {
						//FIXME: Handle BOTH Bluetooth and DolphinBarMode more gracefully.
						WiimoteDeviceInfo wiimoteDevice = new WiimoteDeviceInfo(hid, true);// DolphinBarMode);
						try {
							Connect(wiimoteDevice);
						}
						catch (Exception ex) {
							RaiseConnectionFailed(wiimoteDevice, ex);
						}
					}
					else if (!RaiseDiscovered(null, hid)) {
						return false;
					}
				}
			}
			token.Sleep(1000);
			return true;
		}

		private static bool BluetoothDiscoverLoop(CancellationToken token) {
			HashSet<BluetoothAddress> missingDevices = new HashSet<BluetoothAddress>(ConnectedAddresses);
			var devices = BluetoothDeviceInfo.EnumerateDevices(token, MatchBluetooth);
			Stopwatch watch = Stopwatch.StartNew();
			bool anyPaired = false;
			foreach (BluetoothDeviceInfo device in devices) {
				if (token.IsCancellationRequested)
					return false;
				Debug.WriteLine($"Took {watch.ElapsedMilliseconds}ms to enumerate bluetooth device");
				Wiimote wiimote = null;
				lock (wiimotes) {
					wiimote = wiimotes.Find(wm => wm.Address == device.Address);
				}


				if (device.Connected) {
					if (wiimote != null) {
						// Give Wiimote the updated Bluetooth device
						wiimote.Device.Bluetooth = device;
						missingDevices.Remove(device.Address);
					}
					else {
						HIDDeviceInfo hid = HIDDeviceInfo.GetDevice(device.Address);
						// Drivers must not be installed yet, let's wait a bit
						if (hid != null) {
							if (autoConnect) {
								WiimoteDeviceInfo wiimoteDevice = new WiimoteDeviceInfo(device, hid);
								try {
									Connect(wiimoteDevice);
								}
								catch (Exception ex) {
									RaiseConnectionFailed(wiimoteDevice, ex);
								}
							}
							else if (!RaiseDiscovered(device, hid)) {
								return false;
							}
						}
						/*else if (device.PairDevice(token)) {
							anyPaired = true;
						}
						else {
							Debug.WriteLine("{device} pair failed!");
						}*/
					}
				}
				else {
					if (wiimote != null) {
						lock (wiimotes) {
							wiimote.Dispose();
							wiimotes.Remove(wiimote);
							RaiseDisconnected(wiimote, DisconnectReason.ConnectionLost);
						}
					}
					else if (device.IsDiscoverable() /*|| !device.Remembered*/) {
						if (pairOnDiscover) {
							if (device.PairDevice(token)) {
								anyPaired = true;
							}
							else {
								Debug.WriteLine("{device} pair failed!");
							}
						}
					}
					else if (device.Remembered && unpairOnDisconnect) {
						device.RemoveDevice(token);
					}
				}
				watch.Restart();
			}
			token.Sleep((anyPaired ? driverInstallDelay : 0) + 1000);
			return true;
		}

		private static void IdleTask(CancellationToken token) {
			while (!token.IsCancellationRequested) {
				if (!IdleLoop(token))
					break;
			}
			Debug.WriteLine("Idle Mode: End");
		}

		private static bool IdleLoop(CancellationToken token) {
			//FIXME: Handle BOTH Bluetooth and DolphinBarMode more gracefully.
			//if (DolphinBarMode)
			//	return HIDIdleLoop(token);
			//else
			//	return BluetoothIdleLoop(token);
			bool result = true;
			if (DolphinBarMode)
				result &= HIDIdleLoop(token);
			if (BluetoothMode)
				result &= BluetoothIdleLoop(token);
			return result;
		}

		private static bool HIDIdleLoop(CancellationToken token) {
			var wiimoteList = ConnectedWiimotes;
			if (wiimoteList.Length == 0)
				return false;

			foreach (Wiimote wiimote in wiimoteList) {
				if (token.IsCancellationRequested)
					return false;
				//FIXME: Quick fix to support both Bluetooth and DolphinBar connections.
				//       Notice, that we only continue when in Bluetooth mode, this ensures
				//       Bluetooth devices are handled properly even if they were connected
				//       otherwise.
				if (wiimote.Device.IsBluetooth && BluetoothMode)
					continue;
				try {
#if DEBUG
					wiimote.GetStatus();
#else
					wiimote.GetStatus(800);
#endif
				}
				catch (TimeoutException) {
					// Connection may have been lost
					lock (wiimotes) {
						wiimote.Dispose();
						wiimotes.Remove(wiimote);
						RaiseDisconnected(wiimote, DisconnectReason.ConnectionLost);
					}
				}
			}
			token.Sleep(200);
			return true;
		}

		private static bool BluetoothIdleLoop(CancellationToken token) {
			var wiimoteList = ConnectedWiimotes;
			if (wiimoteList.Length == 0)
				return false;

			foreach (Wiimote wiimote in wiimoteList) {
				if (token.IsCancellationRequested)
					return false;
				//FIXME: Quick fix to support both DolphinBar and Bluetooth connections.
				if (!wiimote.Device.IsBluetooth)
					continue;
				BluetoothDeviceInfo device = wiimote.Device.Bluetooth;
				Stopwatch watch2 = Stopwatch.StartNew();
				device.Refresh();
				Debug.WriteLine($"Took {watch2.ElapsedMilliseconds}ms refresh {device}");
				if (!device.Connected) {
					lock (wiimotes) {
						wiimote.Dispose();
						wiimotes.Remove(wiimote);
						RaiseDisconnected(wiimote, DisconnectReason.ConnectionLost);
					}
				}
				token.Sleep(100);
			}
			token.Sleep(1500);
			return true;
		}

		private static void WriteTask(CancellationToken token) {
			while (!token.IsCancellationRequested) {
				WriteLoop(token);
			}
			Debug.WriteLine("Write Mode: End");
		}

		private static void WriteLoop(CancellationToken token) {
			lock (writeQueue) {
				if (writeQueue.Count != 0) {
					WriteRequest request = writeQueue.Dequeue();
					request.Send();
				}
			}
			token.Sleep(maxWriteFrequency);
			if (writeQueue.Count == 0)
				writeReady.WaitOne();
		}

		/*private static void CheckForTimeOuts(IEnumerable<BluetoothAddress> missingDevices,
			CancellationToken token)
		{
			if (DisconnectTimeout == TimeSpan.Zero)
				return;

			foreach (BluetoothAddress address in missingDevices) {
				lock (wiimotes) {
					if (!wiimotes.TryGetValue(address, out WiimoteNew wiimote))
						continue;
					
					if (wiimote.Device.TimeSinceLastSeen >= DisconnectTimeout) {
						wiimote.Dispose();
						wiimotes.Remove(address);
						RaiseDisconnected(wiimote, DisconnectReason.TimedOut);
					}
				}
			}
		}*/
	}
}

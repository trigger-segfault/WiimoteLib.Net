using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Devices;

namespace WiimoteLib {
	internal static class WiimoteRegistry {

		public static BluetoothDeviceInfo GetBluetoothDevice(string hidPath) {
			return new BluetoothDeviceInfo(GetBluetoothAddress(hidPath));
		}

		public static BluetoothAddress GetBluetoothAddress(string hidPath) {
			RegistryKey key = FindHIDPathKey(hidPath);
			if (key == null)
				return BluetoothAddress.Invalid;

			RegistryKey deviceParamsKey = key.OpenSubKey("Device Parameters");
			if (deviceParamsKey == null)
				return BluetoothAddress.Invalid;

			byte[] data = deviceParamsKey.GetValue("BluetoothAddress") as byte[];
			if (data == null || data.Length != 8)
				return BluetoothAddress.Invalid;

			return new BluetoothAddress(data);
		}

		public static bool MatchesHIDPath(string hidPath, BluetoothAddress address) {
			return FindHIDPathKey(hidPath, address) != null;
		}

		public static bool IsDriverInstalled(string hidPath, BluetoothAddress address) {
			RegistryKey key = FindHIDPathKey(hidPath, address);
			if (key == null)
				return false;

			RegistryKey deviceParamsKey = key.OpenSubKey("Device Parameters");
			return deviceParamsKey != null;
		}

		private static RegistryKey FindHIDPathKey(string hidPath, BluetoothAddress address = default(BluetoothAddress)) {
			foreach (string keyName in WiimoteConstants.RegKeys) {
				RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName);
				if (key == null)
					continue;

				IEnumerable<RegistryKey> subKeys;
				if (address.IsInvalid)
					subKeys = key.EnumerateSubKeys();
				else
					subKeys = key.EnumerateSubKeys(n => n.ContainsMac(address));

				foreach (RegistryKey subKey in subKeys) {
					string prefix = subKey.GetValue("ParentIdPrefix") as string;
					if (prefix != null && hidPath.Contains(prefix)) {
						// This is it, we've located the correct devicePath entry
						return subKey;
					}
				}
			}
			return null;
		}

		private static bool ContainsMac(this string name, BluetoothAddress address) {
			return name.IndexOf($"&{address.Int64:X12}_", StringComparison.OrdinalIgnoreCase) != -1;
		}

		public static IEnumerable<RegistryKey> EnumerateSubKeys(this RegistryKey key) {
			foreach (string subKeyName in key.GetSubKeyNames()) {
				RegistryKey subKey = key.OpenSubKey(subKeyName);
				if (subKey != null)
					yield return subKey;
			}
		}

		public static IEnumerable<RegistryKey> EnumerateSubKeys(this RegistryKey key, Predicate<string> match) {
			foreach (string subKeyName in key.GetSubKeyNames()) {
				if (match(subKeyName)) {
					RegistryKey subKey = key.OpenSubKey(subKeyName);
					if (subKey != null)
						yield return subKey;
				}
			}
		}

	}
}

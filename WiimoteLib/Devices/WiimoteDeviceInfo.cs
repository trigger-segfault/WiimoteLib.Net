using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using WiimoteLib.Util;

namespace WiimoteLib.Devices {
	public class WiimoteDeviceInfo {
		internal BluetoothDeviceInfo Bluetooth;
		internal HIDDeviceInfo HID;

		internal SafeFileHandle Handle => HID.Handle;
		internal IntPtr DangerousHandle => HID.DangerousHandle;
		internal FileStream Stream => HID.Stream;

		public WiimoteType Type { get; }

		//FIXME: Quick fix to detect if we have a Bluetooth connection.
		//       It works, but it doesn't feel like a good solution without
		//       further documentation.
		public bool IsBluetooth => !Bluetooth.Address.IsInvalid;
		public BluetoothAddress Address => Bluetooth.Address;
		public bool Connected => Bluetooth.Connected;
		public bool Remembered => Bluetooth.Remembered;
		public bool Authenticated => Bluetooth.Authenticated;
		public string DeviceName => Bluetooth.Name;
		public DateTime LastSeen => Bluetooth.LastSeen;
		public DateTime LastUsed => Bluetooth.LastUsed;
		public DateTime LastSeenUtc => Bluetooth.LastSeenUtc;
		public DateTime LastUsedUtc => Bluetooth.LastUsedUtc;

		public TimeSpan TimeSinceLastSeen => DateTime.UtcNow - LastSeenUtc;

		public int ProductID => HID.ProductID;
		public int VendorID => HID.VendorID;
		public string DevicePath => HID.DevicePath;
		public bool IsOpen => HID.IsOpen;
		
		internal WiimoteDeviceInfo(BluetoothDeviceInfo bt, HIDDeviceInfo hid) {
			Bluetooth = bt;
			HID = hid;
			Type = GetTypeFromName(bt.Name);
		}

		internal WiimoteDeviceInfo(BluetoothDeviceInfo bt) {
			Bluetooth = bt;
			HID = HIDDeviceInfo.GetDevice(bt.Address);
			if (HID == null)
				throw new IOException("Error opening HID device!");
			Type = GetTypeFromName(bt.Name);
		}

		internal WiimoteDeviceInfo(HIDDeviceInfo hid, bool dolphinBarMode) {
			HID = hid;
			if (dolphinBarMode) {
				Bluetooth = new BluetoothDeviceInfo();
				Type = GetTypeFromPID(hid.ProductID);
			}
			else {
				Bluetooth = BluetoothDeviceInfo.GetDevice(hid.DevicePath);
				if (Bluetooth == null)
					throw new IOException("Error locating Bluetooth device!");
				Type = GetTypeFromName(Bluetooth.Name);
			}
		}

		internal WiimoteDeviceInfo(long address)
			: this(new BluetoothDeviceInfo(address))
		{
		}

		internal WiimoteDeviceInfo(ulong address)
			: this(new BluetoothDeviceInfo(address))
		{
		}

		internal WiimoteDeviceInfo(BluetoothAddress address)
			: this(new BluetoothDeviceInfo(address))
		{
		}

		internal WiimoteDeviceInfo(string hidPath, bool dolphinBarMode) {
			HID = HIDDeviceInfo.GetDevice(hidPath);
			if (HID == null)
				throw new IOException("Error opening HID device!");
			if (dolphinBarMode) {
				Bluetooth = new BluetoothDeviceInfo();
				Type = GetTypeFromPID(HID.ProductID);
			}
			else {
				Bluetooth = WiimoteRegistry.GetBluetoothDevice(hidPath);
				if (Bluetooth == null)
					throw new IOException("Error locating Bluetooth device!");
				Type = GetTypeFromName(Bluetooth.Name);
			}
		}

		public override string ToString() {
			if (!Address.IsInvalid)
				return $"{Type} ({Address})";
			else
				return $"{Type}";
		}

		private static void ThrowIfNotWiimoteIDs(int vendorID, int productID) {
			if (!WiimoteConstants.VendorIDs.Any(v => v == vendorID) ||
				!WiimoteConstants.ProductIDs.Any(p => p == productID))
			{
				throw new ArgumentException($"Vendor ID '{vendorID:X4}' or " +
					$"Product ID '{productID:X4} are not Wiimote IDs!");
			}
		}

		public static WiimoteType GetTypeFromName(string name, bool throwOnError = true) {
			foreach (var field in EnumInfo<WiimoteType>.Fields) {
				if (field.LongValue == 0)
					continue;
				string typeName = field.GetAttribute<DeviceInfoAttribute>().Name;
				if (name == typeName)
					return field.Value;
			}
			if (throwOnError)
				throw new ArgumentException($"{name} is not a Wiimote device name!", nameof(name));
			return WiimoteType.Unknown;
		}

		public static WiimoteType GetTypeFromPID(int productID, bool throwOnError = true) {
			foreach (var field in EnumInfo<WiimoteType>.Fields) {
				if (field.LongValue == 0)
					continue;
				int typeProductId = field.GetAttribute<DeviceInfoAttribute>().ProductID;
				if (productID == typeProductId)
					return field.Value;
			}
			if (throwOnError)
				throw new ArgumentException($"{productID:x4} is not a Wiimote product ID!", nameof(productID));
			return WiimoteType.Unknown;
		}
	}
}

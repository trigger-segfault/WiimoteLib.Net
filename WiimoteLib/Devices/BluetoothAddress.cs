using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Devices {
	public struct BluetoothAddress {

		public static readonly BluetoothAddress Invalid = new BluetoothAddress();

		private long address;


		public BluetoothAddress(long address) {
			this.address = address;
		}

		public BluetoothAddress(ulong address) {
			this.address = unchecked((long) address);
		}

		public BluetoothAddress(string address) {
			this.address = Parse(address).address;
		}

		public BluetoothAddress(byte[] data) {
			this.address = BitConverter.ToInt64(data, 0);
		}


		public string MacAddress {
			get {
				var parts = Bytes.Reverse().Select(b => b.ToString("X2"));
				return string.Join(":", parts);
			}
		}

		public string ShortMacAddress => Int64.ToString("X12");

		public bool IsInvalid => address == 0;

		public long Int64 => address;
		public ulong UInt64 => unchecked((ulong) address);

		public byte[] Bytes {
			get => BitConverter.GetBytes(address).Take(6).ToArray();
		}

		public override string ToString() => MacAddress;
		public override int GetHashCode() => address.GetHashCode();

		public override bool Equals(object obj) {
			switch (obj) {
			case BluetoothAddress btaddr: return this == btaddr;
			case long l: return address == l;
			case ulong ul: return UInt64 == ul;
			default: return false;
			}
		}



		public static bool operator ==(BluetoothAddress a, BluetoothAddress b) {
			return a.address == b.address;
		}

		public static bool operator !=(BluetoothAddress a, BluetoothAddress b) {
			return a.address != b.address;
		}

		public static implicit operator BluetoothAddress(long address) {
			return new BluetoothAddress(address);
		}

		public static implicit operator BluetoothAddress(ulong address) {
			return new BluetoothAddress(address);
		}

		public static bool IsMacAddress(string s) {
			return TryParse(s, out _);
		}

		public static BluetoothAddress Parse(string s) {
			if (s.Length == 12 + 5 + 2 && s.StartsWith("{") && s.EndsWith("}"))
				s = s.Substring(1, s.Length - 2);

			if (s.Length == 12) {
				return new BluetoothAddress(long.Parse(s, NumberStyles.HexNumber));
			}
			else if (s.Length == 12 + 5) {
				string[] parts = s.Split(':');
				if (parts.Length != 5)
					throw new ArgumentException("Parts must be separated with 5 ':'s or no ':'s!");
				for (int i = 0; i < 5; i++) {
					string part = parts[i];
					if (part.Length != 2)
						throw new ArgumentException("Parts separated with ':'s must be two digits long!");
				}
				s = string.Join("", parts);
				return new BluetoothAddress(long.Parse(s, NumberStyles.HexNumber));
			}
			throw new ArgumentException($"Invalid string length '{s.Length}'! Must be 12 or 15 with 5 ':'s.", nameof(s));
		}

		public static bool TryParse(string s, out BluetoothAddress result) {
			result = new BluetoothAddress();
			if (s.Length == 12 + 5 + 2 && s.StartsWith("{") && s.EndsWith("}"))
				s = s.Substring(1, s.Length - 2);

			if (s.Length == 12) {
				if (long.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long address)) {
					result = new BluetoothAddress(address);
					return true;
				}
			}
			else if (s.Length == 12 + 5) {
				string[] parts = s.Split(':');
				if (parts.Length != 5)
					return false;
				for (int i = 0; i < 5; i++) {
					string part = parts[i];
					if (part.Length != 2)
						return false;
					if (!IsHexNumber(part))
						return false;
				}
				s = string.Join("", parts);
				if (long.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long address)) {
					result = new BluetoothAddress(address);
					return true;
				}
			}
			return false;
		}

		private static bool IsHexNumber(string s) {
			foreach (char c in s) {
				if (IsHexDigit(c))
					return true;
			}
			return true;
		}

		private static bool IsHexDigit(char c) {
			return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
		}
	}
}

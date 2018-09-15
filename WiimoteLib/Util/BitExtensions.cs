using System;
using System.Collections.Generic;
using System.Text;

namespace WiimoteLib.Util {
	internal static class BitExtensions {

		public static bool GetBit(this byte b, int bitIndex) {
			return (b & (1 << bitIndex)) != 0;
		}

		public static bool GetBit(this byte[] bytes, int offset, int bit) {
			return (bytes[offset] & (1 << bit)) != 0;
		}

		public static int GetRange(this byte b, int bitStart, int bitLength) {
			return (b & (((1 << bitLength) - 1) << bitStart));
		}

		public static int GetRange(this byte b, int bitStart, int bitLength, int shift) {
			shift -= bitStart;
			int bitMask = ((1 << bitLength) - 1) << bitStart;
			if (shift >= 0)
				return (b & bitMask) << shift;
			else
				return (b & bitMask) >> -shift;
		}

		public static int GetRange(this byte[] bytes, int offset, int bitStart, int bitLength) {
			return (bytes[offset] & (((1 << bitLength) - 1) << bitStart));
		}

		public static int GetRange(this byte[] bytes, int offset, int bitStart, int bitLength, int shift) {
			shift -= bitStart;
			int bitMask = ((1 << bitLength) - 1) << bitStart;
			if (shift > 0)
				return (bytes[offset] & bitMask) << shift;
			else if (shift < 0)
				return (bytes[offset] & bitMask) >> -shift;
			else
				return (bytes[offset] & bitMask);
		}

		public static int GetMask(this byte b, int bitMask, int shift) {
			if (shift >= 0)
				return (b & bitMask) << shift;
			else
				return (b & bitMask) >> -shift;
		}

		public static int GetMask(this byte[] bytes, int offset, int bitMask, int shift) {
			if (shift >= 0)
				return (bytes[offset] & bitMask) << shift;
			else
				return (bytes[offset] & bitMask) >> -shift;
		}
	}
}

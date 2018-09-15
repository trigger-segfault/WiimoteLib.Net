using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib {
	internal static class WiimoteConstants {
		public const int ReportLength = 22;

		public const int VendorID		= 0x057e;
		public const int ProductID		= 0x0306;
		public const int ProductIDPlus	= 0x0330;

		public static readonly int[] VendorIDs = { VendorID };
		public static readonly int[] ProductIDs = { ProductID, ProductIDPlus };

		public const string Wiimote			= "Nintendo RVL-CNT-01";
		public const string WiimotePlus		= "Nintendo RVL-CNT-01-TR";
		public const string ProController	= "Nintendo RVL-CNT-01-UC";
		public const string BalanceBoard	= "Nintendo RVL-WBC-01";

		public const string RegKey		= @"SYSTEM\CurrentControlSet\Enum\BTHENUM\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002057e_PID&0306";
		public const string RegKeyPlus	= @"SYSTEM\CurrentControlSet\Enum\BTHENUM\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002057e_PID&0330";

		public static readonly string[] RegKeys = { RegKey, RegKeyPlus };
	}
}

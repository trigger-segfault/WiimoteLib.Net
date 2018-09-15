using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Sensitivity of the IR camera on the Wiimote
	/// </summary>
	public enum IRSensitivity {
		/// <summary>
		/// Equivalent to level 1 on the Wii console
		/// </summary>
		WiiLevel1,
		/// <summary>
		/// Equivalent to level 2 on the Wii console
		/// </summary>
		WiiLevel2,
		/// <summary>
		/// Equivalent to level 3 on the Wii console (default)
		/// </summary>
		WiiLevel3,
		/// <summary>
		/// Equivalent to level 4 on the Wii console
		/// </summary>
		WiiLevel4,
		/// <summary>
		/// Equivalent to level 5 on the Wii console
		/// </summary>
		WiiLevel5,
		/// <summary>
		/// Maximum sensitivity
		/// </summary>
		Maximum,
	}
}

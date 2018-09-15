using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// The extension plugged into the Wiimote
	/// </summary>
	public enum ExtensionType : long {
		/// <summary>
		/// No extension
		/// </summary>
		None = 0x000000000000,
		/// <summary>
		/// Nunchuk extension
		/// </summary>
		Nunchuk = 0x0000a4200000,
		/// <summary>
		/// Classic Controller extension
		/// </summary>
		ClassicController = 0x0000a4200101,
		/// <summary>
		/// Guitar controller from Guitar Hero 3/WorldTour
		/// </summary>
		//Guitar = 0x0000a4200103,
		/// <summary>
		/// Drum controller from Guitar Hero: World Tour
		/// </summary>
		//Drums = 0x0100a4200103,
		/// <summary>
		/// Wii Fit Balance Board controller
		/// </summary>
		//BalanceBoard = 0x0000a4200402,
		/// <summary>
		/// Taiko "TaTaCon" drum controller
		/// </summary>
		//TaikoDrum = 0x0000a4200111,
		/// <summary>
		/// Wii MotionPlus extension
		/// </summary>
		MotionPlus = 0x0000a4200405,
		/// <summary>
		/// Wii MotionPlus extension
		/// </summary>
		MotionPlusNunchuk = 0x0000a4200505,
		/// <summary>
		/// Wii MotionPlus extension
		/// </summary>
		MotionPlusOther = 0x0000a4200705,
		/// <summary>
		/// Partially inserted extension.  This is an error condition.
		/// </summary>
		ParitallyInserted = 0xffffffffffff
	}
	
	[Serializable]
	public enum MotionPlusExtensionType : byte {
		NotInUse = 0,
		NoExtension = 0x04,
		Nunchuk = 0x05,
		ClassicController = 0x07,
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib {
	[AttributeUsage(AttributeTargets.Field)]
	internal class DeviceInfoAttribute : Attribute {

		public string Name { get; }
		public int ProductID { get; }

		public DeviceInfoAttribute(string name, int productID) {
			Name = name;
			ProductID = productID;
		}
	}

	/// These different Wii Remote Types are used to differentiate between different devices that behave like the Wii Remote.
	[Flags]
	public enum WiimoteType {
		Unknown = 0,
		/// The original Wii Remote (Name: RVL-CNT-01).  This includes all Wii Remotes manufactured for the original Wii.
		[DeviceInfo(WiimoteConstants.Wiimote, WiimoteConstants.ProductID)]
		Wiimote = (1 << 0),
		/// The new Wii Remote Plus (Name: RVL-CNT-01-TR).  Wii Remote Pluses are now standard with Wii U consoles and come
		/// with a built-in Wii Motion Plus extension.
		[DeviceInfo(WiimoteConstants.WiimotePlus, WiimoteConstants.ProductIDPlus)]
		WiimotePlus = (1 << 1),
		/// The Wii U Pro Controller (Name: RVL-CNT-01-UC) behaves identically to a Wii Remote with a Classic Controller
		/// attached.  Obviously the Pro Controller does not support IR so those features will not work.
		[DeviceInfo(WiimoteConstants.ProController, WiimoteConstants.ProductIDPlus)]
		ProController = (1 << 2),

		[DeviceInfo(WiimoteConstants.BalanceBoard, WiimoteConstants.ProductID)]
		BalanceBoard = (1 << 3),
	}

	public enum DisconnectReason {
		UnhandledException,
		ConnectionLost,
		User,
	}
}

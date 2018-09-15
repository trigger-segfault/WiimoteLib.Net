using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.DataTypes;
using WiimoteLib.Devices;

namespace WiimoteLib.Events {
	
	public class WiimoteDeviceEventArgs : EventArgs {

		public WiimoteDeviceInfo Device { get; }
		public WiimoteType Type => Device.Type;

		public WiimoteDeviceEventArgs(WiimoteDeviceInfo device) {
			Device = device;
		}
	}

	public class WiimoteEventArgs : WiimoteDeviceEventArgs {

		public Wiimote Wiimote { get; }

		public WiimoteEventArgs(Wiimote wiimote) : base(wiimote.Device) {
			Wiimote = wiimote;
		}
	}

	public class WiimoteRangeEventArgs : EventArgs {
		public WiimoteDeviceInfo Device => Wiimote.Device;
		public WiimoteType Type => Device.Type;
		public Wiimote Wiimote { get; }
		public bool InRange { get; }

		public WiimoteRangeEventArgs(Wiimote wiimote, bool inRange) {
			Wiimote = wiimote;
			InRange = inRange;
		}
	}

	public class WiimoteDiscoveredEventArgs : WiimoteDeviceEventArgs {

		public bool AddDevice { get; set; }
		public bool KeepSearching { get; set; }

		public WiimoteDiscoveredEventArgs(BluetoothDeviceInfo device)
			: this(new WiimoteDeviceInfo(device))
		{
		}

		public WiimoteDiscoveredEventArgs(HIDDeviceInfo device, bool dolphinBarMode)
			: this(new WiimoteDeviceInfo(device, dolphinBarMode))
		{
		}

		public WiimoteDiscoveredEventArgs(WiimoteDeviceInfo device) : base(device) {
			AddDevice = false;
			KeepSearching = true;
		}
	}

	public class WiimoteDisconnectedEventArgs : WiimoteEventArgs {
		
		public DisconnectReason Reason { get; }

		public WiimoteDisconnectedEventArgs(Wiimote wiimote, DisconnectReason reason)
			: base(wiimote)
		{
			Reason = reason;
		}
	}

	public class WiimoteConnectionFailedEventArgs : WiimoteDeviceEventArgs {

		public Exception Exception { get; }

		public WiimoteConnectionFailedEventArgs(WiimoteDeviceInfo device, Exception ex)
			: base(device)
		{
			Exception = ex;
		}
	}

	public class WiimoteExceptionEventArgs : WiimoteEventArgs {

		public Exception Exception { get; }

		public WiimoteExceptionEventArgs(Wiimote wiimote, Exception ex)
			: base(wiimote)
		{
			Exception = ex;
		}
	}

	public class WiimoteExtensionEventArgs : WiimoteEventArgs {
		/// <summary>The extenstion type inserted or removed.</summary>
		public ExtensionType ExtensionType { get; }
		/// <summary>Whether the extension was inserted or removed.</summary>
		public bool Inserted { get; }

		/// <summary>Constructor</summary>
		/// <param name="type">The extension type inserted or removed</param>
		/// <param name="inserted">Whether the extension was inserted or removed</param>
		public WiimoteExtensionEventArgs(Wiimote wiimote, ExtensionType type,
			bool inserted)
			: base(wiimote)
		{
			ExtensionType = type;
			Inserted = inserted;
		}
	}

	/// <summary>Argument sent through the StateChanged.</summary>
	public class WiimoteStateEventArgs : WiimoteEventArgs {
		/// <summary>The current state of the Wiimote and extension controllers.</summary>
		public WiimoteState WiimoteState { get; }

		/// <summary>Constructor</summary>
		/// <param name="ws">Wiimote state</param>
		public WiimoteStateEventArgs(Wiimote wiimote) : base(wiimote) {
			WiimoteState = wiimote.WiimoteState;
		}
	}
}

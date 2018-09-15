using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Devices;

namespace WiimoteLib.Events {
	public class WiimoteDeviceException : Exception {

		public WiimoteDeviceInfo Device { get; }
		public WiimoteType Type => Device.Type;

		public WiimoteDeviceException(WiimoteDeviceInfo device) {
			Device = device;
		}

		public WiimoteDeviceException(WiimoteDeviceInfo device, string message)
			: base(message)
		{
			Device = device;
		}

		public WiimoteDeviceException(WiimoteDeviceInfo device, string message,
			Exception innerException)
			: base(message, innerException)
		{
			Device = device;
		}
	}
	public class WiimoteException : Exception {

		public WiimoteDeviceInfo Device => Wiimote.Device;
		public WiimoteType Type => Device.Type;
		public Wiimote Wiimote { get; }


		public WiimoteException(Wiimote wiimote) {
			Wiimote = wiimote;
		}

		public WiimoteException(Wiimote wiimote, string message)
			: base(message)
		{
			Wiimote = wiimote;
		}

		public WiimoteException(Wiimote wiimote, string message,
			Exception innerException)
			: base(message, innerException)
		{
			Wiimote = wiimote;
		}
	}

	public class WiimoteAlreadyConnectedException : WiimoteException {
		
		public WiimoteAlreadyConnectedException(Wiimote wiimote)
			: base(wiimote, $"{wiimote.Type} already connected!")
		{
		}

		public WiimoteAlreadyConnectedException(Wiimote wiimote, BluetoothAddress address)
			: base(wiimote, $"{wiimote.Type} with address {{{address}}} already connected!")
		{
		}
		
		public WiimoteAlreadyConnectedException(Wiimote wiimote, string devicePath)
			: base(wiimote, $"{wiimote.Type} with HID device path '{devicePath}' already connected!")
		{
		}
	}
}

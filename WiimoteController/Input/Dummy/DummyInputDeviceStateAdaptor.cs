using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace WiimoteController.Input.Dummy {
	public class DummyInputDeviceStateAdaptor : IInputDeviceStateAdaptor {

		public bool IsHardwareKeyDown(VirtualKeyCode keyCode) {
			return false;
		}

		public bool IsHardwareKeyUp(VirtualKeyCode keyCode) {
			return true;
		}

		public bool IsKeyDown(VirtualKeyCode keyCode) {
			return false;
		}

		public bool IsKeyUp(VirtualKeyCode keyCode) {
			return true;
		}

		public bool IsTogglingKeyInEffect(VirtualKeyCode keyCode) {
			return false;
		}
	}
}

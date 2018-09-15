using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace WiimoteController.Input.Dummy {
	public class DummyInputSimulator : IInputSimulator {
		public IKeyboardSimulator Keyboard { get; }

		public IMouseSimulator Mouse { get; }

		public IInputDeviceStateAdaptor InputDeviceState { get; }

		public DummyInputSimulator() {
			Keyboard = new DummyKeyboardSimulator(this);
			Mouse = new DummyMouseSimulator(this);

			// No reason this can't be the real thing
			InputDeviceState = new WindowsInputDeviceStateAdaptor();
		}
	}
}

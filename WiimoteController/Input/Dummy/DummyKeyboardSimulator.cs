using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace WiimoteController.Input.Dummy {
	public class DummyKeyboardSimulator : IKeyboardSimulator {
		public IMouseSimulator Mouse => sim.Mouse;

		private IInputSimulator sim;

		internal DummyKeyboardSimulator(IInputSimulator sim) {
			this.sim = sim;
		}

		public IKeyboardSimulator KeyDown(VirtualKeyCode keyCode) {
			return this;
		}

		public IKeyboardSimulator KeyPress(VirtualKeyCode keyCode) {
			return this;
		}

		public IKeyboardSimulator KeyPress(params VirtualKeyCode[] keyCodes) {
			return this;
		}

		public IKeyboardSimulator KeyUp(VirtualKeyCode keyCode) {
			return this;
		}

		public IKeyboardSimulator ModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, IEnumerable<VirtualKeyCode> keyCodes) {
			return this;
		}

		public IKeyboardSimulator ModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, VirtualKeyCode keyCode) {
			return this;
		}

		public IKeyboardSimulator ModifiedKeyStroke(VirtualKeyCode modifierKey, IEnumerable<VirtualKeyCode> keyCodes) {
			return this;
		}

		public IKeyboardSimulator ModifiedKeyStroke(VirtualKeyCode modifierKeyCode, VirtualKeyCode keyCode) {
			return this;
		}

		public IKeyboardSimulator Sleep(int millsecondsTimeout) {
			return this;
		}

		public IKeyboardSimulator Sleep(TimeSpan timeout) {
			return this;
		}

		public IKeyboardSimulator TextEntry(string text) {
			return this;
		}

		public IKeyboardSimulator TextEntry(char character) {
			return this;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace WiimoteController.Input.Dummy {
	public class DummyMouseSimulator : IMouseSimulator {
		public int MouseWheelClickSize {
			get => 0;
			set { }
		}

		private IInputSimulator sim;

		internal DummyMouseSimulator(IInputSimulator sim) {
			this.sim = sim;
		}

		public IKeyboardSimulator Keyboard => sim.Keyboard;

		public IMouseSimulator HorizontalScroll(int scrollAmountInClicks) {
			return this;
		}

		public IMouseSimulator LeftButtonClick() {
			return this;
		}

		public IMouseSimulator LeftButtonDoubleClick() {
			return this;
		}

		public IMouseSimulator LeftButtonDown() {
			return this;
		}

		public IMouseSimulator LeftButtonUp() {
			return this;
		}

		public IMouseSimulator MiddleButtonClick() {
			return this;
		}

		public IMouseSimulator MiddleButtonDoubleClick() {
			return this;
		}

		public IMouseSimulator MiddleButtonDown() {
			return this;
		}

		public IMouseSimulator MiddleButtonUp() {
			return this;
		}

		public IMouseSimulator MoveMouseBy(int pixelDeltaX, int pixelDeltaY) {
			return this;
		}

		public IMouseSimulator MoveMouseTo(double absoluteX, double absoluteY) {
			return this;
		}

		public IMouseSimulator MoveMouseToPositionOnVirtualDesktop(double absoluteX, double absoluteY) {
			return this;
		}

		public IMouseSimulator RightButtonClick() {
			return this;
		}

		public IMouseSimulator RightButtonDoubleClick() {
			return this;
		}

		public IMouseSimulator RightButtonDown() {
			return this;
		}

		public IMouseSimulator RightButtonUp() {
			return this;
		}

		public IMouseSimulator Sleep(int millsecondsTimeout) {
			return this;
		}

		public IMouseSimulator Sleep(TimeSpan timeout) {
			return this;
		}

		public IMouseSimulator VerticalScroll(int scrollAmountInClicks) {
			return this;
		}

		public IMouseSimulator XButtonClick(int buttonId) {
			return this;
		}

		public IMouseSimulator XButtonDoubleClick(int buttonId) {
			return this;
		}

		public IMouseSimulator XButtonDown(int buttonId) {
			return this;
		}

		public IMouseSimulator XButtonUp(int buttonId) {
			return this;
		}
	}
}

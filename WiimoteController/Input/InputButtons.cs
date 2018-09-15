using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WiimoteLib;
using WindowsInput;
using WindowsInput.Native;
using MouseButton = System.Windows.Input.MouseButton;
using Timer = System.Threading.Timer;

namespace WiimoteController.Input {
	public enum MouseWheels {
		None,
		Up,
		Left,
		Down,
		Right,
	}

	public interface IInputButton : IInputType {
		//string Name { get; }
		//string Type { get; }
		//bool IsDown { get; }
		//bool IsInitialized { get; }
		//void Initialize(InputSimulator sim);
		bool Update(bool buttonState);
		void Press();
		void Release();
		bool DisabledUntilRelease { get; set; }
		//void Reset();
	}
	public abstract class InputButtonBase : IInputButton {
		private IInputSimulator sim;
		private bool initialized;
		private bool down;
		private bool disabledUntilRelease = false;

		protected IInputSimulator Sim => sim;

		public bool IsDown => down;
		public bool IsInitialized => initialized;
		public bool DisabledUntilRelease {
			get => disabledUntilRelease;
			set {
				if (down && value)
					Release();
				disabledUntilRelease = value;
			}
		}

		public abstract string Name { get; }
		public abstract string Type { get; }

		public override string ToString() => $"{Type}: {Name}";

		public void Initialize(IInputSimulator sim) {
			Dispose();
			this.sim = sim ?? throw new ArgumentNullException(nameof(sim));
			OnInitialize();
			initialized = true;
		}
		public void Dispose() {
			if (initialized) {
				Release();
				OnDispose();
				sim = null;
				down = false;
				initialized = false;
			}
		}
		public bool Update(bool buttonState) {
			if (buttonState != down) {
				if (buttonState) {
					Trace.WriteLine($"{this} PRESS");
					Press();
					return true && !disabledUntilRelease;
				}
				else {
					bool disabledUntilReleasePrev = disabledUntilRelease;
					Trace.WriteLine($"{this} RELEASE");
					Release();
					return true && !disabledUntilReleasePrev;
				}
			}
			return false;
		}
		public void Press() {
			if (!down && initialized) {
				down = true;
				if (!disabledUntilRelease) {
					OnPress();
				}
			}
		}
		public void Release() {
			if (down && initialized) {
				if (!disabledUntilRelease) {
					OnRelease();
				}
				else {
					disabledUntilRelease = false;
				}
				down = false;
			}
		}
		public void Reset() {
			Release();
		}

		protected virtual void OnInitialize() { }
		protected virtual void OnDispose() { }
		protected abstract void OnPress();
		protected abstract void OnRelease();
	}

	public class KeyInput : InputButtonBase {

		private Key key;
		private bool repeat;
		private VirtualKeyCode virtualKey;

		private Timer timer;
		private Stopwatch watch;
		private long lastKepRepeat;
		private int delay;
		private int speed;

		public Key Key {
			get => key;
			set {
				Release();
				key = value;
				virtualKey = (VirtualKeyCode) KeyInterop.VirtualKeyFromKey(Key);
			}
		}
		public bool Repeat {
			get => repeat;
			set {
				if (!value) {
					timer?.Dispose();
					timer = null;
				}
				repeat = value;
			}
		}


		private const int KeyboardDelayRange = 4;
		private const int KeyboardDelayMin = 250;
		private const int KeyboardDelayMax = 1000;
		private const int KeyboardDelayIncrement = (KeyboardDelayMax - KeyboardDelayMin) / (KeyboardDelayRange - 1);

		private const int KeyboardSpeedRange = 32;
		private const float KeyboardSpeedMin = 31f;
		private const float KeyboardSpeedMax = 400f;
		private const float KeyboardSpeedIncrement = (KeyboardSpeedMax - KeyboardSpeedMin) / (KeyboardSpeedRange - 1);

		public KeyInput(Key key, bool repeat = true) {
			this.key = key;
			this.repeat = repeat;
			virtualKey = (VirtualKeyCode) KeyInterop.VirtualKeyFromKey(Key);
		}

		public override string Name => $"{Key} {(repeat ? "(Repeat)" : "")}";
		public override string Type => "Key";

		private int CalculateSpeed() {
			return (int) Math.Round(KeyboardSpeedMin +
				(31 - SystemParameters.KeyboardSpeed) * KeyboardSpeedIncrement);
		}

		private int CalculateDelay() {
			return KeyboardDelayMin +
				SystemParameters.KeyboardDelay * KeyboardDelayIncrement;
		}
		
		protected override void OnPress() {
			Sim.Keyboard.KeyDown(virtualKey);
			if (repeat) {
				timer?.Dispose();
				delay = CalculateDelay();
				speed = CalculateSpeed();
				timer = new Timer(KeyRepeat, null, delay, speed);
				watch = Stopwatch.StartNew();
				lastKepRepeat = 0;
			}
		}
		protected override void OnRelease() {
			timer?.Dispose();
			timer = null;
			Sim.Keyboard.KeyUp(virtualKey);
			watch?.Stop();
			watch = null;
		}
		
		private void KeyRepeat(object state) {
			if (IsDown && IsInitialized) {
				Sim.Keyboard.KeyDown(virtualKey);
				long newTime = watch.ElapsedMilliseconds;
				if (lastKepRepeat == 0)
					Trace.WriteLine($"Delay: {(newTime - lastKepRepeat)}ms / {delay}ms");
				else
					Trace.WriteLine($"Repeat: {(newTime - lastKepRepeat)}ms / {speed}ms");
				lastKepRepeat = newTime;
			}
		}
	}
	public class MouseButtonInput : InputButtonBase {
		private MouseButton button;

		public MouseButton Button {
			get => button;
			set {
				Release();
				button = value;
			}
		}

		public MouseButtonInput(MouseButton button) {
			this.button = button;
		}

		public override string Name => $"{button}";
		public override string Type => "Mouse";

		protected override void OnPress() {
			switch (button) {
			case MouseButton.Left:
				Sim.Mouse.LeftButtonDown();
				break;
			case MouseButton.Middle:
				Sim.Mouse.MiddleButtonDown();
				break;
			case MouseButton.Right:
				Sim.Mouse.RightButtonDown();
				break;
			}
		}
		protected override void OnRelease() {
			switch (button) {
			case MouseButton.Left:
				Sim.Mouse.LeftButtonUp();
				break;
			case MouseButton.Middle:
				Sim.Mouse.MiddleButtonUp();
				break;
			case MouseButton.Right:
				Sim.Mouse.RightButtonUp();
				break;
			}
		}
	}
	public class MouseWheelInput : InputButtonBase {
		const int MaxDistance = 1;
		const float Increment = 0.1f;

		private MouseWheels wheel;
		private int distance;
		private int delay;

		private Timer timer;
		private Stopwatch watch;
		private long lastWheel;

		private int Sign {
			get => ((wheel == MouseWheels.Down || wheel == MouseWheels.Left) ? -1 : 1);
		}

		public bool IsHorizontal => wheel == MouseWheels.Left || wheel == MouseWheels.Right;

		public MouseWheels Wheel {
			get => wheel;
			set {
				Release();
				wheel = value;
			}
		}
		public int Distance {
			get => distance;
			set {
				Release();
				if (value < 1)
					throw new ArgumentOutOfRangeException(nameof(Distance));
				distance = value;
			}
		}
		public int Delay {
			get => delay;
			set {
				Release();
				if (value < 1)
					throw new ArgumentOutOfRangeException(nameof(Delay));
				delay = value;
			}
		}

		public MouseWheelInput(MouseWheels wheel, int distance = 1, int delay = 70) {
			this.wheel = wheel;
			this.distance = distance;
			this.delay = delay;
			if (distance < 1)
				throw new ArgumentOutOfRangeException(nameof(distance));
			if (delay < 1)
				throw new ArgumentOutOfRangeException(nameof(delay));
		}

		public override string Name => $"{wheel}";
		public override string Type => "Wheel";

		protected override void OnPress() {
			if (IsHorizontal)
				Sim.Mouse.HorizontalScroll(Sign * distance);
			else
				Sim.Mouse.VerticalScroll(Sign * distance);
			
			timer?.Dispose();
			timer = new Timer(WheelRepeat, null, delay, delay);
			watch = Stopwatch.StartNew();
			lastWheel = 0;
		}

		protected override void OnRelease() {
			timer.Dispose();
			timer = null;
			watch.Stop();
			watch = null;
		}

		private void WheelRepeat(object state) {
			if (IsDown && IsInitialized) {
				if (IsHorizontal)
					Sim.Mouse.HorizontalScroll(Sign * distance);
				else
					Sim.Mouse.VerticalScroll(Sign * distance);
				long newTime = watch.ElapsedMilliseconds;
				Trace.WriteLine($"Repeat: {(newTime - lastWheel)}ms / {delay}ms");
				lastWheel = newTime;
			}
		}
	}
	public class SoundInput : InputButtonBase {
		public override string Name => "Sound";

		public override string Type => "Sound";

		protected override void OnPress() {
			WiimoteManager.ConnectedWiimotes[0].PlaySound();
		}

		protected override void OnRelease() {

		}
	}
	public class ModifierInput : InputButtonBase {
		public override string Name => "Modifier";

		public override string Type => "Modifier";

		protected override void OnPress() {
		}

		protected override void OnRelease() {
		}
	}
	public class OverlayInput : InputButtonBase {
		public override string Name => "Overlay";

		public override string Type => "Overlay";

		public OverlayWindow Window { get; }

		public OverlayInput(OverlayWindow window) {
			Window = window;
		}
		
		protected override void OnPress() {
			Window.Dispatcher.Invoke(() => {
				Window.Left = 0;
				Window.Top = 0;
				Window.Width = SystemParameters.PrimaryScreenWidth;
				Window.Height = SystemParameters.PrimaryScreenHeight;
				Window.Topmost = true;
				Window.Show();
				Window.Visibility = Visibility.Visible;
			});
		}

		protected override void OnRelease() {
			Window.Dispatcher.Invoke(() => {
				Window.Hide();
				Window.Visibility = Visibility.Hidden;
			});
		}
	}
}

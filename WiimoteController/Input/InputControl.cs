using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WiimoteController.Input.Dummy;
using WiimoteLib;
using WiimoteLib.DataTypes;
using WiimoteLib.Events;
using WindowsInput;
using WindowsInput.Native;
using MouseButton = System.Windows.Input.MouseButton;
using PointF = System.Drawing.PointF;

namespace WiimoteController.Input {
	public enum WiimoteButton {
		Invalid = -1,
		A = 0,
		B,
		One,
		Two,
		Plus,
		Minus,
		Home,
		Up,
		Down,
		Left,
		Right,
		C,
		Z,
		Count,
	}

	public enum WiimoteAnalog {
		Nunchuk = 0,
		Count,
	}

	public enum WiimoteLEDs {
		None = 0,
		LED1 = (1 << 0),
		LED2 = (1 << 1),
		LED3 = (1 << 2),
		LED4 = (1 << 3),
	}

	public interface IInputType : IDisposable {
		string Name { get; }
		string Type { get; }
		bool IsDown { get; }
		bool IsInitialized { get; }
		void Initialize(IInputSimulator sim);
		void Reset();
	}

	public class WiimoteInputControl : IDisposable {

		private IInputSimulator sim;
		private IInputButton[] buttonInputs;
		private IInputButton[] overlayButtonInputs;
		private ModifierInput modifierInput;


		private IInputAnalog[] analogInputs;
		
		private Wiimote wm;

		public Wiimote Wiimote => wm;

		private WiimoteState WiimoteState => (wm?.WiimoteState ?? WiimoteState.EmptyState);
		
		public ExtensionType Extension => WiimoteState.ExtensionType;

		public bool IsConnected => (wm?.IsConnected ?? false);
		
		public WiimoteInputControl() {
#if DUMMY_INPUT
			sim = new DummyInputSimulator();
#else
			sim = new InputSimulator();
#endif
			buttonInputs = new IInputButton[(int) WiimoteButton.Count];
			overlayButtonInputs = new IInputButton[(int) WiimoteButton.Count];
			analogInputs = new IInputAnalog[(int) WiimoteAnalog.Count];

			WiimoteManager.Connected += OnWiimoteConnected;
			WiimoteManager.Discovered += OnWiimoteDiscovered;
			WiimoteManager.AutoDiscoveryCount = 1;
			WiimoteManager.UnpairOnDisconnect = true;
			WiimoteManager.AutoConnect = false;
			WiimoteManager.DolphinBarMode = true;
			WiimoteManager.StartDiscovery();
		}

		public WiimoteButton ModifierButton { get; set; }
		public ModifierInput ModifierInput {
			get => modifierInput;
			set {
				modifierInput?.Dispose();
				modifierInput = value;
				value?.Initialize(sim);
			}
		}

		private bool overlayEnabled = false;

		public bool OverlayEnabled {
			get => overlayEnabled;
			set {
				if (overlayEnabled != value) {
					overlayEnabled = value;
					foreach (var pair in AllButtons) {
						WiimoteButton button = pair.Key;
						IInputButton input = pair.Value;
						if (input != null) {
							input.Reset();
							if (GetButtonState(WiimoteState, button))
								input.DisabledUntilRelease = true;
						}
					}
				}
			}
		}

		private void OnWiimoteDiscovered(object sender, WiimoteDiscoveredEventArgs e) {
			e.AddDevice = true;
			e.KeepSearching = false;
		}

		private void OnWiimoteConnected(object sender, WiimoteEventArgs e) {
			if (!IsConnected) {
				wm = e.Wiimote;
				wm.Disconnected += OnWiimoteDisconnected;
				wm.StateChanged += OnWiimoteStateChanged;
				wm.SetReportType(ReportType.ButtonsAccelIR10Ext6, true);
			}
		}

		private void OnWiimoteDisconnected(object sender, WiimoteDisconnectedEventArgs e) {
			if (e.Wiimote == wm) {
				wm = null;
				// Reset input
				Reset();
			}
		}

		private void OnWiimoteStateChanged(object sender, WiimoteStateEventArgs e) {
			Update(e.WiimoteState);
		}

		/*public void Connect(long? macAddress = null) {
			try {
				if (!IsConnected) {
					if (macAddress.HasValue)
						wm.Connect(macAddress.Value);
					else
						wm.Connect();
					//wm.DisableMotionPlus();
					wm.EnableMotionPlus(MotionPlusExtensionType.Nunchuk);
					wm.SetReportType(InputReport.ButtonsAccelIR10Ext6, true);
					wm.SetLEDs(LEDs.LED1);
					statusTimer = new Timer(OnStatusCheck, null, 2000, 2000);
				}
			}
			catch (Exception ex) {
				Trace.WriteLine(ex.Message);
				CreateWiimote();
				throw;
			}
		}*/

		/*private void OnStatusCheck(object state) {
			if (wm.IsConnected) {
				try {
					//wm.GetStatus();
				}
				catch (WiimoteException ex) {
					Disconnect();
					WiimoteException?.Invoke(wm, new WiimoteExceptionEventArgs(ex));
				}
			}
		}*/

		/*private void CreateWiimote() {
			wm?.Dispose();
			wm = new Wiimote();
			wm.WiimoteException += OnWiimoteException;
			wm.ExtensionChanged += OnWiimoteExtensionChanged;
			wm.StateChanged += OnWiimoteChanged;
		}

		private void OnWiimoteExtensionChanged(object sender, WiimoteExtensionEventArgs e) {
			WiimoteExtensionChanged?.Invoke(sender, e);
		}

		private void OnWiimoteException(object sender, WiimoteExceptionEventArgs e) {
			//Trace.WriteLine(e.Exception.Message);
			Disconnect();
			//Reset();
			WiimoteException?.Invoke(sender, e);
		}*/

		/*public void Disconnect() {
			try {
				if (IsConnected) {
					try {
						wm.SetLEDs(LEDs.None);
						wm.SetRumble(false);
					}
					catch { }
					try {
						wm.Disconnect();
					}
					catch { }
					CreateWiimote();
					Reset();
					statusTimer?.Dispose();
					statusTimer = null;
				}
			}
			catch (Exception) {
				CreateWiimote();
				throw;
			}
		}*/

		public void Reset() {
			foreach (IInputButton input in buttonInputs) {
				input?.Reset();
			}
			foreach (IInputButton input in overlayButtonInputs) {
				input?.Reset();
			}
			foreach (IInputAnalog analog in analogInputs) {
				analog?.Reset();
			}
		}

		public IEnumerable<KeyValuePair<WiimoteButton, IInputButton>> AllButtons {
			get {
				for (WiimoteButton button = WiimoteButton.A; button < WiimoteButton.Count; button++) {
					yield return new KeyValuePair<WiimoteButton, IInputButton>(button, buttonInputs[(int) button]);
				}
				for (WiimoteButton button = WiimoteButton.A; button < WiimoteButton.Count; button++) {
					yield return new KeyValuePair<WiimoteButton, IInputButton>(button, overlayButtonInputs[(int) button]);
				}
			}
		}

		public IEnumerable<KeyValuePair<WiimoteButton, IInputButton>> Buttons {
			get {
				for (WiimoteButton button = WiimoteButton.A; button < WiimoteButton.Count; button++) {
					yield return new KeyValuePair<WiimoteButton, IInputButton>(button, buttonInputs[(int) button]);
				}
			}
		}

		public IEnumerable<KeyValuePair<WiimoteButton, IInputButton>> OverlayButtons {
			get {
				for (WiimoteButton button = WiimoteButton.A; button < WiimoteButton.Count; button++) {
					yield return new KeyValuePair<WiimoteButton, IInputButton>(button, overlayButtonInputs[(int) button]);
				}
			}
		}

		public IEnumerable<KeyValuePair<WiimoteAnalog, IInputAnalog>> Analogs {
			get {
				for (WiimoteAnalog analog = WiimoteAnalog.Nunchuk; analog < WiimoteAnalog.Count; analog++) {
					yield return new KeyValuePair<WiimoteAnalog, IInputAnalog>(analog, analogInputs[(int) analog]);
				}
			}
		}

		public IInputButton this[WiimoteButton button, bool overlay = false] {
			get => (overlay ? overlayButtonInputs : buttonInputs)[(int) button];
			set {
				IInputButton[] buttons = (overlay ? overlayButtonInputs : buttonInputs);
				buttons[(int) button]?.Dispose();
				buttons[(int) button] = value;
				value?.Initialize(sim);
			}
		}

		public IInputAnalog this[WiimoteAnalog analog] {
			get => analogInputs[(int) analog];
			set {
				analogInputs[(int) analog]?.Dispose();
				analogInputs[(int) analog] = value;
				value?.Initialize(sim);
			}
		}

		public void Update(WiimoteState ws) {
			if (UpdateOverlayEnableButton(ws, ModifierButton, modifierInput)) {
				OverlayEnabled = (modifierInput?.IsDown ?? false);
			}
			foreach (var pair in Buttons) {
				UpdateButton(ws, pair.Key, pair.Value);
			}
			foreach (var pair in OverlayButtons) {
				UpdateOverlayButton(ws, pair.Key, pair.Value);
			}
			foreach (var pair in Analogs) {
				UpdateAnalog(ws, pair.Key, pair.Value);
			}
		}

		private void UpdateAnalog(WiimoteState ws, WiimoteAnalog analog, IInputAnalog input) {
			if (input == null)
				return;
			PointF analogPosition = GetAnalogPosition(ws, analog);
			input.Update(analogPosition);
		}
		private bool UpdateOverlayEnableButton(WiimoteState ws, WiimoteButton button, IInputButton input) {
			if (input == null)
				return false;
			bool buttonState = GetButtonState(ws, button);
			return input.Update(buttonState);
		}
		private void UpdateButton(WiimoteState ws, WiimoteButton button, IInputButton input) {
			if (input == null)
				return;
			bool buttonState = (GetButtonState(ws, button) && !OverlayEnabled);
			input.Update(buttonState);
		}
		private void UpdateOverlayButton(WiimoteState ws, WiimoteButton button, IInputButton input) {
			if (input == null)
				return;
			bool buttonState = (GetButtonState(ws, button) && OverlayEnabled);
			if (input.Update(buttonState) && input.IsDown) {
				/*if (modifierInput != null) {
					modifierInput.Reset();
					modifierInput.DisabledUntilRelease = true;
					OverlayEnabled = false;
				}*/
			}
		}

		private PointF GetAnalogPosition(WiimoteState ws, WiimoteAnalog analog) {
			switch (analog) {
			case WiimoteAnalog.Nunchuk:
				//return PointF.Empty;
				return new PointF(ws.Nunchuk.Joystick.X, ws.Nunchuk.Joystick.Y);
			default:
				throw new ArgumentException("Invalid Analog!", nameof(analog));
			}
		}

		private bool GetButtonState(WiimoteState ws, WiimoteButton button) {
			switch (button) {
			case WiimoteButton.Invalid: return false;

			case WiimoteButton.A: return ws.Buttons.A;
			case WiimoteButton.B: return ws.Buttons.B;
			case WiimoteButton.One: return ws.Buttons.One;
			case WiimoteButton.Two: return ws.Buttons.Two;

			case WiimoteButton.Plus: return ws.Buttons.Plus;
			case WiimoteButton.Minus: return ws.Buttons.Minus;
			case WiimoteButton.Home: return ws.Buttons.Home;

			case WiimoteButton.Up: return ws.Buttons.Up;
			case WiimoteButton.Down: return ws.Buttons.Down;
			case WiimoteButton.Left: return ws.Buttons.Left;
			case WiimoteButton.Right: return ws.Buttons.Right;

			case WiimoteButton.C: return ws.Nunchuk.C;
			case WiimoteButton.Z: return ws.Nunchuk.Z;
			default: throw new ArgumentException("Invalid Button!", nameof(button));
			}
		}

		public void Dispose() {
			foreach (IInputButton input in buttonInputs) {
				input?.Dispose();
			}
			foreach (IInputAnalog analog in analogInputs) {
				analog?.Dispose();
			}
		}
	}

	/*public class AnalogMouse : IDisposable {

		public float DeadZone { get; set; } = 0.085f;
		public float MouseSpeed { get; set; } = 6.5f;
		public float Power { get; set; } = 3f;
		public bool Enabled { get; set; } = true;
		public bool IsMoving { get; private set; }
		private Stopwatch watch = new Stopwatch();
		private long lastMilliseconds;

		private PointF leftover;

		private Timer timer;

		public AnalogMouse() {
			leftover = new PointF();
			lastMilliseconds = 0;
			timer = new Timer(MouseRepeat, null, 16, 16);
		}

		public void Dispose() {
			timer.Dispose();
			timer = null;
		}
		private void MouseRepeat(object state) {

		}

		public void Initialize(InputSimulator sim) {

		}

		public void Update(InputSimulator sim, PointF stick) {
			float mag = (float) Math.Sqrt(stick.X * stick.X + stick.Y * stick.Y);
			if (Enabled && mag >= DeadZone) {
				float scale = MouseSpeed / 5f;
				if (IsMoving)
					scale *= Math.Min(1.5f, (watch.ElapsedMilliseconds - lastMilliseconds) / 16f);
				//Trace.WriteLine($"{(watch.ElapsedMilliseconds - lastMilliseconds)}ms {scale}");
				stick.X = scale * (float) Math.Pow(Math.Abs(stick.X) * 5f, Power) * Math.Sign(stick.X);
				stick.Y = scale * (float) Math.Pow(Math.Abs(stick.Y) * 5f, Power) * Math.Sign(stick.Y);
				//Trace.WriteLine(stick);
				stick.X += leftover.X;
				stick.Y += leftover.Y;
				int x = (int) Math.Truncate(stick.X);
				int y = (int) Math.Truncate(stick.Y);
				// Keep the leftover distance so we can move at the correct angle over time
				leftover.X = stick.X - x;
				leftover.Y = stick.Y - y;
				if (x != 0 || y != 0) {
					if (!IsMoving)
						watch.Restart();
					lastMilliseconds = watch.ElapsedMilliseconds;
					sim.Mouse.MoveMouseBy(x, -y); // Y axis is inverted
					IsMoving = true;
					return;
				}
			}
			if (IsMoving) {
				watch.Stop();
				lastMilliseconds = 0;
				IsMoving = false;
				leftover = new PointF();
			}
		}
	}*/

}

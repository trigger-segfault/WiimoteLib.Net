using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WiimoteLib;
using WiimoteLib.DataTypes;
using WiimoteLib.Events;

namespace WiimoteController.Controls {
	/// <summary>
	/// Interaction logic for WiimoteDisplay.xaml
	/// </summary>
	public partial class WiimoteDisplay : UserControl {
		
		private Wiimote wm;
		private DispatcherTimer timer;

		public bool IsConnected => wm?.IsConnected ?? false;

		public WiimoteDisplay() {
			InitializeComponent();
			timer = new DispatcherTimer(
				TimeSpan.FromSeconds(0.05),
				DispatcherPriority.Render,
				OnTick,
				Dispatcher);
			timer.Stop();
			//timer.Tick += OnTick;
		}

		private void OnTick(object sender, EventArgs e) {
			if (IsConnected) {
				UpdateWiimoteState(wm.WiimoteState);
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			Reset();
			WiimoteManager.Connected += OnWiimoteConnected;
		}

		private void Reset() {
			foreach (object child in gridLEDs.Children) {
				Image image = (Image) child;
				image.Visibility = Visibility.Hidden;
			}
			foreach (object child in gridGlows.Children) {
				Image image = (Image) child;
				image.Visibility = Visibility.Hidden;
			}
			if (!IsConnected) {
				gridContainer.Opacity = 0.4;
				imageNunchuk.Opacity = 1.0;
			}
			else {
				gridContainer.Opacity = 1.0;
			}
		}

		private void OnWiimoteDisconnected(object sender, WiimoteDisconnectedEventArgs e) {
			if (wm == e.Wiimote) {
				wm.Disconnected -= OnWiimoteDisconnected;
				wm = null;
				Dispatcher.Invoke(() => {
					Reset();
					timer.Stop();
				});
			}
		}

		private void OnWiimoteConnected(object sender, WiimoteEventArgs e) {
			if (!IsConnected) {
				wm = e.Wiimote;
				Dispatcher.Invoke(() => {
					gridContainer.Opacity = 1.0;
					timer.Start();
				});
				wm.Disconnected += OnWiimoteDisconnected;
				//wm.StateChanged += OnWiimoteStateChanged;
				//wm.ExtensionChanged += OnWiimoteExtensionChanged;
			}
		}

		/*private void OnWiimoteExtensionChanged(object sender, WiimoteExtensionEventArgs e) {
			
		}

		private void OnWiimoteStateChanged(object sender, WiimoteStateEventArgs e) {
			
		}*/

		private void UpdateWiimoteState(WiimoteState state) {
			UpdateButtonState(state.Buttons);
			UpdateNunchukState(state.Nunchuk, state.ExtensionType == ExtensionType.Nunchuk && state.Extension);
			UpdateStatusState(state.Status);
		}

		private void UpdateButtonState(ButtonState state) {
			SetGlowState("A", state.A);
			SetGlowState("B", state.B);
			SetGlowState("Home", state.Home);
			SetGlowState("Plus", state.Plus);
			SetGlowState("Minus", state.Minus);
			SetGlowState("DPad", state.Up || state.Down || state.Left || state.Right);
			SetGlowState("One", state.One);
			SetGlowState("Two", state.Two);
		}

		private void UpdateNunchukState(NunchukState state, bool connected) {
			if (connected)
				imageNunchuk.Opacity = 1.0;
			else
				imageNunchuk.Opacity = 0.4;
			SetGlowState("C", state.C && connected);
			SetGlowState("Z", state.Z && connected);
			SetGlowState("Stick", state.Joystick.Length >= 0.085f && connected);
		}

		private void UpdateStatusState(StatusState state) {
			SetLEDState(LEDs.LED1, state.LEDs);
			SetLEDState(LEDs.LED2, state.LEDs);
			SetLEDState(LEDs.LED3, state.LEDs);
			SetLEDState(LEDs.LED4, state.LEDs);
		}

		private void SetLEDState(LEDs led, LEDs leds) {
			SetLEDState(led, leds.HasFlag(led));
		}

		private void SetLEDState(LEDs led, bool state) {
			Image image = (Image) FindName("imageGlow" + led);
			if (state)
				image.Visibility = Visibility.Visible;
			else
				image.Visibility = Visibility.Hidden;
		}

		private void SetGlowState(string name, bool state) {
			Image image = (Image) FindName("imageGlow" + name);
			if (state)
				image.Visibility = Visibility.Visible;
			else
				image.Visibility = Visibility.Hidden;
		}
	}
}

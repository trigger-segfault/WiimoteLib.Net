using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WiimoteController.Input;
using WiimoteLib;
using WiimoteLib.OldEvents;
using FormsCursor = System.Windows.Forms.Cursor;

namespace WiimoteController {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		/*class DeviceState {
			public int Index;
		}*/

		WiimoteInputControl input;
		//BluetoothClient client;
		//BluetoothDeviceInfo deviceInfo;
		//WiimoteAutoPairer pairer;

		HwndSource hwndSource;

		OverlayWindow overlayWindow;

		public MainWindow() {
			InitializeComponent();
			overlayWindow = new OverlayWindow();
			//wm = new Wiimote();
			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;
			PreviewKeyDown += MainWindow_PreviewKeyDown;
			//input.WiimoteException += Input_WiimoteException;
			//pairer = new WiimoteAutoPairer();
			//pairer.DeviceAvailable += Pairer_DeviceAvailable;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
			WiimoteManager.Connected += WiimoteManager_Connected;
			WiimoteManager.Disconnected += WiimoteManager_Disconnected;

			timer = new DispatcherTimer(TimeSpan.FromSeconds(10), DispatcherPriority.ApplicationIdle, OnTimer, Dispatcher);
			timer.Stop();
		}

		private void WiimoteManager_Disconnected(object sender, WiimoteLib.Events.WiimoteDisconnectedEventArgs e) {
			UpdateBattery();
		}

		private void WiimoteManager_Connected(object sender, WiimoteLib.Events.WiimoteEventArgs e) {
			UpdateBattery();
		}

		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
			Console.WriteLine("UNHANDLED TASK: " + e.Exception.Message);
		}

		/*private void Pairer_DeviceAvailable(object sender, BluetoothDeviceInfo e) {
			
		}*/

		DispatcherTimer timer;
		//DispatcherTimer statusTimer;
		//BluetoothDeviceInfo deviceInfo;
		private void OnTimer(object sender, EventArgs e) {
			UpdateBattery();
		}

		/*private void Input_WiimoteException(object sender, WiimoteExceptionEventArgs e) {
			Trace.WriteLine(e.Exception.Message);
			Dispatcher.Invoke(() => {
				buttonConnect.Content = "Connect";
				pairer.Start();
				timer.Start();
			});
		}*/
		
		private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e) {
			//if (wm.IsConnected)
			//	wm.Disconnect();
			input.Dispose();
			overlayWindow.Close();
			//WiimoteManager.CleanupAsync();
		}

		internal enum DeviceChangeEvent : ushort {
			DevNodesChanged = 0x0007,
			QueryChangeConfig = 0x0017,
			ConfigChanged = 0x0018,
			ConfigChangeCanceled = 0x0019,

			Arrival = 0x8000,
			QueryRemove = 0x8001,
			QueryRemoveFailed = 0x8002,
			RemovePending = 0x8003,
			RemoveComplete = 0x8004,
			TypeSpecifiv = 0x8005,
			CustomEvent = 0x8006,

			UserDefined = 0xFFFF,
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient,
			ref DEV_BROADCAST_DEVICEINTERFACE NotificationFilter,
			int Flags);

		[StructLayout(LayoutKind.Sequential)]
		struct DEV_BROADCAST_HDR {
			public int size;
			public DeviceType deviceType;
			public int reserved;
		}

		enum DeviceType : int {
			OEM = 0x00000000,
			Volume = 0x00000002,
			Port = 0x00000003,
			DeviceInterface = 0x00000005,
			Handle = 0x00000006,
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct DEV_BROADCAST_DEVICEINTERFACE {
			public int size;
			public DeviceType deviceType;
			public int reserved;
			public Guid classguid;
			public char name;
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			//  do stuff
			const int WM_DEVICECHANGE = 0x0219;

			try {
				if (msg == WM_DEVICECHANGE) {
					var evt = (DeviceChangeEvent) wParam.ToInt32();
					Console.WriteLine($"WM_DEVICECHANGE: {evt}");
					switch (evt) {
					case DeviceChangeEvent.Arrival:
					case DeviceChangeEvent.RemovePending:
					case DeviceChangeEvent.RemoveComplete:
						TestStructure(lParam);
						break;
					}
				}
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
			}
			return IntPtr.Zero;
		}

		private void TestStructure(IntPtr lParam) {
			DEV_BROADCAST_HDR hdr = Marshal.PtrToStructure<DEV_BROADCAST_HDR>(lParam);
			if (hdr.deviceType == DeviceType.DeviceInterface) {
				Console.WriteLine("DBT_DEVTYP_DEVICEINTERFACE");
				DEV_BROADCAST_DEVICEINTERFACE device = Marshal.PtrToStructure<DEV_BROADCAST_DEVICEINTERFACE>(lParam);
				IntPtr offsetPtr = new IntPtr(lParam.ToInt32() + sizeof(int) * 3 + Marshal.SizeOf<Guid>());
				string name = Marshal.PtrToStringAuto(offsetPtr);
				Console.WriteLine(name);
			}
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			//timer.Start();
			//pairer.Start();
			//WiimoteManager.DisconnectTimeout = TimeSpan.FromSeconds(15);

			IntPtr handle = new WindowInteropHelper(this).Handle;
			hwndSource = HwndSource.FromHwnd(handle);
			hwndSource.AddHook(new HwndSourceHook(WndProc));

			DEV_BROADCAST_DEVICEINTERFACE device = new DEV_BROADCAST_DEVICEINTERFACE();
			device.deviceType = DeviceType.DeviceInterface;
			device.size = sizeof(int) * 3 + Marshal.SizeOf<Guid>() + 4;
			int flags = 0x4;
			IntPtr result = RegisterDeviceNotification(handle, ref device, flags);
			Console.WriteLine(result);
			input = new WiimoteInputControl();
			/*input[WiimoteButton.Home] = new KeyInput(Key.F12);
			input[WiimoteButton.One] = new MouseWheelInput(MouseWheels.Up);
			input[WiimoteButton.Two] = new MouseWheelInput(MouseWheels.Down);
			input[WiimoteButton.Plus] = new KeyInput(Key.LeftCtrl);
			input[WiimoteButton.Minus] = new KeyInput(Key.V);
			input[WiimoteButton.Up] = new KeyInput(Key.Up);
			input[WiimoteButton.Down] = new KeyInput(Key.Down);
			input[WiimoteButton.Left] = new KeyInput(Key.Left);
			input[WiimoteButton.Right] = new KeyInput(Key.Right);
			input[WiimoteButton.A] = new KeyInput(Key.Space);*/
			input.ModifierButton = WiimoteButton.B;
			input.ModifierInput = new ModifierInput();
			input[WiimoteButton.Up] = new KeyInput(Key.Up);
			input[WiimoteButton.Down] = new KeyInput(Key.Down, false);
			input[WiimoteButton.Left] = new KeyInput(Key.Left);
			input[WiimoteButton.Right] = new KeyInput(Key.Right);
			input[WiimoteButton.A] = new KeyInput(Key.Enter);
			input[WiimoteButton.Minus] = new KeyInput(Key.V);
			input[WiimoteButton.Plus] = new KeyInput(Key.A);
			input[WiimoteButton.Home] = new KeyInput(Key.F12);
			input[WiimoteButton.One] = new KeyInput(Key.LeftCtrl);
			input[WiimoteButton.Two] = new KeyInput(Key.Z);

			input[WiimoteButton.Up, true] = new OverlayInput(overlayWindow);
			input[WiimoteButton.Down, true] = new KeyInput(Key.R);
			input[WiimoteButton.Left, true] = new KeyInput(Key.Space);
			input[WiimoteButton.Right, true] = new KeyInput(Key.B);
			input[WiimoteButton.A, true] = new KeyInput(Key.Back);
			input[WiimoteButton.Minus, true] = new KeyInput(Key.S);
			input[WiimoteButton.Plus, true] = new KeyInput(Key.L);
			input[WiimoteButton.Home, true] = new KeyInput(Key.Escape);
			input[WiimoteButton.One, true] = new KeyInput(Key.Q);
			input[WiimoteButton.Two, true] = new KeyInput(Key.W);

			//input[WiimoteButton.B] = new KeyInput(Key.Escape);
			//input[WiimoteButton.B] = new SoundInput();
			input[WiimoteButton.Z] = new MouseButtonInput(MouseButton.Left);
			input[WiimoteButton.C] = new MouseButtonInput(MouseButton.Right);
			input[WiimoteAnalog.Nunchuk] = new AnalogMouse();
			UpdateBattery();
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			Trace.WriteLine("UNHANDLED: " + e.ExceptionObject);
		}
		private void OnConnect(object sender, RoutedEventArgs e) {
			/*try {
				OnConnect();
			}
			catch (Exception ex) {
				Trace.WriteLine("OnConnect: " + ex.Message);
			}*/
		}

		private void UpdateBattery() {
			Dispatcher.Invoke(() => {
				double rectRange = borderBattery.ActualWidth - 2;
				Wiimote wiimote = WiimoteManager.ConnectedWiimotes.FirstOrDefault();
				if (wiimote != null) {
					borderBattery.Opacity = 1.0;
					rectBattery.Width = rectRange * wiimote.WiimoteState.Status.Battery / 100d;
				}
				else {
					borderBattery.Opacity = 0.5;
					rectBattery.Width = 0;
				}
			});
		}

		private void OnClosed(object sender, EventArgs e) {
			WiimoteManager.Cleanup();
		}
		/*private void OnConnect() {
	if (!input.IsConnected) {
		var addresses = pairer.AvailableDevices;
		for (int i = 0; i < addresses.Length; i++) {
			try {
				input.Connect(addresses[i].ToInt64());
				buttonConnect.Content = "Disconnect";
				timer.Stop();
				pairer.Stop();
			}
			catch (Exception ex) {
				if (i + 1 == addresses.Length)
					throw ex;
			}
		}
	}
	else {
		input.Disconnect();
		buttonConnect.Content = "Connect";
		pairer.Start();
		timer.Start();
	}
}*/

		/*private float DeadZoneGyro(float value, float scale, float power) {
			if (Math.Abs(value) < deadZone)
				return 0;
			return Math.Sign(value) * (float) (scale * Math.Pow(Math.Abs(value), power));
		}

		private float GetGyroX(WiimoteState ws) {
			return -DeadZoneGyro(ws.MotionPlusState.Values.X, xScale, xPower);
		}



		//        static const int GYRO_NEUTRAL_Y = 7893;
		private float GetGyroY(WiimoteState ws) {
			return DeadZoneGyro(ws.MotionPlusState.Values.Y, 120f, 1.3f);
		}


		//        static const int GYRO_NEUTRAL_Z = 7825;
		private float GetGyroZ(WiimoteState ws) {
			return DeadZoneGyro(ws.MotionPlusState.Values.Z, zScale, zPower);
		}

		private void GyroWiimote(WiimoteState ws) {
			//MouseControl.MoveMouseRelative(GetAccelX(ws), GetAccelY(ws));
			MouseControl.MoveMouseRelative((int) GetGyroX(ws), (int) GetGyroZ(ws));
			//FormsCursor.Position = new System.Drawing.Point(FormsCursor.Position.X + GetAccelX(), FormsCursor.Position.Y + GetAccelY());
		}*/
	}
}

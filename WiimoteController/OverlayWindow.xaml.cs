using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WiimoteLib;

namespace WiimoteController {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class OverlayWindow : Window {
		
		public OverlayWindow() {
			InitializeComponent();
			Visibility = Visibility.Hidden;
			Left = 0;
			Top = 0;
			Width = SystemParameters.PrimaryScreenWidth;
			Height = SystemParameters.PrimaryScreenHeight;
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			UpdateBattery();
		}

		public new void Show() {
			if (IsLoaded)
				UpdateBattery();
			base.Show();
		}

		private void UpdateBattery() {
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
		}
	}
}

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

namespace WiimoteController {
	/// <summary>
	/// Interaction logic for OverlayOption.xaml
	/// </summary>
	public partial class OverlayOption : UserControl {
		public OverlayOption() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SelectedProperty =
			DependencyProperty.Register("Selected", typeof(bool), typeof(OverlayOption),
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty LabelContentProperty =
			DependencyProperty.Register("Content", typeof(object), typeof(OverlayOption),
				new FrameworkPropertyMetadata("Option"));

		public bool Selected {
			get => (bool) GetValue(SelectedProperty);
			set => SetValue(SelectedProperty, value);
		}
		public object LabelContent {
			get => (object) GetValue(LabelContentProperty);
			set => SetValue(LabelContentProperty, value);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using WiimoteLib;
using WiimoteLib.DataTypes;

namespace ReportInspector {
	public class WiimoteReportItem : TreeViewItem {

		public int Index { get; }
		public byte[] Data { get; }

		public byte[] WriteData { get; private set; }

		private StackPanel stackPanel;

		public OutputReport ReportType;

		public bool Rumble { get; }
		public bool Acknowledge { get; }
		public bool Enabled { get; }

		public WiimoteReportItem(int index, byte[] data) {
			Index = index;
			Data = data;
			ReportType = (OutputReport) Data[0];
			Rumble = (Data[1] & 0x1) != 0;
			Acknowledge = (Data[1] & 0x2) != 0;
			Enabled = (Data[1] & 0x4) != 0;
			BuildContents();
			//Header = $"{index}) {ReportType} : {BuildContents()}";
		}

		public byte[] GetSubData(int index, int length) {
			length = Math.Min(Data.Length - index, length);
			byte[] result = new byte[length];
			Array.Copy(Data, index, result, 0, length);
			return result;
		}

		public void BuildContents() {
			stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Vertical;
			string summary = "";
			uint address = (uint)((Data[1] << 24) | (Data[2] << 16) | (Data[3] << 8) | Data[4]);
			bool registerAccess = (Data[1] & 0x8) != 0;
			TextBlock textBlock = new TextBlock();
			switch (ReportType) {
			case OutputReport.Rumble:
				summary = (Rumble ? "On" : "Off");
				break;
			case OutputReport.LEDs:
				summary = "";
				for (int i = 0; i < 4; i++) {
					bool on = ((Data[1] & (1 << (4 + i))) != 0);
					if (on) {
						summary += $"{(i + 1)}";
						textBlock.Inlines.Add(new Run($"{(i + 1)}") {
							Foreground = new SolidColorBrush(Color.FromRgb(35, 176, 211)),
						});
					}
					else {
						summary += "*";
						textBlock.Inlines.Add(new Run("*") {
							Foreground = new SolidColorBrush(Color.FromRgb(77, 81, 84)),
						});
					}
				}
				AddRow("LEDs", textBlock);
				break;
			case OutputReport.InputReportType: {
					InputReport mode = (InputReport) Data[2];
					bool continuous = (Data[1] & 0x2) != 0;
					AddRow("Report Type", mode.ToString());
					AddRow("Continuous", continuous ? "Yes" : "No");
					summary = $"{mode} {(continuous ? "Continuous" : "")}";
				}
				break;
			case OutputReport.IRPixelClock:
			case OutputReport.IRLogic:
			case OutputReport.SpeakerEnable:
				summary = (Enabled ? "Enabled" : "Disabled");
				AddRow("Enabled", Enabled.ToString());
				break;
			case OutputReport.SpeakerMute:
				summary = (Enabled ? "Muted" : "Unmuted");
				AddRow("Muted", Enabled.ToString());
				break;
			case OutputReport.Status:
				break;
			case OutputReport.ReadMemory: {
					int size = Data[5] | (Data[6] << 8);
					AddRow("Addresss", "0x" + address.ToString("x8"));
					AddRow("Access Type", (Enabled || registerAccess ? "Control Registers" : "EEPROM"));
					AddRow("Size", size.ToString());
					summary = $"0x{address:x8} {size}";
				}
				break;
			case OutputReport.WriteMemory: {
					int size = Data[5];
					AddRow("Addresss", "0x" + address.ToString("x8"));
					AddRow("Access Type", (Enabled || registerAccess ? "Control Registers" : "EEPROM"));
					AddRow("Size", size.ToString());
					WriteData = GetSubData(6, size);
					AddRow("Data", WriteData);
					summary = $"0x{address:x8} {size}";
				}
				break;
			case OutputReport.SpeakerData: {
					int length = Data[1] >> 3;
					AddRow("Length", length.ToString());
					WriteData = GetSubData(2, length);
					AddRow("Data", WriteData);
					summary = $"{length} bytes";
				}
				break;
			default:
				throw new Exception($"Unknown report type: {((int) ReportType):x2}");
			}
			AddRow("Rumble", (Rumble ? "On" : "Off"));
			AddRow("Acknowledge", (Acknowledge ? "Yes" : "No"));
			if (Acknowledge) {
				if (!string.IsNullOrEmpty(summary))
					summary += " ";
				summary += "(Ackn)";
			}
			if (summary != null)
				Header = $"{Index}) {ReportType} : {summary}";
			Items.Add(stackPanel);
		}

		public void AddRow(string name, byte[] data) {
			TextBlock element = new TextBlock();
			element.Text = string.Join(" ", data.Select(b => b.ToString("x2")));
			element.Foreground = Brushes.Black;
			//element.FontFamily = new FontFamily("Lucida Console");
			AddRow(name, element);
		}
		public void AddRow(string name, string value, Color? color = null) {
			TextBlock element = new TextBlock();
			element.Text = value;
			element.Foreground = Brushes.Black;
			if (color.HasValue)
				element.Foreground = new SolidColorBrush(color.Value);
			AddRow(name, element);
		}
		public void AddRow(string name, FrameworkElement element) {
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(90, GridUnitType.Pixel),
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(1, GridUnitType.Star),
			});
			TextBlock label = new TextBlock();
			label.Text = $"{name}:";
			label.Foreground = Brushes.Black;
			//label.Width = 90;
			Grid.SetColumn(label, 0);
			grid.Children.Add(label);
			Grid.SetColumn(element, 1);
			grid.Children.Add(element);
			stackPanel.Children.Add(grid);
		}
	}
}

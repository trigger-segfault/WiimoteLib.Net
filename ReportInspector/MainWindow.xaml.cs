using System;
using System.Collections.Generic;
using System.IO;
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
using WiimoteLib;
using WiimoteLib.Events;
using WiimoteLib.Helpers;

namespace ReportInspector {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		public int ReportCount { get; }

		public Wiimote Wiimote { get; set; }
		public List<WiimoteReportItem> Reports { get; } = new List<WiimoteReportItem>();

		public MainWindow() {
			InitializeComponent();

			/*int sampleRate = 3000;
			string path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\Release\wav1.wav";
			string adpcmPath = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\Release\adpmc_out1.wav";
			string wavPath = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\Release\wav_out1.wav";

			ADPCMConverter.ConvertToValidWav(path, out path, ref sampleRate);
			ADPCMConverter.Wav2ADPCM(path, adpcmPath);
			ADPCMConverter.ADPCM2Wav(adpcmPath, wavPath);
			Close();
			return;*/

			//FIXME: UnpairOnDisconnect is broken on Windows 10, it'll cause
			//       more problems than solve them at this point in time.
			WiimoteManager.AutoConnect = true;
			WiimoteManager.AutoDiscoveryCount = 1;
			WiimoteManager.DolphinBarMode = true;
			WiimoteManager.BluetoothMode = true;
			WiimoteManager.Connected += OnWiimoteConnected;

			using (FileStream stream = File.OpenRead(@"C:\Users\Onii-chan\My Projects\Dolphin\Binary\x64\Wiimote - Copy.logdat")) {
				BinaryReader reader = new BinaryReader(stream);
				ReportCount = (int) stream.Length / 22;
				for (int i = 0; i < ReportCount; i++) {
					byte[] data = reader.ReadBytes(22);
					if (data[0] == 0) {
						ReportCount--;
						i--;
						continue;
					}
					var report = new WiimoteReportItem(i, data);
					Reports.Add(report);
					treeView.Items.Add(report);
				}
			}

			PreviewKeyDown += OnPreviewKeyDown;
			Closed += OnClosed;
		}

		private void OnClosed(object sender, EventArgs e) {
			WiimoteManager.Cleanup();
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.A) {
				int sampleRate = 3000;
				string path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\Release\wav1.wav";

				ADPCMConverter.ConvertToValidWav(path, out path, ref sampleRate);
				byte[] adpcm = ADPCMConverter.Wav2ADPCMData(path, out sampleRate);
				Wiimote?.EnableSpeaker(new SpeakerConfiguration(SpeakerFormat.ADPCM) {
					Volume = 0.693f,
					SampleRate = sampleRate,
					Unknown2 = 0x0c,
					Unknown3 = 0x0e,
				});
				Wiimote.PlaySound(adpcm);
			}
			if (e.Key == Key.S) {
				var reports = Reports.Where(r => r.ReportType == OutputReport.SpeakerData);
				byte[] data;
				using (MemoryStream stream = new MemoryStream()) {
					BinaryWriter writer = new BinaryWriter(stream);
					foreach (var report in reports) {
						writer.Write(report.WriteData);
					}
					data = stream.ToArray();
				}
				ADPCMConverter.ADPCM2Wav(data, 3000, "adpcm.wav");
				Close();
				return;
				/*Wiimote?.EnableSpeaker(new SpeakerConfiguration(SpeakerFormat.ADPCM) {
					Volume = 1f,
					SampleRate = 3000,
					Unknown2 = 0x0c,
					Unknown3 = 0x0e,
				});
				Wiimote?.PlaySound(data);
				return;*/
				/*PCMModifiers mods = new PCMModifiers {
					OctaveOffset = -1,
					SampleRate = 4000,
					Volume = 1f
				};
				RiffTags adpcm = new RiffTags(@"C:\Users\Onii-chan\My Projects\C#\WiimoteController\WiimoteController\Resources\Oracle_Secret9.wav");

				RiffTag fmt = adpcm["fmt "];
				byte[] b = BitConverter.GetBytes((short)2);
				//b.CopyTo(fmt.Data, 2);
				b = BitConverter.GetBytes(3000);
				//b.CopyTo(fmt.Data, 4);
				adpcm["fmt "] = fmt;
				adpcm["data"] = new RiffTag("data", data);
				adpcm.Save(@"C:\Users\Onii-chan\My Projects\C#\WiimoteController\WiimoteController\Resources\Oracle_Secret8.wav");
				string path = @"C:\Users\Onii-chan\Music\Midi Samples\Musical Scales\C Major Scale.mid";
				//byte[] midi = PCMGenerator.ConvertMidi(path, mods);
				path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\WiimoteController\Resources\Oracle_Secret9.wav";
				//path = @"C:\Users\Onii-chan\Music\iTunes\iTunes Media\Music\Chiptunes = WIN _m_♥_m_\Chiptunes = WIN_ Volume 2\08 Blue (Dj CUTMAN mix).mp3";
				//path = @"C:\Users\Onii-chan\Music\iTunes\iTunes Media\Music\Sabrepulse\Bit Pilot OST\04 Beauty In The Machine.mp3";
				//path = @"C:\Users\Onii-chan\Music\iTunes\iTunes Media\Music\Mikakuning!\Engaged to the Unidentified\Tomadoi -_ Recipe.mp3";
				//path = @"C:\Resources\Sounds\Japanese Theme\Startup.wav";
				//path = @"C:\Users\Onii-chan\Music\iTunes\iTunes Media\Music\MC Hammer\Greatest Hits\01 U Can't Touch This.m4a";
				path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\Release\adpcm1.wav";
				int sampleRate = 3000;
				try {
					byte[] wave = ADPCMReader.ReadADPCM(path, ref sampleRate);
					Wiimote.EnableSpeaker(new SpeakerConfiguration(SpeakerFormat.ADPCM) {
						Volume = 1f,
						SampleRate = sampleRate,
						Unknown2 = 0x0c,
						Unknown3 = 0x0e,
					});
					Console.WriteLine("Sample Rate: " + sampleRate);
					//Wiimote?.PlaySound(midi);
					Wiimote?.PlaySound(wave);
				}
				catch (Exception ex) {
					MessageBox.Show(ex.ToString());
				}*/
			}
		}

		private void OnWiimoteConnected(object sender, WiimoteEventArgs e) {
			if (Wiimote == null) {
				Wiimote = e.Wiimote;
				Wiimote.Disconnected += OnWiimoteDisconnected;
			}
		}

		private void OnWiimoteDisconnected(object sender, WiimoteDisconnectedEventArgs e) {
			Wiimote.Disconnected -= OnWiimoteDisconnected;
			Wiimote = null;
		}
	}
}

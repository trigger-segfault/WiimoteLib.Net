using Microsoft.Win32;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WiimoteLib;
using WiimoteLib.Helpers;
using System.IO;
using WiimoteLib.Events;
using System.Media;
using System.Windows.Media;
using System.Threading;

namespace WiimoteAudioPlayer {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		public Wiimote Wiimote => WiimoteManager.ConnectedWiimotes.FirstOrDefault();

		private MediaPlayer player = new MediaPlayer();

		private int sampleRate = 3000;
		private float volume = 1f;
		private bool maximize = true;

		private string waveFile;
		private string convertedWaveFile;
		private string finalWaveFile;
		private byte[] adpcm;

		private void OnLoadSound(object sender, RoutedEventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog {
				Title = "Open Sound File",
			};
			if (dialog.ShowDialog(this) ?? false)
				LoadSound(dialog.FileName);
		}

		private void LoadSound(string newWaveFile) {
			player.Close();
			Wiimote?.StopSound();
			waveform.Stop();
			Thread.Sleep(300);

			int newSampleRate = sampleRate;
			string newConvertedWaveFile;
			ADPCMConverter.ConvertToValidWav(newWaveFile, out newConvertedWaveFile, ref newSampleRate);
			if (UpdateWaveFile(newConvertedWaveFile, newSampleRate, volume, maximize, true)) {
				waveFile = newWaveFile;
				labelFile.Content = "File: " + Path.GetFileName(waveFile);
				buttonPlayWave.IsEnabled = true;
				buttonPlayWiimote.IsEnabled = WiimoteManager.ConnectedWiimotes.Any();
				buttonStop.IsEnabled = true;
				buttonUpdateSampleRate.IsEnabled = true;
			}
		}

		private void OnPlayReal(object sender, RoutedEventArgs e) {
			Wiimote?.StopSound();
			player.Stop();
			player.Play();
			waveform.Play();
		}

		private void OnPlayWiimote(object sender, RoutedEventArgs e) {
			player.Stop();
			Wiimote?.EnableSpeaker(new SpeakerConfiguration {
				Volume = volume,
				//Unknown2 = 0x0c,
				//Unknown3 = 0x0e,
				SampleRate = sampleRate,
			});
			Wiimote?.PlaySound(adpcm);
			waveform.Play();
		}

		private void UpdateBattery() {
			double rectRange = borderBattery.ActualWidth - 10;
			if (Wiimote != null) {
				borderBattery.Opacity = 1.0;
				rectBattery.Width = rectRange * Wiimote.WiimoteState.Status.Battery / 100d;
			}
		}

		private void OnSampleRateChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (IsLoaded && adpcm != null) {
				labelSampleRate.FontWeight = SampleRateFontWeight;
			}
		}

		private void OnDragEnter(object sender, DragEventArgs e) {
			labelDrop.Content = "Drop Sound Files Here";
			if (OwnedWindows.Count == 0 && e.Data.GetDataPresent(DataFormats.FileDrop)) {
				e.Effects = DragDropEffects.Copy;
				labelDrop.Visibility = Visibility.Visible;
			}
			else {
				e.Effects = DragDropEffects.None;
			}
		}

		private void OnDragLeave(object sender, DragEventArgs e) {
			labelDrop.Visibility = Visibility.Hidden;
		}

		private void OnDragOver(object sender, DragEventArgs e) {
			if (OwnedWindows.Count == 0 && e.Data.GetDataPresent(DataFormats.FileDrop)) {
				e.Effects = DragDropEffects.Copy;
			}
			else {
				e.Effects = DragDropEffects.None;
			}
		}

		private void OnDrop(object sender, DragEventArgs e) {
			if (OwnedWindows.Count == 0 && e.Data.GetDataPresent(DataFormats.FileDrop)) {
				labelDrop.Visibility = Visibility.Hidden;
				labelDrop.Content = "Converting Sound File...";
				string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
				if (files.Any()) {
					LoadSound(files.First());
				}
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			//FIXME: AutoConnect/Unpair is broken on Windows 10, it'll cause
			//       more problems than solve them at this point in time.
			WiimoteManager.AutoConnect = false;// true;
			WiimoteManager.DolphinBarMode = true;
			WiimoteManager.BluetoothMode = true;
			WiimoteManager.AutoDiscoveryCount = 1;
			WiimoteManager.Connected += OnWiimoteConnected;
			WiimoteManager.Disconnected += OnWiimoteDisconnected;
			UpdateBattery();
		}

		private void OnWiimoteConnected(object sender, WiimoteEventArgs e) {
			Dispatcher.Invoke(() => {
				buttonPlayWiimote.IsEnabled = adpcm != null;
				UpdateBattery();
			});
		}

		private void OnWiimoteDisconnected(object sender, WiimoteEventArgs e) {
			Dispatcher.Invoke(() => {
				buttonPlayWiimote.IsEnabled = false;
				UpdateBattery();
			});
		}

		private void OnClosed(object sender, EventArgs e) {
			player.Stop();
			Wiimote?.StopSound();
			waveform.Stop();
			WiimoteManager.Cleanup();
		}

		private void OnStop(object sender, RoutedEventArgs e) {
			player.Stop();
			Wiimote?.StopSound();
			waveform.Stop();
		}

		private void OnUpdateSettings(object sender, RoutedEventArgs e) {
			int newSampleRate = spinnerSampleRate.Value ?? sampleRate;
			float newVolume = spinnerVolume.Value ?? volume;
			bool newMaximize = checkBoxMaximize.IsChecked ?? maximize;
			UpdateWaveFile(newSampleRate, newVolume, newMaximize);
		}

		private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (IsLoaded && adpcm != null) {
				labelVolume.FontWeight = VolumeFontWeight;
			}
		}

		private void OnMaximizeVolumeChanged(object sender, RoutedEventArgs e) {
			if (IsLoaded && adpcm != null) {
				checkBoxMaximize.FontWeight = MaximizeFontWeight;
			}
		}

		private FontWeight VolumeFontWeight {
			get => ((spinnerSampleRate.Value ?? 0) == sampleRate ? FontWeights.Bold : FontWeights.Regular);
		}

		private FontWeight SampleRateFontWeight {
			get => ((spinnerVolume.Value ?? 0f) == volume ? FontWeights.Bold : FontWeights.Regular);
		}

		private FontWeight MaximizeFontWeight {
			get => ((checkBoxMaximize.IsChecked ?? true) == maximize ? FontWeights.Bold : FontWeights.Regular);
		}

		private bool UpdateWaveFile(int newSampleRate, float newVolume, bool newMaximize) {
			return UpdateWaveFile(convertedWaveFile, newSampleRate, newVolume, newMaximize);
		}

		private bool UpdateWaveFile(string newConvertedWaveFile, int newSampleRate, float newVolume, bool newMaximize, bool alreadyStopped = false) {
			try {
				if (!alreadyStopped) {
					player.Close();
					Wiimote?.StopSound();
					waveform.Stop();
					Thread.Sleep(300);
				}

				string newFinalWaveFile = newConvertedWaveFile;
				if (newMaximize)
					ADPCMConverter.MaximizeWavVolume(newConvertedWaveFile, out newFinalWaveFile);
				byte[] data = ADPCMConverter.Wav2ADPCMData(newFinalWaveFile, out newSampleRate);
				player.Open(new Uri(newFinalWaveFile));
				waveform.InitWave(newFinalWaveFile);
				adpcm = data;
				convertedWaveFile = newConvertedWaveFile;
				finalWaveFile = newFinalWaveFile;
				sampleRate = newSampleRate;
				volume = newVolume;
				maximize = newMaximize;
				labelSampleRate.FontWeight = SampleRateFontWeight;
				labelVolume.FontWeight = VolumeFontWeight;
				checkBoxMaximize.FontWeight = MaximizeFontWeight;
				return true;
			}
			catch (Exception ex) {
				MessageBox.Show(this, "Failed to load sound!\n" + ex.Message);
				return false;
			}
		}
	}
}

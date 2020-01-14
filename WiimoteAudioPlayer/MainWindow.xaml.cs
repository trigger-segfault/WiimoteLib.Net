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
		private int convertedSampleRate = 0;

		private SpeakerFormat speakerFormat = SpeakerFormat.PCM;
		private string waveFile;
		private string convertedWaveFile;
		private string finalWaveFile;
		private object soundObj;
		//private byte[] soundData;
		//private PrebufferedSound prebufferedSound;

		private void OnLoadSound(object sender, RoutedEventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog {
				Title = "Open Sound File",
			};
			if (dialog.ShowDialog(this) ?? false)
				LoadSound(dialog.FileName);
		}

		private void LoadSound(string newWaveFile) {
			/*player.Close();
			Wiimote?.StopSound();
			waveform.Stop();
			Thread.Sleep(300);*/

			//int newSampleRate = sampleRate;
			//string newConvertedWaveFile;
			//ADPCMConverter.ConvertToValidWav(newWaveFile, out newConvertedWaveFile, ref newSampleRate);
			//if (PrepareWaveFile(newConvertedWaveFile, newSampleRate, volume, maximize, true)) {
			if (PrepareWaveFile(newWaveFile, sampleRate, volume, maximize, speakerFormat, true)) {
				//waveFile = newWaveFile;
				labelFile.Content = "File: " + Path.GetFileName(newWaveFile);
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
			SpeakerConfiguration config = new SpeakerConfiguration {
				Volume = volume,
				//Unknown2 = 0x0c,
				//Unknown3 = 0x0e,
				SampleRate = sampleRate,
				Format = speakerFormat,
			};
			Wiimote?.DisableSpeaker();
			Wiimote?.EnableSpeaker(config);
			Thread.Sleep(100);
			Wiimote?.DisableSpeaker();
			Wiimote?.EnableSpeaker(config);
			//byte[] configData = Wiimote.ReadData(0x04a20001, 7);
			//if (soundObj is PrebufferedSound prebuffered)
			//	Wiimote?.PlaySound(prebuffered);
			//else
				Wiimote?.PlaySound((byte[]) soundObj);
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
			if (IsLoaded && soundObj != null) {
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
			speakerFormat = SpeakerFormat.ADPCM;
			maximize = true;
			checkBoxMaximize.IsChecked = maximize;
			if (speakerFormat == SpeakerFormat.ADPCM)
				sampleRate = 3000;
			else
				sampleRate = 2000;
			spinnerSampleRate.Value = sampleRate;
			CheckBoxSpeakerFormat = speakerFormat;
			//FIXME: UnpairOnDisconnect is broken on Windows 10, it'll cause
			//       more problems than solve them at this point in time.
			WiimoteManager.UnpairOnDisconnect = false;// true;
			WiimoteManager.PairOnDiscover = false;// true;
			WiimoteManager.AutoConnect = true;
			WiimoteManager.AutoDiscoveryCount = 1;
			WiimoteManager.DolphinBarMode = true;
			WiimoteManager.BluetoothMode = true;
			WiimoteManager.Connected += OnWiimoteConnected;
			WiimoteManager.Disconnected += OnWiimoteDisconnected;
			UpdateBattery();
		}

		private void OnWiimoteConnected(object sender, WiimoteEventArgs e) {
			Dispatcher.Invoke(() => {
				buttonPlayWiimote.IsEnabled = soundObj != null;
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
			SpeakerFormat newSpeakerFormat = CheckBoxSpeakerFormat;
			UpdateWaveFile(newSampleRate, newVolume, newMaximize, newSpeakerFormat);
		}

		private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (IsLoaded && soundObj != null) {
				labelVolume.FontWeight = VolumeFontWeight;
			}
		}

		private void OnMaximizeVolumeChanged(object sender, RoutedEventArgs e) {
			if (IsLoaded && soundObj != null) {
				checkBoxMaximize.FontWeight = MaximizeFontWeight;
			}
		}

		private void OnADPCMChanged(object sender, RoutedEventArgs e) {
			if (IsLoaded && soundObj != null) {
				checkBoxADPCM.FontWeight = ADPCMFontWeight;
			}
		}
		private SpeakerFormat CheckBoxSpeakerFormat {
			get => ((checkBoxADPCM.IsChecked??true) ? SpeakerFormat.ADPCM : SpeakerFormat.PCM);
			set => checkBoxADPCM.IsChecked = (value == SpeakerFormat.ADPCM);
		}

		private FontWeight SampleRateFontWeight {
			get => ((spinnerSampleRate.Value ?? 0) != sampleRate ? FontWeights.Bold : FontWeights.Regular);
		}

		private FontWeight VolumeFontWeight {
			get => ((spinnerVolume.Value ?? 0f) != volume ? FontWeights.Bold : FontWeights.Regular);
		}

		private FontWeight MaximizeFontWeight {
			get => ((checkBoxMaximize.IsChecked ?? true) != maximize ? FontWeights.Bold : FontWeights.Regular);
		}

		private FontWeight ADPCMFontWeight {
			get => (CheckBoxSpeakerFormat != speakerFormat ? FontWeights.Bold : FontWeights.Regular);
		}

		private bool ConvertWaveFile(string newWaveFile, out string newConvertedWaveFile, bool forceConvert, int newSampleRate) {
			if (forceConvert || newWaveFile != waveFile || newSampleRate != convertedSampleRate) {
				ADPCMConverter.ConvertToValidWav(newWaveFile, out newConvertedWaveFile, ref newSampleRate);
				waveFile = newWaveFile;
				convertedWaveFile = newConvertedWaveFile;
				convertedSampleRate = newSampleRate;
				//labelFile.Content = "File: " + Path.GetFileName(waveFile);
				//buttonPlayWave.IsEnabled = true;
				//buttonPlayWiimote.IsEnabled = WiimoteManager.ConnectedWiimotes.Any();
				//buttonStop.IsEnabled = true;
				//buttonUpdateSampleRate.IsEnabled = true;
				return true;
			}
			convertedWaveFile = waveFile;
			newConvertedWaveFile = waveFile;
			return false;
		}

		private bool UpdateWaveFile(int newSampleRate, float newVolume, bool newMaximize, SpeakerFormat newSpeakerFormat) {
			return PrepareWaveFile(waveFile, newSampleRate, newVolume, newMaximize, newSpeakerFormat, false);
		}

		//private bool UpdateWaveFile(string newConvertedWaveFile, int newSampleRate, float newVolume, bool newMaximize, bool shouldStop = false) {
		private bool PrepareWaveFile(string newWaveFile, int newSampleRate, float newVolume, bool newMaximize, SpeakerFormat newSpeakerFormat, bool forceConvert = true, bool stop = true) {
			if (stop) {
				player.Close();
				Wiimote?.StopSound();
				waveform.Stop();
				Thread.Sleep(300);
			}
			
			try {
				string newConvertedWaveFile;
				ConvertWaveFile(newWaveFile, out newConvertedWaveFile, /*forceConvert*/true, newSampleRate);
				string newFinalWaveFile = newConvertedWaveFile;
				if (newMaximize)
					ADPCMConverter.MaximizeWavVolume(newConvertedWaveFile, out newFinalWaveFile);
				object newSoundObj = null;
				byte[] data = null;
				if (newSpeakerFormat == SpeakerFormat.ADPCM) {
					//newSoundObj = WiiRemoteJAudioConverter.bufferADPCMSound(newFinalWaveFile);
					data = ADPCMConverter.Wav2ADPCMData(newFinalWaveFile, out newSampleRate);
				}
				else {
					data = ADPCMConverter.Wav2PCMs8Data(newFinalWaveFile, out newSampleRate);
					//throw new NotSupportedException("8-bit signed PCM not supported yet!");
					//newSoundObj = WiiRemoteJAudioConverter.bufferPCMs8Sound(newFinalWaveFile);
					//data = ADPCMConverter.Wav2PCMs8Data(newFinalWaveFile, out newSampleRate);
					//TODO: We probably dont need to previous step, it'll make things more complicated
				}
				player.Open(new Uri(newFinalWaveFile));
				waveform.InitWave(newFinalWaveFile);
				soundObj = newSoundObj ?? data;
				//soundObj = newSoundObj;
				//soundData = data;
				convertedWaveFile = newConvertedWaveFile;
				finalWaveFile = newFinalWaveFile;
				sampleRate = newSampleRate;
				volume = newVolume;
				maximize = newMaximize;
				speakerFormat = newSpeakerFormat;
				labelSampleRate.FontWeight = SampleRateFontWeight;
				labelVolume.FontWeight = VolumeFontWeight;
				checkBoxMaximize.FontWeight = MaximizeFontWeight;
				checkBoxADPCM.FontWeight = ADPCMFontWeight;
				return true;
			}
			catch (Exception ex) {
				MessageBox.Show(this, "Failed to load sound!\n" + ex.Message);
				return false;
			}
		}
	}
}

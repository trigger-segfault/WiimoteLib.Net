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
using System.Windows.Threading;
using WiimoteLib.Helpers;

namespace WiimoteAudioPlayer {
	/// <summary>
	/// Interaction logic for WaveformDisplay.xaml
	/// </summary>
	public partial class WaveformDisplay : UserControl {

		public string WaveFile { get; private set; }
		public short[] Samples { get; private set; }
		public TimeSpan Duration { get; private set; }

		private DispatcherTimer timer;
		private DateTime startTime;


		public WaveformDisplay() {
			InitializeComponent();

			timer = new DispatcherTimer(TimeSpan.FromMilliseconds(15), DispatcherPriority.Render, OnTick, Dispatcher);
			timer.Stop();
			//SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
		}

		public void OnTick(object sender, EventArgs e) {
			TimeSpan ellapsed = DateTime.UtcNow - startTime;
			if (ellapsed > Duration) {
				Stop();
			}
			else {
				double range = grid.ActualWidth - 2;
				double position = (ellapsed.TotalSeconds / Duration.TotalSeconds * range);
				bar.Margin = new Thickness(position, 0, 0, 0);
			}
		}

		public void InitWave(string waveFile) {
			RiffTags riff = new RiffTags(waveFile);
			if (riff.FileType != "WAVE")
				throw new Exception("Input is not a WAVE file!");

			WaveFmt fmt = WaveFmt.Read(riff["fmt "].Data);
			if (fmt.Format != 1)
				throw new Exception("Input is not PCM format!");
			if (fmt.Channels != 2)
				throw new Exception("Input does not have 2 channels!");
			if (fmt.BitsPerSample != 16)
				throw new Exception("Input does not have 16 bits per sample!");
			//if (fmt.SampleRate > 4000)
			//	throw new Exception("Sample rate must be 4000Hz or less!");

			byte[] data = riff["data"].Data;
			short[] samples = new short[data.Length / 2];
			Buffer.BlockCopy(data, 0, samples, 0, data.Length);
			
			renderer.Samples = samples;

			WaveFile = waveFile;
			Samples = samples;
			Duration = TimeSpan.FromSeconds(data.Length / ((fmt.BitsPerSample * fmt.Channels * fmt.SampleRate) / 8d));
		}

		public void Stop() {
			bar.Margin = new Thickness(0);
			timer.Stop();
		}

		public void Play() {
			Stop();
			if (WaveFile != null) {
				startTime = DateTime.UtcNow;
				timer.Start();
			}
		}
	}
}

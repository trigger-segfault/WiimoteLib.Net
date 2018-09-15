using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WiimoteAudioPlayer {
	public class WaveformRenderer : Control {


		public WaveformRenderer() {
			CacheMode = new BitmapCache();
		}

		//private bool updated = true;
		//private BitmapCacheBrush bitmapCache = new BitmapCacheBrush();

		private static readonly Color WaveformColor = Color.FromRgb(58, 81, 87);
		private static readonly SolidColorBrush WaveformBrush = new SolidColorBrush(WaveformColor);

		private static readonly Color HorizonColor = Color.FromRgb(131, 149, 154);
		private static readonly SolidColorBrush HorizonBrush = new SolidColorBrush(HorizonColor);

		/*private int channels = 1;
		public int Channels {
			get => channels;
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(Channels));
				channels = value;
				InvalidateVisual();
			}
		}*/

		private short[] samples;
		public short[] Samples {
			get => samples;
			set {
				samples = value;
				InvalidateVisual();
			}
		}

		private int renderCount = 0;
		protected override void OnRender(DrawingContext d) {
			/*d.PushGuidelineSet(new GuidelineSet {
				GuidelinesX = new DoubleCollection(new[] { 0.5, ActualWidth + 0.5 }),
				GuidelinesY = new DoubleCollection(new[] { 0.5, ActualHeight + 0.5 }),
			});*/
			Console.WriteLine("Render: " + renderCount++);
			int center = (int) (ActualHeight / 2);
			int height = center * 2 - 2;
			int halfHeight = center - 1;
			
			if (samples != null && samples.Length > 0) {
				int width = (int) ActualWidth;
				double ratio = samples.Length / width;
				double next = ratio;

				int i = 0;
				int pos = 0;
				while (i < samples.Length) {
					short min = 0;
					short max = 0;
					bool any = false;
					for (; i < samples.Length && i <= (int) next; i++) {
						any = true;
						min = Math.Min(min, samples[i]);
						max = Math.Max(max, samples[i]);
					}
					if (any) {
						int top = center - (max * halfHeight) / short.MaxValue;
						int bottom = center + (min * halfHeight) / short.MinValue;
						int total = bottom - top;
						d.DrawRectangle(WaveformBrush, new Pen(WaveformBrush, 1), new Rect(pos, top, 0, total));
					}
					pos++;
					next += ratio;
				}
			}
			
			d.DrawRectangle(HorizonBrush, new Pen(HorizonBrush, 1), new Rect(0, center, ActualWidth, 0));
			//d.Pop();
		}
	}
}

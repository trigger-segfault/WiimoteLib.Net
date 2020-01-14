using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {
	[Flags]
	public enum PCMFlags {
		None = 0,
		S8 = 0,
		S16LE = (1 << 0),
		S16BE = (1 << 1),
		//S16 = S16LE|S16BE,
		Stereo = 0,
		MonoL = (1 << 2),
		MonoR = (1 << 3),
		MonoLR = MonoL|MonoR,
		//Mono = MonoL|MonoR,
		Interleave = 0,
		Deinterleave = (1 << 4),

	}
	/// <summary>
	/// https://github.com/losinggeneration/kos/blob/master/utils/wav2adpcm/wav2adpcm.c
	/// </summary>
	public static class ADPCMConverter {

		private static readonly int[] DiffLookup = {
			1,3,5,7,9,11,13,15,
			-1,-3,-5,-7,-9,-11,-13,-15,
		};

		private static readonly int[] IndexScale = {
			0x0e6, 0x0e6, 0x0e6, 0x0e6, 0x133, 0x199, 0x200, 0x266,
			0x0e6, 0x0e6, 0x0e6, 0x0e6, 0x133, 0x199, 0x200, 0x266 //same value for speedup
		};

		private static int Clamp(int val, int min, int max) {
			return (val < min ? min : (val > max ? max : val));
		}
		private struct ADPCMState {
			public int predictor;
			public int step;
		};

		private static readonly int[] YamahaDiffLookup = {
			 1,  3,  5,  7,  9,  11,  13,  15,
			-1, -3, -5, -7, -9, -11, -13, -15
		};

		private static readonly int[] YamahaIndexScale = {
			230, 230, 230, 230, 307, 409, 512, 614,
			230, 230, 230, 230, 307, 409, 512, 614
		};

		private static short AVClip16(int a) {
			if (((a + short.MaxValue + 1) & ~ushort.MaxValue) != 0)
				return unchecked((short) ((a >> 31) ^ short.MaxValue));
			else
				return unchecked((short) a);
		}

		private static int AVClip(int a, int amin, int amax) {
			return (a < amin ? amin : (a > amax ? amax : a));
		}

		private static short ADPCMYamahaExpandNibble(ref ADPCMState s, byte nibble) {
			s.predictor += (s.step * YamahaDiffLookup[nibble]) / 8;
			s.predictor = AVClip16(s.predictor);
			s.step = (s.step * YamahaIndexScale[nibble]) >> 8;
			s.step = AVClip(s.step, 0x7F, 0x6000);
			return unchecked((short) s.predictor);
		}

		private static byte ADPCMYamahaCompressSample(ref ADPCMState s, short sample) {
			byte nibble;
			int delta;

			if (s.step == 0) {
				s.predictor = 0;
				s.step = 0x7F;
			}

			delta = sample - s.predictor;

			nibble = (byte) (Math.Min(7, Math.Abs(delta) * 4 / s.step) + (delta < 0 ? 8 : 0));

			s.predictor += (s.step * YamahaDiffLookup[nibble]) / 8;
			s.predictor = AVClip16(s.predictor);
			s.step = (s.step * YamahaIndexScale[nibble]) >> 8;
			s.step = AVClip(s.step, 0x7F, 0x6000);

			return nibble;
		}

		private static void ADPCM2PCM2(short[] dst, int dstIndex, byte[] src, int srcIndex) {
			ADPCMState state = new ADPCMState {
				predictor = 0,
				step = 0x7F,
			};
			// 4 bit Yamaha ADPCM (same as dreamcast)
			for (int i = 0; i < src.Length; i++) {
				dst[i * 2] = ADPCMYamahaExpandNibble(ref state, (byte) ((src[i] >> 4) & 0xf));
				dst[i * 2 + 1] = ADPCMYamahaExpandNibble(ref state, (byte) (src[i] & 0xf));
			}
		}

		private static void PCM2ADPCM2(byte[] dst, int dstIndex, short[] src, int srcIndex) {
			ADPCMState state = new ADPCMState {
				predictor = 0,
				step = 0x7F,
			};
			int nibble;
			// 4 bit Yamaha ADPCM (same as dreamcast)
			for (int i = 0; i < dst.Length; i++) {
				nibble = ADPCMYamahaCompressSample(ref state, src[i * 2 + 0]);
				nibble |= ADPCMYamahaCompressSample(ref state, src[i * 2 + 1]) << 4;
				dst[i] = (byte) nibble;
			}
		}

		public static void PCM2ADPCM(byte[] dst, int dstIndex, short[] src, int srcIndex, int length) {
			int signal = 0;
			int step = 0x7f;

			// length/=4;
			length = (length + 3) / 4;
			do {
				int data, val, diff;

				/* hign nibble */
				diff = src[srcIndex++] - signal;
				diff = (diff * 8) / step;

				val = Math.Abs(diff) / 2;
				if (val > 7)
					val = 7;
				if (diff < 0)
					val += 8;

				signal += (step * DiffLookup[val]) / 8;
				signal = Clamp(signal, short.MinValue, short.MaxValue);

				step = (step * IndexScale[val]) >> 8;
				step = Clamp(step, 0x7f, 0x6000);

				data = val;

				/* low nibble */
				diff = src[srcIndex++] - signal;
				diff = (diff * 8) / step;

				val = (Math.Abs(diff)) / 2;
				if (val > 7)
					val = 7;
				if (diff < 0)
					val += 8;

				signal += (step * DiffLookup[val]) / 8;
				signal = Clamp(signal, short.MinValue, short.MaxValue);

				step = (step * IndexScale[val]) >> 8;
				step = Clamp(step, 0x7f, 0x6000);

				data |= val << 4;

				dst[dstIndex++] = (byte) data;

			}
			while (--length != 0);
		}

		public static void ADPCM2PCM(short[] dst, int dstIndex, byte[] src, int srcIndex, int length) {
			int signal = 0; //int j = 0;
			int step = 0x7f; //int k = 127;

			do {
				int data, val;

				data = src[srcIndex++];

				/* low nibble */
				val = data & 15;

				signal += (step* DiffLookup[val]) / 8;
				signal = Clamp(signal, short.MinValue, short.MaxValue);

				step = (step* IndexScale[val & 7]) >> 8;
				step = Clamp(step, 0x7f, 0x6000);

				dst[dstIndex++] = (short) signal;

				/* high nibble */
				val = (data >> 4) & 15;

				signal += (step* DiffLookup[val]) / 8;
				signal = Clamp(signal, short.MinValue, short.MaxValue);

				step = (step* IndexScale[val & 7]) >> 8;
				step = Clamp(step, 0x7f, 0x6000);

				dst[dstIndex++] = (short) signal;
			}
			while (--length != 0);
		}

		public static void Deinterleave<T>(T[] buffer) {
			T[] output = new T[buffer.Length];

			// Half length
			int h = buffer.Length / 2;
			for (int i = 0, j = 0; j < buffer.Length; i++, j+=2) {
				output[i + 0] = buffer[j + 0];
				output[i + h] = buffer[j + 1];
			}

			Array.Copy(output, buffer, buffer.Length);
		}

		public static void Interleave<T>(T[] buffer) {
			T[] output = new T[buffer.Length];

			// Half length
			int h = buffer.Length / 2;
			for (int i = 0, j = 0; i < buffer.Length; i+=2, j++) {
				output[i + 0] = buffer[j + 0];
				output[i + 1] = buffer[j + h];
			}

			Array.Copy(output, buffer, buffer.Length);
		}

		//FIXME: Quick fix for dumb FFmpeg hardcoded location.
		//       For now it must either be in the Windows %PATH%, or your program directory.
		//That's awful, absolutely awful.
		const string FFMpeg = "ffmpeg.exe";

		public static bool MaximizeWavVolume(string inputFile, out string outputFile) {
			RiffTags riff = new RiffTags(inputFile);
			// Remove unnecissary tags
			foreach (string key in riff.Keys.ToArray()) {
				if (key != "fmt " && key != "data")
					riff.Remove(key);
			}

			if (riff.FileType != "WAVE")
				throw new Exception("Input is not a WAVE file!");

			WaveFmt fmt = WaveFmt.Read(riff["fmt "].Data);
			if (fmt.Format != 1)
				throw new Exception("Input is not PCM format!");
			if (fmt.Channels != 2)
				throw new Exception("Input does not have 2 channels!");
			if (fmt.BitsPerSample != 16)
				throw new Exception("Input does not have 16 bits per sample!");
			if (fmt.SampleRate > 5000)
				throw new Exception("Sample rate must be 5000Hz or less!");

			byte[] data = riff["data"].Data;
			short[] wavSamples = new short[data.Length / 2];
			Buffer.BlockCopy(data, 0, wavSamples, 0, data.Length);

			short max = 0;
			short min = 0;
			for (int i = 0; i < wavSamples.Length; i++) {
				max = Math.Max(max, wavSamples[i]);
				min = Math.Min(min, wavSamples[i]);
			}

			// Silent File
			if (min == 0 && max == 0) {
				outputFile = inputFile;
				return false;
			}

			// Lazy maximum volume checking
			const short range = 32;
			if (min <= (short.MinValue + range) || max >= (short.MaxValue - range)) {
				outputFile = inputFile;
				return false;
			}

			double maxScale = (double) short.MaxValue / max;
			double minScale = (double) short.MinValue / min;
			double scale = Math.Min(maxScale, minScale);

			for (int i = 0; i < wavSamples.Length; i++) {
				wavSamples[i] = (short) (wavSamples[i] * scale);
			}

			byte[] wavData = new byte[wavSamples.Length * sizeof(short)];
			Buffer.BlockCopy(wavSamples, 0, wavData, 0, wavData.Length);

			outputFile = Path.Combine(Directory.GetCurrentDirectory(), "wiimote.maximized.wav");
			riff["data"] = new RiffTag("data", wavData);
			riff.Save(outputFile);
			return false;
		}

		public static bool ConvertToValidWav(string inputFile, out string outputFile, ref int sampleRate) {
			try {
				RiffTags riff = new RiffTags(inputFile);
				// Remove unnecissary tags
				foreach (string key in riff.Keys.ToArray()) {
					if (key != "fmt " && key != "data")
						riff.Remove(key);
				}

				if (riff.FileType != "WAVE")
					throw new Exception("Input is not a WAVE file!");

				WaveFmt fmt = WaveFmt.Read(riff["fmt "].Data);
				if (fmt.Format != 1)
					throw new Exception("Input is not PCM format!");
				if (fmt.Channels != 2)
					throw new Exception("Input does not have 2 channels!");
				if (fmt.BitsPerSample != 16)
					throw new Exception("Input does not have 16 bits per sample!");
				if (fmt.SampleRate > 5000)
					throw new Exception("Sample rate must be 5000Hz or less!");
					
				sampleRate = fmt.SampleRate;
				outputFile = inputFile;
				return false;
			}
			catch (Exception) {
				outputFile = Path.Combine(Directory.GetCurrentDirectory(), "wiimote.converted.wav");
				ProcessStartInfo startInfo = new ProcessStartInfo {
					FileName = FFMpeg,
					Arguments = $"-i \"{inputFile}\" -y -acodec pcm_s16le -ar {sampleRate} -ac 2 \"{outputFile}\"",
					//RedirectStandardOutput = true,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					WorkingDirectory = Directory.GetCurrentDirectory(),
				};
				Process process = Process.Start(startInfo);
				process.WaitForExit();
				if (process.ExitCode != 0)
					throw new Exception($"FFMpeg exited with {process.ExitCode}");
				return true;
			}
		}

		public static void ADPCM2Wav(byte[] data, int sampleRate, string outputFile) {
			using (FileStream output = File.OpenWrite(outputFile)) {
				output.SetLength(0);
				ADPCM2Wav(data, sampleRate, output);
			}
		}
		public static void ADPCM2Wav(byte[] data, int sampleRate, Stream output) {
			RiffTags riff = new RiffTags() {
				FileType = "WAVE",
			};
			
			byte[] adpcm = new byte[data.Length];
			short[] wavSamples = new short[data.Length * 2];
			Buffer.BlockCopy(data, 0, adpcm, 0, data.Length);

			ADPCM2PCM2(wavSamples, 0, adpcm, 0);
			/*Deinterleave(adpcm);
			ADPCM2PCM(wavSamples, 0, adpcm, 0, adpcm.Length / 2);
			ADPCM2PCM(wavSamples, wavSamples.Length / 2, adpcm, adpcm.Length / 2, adpcm.Length / 2);
			Interleave(wavSamples);*/

			byte[] wavData = new byte[wavSamples.Length * sizeof(short)];
			Buffer.BlockCopy(wavSamples, 0, wavData, 0, wavData.Length);

			WaveFmt fmt = new WaveFmt {
				Format = 1,
				Channels = 2,
				SampleRate = sampleRate * 1,
				BitsPerSample = 16,
			};
			fmt.UpdateValues();
			riff["fmt "] = new RiffTag("fmt ", fmt.ToBytes());
			riff["data"] = new RiffTag("data", wavData);
			riff.Save(output);
		}

		public static byte[] Wav2ADPCMData(string inputFile, out int sampleRate) {
			using (FileStream input = File.OpenRead(inputFile))
				return Wav2ADPCMData(input, out sampleRate);
		}

		public static byte[] Wav2ADPCMData(Stream input, out int sampleRate) {
			RiffTags riff = new RiffTags(input);
			// Remove unnecissary tags
			foreach (string key in riff.Keys.ToArray()) {
				if (key != "fmt " && key != "data")
					riff.Remove(key);
			}

			if (riff.FileType != "WAVE")
				throw new Exception("Input is not a WAVE file!");

			WaveFmt fmt = WaveFmt.Read(riff["fmt "].Data);
			if (fmt.Format != 1)
				throw new Exception("Input is not PCM format!");
			if (fmt.Channels != 2)
				throw new Exception("Input does not have 2 channels!");
			if (fmt.BitsPerSample != 16)
				throw new Exception("Input does not have 16 bits per sample!");

			byte[] data = riff["data"].Data;
			short[] wavSamples = new short[data.Length / 2];
			byte[] adpcm = new byte[data.Length / 4];
			Buffer.BlockCopy(data, 0, wavSamples, 0, data.Length);

			/*Deinterleave(wavSamples);
			PCM2ADPCM(adpcm, 0, wavSamples, 0, adpcm.Length / 2);
			PCM2ADPCM(adpcm, adpcm.Length / 2, wavSamples, wavSamples.Length / 2, adpcm.Length / 2);*/
			PCM2ADPCM2(adpcm, 0, wavSamples, 0);

			sampleRate = fmt.SampleRate;
			return adpcm;
		}

		public static byte[] Wav2PCMs8Data(string inputFile, out int sampleRate) {
			using (FileStream input = File.OpenRead(inputFile))
				return Wav2PCMs8Data(input, out sampleRate);
		}
		public const PCMFlags pf =	PCMFlags.S8 |
									PCMFlags.Stereo |
									PCMFlags.Interleave;

		public static byte[] Wav2PCMs8Data(Stream input, out int sampleRate) {
			RiffTags riff = new RiffTags(input);
			// Remove unnecissary tags
			foreach (string key in riff.Keys.ToArray()) {
				if (key != "fmt " && key != "data")
					riff.Remove(key);
			}

			if (riff.FileType != "WAVE")
				throw new Exception("Input is not a WAVE file!");

			WaveFmt fmt = WaveFmt.Read(riff["fmt "].Data);
			if (fmt.Format != 1)
				throw new Exception("Input is not PCM format!");
			if (fmt.Channels != 2)
				throw new Exception("Input does not have 2 channels!");
			if (fmt.BitsPerSample != 16)
				throw new Exception("Input does not have 16 bits per sample!");

			byte[] data = riff["data"].Data;
			short[] wavSamples = new short[data.Length / 2];
			Buffer.BlockCopy(data, 0, wavSamples, 0, data.Length);

			int pcmLength = data.Length / 2;
			if (pf.HasFlag(PCMFlags.S16LE) || pf.HasFlag(PCMFlags.S16BE)) pcmLength *= 2;
			if (pf.HasFlag(PCMFlags.MonoL) || pf.HasFlag(PCMFlags.MonoR)) pcmLength /= 2;
			byte[] pcm = new byte[pcmLength];

			if (pf.HasFlag(PCMFlags.Deinterleave)) {
				Deinterleave(wavSamples);
			}

			for (int w = 0, p = 0; w < wavSamples.Length; w+=2, p+=2) {
				short sample = 0;
				if (pf.HasFlag(PCMFlags.MonoLR)) {
					sample = (short) ((wavSamples[w+0] + wavSamples[w+1]) / 2);
				}
				else if (pf.HasFlag(PCMFlags.MonoR)) {
					sample = wavSamples[w+1];
				}
				else if (pf.HasFlag(PCMFlags.MonoL)) {
					sample = wavSamples[w+0];
				}
				else {
					sample = wavSamples[w];
					w--;
				}
				
				if (pf.HasFlag(PCMFlags.S16LE)) {
					//Signed 16-bit LITTLE ENDIAN:
					pcm[p+0] = unchecked((byte) (sample & 0xFF));
					pcm[p+1] = unchecked((byte) ((sample >> 8) & 0xFF));
				}
				else if (pf.HasFlag(PCMFlags.S16BE)) {
					//Signed 16-bit BIG ENDIAN:
					pcm[p+0] = unchecked((byte) ((sample >> 8) & 0xFF));
					pcm[p+1] = unchecked((byte) (sample & 0xFF));
				}
				else {
					//Signed 8-bit:
					pcm[p] = unchecked((byte) ((sample >> 8) & 0xFF));
					p--;
				}
			}



			/*int outSampleCount = wavSamples
			//byte[] pcm = new byte[data.Length / 2];//;
			bool? s16e = true;// null;
			//s16e (stereo)
			if (s16e.HasValue)
				pcm = new byte[data.Length];
			//pcm = new byte[data.Length];
			//s16e = true;
			//s8 (stereo)
			//pcm = new byte[data.Length / 2];
			//byte[] pcm = new byte[data.Length / 4];
			Buffer.BlockCopy(data, 0, wavSamples, 0, data.Length);

			//Deinterleave(wavSamples);
			//PCM2ADPCM(adpcm, 0, wavSamples, 0, adpcm.Length / 2);
			//PCM2ADPCM(adpcm, adpcm.Length / 2, wavSamples, wavSamples.Length / 2, adpcm.Length / 2);
			PCM2PCMs82(pcm, 0, wavSamples, 0, s16e);*/

			sampleRate = fmt.SampleRate;
			return pcm;
		}

		private static void PCM2PCMs82(byte[] dst, int dstIndex, short[] src, int srcIndex, bool? s16e = null) {
			if (!s16e.HasValue) {
				for (int i = 0; i < src.Length; i++) {
					//BIG ENDIAN:
					//dst[i * 2 + 0] = (byte) ((src[i] >> 8) & 0xFF);
					//dst[i * 2 + 1] = (byte) (src[i] & 0xFF);
					//LITTLE ENDIAN:
					//Signed 8-bit PCM (stereo)
					dst[i] = unchecked((byte) ((src[i] >> 8) & 0xFF));
					//dst[i * 2 + 0] = (byte) (src[i] & 0xFF);
					//dst[i * 2 + 1] = (byte) ((src[i] >> 8) & 0xFF);
				}
			}
			else if (s16e.Value) {
				for (int i = 0; i < src.Length; i++) {
					//BIG ENDIAN:
					//dst[i * 2 + 0] = (byte) ((src[i] >> 8) & 0xFF);
					//dst[i * 2 + 1] = (byte) (src[i] & 0xFF);
					//LITTLE ENDIAN:
					dst[i * 2 + 0] = (byte) (src[i] & 0xFF);
					dst[i * 2 + 1] = (byte) ((src[i] >> 8) & 0xFF);
				}
			}
			else {
				for (int i = 0; i < src.Length; i++) {
					//BIG ENDIAN:
					dst[i * 2 + 0] = (byte) ((src[i] >> 8) & 0xFF);
					dst[i * 2 + 1] = (byte) (src[i] & 0xFF);
					//LITTLE ENDIAN:
					//dst[i * 2 + 0] = (byte) (src[i] & 0xFF);
					//dst[i * 2 + 1] = (byte) ((src[i] >> 8) & 0xFF);
				}
			}
		}

		public static void Wav2ADPCM(string inputFile, string outputFile) {
			using (FileStream input = File.OpenRead(inputFile))
			using (FileStream output = File.OpenWrite(outputFile)) {
				output.SetLength(0);
				Wav2ADPCM(input, output);
			}
		}

		public static void Wav2ADPCM(Stream input, Stream output) {
			RiffTags riff = new RiffTags(input);
			// Remove unnecissary tags
			foreach (string key in riff.Keys.ToArray()) {
				if (key != "fmt " && key != "data")
					riff.Remove(key);
			}

			if (riff.FileType != "WAVE")
				throw new Exception("Input is not a WAVE file!");

			WaveFmt fmt = WaveFmt.Read(riff["fmt "].Data);
			if (fmt.Format != 1)
				throw new Exception("Input is not PCM format!");
			if (fmt.Channels != 2)
				throw new Exception("Input does not have 2 channels!");
			if (fmt.BitsPerSample != 16)
				throw new Exception("Input does not have 16 bits per sample!");

			byte[] data = riff["data"].Data;
			short[] wavSamples = new short[data.Length / 2];
			byte[] adpcm = new byte[data.Length / 4];
			Buffer.BlockCopy(data, 0, wavSamples, 0, data.Length);

			/*Deinterleave(wavSamples);
			PCM2ADPCM(adpcm, 0, wavSamples, 0, adpcm.Length / 2);
			PCM2ADPCM(adpcm, adpcm.Length / 2, wavSamples, wavSamples.Length / 2, adpcm.Length / 2);*/
			PCM2ADPCM2(adpcm, 0, wavSamples, 0);

			fmt.Format = 20;
			fmt.BitsPerSample = 4;
			fmt.UpdateValues();
			riff["fmt "] = new RiffTag("fmt ", fmt.ToBytes());
			riff["data"] = new RiffTag("data", adpcm);
			riff.Save(output);
		}

		public static void ADPCM2Wav(string inputFile, string outputFile) {
			using (FileStream input = File.OpenRead(inputFile))
			using (FileStream output = File.OpenWrite(outputFile)) {
				output.SetLength(0);
				ADPCM2Wav(input, output);
			}
		}
		
		public static void ADPCM2Wav(Stream input, Stream output) {
			RiffTags riff = new RiffTags(input);
			// Remove unnecissary tags
			foreach (string key in riff.Keys.ToArray()) {
				if (key != "fmt " && key != "data")
					riff.Remove(key);
			}

			if (riff.FileType != "WAVE")
				throw new Exception("Input is not a WAVE file!");

			WaveFmt fmt = WaveFmt.Read(riff["fmt "].Data);
			if (fmt.Format != 20)
				throw new Exception("Input is not ADPCM format!");
			if (fmt.Channels != 2)
				throw new Exception("Input does not have 2 channels!");
			if (fmt.BitsPerSample != 4)
				throw new Exception("Input does not have 4 bits per sample!");

			byte[] data = riff["data"].Data;
			byte[] adpcm = new byte[data.Length];
			short[] wavSamples = new short[data.Length * 2];
			Buffer.BlockCopy(data, 0, adpcm, 0, data.Length);

			ADPCM2PCM2(wavSamples, 0, adpcm, 0);
			ADPCM2PCM(wavSamples, 0, adpcm, 0, adpcm.Length / 2);
			ADPCM2PCM(wavSamples, wavSamples.Length / 2, adpcm, adpcm.Length / 2, adpcm.Length / 2);
			//Interleave(wavSamples);
			
			byte[] wavData = new byte[wavSamples.Length * sizeof(short)];
			Buffer.BlockCopy(wavSamples, 0, wavData, 0, wavData.Length);

			fmt.Format = 1;
			fmt.BitsPerSample = 16;
			fmt.UpdateValues();
			riff["fmt "] = new RiffTag("fmt ", fmt.ToBytes());
			riff["data"] = new RiffTag("data", wavData);
			riff.Save(output);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WaveFmt {
		public short Format;
		public short Channels;
		public int SampleRate;
		public int BytesPerSecond;
		public short BlockSize;
		public short BitsPerSample;

		public static WaveFmt Read(byte[] data) {
			WaveFmt fmt = new WaveFmt();
			fmt.Format = BitConverter.ToInt16(data, 0);
			fmt.Channels = BitConverter.ToInt16(data, 2);
			fmt.SampleRate = BitConverter.ToInt32(data, 4);
			fmt.BytesPerSecond = BitConverter.ToInt32(data, 8);
			fmt.BlockSize = BitConverter.ToInt16(data, 12);
			fmt.BitsPerSample = BitConverter.ToInt16(data, 14);
			return fmt;
		}

		public void UpdateValues() {
			BytesPerSecond = (SampleRate * BitsPerSample * Channels) / 8;
			BlockSize = (short) ((BitsPerSample * Channels) / 8);
		}

		public byte[] ToBytes() {
			using (MemoryStream stream = new MemoryStream(new byte[16])) {
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(Format);
				writer.Write(Channels);
				writer.Write(SampleRate);
				writer.Write(BytesPerSecond);
				writer.Write(BlockSize);
				writer.Write(BitsPerSample);
				return stream.ToArray();
			}
		}
	}
}

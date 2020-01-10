using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {
	public class WaveReader {

		//FIXME: Quick fix for dumb FFmpeg hardcoded location.
		//       For now it must either be in the Windows %PATH%, or your program directory.
		//Just say NO to hardcoded paths.
		const string FFMpeg = "ffmpeg.exe";

		public static byte[] ReadPCM(string file, ref int convSampleRate) {
			return ReadPCM(file, ref convSampleRate, false);
		}

		private static byte[] ReadPCM(string file, ref int convSampleRate, bool ffmpeg) {
			try {
				using (FileStream stream = File.OpenRead(file)) {
					RiffTags riff = new RiffTags(stream);
					if (riff.FileType != "WAVE")
						throw new Exception("File is a not a wave file!");
					byte[] fmt = riff["fmt "].Data;
					short type = BitConverter.ToInt16(fmt, 0);
					if (type != 1)
						throw new Exception("Wave file is not PCM!");
					short channels = BitConverter.ToInt16(fmt, 2);
					int sampleRate = BitConverter.ToInt32(fmt, 4);
					int bitsPerSample = BitConverter.ToInt16(fmt, 14);


					if (type == 1 && channels == 1 && sampleRate <= 4000 && bitsPerSample == 8) {
						convSampleRate = sampleRate;
						return UnsignedToSigned8BitPCM(riff["data"].Data);
					}
					else if (ffmpeg) {
						throw new Exception("Unexpected ffmpeg output!");
					}
				}
			}
			catch (FileNotFoundException) {
				throw;
			}
			catch (Exception ex) {
				if (ffmpeg)
					throw;
			}
			return Convert(file, convSampleRate);
		}

		private static byte[] UnsignedToSigned8BitPCM(byte[] data) {
			sbyte[] newData = new sbyte[data.Length];
			unchecked {
				for (int i = 0; i < data.Length; i++) {
					byte b = data[i];
					newData[i] = (sbyte) (data[i] - 128);
				}
			}
			Buffer.BlockCopy(newData, 0, data, 0, data.Length);
			return data;
		}

		public static byte[] Convert(string file, int sampleRate) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = FFMpeg,
				Arguments = $"-i \"{file}\" -y -acodec pcm_u8 -ar {sampleRate} -ac 1 \"converted.wav\"",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				WorkingDirectory = Directory.GetCurrentDirectory(),
			};
			Process process = Process.Start(startInfo);
			process.WaitForExit();
			if (process.ExitCode != 0)
				throw new Exception($"FFMpeg exited with {process.ExitCode}");
			return ReadPCM("converted.wav", ref sampleRate, true);
		}
	}
}

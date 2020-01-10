using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {
	public static class ADPCMReader {

		//FIXME: Quick fix for dumb FFmpeg hardcoded location.
		//       For now it must either be in the Windows %PATH%, or your program directory.
		//Stop it, that's unethical.
		const string FFMpeg = "ffmpeg.exe";

		public static byte[] ReadADPCM(string file, ref int convSampleRate) {
			return ReadADPCM(file, ref convSampleRate, false);
		}

		private static byte[] ReadADPCM(string file, ref int convSampleRate, bool ffmpeg) {
			try {
				using (FileStream stream = File.OpenRead(file)) {
					RiffTags riff = new RiffTags(stream);
					if (riff.FileType != "WAVE")
						throw new Exception("File is a not a wave file!");
					byte[] fmt = riff["fmt "].Data;
					short type = BitConverter.ToInt16(fmt, 0);
					if (type != 20)
						throw new Exception("File file is not ADPCM!");
					short channels = BitConverter.ToInt16(fmt, 2);
					int sampleRate = BitConverter.ToInt32(fmt, 4);
					int bitsPerSample = BitConverter.ToInt16(fmt, 14);


					if (type == 20 && channels >= 1 && channels <= 2 && sampleRate <= 4000 && bitsPerSample == 4) {
						convSampleRate = sampleRate;
						byte[] data = riff["data"].Data;
						byte[] newData = new byte[data.Length];
						int halfLength = data.Length / 2;
						for (int i = 0, j = 0; i < data.Length; i+=2, j++) {
							newData[i + 0] = data[j];
							newData[i + 1] = data[halfLength + j];
						}
						return newData;
					}
					//else if (ffmpeg) {
						throw new Exception("Unexpected ffmpeg output!");
					//}
				}
			}
			catch (FileNotFoundException) {
				throw;
			}
			catch (Exception ex) {
				//if (ffmpeg)
					throw;
			}
			return Convert(file, convSampleRate);
		}

		public static byte[] Convert(string file, int sampleRate) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = FFMpeg,
				Arguments = $"-i \"{file}\" -y -acodec adpcm_yamaha -ar {sampleRate} -ac 1 \"converted.wav\"",
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
			return ReadADPCM("converted.wav", ref sampleRate, true);
		}
	}
}

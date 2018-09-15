using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {

	public enum SpeakerFormat : byte {
		ADPCM = 0x00,
		PCM = 0x40,
	}

	public struct SpeakerConfiguration {

		private const int ADPCMSampleRateMultiplier = 6000000;
		private const int PCMSampleRateMultiplier = 12000000;
		private const int MaxADPCMVolume = 0x7F;
		private const int MaxPCMVolume = byte.MaxValue;

		public const int MaxADPCMSampleRate = ADPCMSampleRateMultiplier / 1;
		public const int MaxPCMSampleRate = PCMSampleRateMultiplier / 1;

		private int sampleRate;
		private float volume;

		public SpeakerFormat Format { get; }
		public byte Unknown1 { get; set; }
		public byte Unknown2 { get; set; }
		public byte Unknown3 { get; set; }
		
		public SpeakerConfiguration(SpeakerFormat format) {
			Format = format;
			volume = 1f;
			if (format == SpeakerFormat.ADPCM)
				sampleRate = 3000;
			else if (format == SpeakerFormat.PCM)
				sampleRate = 2000;
			else
				throw new ArgumentException($"Invalid {nameof(SpeakerFormat)} ({format})!", nameof(format));
			Unknown1 = 0;
			Unknown2 = 0;
			Unknown3 = 0;
		}

		public int MillisecondsPerReport {
			get {
				if (Format == SpeakerFormat.ADPCM)
					return 20000 / sampleRate;
				else
					return 20000 / sampleRate;
			}
		}

		public int MicrosecondsPerReport {
			get {
				if (Format == SpeakerFormat.ADPCM)
					return 20000000 / sampleRate;
				else
					return 20000000 / sampleRate;
			}
		}

		public int MaxSampleRate {
			get {
				if (Format == SpeakerFormat.ADPCM)
					return MaxADPCMSampleRate;
				else
					return MaxPCMSampleRate;
			}
		}

		public int SampleRate {
			get => sampleRate;
			set {
				if (value <= 0 || value > MaxSampleRate)
					throw new ArgumentOutOfRangeException(nameof(SampleRate));
				sampleRate = value;
			}
		}
		
		public float Volume {
			get => volume;
			set {
				if (value < 0f || value > 1f)
					throw new ArgumentOutOfRangeException(nameof(Volume));
				volume = value;
			}
		}

		internal byte VolumeRaw {
			get {
				if (Format == SpeakerFormat.ADPCM)
					return (byte) Math.Round(volume * MaxADPCMVolume);
				else
					return (byte) Math.Round(volume * MaxPCMVolume);
			}
		}

		internal ushort SampleRateRaw {
			get {
				if (Format == SpeakerFormat.ADPCM)
					return (ushort) (ADPCMSampleRateMultiplier / sampleRate);
				else
					return (ushort) (PCMSampleRateMultiplier / sampleRate);
			}
		}

		internal byte[] ToBytes() {
			byte[] data = new byte[7];
			data[0] = Unknown1;
			data[1] = (byte) Format;
			data[2] = (byte) ((SampleRateRaw >> 8) & 0xFF);
			data[3] = (byte) (SampleRateRaw & 0xFF);
			//data[2] = (byte) ((0x1770 >> 8) & 0xFF);
			//data[3] = (byte) (0x1770 & 0xFF);
			data[4] = VolumeRaw;
			data[5] = Unknown2;
			data[6] = Unknown3;
			return data;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {

	public enum SpeakerFormat : byte {
		// Stereo 4-bit Yamaha ADPCM (like on Dreamcast)
		ADPCM = 0x00,
		// Stereo 8-bit Signed PCM
		PCM = 0x40,
	}

	public struct SpeakerConfiguration {

		private const int ADPCMSampleRateMultiplier = 6000000;
		private const int PCMSampleRateMultiplier = 12000000;
		private const int MaxADPCMVolume = 0x40;//0x7F;
		private const int MaxPCMVolume = 0xFF;//byte.MaxValue;

		public const int MaxADPCMSampleRate = ADPCMSampleRateMultiplier / 1;
		public const int MaxPCMSampleRate = PCMSampleRateMultiplier / 1;

		private int sampleRate;
		private float volume;

		public SpeakerFormat Format { get; set; }
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

		private static int PCMMult {
			get {
				var pf = ADPCMConverter.pf;
				int mult = 2;
				if (pf.HasFlag(PCMFlags.S16LE) || pf.HasFlag(PCMFlags.S16BE))
					mult *= 2;
				if (pf.HasFlag(PCMFlags.MonoL) || pf.HasFlag(PCMFlags.MonoR))
					mult /= 2;
				return mult;
			}
		}

		public int MillisecondsPerReport {
			get {
				if (Format == SpeakerFormat.ADPCM)
					return 20000 / sampleRate;
				else
					return 20000/PCMMult / sampleRate;
			}
		}

		public int MicrosecondsPerReport {
			get {
				if (Format == SpeakerFormat.ADPCM)
					return 20000000 / sampleRate;
				else
					return 20000000/PCMMult / sampleRate;
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
			//TODO: Confirm this WiiRemoteJ implementation isn't just bunk
			ushort rawSampleRate;
			//if (Format == SpeakerFormat.ADPCM)
			//	rawSampleRate = (byte) (48000/2 / sampleRate);
			//else
			//	rawSampleRate = (byte) (48000/1 / sampleRate);

			// Use old method?
			rawSampleRate = SampleRateRaw;

			//data[3] = (byte) (rawSampleRate & 0xFF);
			//data[2] = (byte) ((rawSampleRate >> 8) & 0xFF);
			//TODO: Stupid sexy Little Endian (swapped byte 2 and 3 order), is this really correct?
			data[2] = (byte) (rawSampleRate & 0xFF);
			data[3] = (byte) ((rawSampleRate >> 8) & 0xFF);

			//data[2] = 0xD0;
			//data[3] = 0x07;
			data[4] = VolumeRaw;
			data[5] = Unknown2;
			data[6] = Unknown3;
			//data[4] = 0x40;
			//data[5] = 0x0C;// Unknown2;
			//data[6] = 0x0E;// Unknown3;
			return data;
		}
	}
}

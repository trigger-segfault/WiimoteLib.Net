using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportInspector {
	public struct MidiNote {
		private TimeSpan start;
		private TimeSpan duration;
		public MusicNote Note { get; set; }

		public TimeSpan Start {
			get => start;
			set {
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(nameof(Start));
				start = value;
			}
		}

		public TimeSpan Duration {
			get => duration;
			set {
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(nameof(Duration));
				duration = value;
			}
		}

		public TimeSpan End {
			get => start + duration;
			set {
				if (value < start)
					throw new ArgumentOutOfRangeException(nameof(End));
				duration = value - start;
			}
		}
	}
	public struct MusicNote {
		private float volume;
		private int octave;
		private Note note;

		private const double FreqBase = 1.0594630943592952645618252949463;
		private const int FreqA4 = 440;

		public static readonly MusicNote A4 = new MusicNote(Note.A, 4);

		public Note Note {
			get => note;
			set {
				if (value < Note.C || value > Note.B)
					throw new ArgumentException($"Invalid {nameof(Note)} ({value})!", nameof(Note));
				note = value;
			}
		}

		public int Octave {
			get => octave;
			set {
				if (octave < -1)
					throw new ArgumentOutOfRangeException(nameof(Octave), $"{nameof(Octave)} ({value}) cannot be below -1!");
				else if (octave > 11)
					throw new ArgumentOutOfRangeException(nameof(Octave), $"{nameof(Octave)} ({value}) cannot be above 11!");
				octave = value;
			}
		}
		public float Volume {
			get => volume;
			set {
				if (value < 0f || value > 1f)
					throw new ArgumentOutOfRangeException(nameof(Volume), $"{nameof(Volume)} ({value}) must be between 0 and 1!");
				volume = value;
			}
		}

		public int Semitone {
			get => Octave * 12 + (int) Note;
			set {
				if (value < 0 || value >= 12 * 12)
					throw new ArgumentOutOfRangeException(nameof(Semitone));
				octave = value / 12;
				note = (Note) (value % 12);
			}
		}

		public float Frequency {
			get => (float) (FreqA4 * Math.Pow(FreqBase, Semitone - A4.Semitone));
		}

		public override string ToString() => $"{Note}{Octave}";
		
		public MusicNote(Note note, int octave, float volume = 1f) : this() {
			Note = note;
			Octave = octave;
			Volume = volume;
		}

		public MusicNote(int semitone, float volume = 1f) : this() {
			Semitone = semitone;
			Volume = volume;
		}
	}

	public enum Note {
		C = 0,
		Cs = 1, Db = 1,
		D = 2,
		Ds = 3, Eb = 3,
		E = 4,
		F = 5,
		Fs = 6, Gb = 6,
		G = 7,
		Gs = 8, Ab = 8,
		A = 9,
		As = 10, Bb = 10,
		B = 11,
	}
}

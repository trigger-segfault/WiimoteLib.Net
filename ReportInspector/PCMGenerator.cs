using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportInspector {
	public enum WaveType {
		Square,
		Sine,
		Saw,
		Triangle,
	}

	public struct PCMModifiers {
		public int SampleRate { get; set; }
		public int OctaveOffset { get; set; }
		public int MaxOctave { get; set; }
		public float Volume { get; set; }
		public WaveType Wave { get; set; }
		
		public PCMModifiers Init() {
			if (SampleRate == 0)
				SampleRate = 4000;
			if (MaxOctave == 0)
				MaxOctave = 5;
			if (Volume == 0f)
				Volume = 1f;
			return this;
		}
	}

	public static class PCMGenerator {
		private static Sequencer sequencer = new Sequencer();
		
		public static byte[] CreateTone(MusicNote note, TimeSpan duration, PCMModifiers mods) {
			mods.Init();
			using (MemoryStream stream = new MemoryStream()) {
				BinaryWriter writer = new BinaryWriter(stream);
				WriteNote(writer,
					new MidiNote() {
					Note = note,
					Duration = duration,
				}, mods);
				return stream.ToArray();
			}
		}

		private static void WriteNote(BinaryWriter writer, MidiNote note, PCMModifiers mods) {
			// Square wave
			MusicNote musicNote = note.Note;
			musicNote.Octave += mods.OctaveOffset;
			musicNote.Octave = Math.Max(mods.MaxOctave, musicNote.Octave);
			int max = (int) Math.Round(sbyte.MaxValue * note.Note.Volume * mods.Volume);
			int min = (int) Math.Round(sbyte.MinValue * note.Note.Volume * mods.Volume);
			int range = max - min;
			//int halfRange = range / 2;
			float freq = note.Note.Frequency;
			float period = mods.SampleRate / freq;
			int start = (int) Math.Round(note.Start.TotalSeconds * mods.SampleRate);
			int length = (int) Math.Round(note.Duration.TotalSeconds * mods.SampleRate);
			writer.BaseStream.Position = start;
			bool up = true;
			int inc = (int) Math.Round(period / 2);
			int next = inc;
			for (int i = 0; i < length; i++) {
				int sample;
				float dif = next - i;
				if (dif <= 0) {
					up = !up;
					next += inc;
					//dif++;
					dif++;
				}
				if (dif >= 1) {
					if (up)
						sample = max;
					else
						sample = min;
				}
				else {
					if (up != (dif >= 0.5f))
						sample = max;
					else
						sample = min;
					/*if (up)
						sample = (int) (max - range * dif);
					else
						sample = (int) (min + range * dif);*/
				}
				writer.Write(unchecked((byte) (sample & 0xFF)));
			}
		}

		public static byte[] ConvertMidi(string file, PCMModifiers mods) {
			Sequence sequence = new Sequence(file);
			return ConvertMidi(sequence, mods);
		}

		public static byte[] ConvertMidi(Sequence sequence, PCMModifiers mods) {
			mods.Init();
			var notes = GetNotes(sequence);
			int length = (int) Math.Round(notes.LastOrDefault().End.TotalSeconds * mods.SampleRate);
			byte[] data = new byte[length];
			using (MemoryStream stream = new MemoryStream(data)) {
				BinaryWriter writer = new BinaryWriter(stream);
				foreach (MidiNote note in notes) {
					WriteNote(writer, note, mods);
				}
				return stream.ToArray();
			}
		}

		public static List<MidiNote> GetNotes(Sequence sequence) {
			sequencer.Sequence = sequence;
			List<MidiNote> notes = new List<MidiNote>();
			foreach (Track track in sequence) {
				// Scan all of the notes
				for (int i = 0; i < track.Count; i++) {
					MidiEvent midiEvent = track.GetMidiEvent(i);
					if (midiEvent.MidiMessage.MessageType == MessageType.Channel) {
						var message = midiEvent.MidiMessage as ChannelMessage;
						if (message.Data2 > 0 && message.Command == ChannelCommand.NoteOn) {
							MidiNote note = CreateNote(track, midiEvent, i);
							notes.Add(note);
						}
					}
				}
			}
			notes.Sort((a, b) => a.Start.CompareTo(b.Start));
			return SplitNotes(notes);
		}

		private static List<MidiNote> SplitNotes(List<MidiNote> inputNotes) {
			List<MidiNote> outputNotes = new List<MidiNote>();
			//Dictionary<int, MidiNote> playingNotes = new Dictionary<int, MidiNote>();
			MidiNote lastNote = new MidiNote();
			for (int i = 0; i < inputNotes.Count; i++) {
				MidiNote note = inputNotes[i];
				/*foreach (var pair in playingNotes.ToArray()) {
					int index = pair.Key;
					MidiNote pNote = pair.Value;
					if (note.Start >= pNote.End) {
						playingNotes.Remove(index);
					}
					else {
						outputNotes[index].End = note.Start;
					}
				}*/
				if (i != 0 && lastNote.End > note.Start) {
					lastNote.End = note.Start;
					outputNotes[outputNotes.Count - 1] = lastNote;
				}
				//playingNotes.Add(i, note);
				outputNotes.Add(note);
			}
			for (int i = 0; i < outputNotes.Count; i++) {
				MidiNote note = outputNotes[i];
				if (note.Duration <= TimeSpan.Zero) {
					outputNotes.RemoveAt(i);
					i--;
				}
			}
			return outputNotes;
		}

		private static MidiNote CreateNote(Track track, MidiEvent startEvent, int i) {
			MidiEvent endEvent = FindNoteOff(track, startEvent, i);
			ChannelMessage start = startEvent.MidiMessage as ChannelMessage;
			ChannelMessage end = endEvent.MidiMessage as ChannelMessage;
			MidiNote midiNote = new MidiNote();
			float volume = Math.Min(1f, start.Data2 / 127f);
			midiNote.Note = new MusicNote(Math.Max(0, start.Data1 - 12));//, volume);
			midiNote.Start = TimeSpan.FromMilliseconds(sequencer.TicksToMilliseconds(startEvent.AbsoluteTicks));
			if (end == null)
				midiNote.Duration = TimeSpan.FromMilliseconds(sequencer.TicksToMilliseconds(track.Length)) - midiNote.Start;
			else
				midiNote.Duration = TimeSpan.FromMilliseconds(sequencer.TicksToMilliseconds(endEvent.AbsoluteTicks)) - midiNote.Start;
			return midiNote;
		}

		private static MidiEvent FindNoteOff(Track track, MidiEvent startEvent, int i) {
			ChannelMessage start = startEvent.MidiMessage as ChannelMessage;
			for (; i < track.Count; i++) {
				MidiEvent midiEvent = track.GetMidiEvent(i);
				if (midiEvent.MidiMessage.MessageType == MessageType.Channel) {
					ChannelMessage message = midiEvent.MidiMessage as ChannelMessage;
					if (message.MidiChannel == start.MidiChannel &&
						message.Data1 == start.Data1 &&
						message.Command == ChannelCommand.NoteOff)
					{
						return midiEvent;
					}
				}
			}
			return null;
		}
	}
}

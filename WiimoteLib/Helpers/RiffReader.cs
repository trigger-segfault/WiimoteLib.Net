using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {
	public class RiffTags : Dictionary<string, RiffTag> {
		
		public int FileLength { get; set; }

		public string FileType { get; set; }

		public RiffTags() {
		}

		public RiffTags(string file) {
			using (FileStream stream = File.OpenRead(file))
				Read(stream);
		}

		public RiffTags(Stream stream) {
			Read(stream);
		}

		private void Read(Stream stream) {
			BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);
			string riff = ReadTagName(reader);
			if (riff != "RIFF")
				throw new Exception("Not a RIFF file!");
			FileLength = reader.ReadInt32();
			FileType = ReadTagName(reader);
			RiffTag riffTag;
			do {
				riffTag = ReadTag(reader);
				Add(riffTag.Tag, riffTag);
			} while(riffTag.Tag != "data");
		}

		public void Save(string file) {
			using (FileStream stream = File.OpenWrite(file)) {
				stream.SetLength(0);
				Save(stream);
			}
		}

		public void Save(Stream stream) {
			BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);
			WriteTagName(writer, "RIFF");
			FileLength = this.Sum(p => p.Value.Length + 8) + 4;
			writer.Write(FileLength);
			WriteTagName(writer, FileType);
			foreach (RiffTag tag in Values) {
				WriteTag(writer, tag);
			}
		}

		private static string ReadTagName(BinaryReader reader) {
			return new string(reader.ReadChars(4));
		}

		private static void WriteTagName(BinaryWriter writer, string name) {
			writer.Write(name.ToCharArray());
		}

		private static void WriteTag(BinaryWriter writer, RiffTag tag) {
			WriteTagName(writer, tag.Tag);
			writer.Write(tag.Length);
			writer.Write(tag.Data);
		}

		private static RiffTag ReadTag(BinaryReader reader) {
			string tag = ReadTagName(reader);
			int length = reader.ReadInt32();
			byte[] data = reader.ReadBytes(length);
			return new RiffTag(tag, data);
		}
	}

	public struct RiffTag {
		public string Tag { get; }
		public byte[] Data { get; set; }
		public int Length => Data.Length;

		public RiffTag(string tag, byte[] data) {
			Tag = tag;
			Data = data;
		}
		public RiffTag(string tag, int length) {
			Tag = tag;
			Data = new byte[length];
		}
	}
}

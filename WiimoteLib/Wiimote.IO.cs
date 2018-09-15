using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteLib.Events;
using WiimoteLib.Helpers;
using WiimoteLib.Util;

namespace WiimoteLib {
	public partial class Wiimote : IDisposable {

		private class AsyncReadState : IDisposable {
			public byte[] Buffer { get; private set; }
			public int Length => Buffer.Length;
			public bool ContinueAsync { get; set; }
			public bool IsPrimary { get; }
			public AsyncReadState(bool primary) {
				ContinueAsync = true;
				IsPrimary = primary;
				NewBuffer();
			}
			public void NewBuffer() {
				Buffer = new byte[WiimoteConstants.ReportLength];
			}

			public void Dispose() {
				ContinueAsync = false;
			}
		}

		/// <summary>
		/// Start reading asynchronously from the controller
		/// </summary>
		private AsyncReadState BeginAsyncRead() {
			lock (ioLock) {
				Debug.WriteLine("Read Start");
				// if the stream is valid and ready
				if (device.Stream != null && device.Stream.CanRead) {
					// setup the read and the callback
					AsyncReadState state = new AsyncReadState(false);
					readStates.Add(device.Stream.BeginRead(state.Buffer, 0, state.Length, OnReadData, state));
					Debug.WriteLine($"Read Threads: {ReadThreads}");
					return state;
				}
			}
			throw new OperationCanceledException();
		}

		/// <summary>
		/// Start reading asynchronously from the controller
		/// </summary>
		private AsyncReadState BeginAsyncRead(AsyncReadState state) {
			lock (ioLock) {
				// if the stream is valid and ready
				if (device.Stream != null && device.Stream.CanRead) {
					// setup the read and the callback
					state.NewBuffer();
					readStates.Add(device.Stream.BeginRead(state.Buffer, 0, state.Length, OnReadData, state));
					return state;
				}
			}
			throw new OperationCanceledException();
		}

		private HashSet<IAsyncResult> readStates = new HashSet<IAsyncResult>();

		private int ReadThreads => readStates.Count;

		/// <summary>
		/// Callback when data is ready to be processed
		/// </summary>
		/// <param name="ar">State information for the callback</param>
		private void OnReadData(IAsyncResult ar) {

			try {
				lock (ioLock) {
					if (device.Stream == null)
						return;

					if (readStates.Remove(ar)) {
						// end the current read
						device.Stream.EndRead(ar);
					}
				}
			}
			catch (OperationCanceledException) {
				return;
			}

			// grab the byte buffer and other state settings
			AsyncReadState state = (AsyncReadState) ar.AsyncState;
			try {
				/*lock (ioLock) {
					if (device.Stream == null)
						return;

					// end the current read
					device.Stream.EndRead(ar);
				}*/

				// parse it
				bool newInput = ParseInputReport(state.Buffer);
					// post an event
					//Debug.WriteLine("State Changed Start");
					//Debug.WriteLine("State Changed End");
				//}

				// start reading again
				if (state.ContinueAsync) {
					//Debug.WriteLine("Read Lock");
					BeginAsyncRead(state);
					//Debug.WriteLine("Read Continue");
				}
				else {
					Debug.WriteLine($"Read Stop {ReadThreads}");
				}

				if (newInput)
					RaiseStateChanged();
			}
			catch (OperationCanceledException) {
				//ThrowException(ex);
			}
			catch (TimeoutException) {
				try {
					// start reading again
					if (state.ContinueAsync)
						BeginAsyncRead(state);
				}
				catch (OperationCanceledException) { }
			}
			catch (Exception ex) {
				RaiseWiimoteException(ex);
				try {
					// start reading again
					if (state.ContinueAsync)
						BeginAsyncRead(state);
				}
				catch (OperationCanceledException) { }
			}
		}

		/// <summary>Parse data returned from a read report.</summary>
		/// <param name="buff">Data buffer</param>
		private void ParseReadData(byte[] buff) {
			if ((buff[3] & 0x08) != 0) {
				Exception ex = new WiimoteException(this, "Error reading data from Wiimote: Bytes do not exist.");
				RaiseWiimoteException(ex);
				return;
			}

			if ((buff[3] & 0x07) != 0) {
				Debug.WriteLine("*** read from write-only: " + readAddress.ToString("X8"));
				//LastReadStatus = LastReadStatus.ReadFromWriteOnlyMemory;
				//mReadDone.Set();
				//Respond(OutputReport.ReadMemory, null);
				return;
			}
			else if ((buff[3] & 0x0F) != 0) {
				int error = buff[3] & 0x0F;
				Exception ex = new WiimoteException(this, $"{this} Read Error: {error:X1}");
				RaiseWiimoteException(ex);
				return;
			}

			// get our size and offset from the report
			int size = (buff[3] >> 4) + 1;
			int offset = (buff[4] << 8 | buff[5]);

			/*int readAddress = request.ReadAddress;
			int readSize = request.ReadSize;
			byte[] readBuffer = request.ReadBuffer;*/

			if (readAddress > offset) {
				Exception ex = new WiimoteException(this, $"Out of range address or offset: {readAddress:X4} {offset:X4}");
				RaiseWiimoteException(ex);
				return;
			}
			// add it to the buffer
			Array.Copy(buff, 6, readBuff, offset - readAddress, size);

			// if we've read it all, set the event
			if (readAddress + readSize == offset + size)
				readDone.Set();
				//Respond(OutputReport.ReadMemory, readBuffer);

			//LastReadStatus = LastReadStatus.Success;
		}

		/// <summary>Read data or register from Wiimote.</summary>
		/// <param name="address">Address to read</param>
		/// <param name="size">Length to read</param>
		/// <returns>Data buffer</returns>
		public byte ReadByte(int address, int timeout = 1000) {
			return ReadData(address, 1, timeout)[0];
		}

		/// <summary>Read data or register from Wiimote.</summary>
		/// <param name="address">Address to read</param>
		/// <param name="size">Length to read</param>
		/// <returns>Data buffer</returns>
		public byte[] ReadData(int address, short size, int timeout = 1000) {
			Debug.WriteLine($"Read Start: {address:X8}");
			lock (readDone) {
				Debug.WriteLine($"Read Lock: {address:X8}");
				readBuff = new byte[size];
				readAddress = address & 0xffff;
				readSize = size;

				byte[] buff = CreateReport(OutputReport.ReadMemory);

				buff[1] = (byte) ((address & 0xff000000) >> 24);
				buff[2] = (byte) ((address & 0x00ff0000) >> 16);
				buff[3] = (byte) ((address & 0x0000ff00) >> 8);
				buff[4] = (byte) ((address & 0x000000ff));

				buff[5] = (byte) ((size & 0xff00) >> 8);
				buff[6] = (byte) ((size & 0x00ff));

				WriteReport(buff);

				if (!readDone.WaitOne(timeout, false)) {
					Debug.WriteLine($"Read Timeout: {address:X8}");
					throw new TimeoutException("ReadData: Error reading data from Wiimote...is it connected?");
				}

				Debug.WriteLine($"Read End: {address:X8}");
				return readBuff;
			}
		}


		/// <summary>Write a single byte to the Wiimote</summary>
		/// <param name="address">Address to write</param>
		/// <param name="data">Byte to write</param>
		public void WriteByte(int address, byte data, int timeout = 1000) {
			WriteData(address, 1, new byte[] { data }, timeout);
		}

		/// <summary>Write a byte array to a specified address</summary>
		/// <param name="address">Address to write</param>
		/// <param name="size">Length of buffer</param>
		/// <param name="data">Data buffer</param>
		public bool WriteData(int address, byte size, byte[] data, int timeout = 1000) {
			Debug.WriteLine($"Write Start: {address:X8}");
			lock (writeDone) {
				Debug.WriteLine($"Write Lock: {address:X8}");
				byte[] buff = CreateReport(OutputReport.WriteMemory);

				buff[1] = (byte) ((address & 0xff000000) >> 24);
				buff[2] = (byte) ((address & 0x00ff0000) >> 16);
				buff[3] = (byte) ((address & 0x0000ff00) >> 8);
				buff[4] = (byte) ((address & 0x000000ff));
				buff[5] = size;
				Array.Copy(data, 0, buff, 6, size);

				WriteReport(buff);

				if (!writeDone.WaitOne(timeout, false)) {
					Debug.WriteLine($"Write Timeout: {address:X8}");
					return false;
				}
				Debug.WriteLine($"Write End: {address:X8}");
				return true;
			}
		}

		private byte[] CreateReport(OutputReport type) {
			byte[] buff = new byte[WiimoteConstants.ReportLength];
			buff[0] = (byte) type;
			buff[1] = GetRumbleBit();
			return buff;
		}

		/// <summary>Returns whether rumble is currently enabled.</summary>
		/// <returns>Byte indicating true (0x01) or false (0x00)</returns>
		private byte GetRumbleBit() {
			return (byte) (wiimoteState.Status.Rumble ? 0x01 : 0x00);
		}

		private int WriteReport(byte[] buff) {
			// Automatically do this so we don't have to do it for every report
			buff[1] |= GetRumbleBit();
			OutputReport type = (OutputReport) buff[0];
			
			return WiimoteManager.QueueWriteRequest(this, buff);
		}

		private void SetSpeakerEnabled(bool enabled) {
			byte[] buff = CreateReport(OutputReport.SpeakerEnable);
			buff[1] = (byte) (enabled ? 0x4 : 0x0);
			WriteReport(buff);
		}

		private void SetSpeakerMuted(bool muted) {
			byte[] buff = CreateReport(OutputReport.SpeakerMute);
			buff[1] = (byte) (muted ? 0x4 : 0x0);
			WriteReport(buff);
		}

		public void EnableSpeaker(SpeakerConfiguration config) {
			//using (AsyncReadState state = BeginAsyncRead()) {
			byte[] buff = CreateReport(OutputReport.SpeakerEnable);
			SetSpeakerEnabled(true);
			SetSpeakerMuted(true);
			WriteByte(Registers.Speaker1, 0x01);
			WriteByte(Registers.SpeakerConfig, 0x80);
			WriteData(Registers.SpeakerConfig, 7, config.ToBytes());
			/*WriteByte(Registers.Speaker1, 0x55);
			WriteByte(Registers.SpeakerConfig, 0x08);
			WriteData(Registers.SpeakerConfig, 7, config.ToBytes());
			WriteByte(Registers.Speaker2, 0x01);*/
			SetSpeakerMuted(false);
			WriteByte(Registers.Speaker2, 0x01);
			speakerConfig = config;
			wiimoteState.Status.Speaker = true;
			//}
		}

		public void DisableSpeaker() {
			SetSpeakerEnabled(false);
			wiimoteState.Status.Speaker = false;
		}

		private object speakerLock = new object();
		private MicroTimer speakerTimer;
		private byte[] speakerData;
		private Stream speakerStream;
		private byte[] nextSpeakerReport;
		private int speakerIndex;

		public void StopSound() {
			lock (speakerLock) {
				speakerTimer?.Stop();
				speakerTimer = null;
				speakerStream?.Close();
				speakerStream = null;
				speakerData = null;
			}
		}
		public void PlayTone(int frequency, TimeSpan duration) {

		}
		public void PlaySound(byte[] data) {
			lock (speakerLock) {
				if (disposed)
					return;
				speakerTimer?.Stop();
				/*var config = new SpeakerConfiguration(SpeakerFormat.PCM) {
					Volume = .1f,
					SampleRate = 4000,
				};
				EnableSpeaker(config);*/
				speakerTimer = new MicroTimer(speakerConfig.MicrosecondsPerReport);
				speakerTimer.MicroTimerElapsed += OnSpeakerTimerEllapsed;
				Debug.WriteLine("Milliseconds Per Report: " + speakerConfig.MillisecondsPerReport);
				//speakerTimer2 = new ATimer(3, speakerConfig.MillisecondsPerReport, OnSpeakerTimer2Ellapsed);
				//}

				//string path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\WiimoteController\Resources\Oracle_Secret2Raw.wav";
				/*string path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\WiimoteController\Resources\Oracle_Secret4.wav";
				using (FileStream stream = File.OpenRead(path)) {
					BinaryReader reader = new BinaryReader(stream);
					int length = reader.ReadInt32();
					speakerData = reader.ReadBytes(length);
				}*/
				speakerData = data;
				//speakerData = new byte[20000000];
				speakerIndex = 0;
				NextSpeakerReport();
				/*for (int i = 0; i < 2000; i++) {
					OnSpeakerTimer2Ellapsed();
				}*/
				//Console.WriteLine("DONE");

				speakerTimer.Start();
				//speakerTimer2.Start();
				//speakerWatch = Stopwatch.StartNew();
			}
		}
		public void StreamSound(Stream stream) {

		}

		public void PlaySound() {
			/*var config = new SpeakerConfiguration(SpeakerFormat.ADPCM) {
				Volume = 1f,
				SampleRate = 1500,
			};
			EnableSpeaker(config);
			//lock (speakerTimer) {
			speakerTimer2?.Stop();
				speakerTimer?.Stop();
			//speakerTimer = new MicroTimer(speakerConfig.MicrosecondsPerReport);
			//speakerTimer.MicroTimerElapsed += OnSpeakerTimerEllapsed;
			Debug.WriteLine("Milliseconds Per Report: " + speakerConfig.MillisecondsPerReport);
			speakerTimer2 = new ATimer(3, speakerConfig.MillisecondsPerReport, OnSpeakerTimer2Ellapsed);
			//}

			//string path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\WiimoteController\Resources\Oracle_Secret2Raw.wav";
			string path = @"C:\Users\Onii-chan\My Projects\C#\WiimoteController\WiimoteController\Resources\Oracle_Secret4.wav";
			using (FileStream stream = File.OpenRead(path)) {
				BinaryReader reader = new BinaryReader(stream);
				int length = reader.ReadInt32();
				speakerData = reader.ReadBytes(length);
				speakerIndex = 0;
				speakerData = new byte[20000000];
				NextSpeakerReport();
				playID++;
			}*/
			/*for (int i = 0; i < 2000; i++) {
				OnSpeakerTimer2Ellapsed();
			}*/
			//Console.WriteLine("DONE");

			//speakerTimer.Start();
			/*speakerTimer2.Start();
			speakerWatch = Stopwatch.StartNew();*/
		}

		private void OnSpeakerTimerEllapsed(object sender, MicroTimerEventArgs e) {
			if (sender == speakerTimer) {
				//Console.WriteLine(speakerWatch.ElapsedMilliseconds);
				//speakerWatch = Stopwatch.StartNew();
				WriteReport(nextSpeakerReport);
				//Console.WriteLine(speakerWatch.ElapsedMilliseconds);
				if (NextSpeakerReport()) {
					speakerTimer.Stop();
					Trace.WriteLine("Sound END");
				}
			}
		}

		private bool NextSpeakerReport() {
			int length = Math.Min(20, speakerData.Length - speakerIndex);
			if (length <= 0)
				return true;
			nextSpeakerReport = CreateReport(OutputReport.SpeakerData);
			nextSpeakerReport[1] = (byte) (length << 3);
			Buffer.BlockCopy(speakerData, speakerIndex, nextSpeakerReport, 2, length);
			//Random r = new Random();
			/*for (int i = 0; i < length; i++) {
				byte b = (byte) (speakerIndex % 20 < 10 ? 0x7F : 0x80);
				//byte b = (byte) (i % 12 < 6 ? 0x7F : 0x80);
				nextSpeakerReport[i + 2] = b;//(byte) r.Next(256);
				if (speakerIndex == 0 || speakerIndex == 9)
					nextSpeakerReport[i + 2] = 0x40;
				else if (speakerIndex < 9)
					nextSpeakerReport[i + 2] = 0x7F;
				else if (speakerIndex == 10 || speakerIndex == 19)
					nextSpeakerReport[i + 2] = 0xC0;
				else if (speakerIndex < 19)
					nextSpeakerReport[i + 2] = 0x7F;
			}*/
			speakerIndex += length;
			return false;
		}

		/*private WriteRequest WriteReport(byte[] buff, WaitType waitType = WaitType.None) {
			return WriteReport(this, waitType));
		}

		private WriteRequest WriteReport(WriteRequest request) {
			// Automatically do this so we don't have to do it for every report
			request.Data[1] |= GetRumbleBit();

			lock (acknowledgeRequests) {
				if (!acknowledgeRequests.TryGetValue(request.Type, out var queue)) {
					queue = new Queue<WriteRequest>();
					acknowledgeRequests.Add(request.Type, queue);
				}
				queue.Enqueue(request);
			}
			if (request.WaitType == WaitType.Response) {
				lock (responseRequests) {
					if (!responseRequests.TryGetValue(request.Type, out var queue)) {
						queue = new Queue<WriteRequest>();
						responseRequests.Add(request.Type, queue);
					}
					queue.Enqueue(request);
				}
			}

			WiimoteManager.QueueWriteRequest(request);
			return request;
		}*/

		/*private void Acknowledge(OutputReport type, WriteResult result) {
			lock (acknowledgeRequests) {
				var queue = acknowledgeRequests[type];
				queue.Dequeue().Acknowledge(result);
			}
		}*/

		/*private void Respond(OutputReport type, object response) {
			lock (responseRequests) {
				var queue = responseRequests[type];
				queue.Dequeue().Respond(response);
			}
		}*/

		/*private WriteRequest PeekResponse(OutputReport type) {
			lock (responseRequests) {
				var queue = responseRequests[type];
				return queue.Peek();
			}
		}*/
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.Native;
using WiimoteLib.DataTypes;
using WiimoteLib.OldEvents;

namespace WiimoteLib {
	/// <summary>
	/// Implementation of Wiimote
	/// </summary>
	public partial class WiimoteOld : IDisposable {
		/// <summary>
		/// Start reading asynchronously from the controller
		/// </summary>
		private void BeginAsyncRead() {
			// if the stream is valid and ready
			if (mStream != null && mStream.CanRead) {
				// setup the read and the callback
				byte[] buff = CreateReport();
				mStream.BeginRead(buff, 0, WiimoteConstants.ReportLength, new AsyncCallback(OnReadData), buff);
			}
		}

		/// <summary>
		/// Callback when data is ready to be processed
		/// </summary>
		/// <param name="ar">State information for the callback</param>
		private void OnReadData(IAsyncResult ar) {
			// grab the byte buffer
			byte[] buff = (byte[]) ar.AsyncState;

			try {
				// end the current read
				mStream.EndRead(ar);

				// parse it
				if (ParseInputReport(buff)) {
					// post an event
					WiimoteChanged?.Invoke(this, new WiimoteChangedEventArgs(mWiimoteState));
				}

				// start reading again
				BeginAsyncRead();
			}
			catch (OperationCanceledException) {
				//ThrowException(ex);
			}
			catch (Exception ex) {
				ThrowException(ex);
				return;
			}
		}



		/// <summary>
		/// Decrypts data sent from the extension to the Wiimote
		/// </summary>
		/// <param name="buff">Data buffer</param>
		/// <returns>Byte array containing decoded data</returns>
		private byte[] DecryptBuffer(byte[] buff) {
			for (int i = 0; i < buff.Length; i++)
				buff[i] = (byte) (((buff[i] ^ 0x17) + 0x17) & 0xff);

			return buff;
		}




		/// <summary>
		/// Parse data returned from a read report
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseReadData(byte[] buff) {
			if ((buff[3] & 0x08) != 0) {
				Exception ex = new WiimoteException("Error reading data from Wiimote: Bytes do not exist.");
				WiimoteException?.Invoke(this, new WiimoteExceptionEventArgs(ex));
				return;
			}

			if ((buff[3] & 0x07) != 0) {
				Debug.WriteLine("*** read from write-only");
				LastReadStatus = LastReadStatus.ReadFromWriteOnlyMemory;
				mReadDone.Set();
				return;
			}

			// get our size and offset from the report
			int size = (buff[3] >> 4) + 1;
			int offset = (buff[4] << 8 | buff[5]);

			// add it to the buffer
			Array.Copy(buff, 6, mReadBuff, offset - mAddress, size);

			// if we've read it all, set the event
			if (mAddress + mSize == offset + size)
				mReadDone.Set();

			LastReadStatus = LastReadStatus.Success;
		}

		/// <summary>
		/// Parse data returned from a read report
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseReadData2(byte[] buff) {
			if ((buff[2] & 0x08) != 0) {
				Exception ex = new WiimoteException("Error reading data from Wiimote: Bytes do not exist.");
				WiimoteException?.Invoke(this, new WiimoteExceptionEventArgs(ex));
				return;
			}

			if ((buff[2] & 0x07) != 0) {
				Debug.WriteLine("*** read from write-only");
				LastReadStatus = LastReadStatus.ReadFromWriteOnlyMemory;
				mReadDone.Set();
				return;
			}

			// get our size and offset from the report
			int size = (buff[2] >> 4) + 1;
			int offset = (buff[3] << 8 | buff[4]);

			// add it to the buffer
			Array.Copy(buff, 5, mReadBuff, offset - mAddress, size);

			// if we've read it all, set the event
			if (mAddress + mSize == offset + size)
				mReadDone.Set();

			LastReadStatus = LastReadStatus.Success;
		}
		/// <summary>
		/// Write a report to the Wiimote
		/// </summary>
		private void WriteReport(byte[] buff) {
			Debug.WriteLine("WriteReport: " + Enum.Parse(typeof(OutputReport), buff[0].ToString()));
			if (mAltWriteMethod)
				NativeMethods.HidD_SetOutputReport(mHandle.DangerousGetHandle(), buff, buff.Length);
			else if (mStream != null)
				mStream.Write(buff, 0, WiimoteConstants.ReportLength);

			if (buff[0] == (byte) OutputReport.WriteMemory) {
				//				Debug.WriteLine("Wait");
				if (!mWriteDone.WaitOne(1000, false))
					Debug.WriteLine("Wait failed");
				//throw new WiimoteException("Error writing data to Wiimote...is it connected?");
			}
		}

		/// <summary>
		/// Write a report to the Wiimote
		/// </summary>
		private void WriteReport2(OutputReport type, byte[] buff) {
			Debug.WriteLine($"WriteReport: {type}");

			// Expand the report to include the report type
			Buffer.BlockCopy(buff, 0, buff, 1, buff.Length - 1);
			buff[0] = (byte) type;

			if (mAltWriteMethod)
				NativeMethods.HidD_SetOutputReport(mHandle.DangerousGetHandle(), buff, buff.Length);
			else if (mStream != null)
				mStream.Write(buff, 0, WiimoteConstants.ReportLength);

			if (type == OutputReport.WriteMemory) {
				//Debug.WriteLine("Wait");
				if (!mWriteDone.WaitOne(1000, false))
					Debug.WriteLine("Wait failed");
				//throw new WiimoteException("Error writing data to Wiimote...is it connected?");
			}
		}

		/// <summary>
		/// Read data or register from Wiimote
		/// </summary>
		/// <param name="address">Address to read</param>
		/// <param name="size">Length to read</param>
		/// <returns>Data buffer</returns>
		public byte[] ReadData(int address, short size) {
			mReadBuff = new byte[size];
			mAddress = address & 0xffff;
			mSize = size;

			/*byte[] buff = CreateReport();

			buff[0] = (byte) OutputReport.ReadMemory;
			buff[1] = (byte) (((address & 0xff000000) >> 24) | GetRumbleBit());
			buff[2] = (byte) ((address & 0x00ff0000) >> 16);
			buff[3] = (byte) ((address & 0x0000ff00) >> 8);
			buff[4] = (byte) (address & 0x000000ff);

			buff[5] = (byte) ((size & 0xff00) >> 8);
			buff[6] = (byte) (size & 0xff);

			WriteReport(buff);*/


			byte[] buff = CreateReport2();
			
			buff[0] = (byte) (((address & 0xff000000) >> 24) | GetRumbleBit());
			buff[1] = (byte) ((address & 0x00ff0000) >> 16);
			buff[2] = (byte) ((address & 0x0000ff00) >> 8);
			buff[3] = (byte) (address & 0x000000ff);

			buff[4] = (byte) ((size & 0xff00) >> 8);
			buff[5] = (byte) (size & 0xff);

			WriteReport2(OutputReport.ReadMemory, buff);

			if (!mReadDone.WaitOne(1000, false))
				throw new TimeoutException("Error reading data from Wiimote...is it connected?");

			return mReadBuff;
		}

		/// <summary>
		/// Write a single byte to the Wiimote
		/// </summary>
		/// <param name="address">Address to write</param>
		/// <param name="data">Byte to write</param>
		public void WriteData(int address, byte data) {
			WriteData(address, 1, new byte[] { data });
		}

		/// <summary>
		/// Write a byte array to a specified address
		/// </summary>
		/// <param name="address">Address to write</param>
		/// <param name="size">Length of buffer</param>
		/// <param name="data">Data buffer</param>
		public void WriteData(int address, byte size, byte[] data) {
			/*byte[] buff = CreateReport();

			buff[0] = (byte) OutputReport.WriteMemory;
			buff[1] = (byte) (((address & 0xff000000) >> 24) | GetRumbleBit());
			buff[2] = (byte) ((address & 0x00ff0000) >> 16);
			buff[3] = (byte) ((address & 0x0000ff00) >> 8);
			buff[4] = (byte) (address & 0x000000ff);
			buff[5] = size;
			Array.Copy(data, 0, buff, 6, size);

			WriteReport(buff);*/

			byte[] buff = CreateReport2();
			
			buff[0] = (byte) (((address & 0xff000000) >> 24) | GetRumbleBit());
			buff[1] = (byte) ((address & 0x00ff0000) >> 16);
			buff[2] = (byte) ((address & 0x0000ff00) >> 8);
			buff[3] = (byte) (address & 0x000000ff);
			buff[4] = size;
			Array.Copy(data, 0, buff, 5, size);

			WriteReport2(OutputReport.WriteMemory, buff);
		}
	}
}

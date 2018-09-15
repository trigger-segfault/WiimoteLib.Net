using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteLib.DataTypes;
using WiimoteLib.Devices;
using WiimoteLib.Events;
using WiimoteLib.Helpers;
using WiimoteLib.Util;

namespace WiimoteLib {
	public partial class Wiimote : IDisposable {
		
		private WiimoteDeviceInfo device;

		private bool disposed;
		private readonly object ioLock;

		// current state of controller
		private WiimoteState wiimoteState;

		// use a different method to write reports
		private bool altWriteMethod;
		
		//private CancellationTokenSource monitorToken;
		//private Task monitorTask;
		//private CancellationTokenSource rumbleToken;
		//private Task rumbleTask;

		// event for read data processing
		private readonly AutoResetEvent readDone = new AutoResetEvent(false);
		private readonly AutoResetEvent writeDone = new AutoResetEvent(false);

		// event for status report
		private readonly AutoResetEvent statusDone = new AutoResetEvent(false);
		
		// read data buffer
		private byte[] readBuff;

		// address to read from
		private int readAddress;

		// size of requested read
		private short readSize;

		private SpeakerConfiguration speakerConfig;

		internal Wiimote(WiimoteDeviceInfo device) {
			this.device = device;
			disposed = false;
			ioLock = new object();
			wiimoteState = new WiimoteState();
			altWriteMethod = false;

			//mAltWriteMethod = false;

			device.HID.Open();

			try {
				// We're not ready for this yet
				BeginAsyncRead();

				// read the calibration info from the controller
				//ReadWiimoteCalibration();
				try {
					ReadWiimoteCalibration();
				}
				catch {
					// if we fail above, try the alternate HID writes
					//Debug.WriteLine("AltWriteMethod");
					altWriteMethod = true;
					ReadWiimoteCalibration();
				}

				// force a status check to get the state of any extensions plugged in at startup
				GetStatus(500);
			}
			catch {
				Dispose();
				throw;
			}
		}
		
		/// <summary>Read calibration information stored on Wiimote.</summary>
		private void ReadWiimoteCalibration() {
			// this appears to change the report type to 0x31
			byte[] buff = ReadData(Registers.WiimoteCalibration, 7, 250);

			wiimoteState.AccelCalibrationInfo.Parse(buff, 0);
		}

		/// <summary>Retrieve the current status of the Wiimote and extensions.
		/// Replaces GetBatteryLevel() since it was poorly named.</summary>
		public void GetStatus(int timeout = 3000) {
			Debug.WriteLine("GetStatus Start");
			lock (statusDone) {
				Debug.WriteLine("GetStatus Lock");

				byte[] buff = CreateReport(OutputReport.Status);

				WriteReport(buff);

				// signal the status report finished
				if (!statusDone.WaitOne(timeout, false)) {
					Debug.WriteLine("GetStatus Timeout");
					throw new TimeoutException("Timed out waiting for status report");
				}
				Debug.WriteLine("GetStatus End");
			}
		}

		/// <summary>Turn on the IR sensor.</summary>
		/// <param name="mode">The data report mode</param>
		/// <param name="irSensitivity">IR sensitivity</param>
		private void EnableIR(IRMode mode, IRSensitivity irSensitivity) {
			wiimoteState.Status.IREnabled = true;
			wiimoteState.IRState.Mode = mode;
			wiimoteState.IRState.Sensitivity = irSensitivity;

			byte[] buff = CreateReport(OutputReport.IRPixelClock);
			buff[1] = 0x04;
			WriteReport(buff);

			buff = CreateReport(OutputReport.IRLogic);
			buff[1] = 0x04;
			WriteReport(buff);

			WriteByte(Registers.IR, 0x08);
			switch (irSensitivity) {
			case IRSensitivity.WiiLevel1:
				WriteData(Registers.IRSensitivity1, 9, new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x64, 0x00, 0xfe });
				WriteData(Registers.IRSensitivity2, 2, new byte[] { 0xfd, 0x05 });
				break;
			case IRSensitivity.WiiLevel2:
				WriteData(Registers.IRSensitivity1, 9, new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x96, 0x00, 0xb4 });
				WriteData(Registers.IRSensitivity2, 2, new byte[] { 0xb3, 0x04 });
				break;
			case IRSensitivity.WiiLevel3:
				WriteData(Registers.IRSensitivity1, 9, new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 });
				WriteData(Registers.IRSensitivity2, 2, new byte[] { 0x63, 0x03 });
				break;
			case IRSensitivity.WiiLevel4:
				WriteData(Registers.IRSensitivity1, 9, new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xc8, 0x00, 0x36 });
				WriteData(Registers.IRSensitivity2, 2, new byte[] { 0x35, 0x03 });
				break;
			case IRSensitivity.WiiLevel5:
				WriteData(Registers.IRSensitivity1, 9, new byte[] { 0x07, 0x00, 0x00, 0x71, 0x01, 0x00, 0x72, 0x00, 0x20 });
				WriteData(Registers.IRSensitivity2, 2, new byte[] { 0x1, 0x03 });
				break;
			case IRSensitivity.Maximum:
				WriteData(Registers.IRSensitivity1, 9, new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x90, 0x00, 0x41 });
				WriteData(Registers.IRSensitivity2, 2, new byte[] { 0x40, 0x00 });
				break;
			default:
				throw new ArgumentOutOfRangeException("irSensitivity");
			}
			WriteByte(Registers.IRMode, (byte) mode);
			WriteByte(Registers.IR, 0x08);
		}

		/// <summary>Disable the IR sensor.</summary>
		private void DisableIR() {
			wiimoteState.Status.IREnabled = false;
			wiimoteState.IRState.Mode = IRMode.Off;

			byte[] buff = CreateReport(OutputReport.IRPixelClock);
			WriteReport(buff);

			buff = CreateReport(OutputReport.IRLogic);
			WriteReport(buff);
		}

		public void Disconnect() {
			if (IsConnected) {
				Dispose();
				WiimoteManager.Disconnect(this);
			}
		}

		public void Dispose() {
			Dispose(true);
			//GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			// close up our handles
			if (disposing && !disposed) {
				Debug.WriteLine($"{this} Disposing");
				lock (ioLock) {
					Debug.WriteLine($"{this} Disposing Lock");
					readStates.Clear();
					/*foreach (IAsyncResult ar in readStates.ToArray()) {
						try {
							if (readStates.Remove(ar)) {
								device.Stream.EndRead(ar);
							}
						}
						catch { }
					}*/
					// Cleanup the state incase anyone is still reading it for input
					wiimoteState = new WiimoteState();
					device.HID.Close();
					StopSound();
					disposed = true;
					readDone.Dispose();
					writeDone.Dispose();
					statusDone.Dispose();
				}
			}
		}

		public override string ToString() {
			if (!Address.IsInvalid)
				return $"{Type} ({Address})";
			else
				return $"{Type}";
		}

		/// <summary>The information about this Wiimote device.</summary>
		public WiimoteDeviceInfo Device => device;

		/// <summary>The address of this Wiimote device.</summary>
		public BluetoothAddress Address => Device.Address;

		/// <summary>The HID device path of this Wiimote device.</summary>
		public string DevicePath => Device.DevicePath;

		/// <summary>The type of this Wiimote.</summary>
		public WiimoteType Type => device.Type;

		/// <summary>True if the wiimote is connected.</summary>
		public bool IsConnected => Device.IsOpen;

		/// <summary>True if the wiimote has been disposed off.</summary>
		public bool IsDisposed => disposed;

		/// <summary>The current state of the wiimote.</summary>
		public WiimoteState WiimoteState => wiimoteState;

		internal bool AltWriteMethod => altWriteMethod;

		public SpeakerConfiguration SpeakerConfig => speakerConfig;
	}
}

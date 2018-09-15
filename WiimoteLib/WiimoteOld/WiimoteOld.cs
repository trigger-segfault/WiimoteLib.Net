//////////////////////////////////////////////////////////////////////////////////
//	Wiimote.cs
//	Managed Wiimote Library
//	Written by Brian Peek (http://www.brianpeek.com/)
//	for MSDN's Coding4Fun (http://msdn.microsoft.com/coding4fun/)
//	Visit http://blogs.msdn.com/coding4fun/archive/2007/03/14/1879033.aspx
//  and http://www.codeplex.com/WiimoteLib
//	for more information
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Collections.Generic;
using System.Management;
using Microsoft.Win32;
using WiimoteLib.Geometry;
using WiimoteLib.Native;
using WiimoteLib.DataTypes;
using WiimoteLib.Util;
using WiimoteLib.OldEvents;
using WiimoteLib.Devices;

namespace WiimoteLib {
	/// <summary>
	/// Implementation of Wiimote
	/// </summary>
	public partial class WiimoteOld : IDisposable {
		/// <summary>
		/// Event raised when Wiimote state is changed
		/// </summary>
		public event EventHandler<WiimoteChangedEventArgs> WiimoteChanged;

		/// <summary>
		/// Event raised when an extension is inserted or removed
		/// </summary>
		public event EventHandler<WiimoteExtensionChangedEventArgs> WiimoteExtensionChanged;

		/// <summary>
		/// Event raised when an exception occurrs while the Wiimote is connected
		/// </summary>
		public event EventHandler<WiimoteExceptionEventArgs> WiimoteException;


		//const string BaseWiimoteKey = @"SYSTEM\CurrentControlSet\Enum\BTHENUM\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002057e_PID&";

		//const string WiimoteKey = BaseWiimoteKey + "0306";
		//const string WiimotePlusKey = BaseWiimoteKey + "0330";

		// VID = Nintendo, PID = Wiimote
		//private const int VID = 0x057e;
		//private const int PID = 0x0306;
		//private const int PIDPlus = 0x0330;

		// sure, we could find this out the hard way using HID, but trust me, it's 22
		//private const int ReportLength = 22;

		// Wiimote registers
		/*private const int REGISTER_IR = 0x04b00030;
		private const int REGISTER_IR_SENSITIVITY_1 = 0x04b00000;
		private const int REGISTER_IR_SENSITIVITY_2 = 0x04b0001a;
		private const int REGISTER_IR_MODE = 0x04b00033;

		private const int REGISTER_EXTENSION_INIT_1 = 0x04a400f0;
		private const int REGISTER_EXTENSION_INIT_2 = 0x04a400fb;
		private const int REGISTER_EXTENSION_TYPE = 0x04a400fa;
		private const int REGISTER_EXTENSION_TYPE_2 = 0x04a400fe;
		private const int REGISTER_EXTENSION_CALIBRATION = 0x04a40020;

		private const int REGISTER_MOTIONPLUS_INIT = 0x04a600fe;*/
		
		// length between board sensors
		private const int BSL = 43;

		// width between board sensors
		private const int BSW = 24;

		// read/write handle to the device
		private SafeFileHandle mHandle;

		// a pretty .NET stream to read/write from/to
		private FileStream mStream;

		// read data buffer
		private byte[] mReadBuff;

		// address to read from
		private int mAddress;

		// size of requested read
		private short mSize;

		// current state of controller
		private readonly WiimoteState mWiimoteState = new WiimoteState();

		// event for read data processing
		private readonly AutoResetEvent mReadDone = new AutoResetEvent(false);
		private readonly AutoResetEvent mWriteDone = new AutoResetEvent(false);

		// event for status report
		private readonly AutoResetEvent mStatusDone = new AutoResetEvent(false);

		// use a different method to write reports
		private bool mAltWriteMethod;

		// HID device path of this Wiimote
		private string mDevicePath = string.Empty;

		private BluetoothAddress mMacAddress;

		// unique ID
		private readonly Guid mID = Guid.NewGuid();

		// delegate used for enumerating found Wiimotes
		internal delegate bool WiimoteFoundDelegate(string devicePath);

		// kilograms to pounds
		private const float KG2LB = 2.20462262f;

		// Poll if Wii Motion Plus is present
		private Timer timerMotionPlus;

		/// <summary>
		/// Default constructor
		/// </summary>
		public WiimoteOld() {
		}

		~WiimoteOld() {
		}

		internal WiimoteOld(string devicePath) {
			mDevicePath = devicePath;
		}

		/// <summary>
		/// Connect to the first-found Wiimote
		/// </summary>
		/// <exception cref="WiimoteNotFoundException">Wiimote not found in HID device list</exception>
		public void Connect() {
			if (string.IsNullOrEmpty(mDevicePath))
				FindWiimote(WiimoteFound);
			else
				OpenWiimoteDeviceHandle(mDevicePath);
		}

		/// <summary>
		/// Connect to the first-found Wiimote
		/// </summary>
		/// <exception cref="WiimoteNotFoundException">Wiimote not found in HID device list</exception>
		public void Connect(long macAddress) {
			if (string.IsNullOrEmpty(mDevicePath)) {
				FindWiimote(WiimoteFound, macAddress);
				mMacAddress = new BluetoothAddress(macAddress);
			}
			else
				OpenWiimoteDeviceHandle(mDevicePath);
		}

		internal static void FindWiimote(WiimoteFoundDelegate wiimoteFound, BluetoothAddress macAddress = default(BluetoothAddress)) {
			int index = 0;
			bool found = false;
			Guid guid;
			SafeFileHandle mHandle;

			// get the GUID of the HID class
			NativeMethods.HidD_GetHidGuid(out guid);

			// get a handle to all devices that are part of the HID class
			// Fun fact:  DIGCF_PRESENT worked on my machine just fine.  I reinstalled Vista, and now it no longer finds the Wiimote with that parameter enabled...
			IntPtr hDevInfo = NativeMethods.SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, DeviceInfoFlags.DeviceInterface);// | DeviceInfoFlags.Present);

			// create a new interface data struct and initialize its size
			SP_DEVICE_INTERFACE_DATA diData = new SP_DEVICE_INTERFACE_DATA();
			diData.cbSize = Marshal.SizeOf(diData);

			// get a device interface to a single device (enumerate all devices)
			SP_DEVINFO_DATA devData = new SP_DEVINFO_DATA();
			devData.cbSize = Marshal.SizeOf(devData);

			while (NativeMethods.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guid, index, ref diData)) {
				int size;

				// get the buffer size for this device detail instance (returned in the size parameter)
				NativeMethods.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, IntPtr.Zero, 0, out size, IntPtr.Zero);

				// create a detail struct and set its size
				SP_DEVICE_INTERFACE_DETAIL_DATA diDetail = new SP_DEVICE_INTERFACE_DETAIL_DATA();

				// yeah, yeah...well, see, on Win x86, cbSize must be 5 for some reason.  On x64, apparently 8 is what it wants.
				// someday I should figure this out.  Thanks to Paul Miller on this...
				diDetail.cbSize = (IntPtr.Size == 8 ? 8 : 5);

				//HIDImports.SP_DEVICE_INTERFACE_DATA diData;


				// actually get the detail struct
				if (NativeMethods.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, ref diDetail, size, out size, IntPtr.Zero)) {
					Debug.WriteLine(string.Format("{0}: {1} - {2}", index, diDetail.DevicePath, Marshal.GetLastWin32Error()));

					// open a read/write handle to our device using the DevicePath returned
					mHandle = NativeMethods.CreateFile(diDetail.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);

					// create an attributes struct and initialize the size
					HIDD_ATTRIBUTES attrib = new HIDD_ATTRIBUTES();
					attrib.Size = Marshal.SizeOf(attrib);

					// get the attributes of the current device
					if (NativeMethods.HidD_GetAttributes(mHandle.DangerousGetHandle(), ref attrib)) {
						// if the vendor and product IDs match up
						if (attrib.VendorID == WiimoteConstants.VendorID && attrib.ProductID == WiimoteConstants.ProductID) {
							// it's a Wiimote!
							Trace.WriteLine("Found one!");
							if (macAddress.IsInvalid || WiimoteRegistry.MatchesHIDPath(diDetail.DevicePath, macAddress)) {
								found = true;
								Trace.WriteLine($"Device path: {diDetail.DevicePath}");

								// fire the callback function...if the callee doesn't care about more Wiimotes, break out
								if (!wiimoteFound(diDetail.DevicePath))
									break;
							}
						}
					}
					mHandle.Close();
				}
				else {
					// failed to get the detail struct
					throw new WiimoteException("SetupDiGetDeviceInterfaceDetail failed on index " + index);
				}

				// move to the next device
				index++;
			}

			// clean up our list
			NativeMethods.SetupDiDestroyDeviceInfoList(hDevInfo);

			// if we didn't find a Wiimote, throw an exception
			if (!found)
				throw new WiimoteNotFoundException("No Wiimotes found in HID device list.");
		}

		private bool WiimoteFound(string devicePath) {
			mDevicePath = devicePath;

			// if we didn't find a Wiimote, throw an exception
			OpenWiimoteDeviceHandle(mDevicePath);

			return false;
		}

		private void OpenWiimoteDeviceHandle(string devicePath) {
			// open a read/write handle to our device using the DevicePath returned
			mHandle = NativeMethods.CreateFile(devicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);

			// create an attributes struct and initialize the size
			HIDD_ATTRIBUTES attrib = new HIDD_ATTRIBUTES();
			attrib.Size = Marshal.SizeOf(attrib);

			// get the attributes of the current device
			if (NativeMethods.HidD_GetAttributes(mHandle.DangerousGetHandle(), ref attrib)) {
				// if the vendor and product IDs match up
				if (attrib.VendorID == WiimoteConstants.VendorID && attrib.ProductID == WiimoteConstants.ProductID) {
					// create a nice .NET FileStream wrapping the handle above
					mStream = new FileStream(mHandle, FileAccess.ReadWrite, WiimoteConstants.ReportLength, true);

					// start an async read operation on it
					BeginAsyncRead();

					// read the calibration info from the controller
					try {
						ReadWiimoteCalibration();
					}
					catch {
						// if we fail above, try the alternate HID writes
						mAltWriteMethod = true;
						ReadWiimoteCalibration();
					}

					// force a status check to get the state of any extensions plugged in at startup
					GetStatus();
				}
				else {
					// otherwise this isn't the controller, so close up the file handle
					mHandle.Close();
					throw new WiimoteException("Attempted to open a non-Wiimote device.");
				}
			}
		}

		/// <summary>
		/// Disconnect from the controller and stop reading data from it
		/// </summary>
		public void Disconnect() {
			// close up the stream and handle
			mStream?.Close();
			mStream = null;
			
			mHandle?.Close();
			mHandle = null;

			timerMotionPlus?.Dispose();
			timerMotionPlus = null;
		}

		

		/// <summary>
		/// Returns whether rumble is currently enabled.
		/// </summary>
		/// <returns>Byte indicating true (0x01) or false (0x00)</returns>
		private byte GetRumbleBit() {
			return (byte) (mWiimoteState.Status.Rumble ? 0x01 : 0x00);
		}

		/// <summary>
		/// Read calibration information stored on Wiimote
		/// </summary>
		private void ReadWiimoteCalibration() {
			// this appears to change the report type to 0x31
			byte[] buff = ReadData(Registers.WiimoteCalibration, 7);

			mWiimoteState.AccelCalibrationInfo.Parse(buff, 0);
			/*mWiimoteState.AccelCalibrationInfo.X0 = buff[0];
			mWiimoteState.AccelCalibrationInfo.Y0 = buff[1];
			mWiimoteState.AccelCalibrationInfo.Z0 = buff[2];
			mWiimoteState.AccelCalibrationInfo.XG = buff[4];
			mWiimoteState.AccelCalibrationInfo.YG = buff[5];
			mWiimoteState.AccelCalibrationInfo.ZG = buff[6];*/
		}

		

		/// <summary>
		/// Retrieve the current status of the Wiimote and extensions.  Replaces GetBatteryLevel() since it was poorly named.
		/// </summary>
		public void GetStatus() {
			Debug.WriteLine("GetStatus");

			/*byte[] buff = CreateReport();

			buff[0] = (byte) OutputReport.Status;
			buff[1] = GetRumbleBit();

			WriteReport(buff);*/

			byte[] buff = CreateReport2();
			
			buff[0] = GetRumbleBit();

			WriteReport2(OutputReport.Status, buff);

			// signal the status report finished
			if (!mStatusDone.WaitOne(3000, false))
				throw new TimeoutException("Timed out waiting for status report");
		}

		/// <summary>
		/// Turn on the IR sensor
		/// </summary>
		/// <param name="mode">The data report mode</param>
		/// <param name="irSensitivity">IR sensitivity</param>
		private void EnableIR(IRMode mode, IRSensitivity irSensitivity) {
			mWiimoteState.Status.IREnabled = true;
			mWiimoteState.IRState.Mode = mode;

			/*byte[] buff = CreateReport();
			buff[0] = (byte) OutputReport.IR1;
			buff[1] = (byte) (0x04 | GetRumbleBit());
			WriteReport(buff);

			Array.Clear(buff, 0, buff.Length);
			buff[0] = (byte) OutputReport.IR2;
			buff[1] = (byte) (0x04 | GetRumbleBit());
			WriteReport(buff);*/

			byte[] buff = CreateReport2();
			buff[0] = (byte) (0x04 | GetRumbleBit());
			WriteReport2(OutputReport.IR1, buff);

			Array.Clear(buff, 0, buff.Length);
			buff[0] = (byte) (0x04 | GetRumbleBit());
			WriteReport2(OutputReport.IR2, buff);

			WriteData(Registers.IR, 0x08);
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
			WriteData(Registers.IRMode, (byte) mode);
			WriteData(Registers.IR, 0x08);
		}

		/// <summary>
		/// Disable the IR sensor
		/// </summary>
		private void DisableIR() {
			mWiimoteState.Status.IREnabled = false;
			mWiimoteState.IRState.Mode = IRMode.Off;

			/*byte[] buff = CreateReport();
			buff[0] = (byte) OutputReport.IR1;
			buff[1] = GetRumbleBit();
			WriteReport(buff);

			Array.Clear(buff, 0, buff.Length);
			buff[0] = (byte) OutputReport.IR2;
			buff[1] = GetRumbleBit();
			WriteReport(buff);*/

			byte[] buff = CreateReport2();
			buff[0] = GetRumbleBit();
			WriteReport2(OutputReport.IR1, buff);

			Array.Clear(buff, 0, buff.Length);
			buff[0] = GetRumbleBit();
			WriteReport2(OutputReport.IR2, buff);
		}

		/// <summary>
		/// Initialize the report data buffer
		/// </summary>
		private byte[] CreateReport() {
			return new byte[WiimoteConstants.ReportLength];
		}

		/// <summary>
		/// Initialize the report data buffer
		/// </summary>
		private byte[] CreateReport2() {
			return new byte[WiimoteConstants.ReportLength];
		}

		

		/// <summary>
		/// </summary>
		private void ThrowException(string message) {
			ThrowException(new WiimoteException(message));
		}

		/// <summary>
		/// </summary>
		private void ThrowException(string message, Exception innerException) {
			ThrowException(new WiimoteException(message, innerException));
		}

		/// <summary>
		/// </summary>
		private void ThrowException(Exception ex) {
			Disconnect();
			WiimoteException?.Invoke(this, new WiimoteExceptionEventArgs(ex));
		}

		/// <summary>
		/// Current Wiimote state
		/// </summary>
		public WiimoteState WiimoteState {
			get { return mWiimoteState; }
		}

		///<summary>
		/// Unique identifier for this Wiimote (not persisted across application instances)
		///</summary>
		public Guid Guid {
			get { return mID; }
		}

		/// <summary>
		/// HID device path for this Wiimote (valid until Wiimote is disconnected)
		/// </summary>
		public string HIDDevicePath {
			get { return mDevicePath; }
		}

		public BluetoothAddress MacAddress {
			get { return mMacAddress; }
		}

		/// <summary>
		/// The Wiimote is currently connected
		/// </summary>
		public bool IsConnected {
			get { return mHandle != null && !mHandle.IsClosed; }
		}

		/// <summary>
		/// Status of last ReadMemory operation
		/// </summary>
		public LastReadStatus LastReadStatus { get; private set; }

		#region IDisposable Members

		/// <summary>
		/// Dispose Wiimote
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose wiimote
		/// </summary>
		/// <param name="disposing">Disposing?</param>
		protected virtual void Dispose(bool disposing) {
			// close up our handles
			if (disposing)
				Disconnect();
		}
		#endregion
	}

	/// <summary>
	/// Thrown when no Wiimotes are found in the HID device list
	/// </summary>
	[Serializable]
	public class WiimoteNotFoundException : ApplicationException {
		/// <summary>
		/// Default constructor
		/// </summary>
		public WiimoteNotFoundException() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		public WiimoteNotFoundException(string message) : base(message) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exception</param>
		public WiimoteNotFoundException(string message, Exception innerException) : base(message, innerException) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="info">Serialization info</param>
		/// <param name="context">Streaming context</param>
		protected WiimoteNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}

	/// <summary>
	/// Represents errors that occur during the execution of the Wiimote library
	/// </summary>
	[Serializable]
	public class WiimoteException : ApplicationException {
		/// <summary>
		/// Default constructor
		/// </summary>
		public WiimoteException() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		public WiimoteException(string message) : base(message) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exception</param>
		public WiimoteException(string message, Exception innerException) : base(message, innerException) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="info">Serialization info</param>
		/// <param name="context">Streaming context</param>
		protected WiimoteException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}
}
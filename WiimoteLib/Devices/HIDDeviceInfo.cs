using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteLib.Native;

namespace WiimoteLib.Devices {
	public class HIDDeviceInfo {

		internal IntPtr DangerousHandle => Handle.DangerousGetHandle();
		internal SafeFileHandle Handle;
		internal FileStream Stream;

		internal HIDD_ATTRIBUTES HIDAttr;
		internal SP_DEVICE_INTERFACE_DETAIL_DATA DIDetail;
		internal SP_DEVICE_INTERFACE_DATA DIData;


		private static Guid HIDGuid;

		static HIDDeviceInfo() {
			NativeMethods.HidD_GetHidGuid(out HIDGuid);
		}
		

		public int ProductID => HIDAttr.ProductID;
		public int VendorID => HIDAttr.VendorID;
		public string DevicePath => DIDetail.DevicePath;

		internal HIDDeviceInfo() {
			DIData.cbSize = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>();
			// yeah, yeah...well, see, on Win x86, cbSize must be 5 for some reason.  On x64, apparently 8 is what it wants.
			// someday I should figure this out.  Thanks to Paul Miller on this...
			DIDetail.cbSize = (int) (IntPtr.Size == 8 ? 8 : 5);
			HIDAttr.Size = Marshal.SizeOf<HIDD_ATTRIBUTES>();

			Handle = null;
			Stream = null;
		}

		internal HIDDeviceInfo(SP_DEVICE_INTERFACE_DATA diData)
			: this()
		{
			DIData = diData;
		}


		private bool OpenHandle() {
			Handle?.Close();
			Handle = null;
			Handle = NativeMethods.CreateFile(DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);
			return !Handle.IsInvalid;
		}

		private void OpenFileStream() {
			Stream?.Close();
			Stream = null;
			// create a nice .NET FileStream wrapping the handle above
			Stream = new FileStream(Handle, FileAccess.ReadWrite, WiimoteConstants.ReportLength, true);
		}

		internal bool IsOpen => Handle != null && !Handle.IsClosed && !Handle.IsInvalid;

		internal void Open() {
			Close();
			if (OpenHandle()) {
				try {
					OpenFileStream();
				}
				catch (Exception) {
					Handle?.Close();
					Handle = null;
					throw;
				}
			}
			else {
				throw new Win32Exception();
			}
		}

		internal void Close() {
			Handle?.Close();
			Handle = null;
			Stream?.Close();
			Stream = null;
		}
		
		internal static HIDDeviceInfo GetDevice(string hidPath) {
			bool result;
			HIDDeviceInfo hid = new HIDDeviceInfo();
			hid.DIDetail.DevicePath = hidPath;

			// Open the device handle
			if (!hid.OpenHandle())
				return null;

			// Get the attributes of the current device
			result = NativeMethods.HidD_GetAttributes(hid.Handle.DangerousGetHandle(), ref hid.HIDAttr);
			// Close the device before returning in
			// case the user doesn't want it afterall.
			hid.Close();
			if (result)
				return hid;

			// Otherwise this isn't an HID device
			return null;
		}

		internal static HIDDeviceInfo GetDevice(BluetoothAddress address) {
			return EnumerateDevices(d => WiimoteRegistry.IsDriverInstalled(d.DevicePath, address)).FirstOrDefault();
		}

		internal static HIDDeviceInfo[] GetDevices() {
			return EnumerateDevices(new CancellationToken()).ToArray();
		}

		internal static HIDDeviceInfo[] GetDevices(CancellationToken token) {
			return EnumerateDevices(token).ToArray();
		}

		internal static HIDDeviceInfo[] GetDevices(Predicate<HIDDeviceInfo> match) {
			return EnumerateDevices(new CancellationToken(), match).ToArray();
		}

		internal static HIDDeviceInfo[] GetDevices(CancellationToken token, Predicate<HIDDeviceInfo> match) {
			return EnumerateDevices(token, match).ToArray();
		}

		internal static IEnumerable<HIDDeviceInfo> EnumerateDevices() {
			return EnumerateDevices(new CancellationToken());
		}

		internal static IEnumerable<HIDDeviceInfo> EnumerateDevices(CancellationToken token) {
			return EnumerateDevices(token, null);
		}

		internal static IEnumerable<HIDDeviceInfo> EnumerateDevices(Predicate<HIDDeviceInfo> match) {
			return EnumerateDevices(new CancellationToken(), match);
		}

		internal static IEnumerable<HIDDeviceInfo> EnumerateDevices(CancellationToken token, Predicate<HIDDeviceInfo> match) {
			IntPtr hDevInfo = IntPtr.Zero;
			try {
				int index = -1;
				bool result;
				int size;
				HIDDeviceInfo hid;
				
				// Create a new interface data struct and initialize its size
				SP_DEVICE_INTERFACE_DATA diData = new SP_DEVICE_INTERFACE_DATA();
				diData.cbSize = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>();

				// Get a handle to all devices that are part of the HID class
				// Fun fact:  DIGCF_PRESENT worked on my machine just fine.  I reinstalled Vista, and now it no longer finds the Wiimote with that parameter enabled...
				hDevInfo = NativeMethods.SetupDiGetClassDevs(ref HIDGuid, null, IntPtr.Zero, DeviceInfoFlags.DeviceInterface);// | DeviceInfoFlags.Present);

				while (NativeMethods.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref HIDGuid, ++index, ref diData)) {
					hid = new HIDDeviceInfo(diData);
					size = 0;

					if (token.IsCancellationRequested)
						yield break;

					// Get the buffer size for this device detail instance (returned in the size parameter)
					NativeMethods.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref hid.DIData, IntPtr.Zero, 0, out size, IntPtr.Zero);
					
					// Actually get the detail struct
					if (!NativeMethods.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref hid.DIData, ref hid.DIDetail, size, out size, IntPtr.Zero))
						continue;

					//Debug.WriteLine(string.Format("{0}: {1} - {2}", index, hid.Path, Marshal.GetLastWin32Error()));

					// Open the device handle
					if (!hid.OpenHandle())
						continue;

					result = NativeMethods.HidD_GetAttributes(hid.DangerousHandle, ref hid.HIDAttr);
					// Close the device before returning in
					// case the user doesn't want it afterall.
					hid.Close();
					if (result && (match?.Invoke(hid) ?? true))
						yield return hid;
				}
			}
			finally {
				// clean up our list
				if (hDevInfo != IntPtr.Zero)
					NativeMethods.SetupDiDestroyDeviceInfoList(hDevInfo);
			}
		}
	}
}

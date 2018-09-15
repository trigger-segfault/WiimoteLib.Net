//////////////////////////////////////////////////////////////////////////////////
//	HIDImports.cs
//	Managed Wiimote Library
//	Written by Brian Peek (http://www.brianpeek.com/)
//	for MSDN's Coding4Fun (http://msdn.microsoft.com/coding4fun/)
//	Visit http://blogs.msdn.com/coding4fun/archive/2007/03/14/1879033.aspx
//  and http://www.codeplex.com/WiimoteLib
//	for more information
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace WiimoteLib.Native {
	/// <summary>Win32 import information for use with the Wiimote library.</summary>
	internal class NativeMethods {

		[DllImport("ws2_32.dll", SetLastError = true)]
		internal static extern int WSALookupServiceBegin(ref WSAQUERYSET pQuerySet, LookupFlags dwFlags, out IntPtr lphLookup);

		[DllImport("ws2_32.dll", SetLastError = true)]
		public static extern int WSALookupServiceEnd(IntPtr hLookup);

		[DllImport("ws2_32.dll")]
		public static extern int WSAGetLastError();

		[DllImport("irprops.cpl", SetLastError = true)]
		public static extern IntPtr BluetoothFindFirstDevice(ref BLUETOOTH_DEVICE_SEARCH_PARAMS pbtsp, ref BLUETOOTH_DEVICE_INFO pbtdi);

		[DllImport("irprops.cpl", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool BluetoothFindNextDevice(IntPtr hFind, ref BLUETOOTH_DEVICE_INFO pbtdi);

		[DllImport("irprops.cpl", SetLastError = true)]
		public static extern bool BluetoothFindDeviceClose(IntPtr hFind);

		[DllImport("irprops.cpl", SetLastError = true)]
		public static extern int BluetoothGetDeviceInfo(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi);

		[DllImport("irprops.cpl", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int BluetoothRemoveDevice(byte[] pAddress);

		[DllImport("irprops.cpl", SetLastError = true)]
		public static extern int BluetoothEnumerateInstalledServices(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref int pcServices, Guid[] pGuidServices);

		[DllImport("irprops.cpl", SetLastError = true)]
		public static extern int BluetoothSetServiceState(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref Guid pGuidService, BluetoothServiceFlags dwServiceFlags);

		[DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern void HidD_GetHidGuid(out Guid gHid);

		[DllImport("hid.dll")]
		public static extern bool HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

		[DllImport("hid.dll")]
		internal extern static bool HidD_SetOutputReport(
			IntPtr HidDeviceObject,
			byte[] lpReportBuffer,
			int ReportBufferLength);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetupDiGetClassDevs(
			ref Guid ClassGuid,
			[MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
			IntPtr hwndParent,
			DeviceInfoFlags Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInterfaces(
			IntPtr hDevInfo,
			//ref SP_DEVINFO_DATA devInfo,
			IntPtr devInvo,
			ref Guid interfaceClassGuid,
			int memberIndex,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

		/*[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiOpenDeviceInterface(
			IntPtr DeviceInfoSet,
			string DevicePath,
			int OpenFlags,
			out SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiDeleteDeviceInterfaceData(
			IntPtr DeviceInfoSet,
			ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int SetupDiOpenDeviceInfo(
			IntPtr DeviceInfoSet,
			string DeviceInstanceId,
			IntPtr hwndParent,
			int OpenFlags,
			out SP_DEVINFO_DATA DeviceInfoData);*/
		
		// Used for passing IntPtr.Zero and getting the required size
		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(
			IntPtr hDevInfo,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			IntPtr deviceInterfaceDetailData,
			int deviceInterfaceDetailDataSize,
			out int requiredSize,
			IntPtr deviceInfoDate);

		// Used for passing SP_DEVICE_INTERFACE_DETAIL_DATA and getting the details
		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(
			IntPtr hDevInfo,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
			int deviceInterfaceDetailDataSize,
			out int requiredSize,
			IntPtr deviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern ushort SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(
			string fileName,
			[MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
			[MarshalAs(UnmanagedType.U4)] FileShare fileShare,
			IntPtr securityAttributes,
			[MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
			[MarshalAs(UnmanagedType.U4)] EFileAttributes flags,
			IntPtr template);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Native.Windows {
	/// <summary><see cref="WindowsMessage.WM_DEVICECHANGE"/> wParam events.</summary>
	/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/wm-devicechange"/></remarks>
	internal enum DeviceManagementMessage2 {
		/// <summaru>A device has been added to or removed from the system.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devnodes-changed"/></remarks>
		DevNodesChanged = 0x0007,
		/// <summaru>Permission is requested to change the current configuration (dock or undock).</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-querychangeconfig"/></remarks>
		QueryChangeConfig = 0x0017,
		/// <summaru>The current configuration has changed, due to a dock or undock.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-configchanged"/></remarks>
		ConfigChanged = 0x0018,
		/// <summaru>A request to change the current configuration (dock or undock) has been canceled.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-configchangecanceled"/></remarks>
		ConfigChangeCanceled = 0x0019,
		/// <summaru>A device or piece of media has been inserted and is now available.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicearrival"/></remarks>
		DeviceArrival = 0x8000,
		/// <summaru>Permission is requested to remove a device or piece of media. Any application can deny this request and cancel the removal.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicequeryremove"/></remarks>
		DeviceQueryRemove = 0x8001,
		/// <summaru>A request to remove a device or piece of media has been canceled.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicequeryremovefailed"/></remarks>
		DeviceQueryRemoveFailed = 0x8002,
		/// <summaru>A device or piece of media is about to be removed. Cannot be denied.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-deviceremovepending"/></remarks>
		DeviceRemovePending = 0x8003,
		/// <summaru>A device or piece of media has been removed.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-deviceremovecomplete"/></remarks>
		DeviceRemoveComplete = 0x8004,
		/// <summaru>A device-specific event has occurred.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicetypespecific"/></remarks>
		DeviceTypeSpecific = 0x8005,
		/// <summaru>A custom event has occurred.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-customevent"/></remarks>
		CustomEvent = 0x8006,
		/// <summaru>The meaning of this message is user-defined.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-userdefined"/></remarks>
		UserDefined = 0xFFFF,
	}
}

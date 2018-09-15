using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Native.Windows {
	/// <summary><see cref="WindowsMessage.WM_DEVICECHANGE"/> wParam events.</summary>
	/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/wm-devicechange"/></remarks>
	internal enum DeviceManagementMessage {
		/// <summaru>A device has been added to or removed from the system.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devnodes-changed"/></remarks>
		DBT_DEVNODES_CHANGED = 0x0007,
		/// <summaru>Permission is requested to change the current configuration (dock or undock).</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-querychangeconfig"/></remarks>
		DBT_QUERYCHANGECONFIG = 0x0017,
		/// <summaru>The current configuration has changed, due to a dock or undock.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-configchanged"/></remarks>
		DBT_CONFIGCHANGED = 0x0018,
		/// <summaru>A request to change the current configuration (dock or undock) has been canceled.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-configchangecanceled"/></remarks>
		DBT_CONFIGCHANGECANCELED = 0x0019,
		/// <summaru>A device or piece of media has been inserted and is now available.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicearrival"/></remarks>
		DBT_DEVICEARRIVAL = 0x8000,
		/// <summaru>Permission is requested to remove a device or piece of media. Any application can deny this request and cancel the removal.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicequeryremove"/></remarks>
		DBT_DEVICEQUERYREMOVE = 0x8001,
		/// <summaru>A request to remove a device or piece of media has been canceled.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicequeryremovefailed"/></remarks>
		DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
		/// <summaru>A device or piece of media is about to be removed. Cannot be denied.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-deviceremovepending"/></remarks>
		DBT_DEVICEREMOVEPENDING = 0x8003,
		/// <summaru>A device or piece of media has been removed.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-deviceremovecomplete"/></remarks>
		DBT_DEVICEREMOVECOMPLETE = 0x8004,
		/// <summaru>A device-specific event has occurred.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-devicetypespecific"/></remarks>
		DBT_DEVICETYPESPECIFIC = 0x8005,
		/// <summaru>A custom event has occurred.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-customevent"/></remarks>
		DBT_CUSTOMEVENT = 0x8006,
		/// <summaru>The meaning of this message is user-defined.</summaru>
		/// <remarks><see href="https://docs.microsoft.com/en-us/windows/desktop/DevIO/dbt-userdefined"/></remarks>
		DBT_USERDEFINED = 0xFFFF,
	}
}

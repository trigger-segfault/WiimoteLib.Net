using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WiimoteController.Pairing.NativeMethods;

namespace WiimoteController.Pairing {
	public static class WiimotePairing {


		public static bool Pair() {
			IntPtr[] hRadios = new IntPtr[256];
			int nRadios = 0;
			int nPaired = 0;

			///////////////////////////////////////////////////////////////////////
			// Enumerate BT radios
			///////////////////////////////////////////////////////////////////////
			{
				IntPtr hFindRadio;
				BLUETOOTH_FIND_RADIO_PARAMS radioParam;
				
				radioParam.dwSize = Marshal.SizeOf<BLUETOOTH_FIND_RADIO_PARAMS>();

				nRadios = 0;
				hFindRadio = BluetoothFindFirstRadio(ref radioParam, out hRadios[nRadios++]);
				if (hFindRadio != IntPtr.Zero) {
					while (nRadios < 256 && BluetoothFindNextRadio(hFindRadio, out hRadios[nRadios++]));
					BluetoothFindRadioClose(hFindRadio);
				}
				else {
					//ShowErrorCode("Error enumerating radios", GetLastError());
					return false;
				}
				nRadios--;
				Trace.WriteLine($"Found {nRadios} radios");
			}

			//hRadios[0] = InTheHand.Net.Bluetooth.BluetoothRadio.PrimaryRadio.Handle;
			//nRadios = 1;

			///////////////////////////////////////////////////////////////////////
			// Keep looping until we pair with a Wii device
			///////////////////////////////////////////////////////////////////////
			while (nPaired == 0) {
				int radio;

				for (radio = 0; radio < nRadios; radio++) {
					BLUETOOTH_RADIO_INFO radioInfo = new BLUETOOTH_RADIO_INFO();
					IntPtr hFind;
					BLUETOOTH_DEVICE_INFO btdi = new BLUETOOTH_DEVICE_INFO();
					BLUETOOTH_DEVICE_SEARCH_PARAMS srch = new BLUETOOTH_DEVICE_SEARCH_PARAMS();

					radioInfo.dwSize = Marshal.SizeOf<BLUETOOTH_RADIO_INFO>();
					btdi.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>();
					srch.dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_SEARCH_PARAMS>();

					BluetoothGetRadioInfo(hRadios[radio], ref radioInfo);
					
					srch.fReturnAuthenticated = true;
					srch.fReturnRemembered = true;
					srch.fReturnConnected = true;
					srch.fReturnUnknown = true;
					srch.fIssueInquiry = true;
					srch.cTimeoutMultiplier = 2;
					srch.hRadio = hRadios[radio];
					
					hFind = BluetoothFindFirstDevice(ref srch, ref btdi);
					//
					// MessageId: ERROR_NO_MORE_ITEMS
					//
					// MessageText:
					//
					// No more data is available.
					//
					const long ERROR_NO_MORE_ITEMS = 259L;

					if (hFind == IntPtr.Zero) {
						if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS) {
							//printf();
							Trace.WriteLine("No bluetooth devices found.");
						}
						else {
							//ShowErrorCode("Error enumerating devices", GetLastError());
							return false;
						}
					}
					else {
						do {
							Trace.WriteLine($"Found: {btdi.szName}");

							if (btdi.szName != "Nintendo RVL-WBC-01" && btdi.szName != "Nintendo RVL-CNT-01")
								continue;

							string pass = "";
							int pcServices = 16;
							Guid[] guids = new Guid[16];
							bool error = false;

							if (!error) {
								if (btdi.fRemembered) {
									// Make Windows forget pairing
									if (BluetoothRemoveDevice(btdi.Address) != 0) {
										error = true;
									}
									else {
										Trace.WriteLine("Device Removed");
										Thread.Sleep(5000);
										//continue;
										return Pair();
									}
								}
							}

							// MAC address is passphrase
							/*pass[0] = radioInfo.address.rgBytes[0];
							pass[1] = radioInfo.address.rgBytes[1];
							pass[2] = radioInfo.address.rgBytes[2];
							pass[3] = radioInfo.address.rgBytes[3];
							pass[4] = radioInfo.address.rgBytes[4];
							pass[5] = radioInfo.address.rgBytes[5];*/
							/*for (int i = 0; i < 6; i++) {
								printf("%d %d, ", radioInfo.address[i], btdi.Address[i]);
							}
							printf("\n");*/
							pass += (char) btdi.Address[0];
							pass += (char) btdi.Address[1];
							pass += (char) btdi.Address[2];
							pass += (char) btdi.Address[3];
							pass += (char) btdi.Address[4];
							pass += (char) btdi.Address[5];
							
							if (!error) {
								// Pair with Wii device
								if (BluetoothAuthenticateDevice(IntPtr.Zero, hRadios[radio], ref btdi, pass, 6) != 0)
									error = true;
							}

							if (!error) {
								// If this is not done, the Wii device will not remember the pairing
								if (BluetoothEnumerateInstalledServices(hRadios[radio], ref btdi, ref pcServices, guids) != 0)
									error = true;
							}

							if (!error) {
								// Activate service
								Guid uuid = Uuids.HumanInterfaceDeviceServiceClass_UUID;
								if (BluetoothSetServiceState(hRadios[radio], ref btdi, ref uuid, BLUETOOTH_SERVICE_ENABLE) != 0)
									error = true;
							}

							if (!error) {
								nPaired++;
							}
						}
						while (BluetoothFindNextDevice(hFind, ref btdi));
					} // if (hFind == NULL)
				} // for (radio = 0; radio < nRadios; radio++)

				if (nPaired == 0)
					Trace.WriteLine("Retring...");
				Thread.Sleep(1000);
			}

			///////////////////////////////////////////////////////////////////////
			// Clean up
			///////////////////////////////////////////////////////////////////////
			
			{
				int radio;

				for (radio = 0; radio < nRadios; radio++) {
					CloseHandle(hRadios[radio]);
				}
			}
			Trace.WriteLine("=============================================");
			Trace.WriteLine($"{nPaired} Wii devices paired");

			return true;
		}

	}
}

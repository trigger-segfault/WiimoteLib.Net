using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiimoteLib.Native.Windows;

namespace WiimoteLib {
	public static partial class WiimoteManager {

		private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			Message m = new Message() {
				HWnd = hwnd,
				Msg = msg,
				WParam = wParam,
				LParam = lParam,
				Result = IntPtr.Zero,
			};
			WndProc(ref m);
			return m.Result;
		}

		private static void WndProc(ref Message m) {
			WindowsMessage msg = (WindowsMessage) m.Msg;

			switch (msg) {
			case WindowsMessage.WM_DEVICECHANGE:

				break;
			}

		}

		public static void HookWndProc(Form form) {

		}
	}
}

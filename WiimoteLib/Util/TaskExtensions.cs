using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WiimoteLib.Util {
	internal static class TaskExtensions {

		public static void Sleep(this CancellationToken token, int milliseconds) {
			Task.Delay(milliseconds, token).ContinueWith(task => { }).Wait();
		}

		public static void Sleep(this CancellationToken token, TimeSpan timespan) {
			Task.Delay(timespan, token).ContinueWith(task => { }).Wait();
		}
	}
}

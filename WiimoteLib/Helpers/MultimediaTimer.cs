using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {
	public class MultimediaTimer {
		private int _interval;

		private ElapsedTimer3Delegate _elapsedTimer3Handler;

		private ElapsedTimerDelegate _elapsedTimerHandler;

		public MultimediaTimer(int intervalMS, ElapsedTimerDelegate callback) {
			_interval = intervalMS;

			_elapsedTimerHandler = callback;
		}

		~MultimediaTimer() {

		}

		public delegate void ElapsedTimerDelegate();

		public delegate void ElapsedTimer3Delegate(int tick, TimeSpan span);

		//private delegate void TestEventHandler(int tick, TimeSpan span);

		private void Timer3Handler(int id, int msg, IntPtr user, int dw1, int dw2) {

			_elapsedTimerHandler();

		}

		public void Start() {
			TimeBeginPeriod(1);
			mHandler = new TimerEventHandler(Timer3Handler);
			mTimerId = timeSetEvent(_interval, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
			mTestStart = DateTime.Now;
		}

		public void Stop() {
			int err = TimeKillEvent(mTimerId);
			TimeEndPeriod(1);
			mTimerId = 0;
		}

		private int mTimerId;
		private TimerEventHandler mHandler;
		private DateTime mTestStart;

		// P/Invoke declarations
		private delegate void TimerEventHandler(int id, int msg, IntPtr user, int dw1, int dw2);

		private const int TIME_PERIODIC = 1;
		private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
		[DllImport("winmm.dll")]
		private static extern int timeSetEvent(int delay, int resolution,
												TimerEventHandler handler, IntPtr user, int eventType);
		[DllImport("winmm.dll", EntryPoint = "timeKillEvent")]
		private static extern int TimeKillEvent(int id);
		[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
		private static extern int TimeBeginPeriod(int msec);
		[DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
		private static extern int TimeEndPeriod(int msec);
	}
}

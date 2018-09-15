using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {
	public class ATimer {
		/// <summary>
		/// Timer type.
		/// 0 = System.Threading ( default )
		/// 1 = Windows.Forms
		/// 2 = Microtimer ( custom code ).
		/// 3 = Multimedia timer ( windows mm dll ).
		/// </summary>
		private int _timerType;

		private System.Threading.Timer _timer0;

		private System.Windows.Forms.Timer _timer1;

		private MicroTimer _timer2;

		private int _interval;

		private ElapsedTimer0Delegate _elapsedTimer0Handler;

		private ElapsedTimer1Delegate _elapsedTimer1Handler;

		private ElapsedTimer2Delegate _elapsedTimer2Handler;

		private ElapsedTimer3Delegate _elapsedTimer3Handler;

		private ElapsedTimerDelegate _elapsedTimerHandler;

		private bool _enabled;

		public ATimer(int timerType, int intervalMS, ElapsedTimerDelegate callback) {
			_timerType = timerType;

			_interval = intervalMS;

			_elapsedTimerHandler = callback;

			if (timerType == 0) {
				_elapsedTimer0Handler = Timer0Handler;
			}
			else if (timerType == 1) {
				_elapsedTimer1Handler = Timer1Handler;

				_timer1 = new System.Windows.Forms.Timer();

				_timer1.Interval = _interval;

				_timer1.Tick += Timer1Handler;
			}
			else if (timerType == 2) {
				_timer2 = new MicroTimer();

				_timer2.Interval = _interval * 1000;

				_timer2.MicroTimerElapsed += Timer2Handler;

			}
			else if (timerType == 3) {
			}
		}

		public delegate void ElapsedTimerDelegate();

		public delegate void ElapsedTimer0Delegate(object sender);

		public delegate void ElapsedTimer1Delegate(object sender, EventArgs e);

		public delegate void ElapsedTimer2Delegate(object sender, MicroTimerEventArgs e);

		public delegate void ElapsedTimer3Delegate(int tick, TimeSpan span);

		//private delegate void TestEventHandler(int tick, TimeSpan span);

		public void Timer0Handler(object sender) {
			_elapsedTimerHandler();
		}

		public void Timer1Handler(object sender, EventArgs e) {
			_elapsedTimerHandler();
		}

		public void Timer2Handler(object sender, MicroTimerEventArgs e) {
			_elapsedTimerHandler();
		}

		private void Timer3Handler(int id, int msg, IntPtr user, int dw1, int dw2) {

			_elapsedTimerHandler();

		}

		public void Start() {
			if (_timerType == 0) {
				_timer0 = new System.Threading.Timer((new TimerCallback(_elapsedTimer0Handler)),
													  null, 0, _interval);
			}
			else if (_timerType == 1) {
				_timer1.Start();
			}
			else if (_timerType == 2) {
				_timer2.Enabled = true;
			}
			else if (_timerType == 3) {
				timeBeginPeriod(1);
				mHandler = new TimerEventHandler(Timer3Handler);
				mTimerId = timeSetEvent(_interval, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
				mTestStart = DateTime.Now;
				mTestTick = 0;
			}
		}

		public void Stop() {
			if (_timerType == 0) {
				_timer0.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
			}
			else if (_timerType == 1) {
				_timer1.Stop();
			}
			else if (_timerType == 2) {
				_timer2.Stop();
			}
			else if (_timerType == 3) {
				int err = timeKillEvent(mTimerId);
				timeEndPeriod(1);
				mTimerId = 0;
			}
		}

		private int mTimerId;
		private TimerEventHandler mHandler;
		private int mTestTick;
		private DateTime mTestStart;

		// P/Invoke declarations
		private delegate void TimerEventHandler(int id, int msg, IntPtr user, int dw1, int dw2);

		private const int TIME_PERIODIC = 1;
		private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
		[DllImport("winmm.dll")]
		private static extern int timeSetEvent(int delay, int resolution,
												TimerEventHandler handler, IntPtr user, int eventType);
		[DllImport("winmm.dll")]
		private static extern int timeKillEvent(int id);
		[DllImport("winmm.dll")]
		private static extern int timeBeginPeriod(int msec);
		[DllImport("winmm.dll")]
		private static extern int timeEndPeriod(int msec);
	}
}

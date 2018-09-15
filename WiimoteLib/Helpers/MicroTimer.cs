using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WiimoteLib.Helpers {
	/// <summary>
	/// A stopwatch with an added <see cref="MicroStopwatch.ElapsedMicroseconds"/> property.
	/// </summary>
	public class MicroStopwatch : Stopwatch {
		/// <summary>The constant number of microseconds per tick.</summary>
		private readonly double MicroSecPerTick = 1000000d / Frequency;

		/// <summary>Constructs the <see cref="MicroStopwatch"/>.</summary>
		/// 
		/// <exception cref="NotSupportedException">
		/// <see cref="Stopwatch.IsHighResolution"/> is false.
		/// </exception>
		public MicroStopwatch() : this(true) { }

		/// <summary>Constructs the <see cref="MicroStopwatch"/>.</summary>
		/// 
		/// <param name="throwOnLowResolution">
		/// If true and <see cref="Stopwatch.IsHighResolution"/> is false, throws an exception.
		/// </param>
		/// 
		/// <exception cref="NotSupportedException">
		/// <see cref="Stopwatch.IsHighResolution"/> is false and <paramref name="throwOnLowResolution"/> is
		/// true.
		/// </exception>
		public MicroStopwatch(bool throwOnLowResolution) {
			if (!IsHighResolution && throwOnLowResolution)
				throw new NotSupportedException("On this system the high-resolution " +
									"performance counter is not available");
		}

		/// <summary>
		/// Initializes a new <see cref="MicroStopwatch"/> instance, sets the elapsed time property to zero,
		/// and starts measuring elapsed time.
		/// </summary>
		///
		/// <returns>A <see cref="MicroStopwatch"/> that has just begun measuring elapsed time.</returns>  
		/// 
		/// <exception cref="NotSupportedException">
		/// <see cref="Stopwatch.IsHighResolution"/> is false.
		/// </exception>
		public new static MicroStopwatch StartNew() {
			return StartNew(true);
		}

		/// <summary>
		/// Initializes a new <see cref="MicroStopwatch"/> instance, sets the elapsed time property to zero,
		/// and starts measuring elapsed time.
		/// </summary>
		///
		/// <param name="throwOnLowResolution">
		/// If true and <see cref="Stopwatch.IsHighResolution"/> is false, throws an exception.
		/// </param>
		/// <returns>A <see cref="MicroStopwatch"/> that has just begun measuring elapsed time.</returns>  
		/// 
		/// <exception cref="NotSupportedException">
		/// <see cref="Stopwatch.IsHighResolution"/> is false and <paramref name="throwOnLowResolution"/> is
		/// true.
		/// </exception>
		public static MicroStopwatch StartNew(bool throwOnLowResolution) {
			MicroStopwatch watch = new MicroStopwatch(throwOnLowResolution);
			watch.Start();
			return watch;
		}

		/// <summary>
		/// Gets the total elapsed time measured by the current instance, in microseconds.
		/// </summary>
		public long ElapsedMicroseconds {
			get { return (long) (ElapsedTicks * MicroSecPerTick); }
		}
	}

	public delegate void MicroTimerElapsedEventHandler(object sender, MicroTimerEventArgs e);

	public class MicroTimer {
		public event MicroTimerElapsedEventHandler MicroTimerElapsed;

		Thread timerThread = null;
		long ignoreEventIfLateBy = long.MaxValue;
		long timerIntervalInMicroSec = 0;
		bool stopTimer = true;

		public MicroTimer() {
		}

		public MicroTimer(long lTimerIntervalInMicroseconds) {
			Interval = lTimerIntervalInMicroseconds;
		}

		public long Interval {
			get { return timerIntervalInMicroSec; }
			set { timerIntervalInMicroSec = value; }
		}

		public long IgnoreEventIfLateBy {
			get {
				return ignoreEventIfLateBy;
			}
			set {
				if (value == 0)
					ignoreEventIfLateBy = long.MaxValue;
				else
					ignoreEventIfLateBy = value;
			}
		}

		public bool Enabled {
			set {
				if (value)
					Start();
				else
					Stop();
			}
			get {
				return (timerThread != null && timerThread.IsAlive);
			}
		}

		public void Start() {
			if ((timerThread == null || !timerThread.IsAlive) && Interval > 0) {
				stopTimer = false;
				ThreadStart threadStart =
				  () => {
					  NotificationTimer(Interval, IgnoreEventIfLateBy, ref stopTimer);
				  };
				timerThread = new Thread(threadStart);
				timerThread.Priority = ThreadPriority.Highest;
				timerThread.Start();
			}
		}

		public void Stop() {
			stopTimer = true;

			// Don't wait forever if this was called from within the timer thread event
			if (Thread.CurrentThread != timerThread) {
				while (Enabled) {
					Thread.Sleep(1);
				}
			}
		}

		void NotificationTimer(long lTimerInterval,
			long lIgnoreEventIfLateBy, ref bool bStopTimer)
		{
			int nTimerCount = 0;
			long lNextNotification = 0;
			long lCallbackFunctionExecutionTime = 0;

			MicroStopwatch microStopwatch = new MicroStopwatch();
			microStopwatch.Start();

			while (!bStopTimer) {
				lCallbackFunctionExecutionTime =
				  microStopwatch.ElapsedMicroseconds - lNextNotification;
				lNextNotification += lTimerInterval;
				nTimerCount++;
				long lElapsedMicroseconds = 0;

				while ((lElapsedMicroseconds =
						microStopwatch.ElapsedMicroseconds) < lNextNotification) {
				}

				long lTimerLateBy = lElapsedMicroseconds - (nTimerCount * lTimerInterval);

				if (lTimerLateBy < lIgnoreEventIfLateBy) {
					MicroTimerEventArgs microTimerEventArgs =
					  new MicroTimerEventArgs(nTimerCount, lElapsedMicroseconds,
					  lTimerLateBy, lCallbackFunctionExecutionTime);
					MicroTimerElapsed(this, microTimerEventArgs);
				}
			}

			microStopwatch.Stop();
		}
	}

	public class MicroTimerEventArgs : EventArgs {
		// Simple counter, number times timed event (callback function) executed
		public int TimerCount { get; private set; }
		// Time when timed event was called since timer started
		public long ElapsedMicroseconds { get; private set; }
		// How late the timer was compared to when it should have been called
		public long TimerLateBy { get; private set; }
		// The time it took to execute the previous
		// call to the callback function (OnTimedEvent)
		public long CallbackFunctionExecutionTime { get; private set; }

		public MicroTimerEventArgs(int nTimerCount, long lElapsedMicroseconds,
			   long lTimerLateBy, long lCallbackFunctionExecutionTime) {
			TimerCount = nTimerCount;
			ElapsedMicroseconds = lElapsedMicroseconds;
			TimerLateBy = lTimerLateBy;
			CallbackFunctionExecutionTime = lCallbackFunctionExecutionTime;
		}
	}
}

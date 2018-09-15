using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WiimoteLib.Util {
	internal class LockedLocation {
		public string CallerName { get; }
		public string FilePath { get; }
		public string FileName { get; }
		public int LineNumber { get; }

		internal LockedLocation(string caller = null,
								string filePath = null,
								int lineNumber = 0)
		{
			CallerName = caller;
			FilePath = filePath;
			FileName = Path.GetFileName(filePath);
			LineNumber = lineNumber;
		}

		public override string ToString() => $"{CallerName} in {FileName} at {LineNumber}";
	}
	/*internal class LockedObject : IDisposable {
		public object Locked { get; }
		public string CallerName { get; }
		public string FilePath { get; }
		public string FileName { get; }
		public int LineNumber { get; }

		internal LockedObject(object locked,
							string caller = null,
							string filePath = null,
							int lineNumber = 0)
		{
			Locked = locked;
			CallerName = caller;
			FilePath = filePath;
			FileName = Path.GetFileName(filePath);
			LineNumber = lineNumber;
		}

		public override string ToString() => $"{CallerName} in {FileName} at {LineNumber}";

		public void Dispose() {
			Monitor.Exit(Locked);
		}
	}*/
	
	internal class LockTracker {
		private Stack<LockedLocation> lockStack = new Stack<LockedLocation>();

		public string Name { get; }
		public bool TrackUnlocks { get; }
		public bool IsLocked => lockStack.Any();
		public int LockLevel => lockStack.Count;

		public override string ToString() {
			if (!IsLocked)
				return $"{Name} Unlocked";
			else if (TrackUnlocks)
				return $"{Name} LOCKED({lockStack.Count}) {lockStack.Peek()}";
			else
				return $"{Name} LOCKED {lockStack.Peek()}";
		}

		public LockTracker(string name, bool trackUnlocks = true) {
			Name = name;
			TrackUnlocks = trackUnlocks;
		}
		
		public void EnterLock([CallerMemberName] string caller = null,
							  [CallerFilePath] string filePath = null,
							  [CallerLineNumber] int lineNumber = 0)
		{
			if (!TrackUnlocks)
				lockStack.Clear();
			lockStack.Push(new LockedLocation(caller, filePath, lineNumber));
		}
		
		public void ExitLock() {
			if (lockStack.Any())
				lockStack.Pop();
		}
	}

	internal class DebugLockTracker : LockTracker {

		public DebugLockTracker(string name, bool trackUnlocks = true)
			: base(name, trackUnlocks)
		{
		}
		
		[Conditional("DEBUG")]
		public new void EnterLock([CallerMemberName] string caller = null,
								  [CallerFilePath] string filePath = null,
								  [CallerLineNumber] int lineNumber = 0)
		{
			base.EnterLock(caller, filePath, lineNumber);
		}

		[Conditional("DEBUG")]
		public new void ExitLock() {
			base.ExitLock();
		}
	}
}

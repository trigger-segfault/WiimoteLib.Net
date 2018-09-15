using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib.DataTypes;
using WiimoteLib.Events;

namespace WiimoteLib {
	public partial class Wiimote : IDisposable {
		public event EventHandler<WiimoteDisconnectedEventArgs> Disconnected;
		public event EventHandler<WiimoteExceptionEventArgs> WiimoteException;
		public event EventHandler<WiimoteExtensionEventArgs> ExtensionChanged;
		public event EventHandler<WiimoteStateEventArgs> StateChanged;
		public event EventHandler<WiimoteRangeEventArgs> InRange;
		public event EventHandler<WiimoteRangeEventArgs> OutOfRange;

		// Called by Wiimote

		private void RaiseWiimoteException(Exception ex) {
			WiimoteManager.RaiseWiimoteException(this, ex);
			WiimoteException?.Invoke(this, new WiimoteExceptionEventArgs(this, ex));
		}

		private void RaiseExtensionChanged(ExtensionType type, bool inserted) {
			WiimoteManager.RaiseExtensionChanged(this, type, inserted);
			ExtensionChanged?.Invoke(this, new WiimoteExtensionEventArgs(this, type, inserted));
		}

		private void RaiseStateChanged() {
			WiimoteManager.RaiseStateChanged(this);
			StateChanged?.Invoke(this, new WiimoteStateEventArgs(this));
		}

		// Called by Manager

		internal void RaiseDisconnected(DisconnectReason reason) {
			Disconnected?.Invoke(this, new WiimoteDisconnectedEventArgs(this, reason));
		}

		internal void RaiseInRange() {
			InRange?.Invoke(this, new WiimoteRangeEventArgs(this, true));
		}

		internal void RaiseOutOfRange() {
			OutOfRange?.Invoke(this, new WiimoteRangeEventArgs(this, false));
		}
	}
}

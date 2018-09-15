using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.DataTypes {
	/// <summary>Current overall state of the Wiimote and all attachments.</summary>
	[Serializable]
	public class WiimoteState {
		/// <summary>A wiimote state with no settings set.</summary>
		public static readonly WiimoteState EmptyState = new WiimoteState();

		/// <summary>Current calibration information.</summary>
		public AccelCalibrationInfo AccelCalibrationInfo;
		/// <summary>Current state of accelerometers.</summary>
		public AccelState Accel;
		/// <summary>Current state of buttons.</summary>
		public ButtonState Buttons;
		/// <summary>Current state of IR sensors.</summary>
		public IRState IRState;
		/// <summary>Current state of Nunchuk extension.</summary>
		public NunchukState Nunchuk;
		/// <summary>Current state of Classic Controller extension.</summary>
		public ClassicControllerState ClassicController;
		/// <summary>Current state of the MotionPlus controller.</summary>
		public MotionPlusState MotionPlus;
		/// <summary>Current state of Wiimote's status.</summary>
		public StatusState Status;
		/// <summary>Extension controller currently inserted, if any.</summary>
		public ExtensionType ExtensionType;
		/// <summary>Is an extension controller inserted?</summary>
		public bool Extension {
			get => Status.Extension;
			set => Status.Extension = value;
		}

		public ReportType ReportType = ReportType.Buttons;
		public bool ContinuousReport = false;

		/// <summary>
		/// Constructor for WiimoteState class
		/// </summary>
		/*public WiimoteState() {
			IRState.IRSensors = new IRSensor[4];
		}*/
	}
}

//////////////////////////////////////////////////////////////////////////////////
//	DataTypes.cs
//	Managed Wiimote Library
//	Written by Brian Peek (http://www.brianpeek.com/)
//	for MSDN's Coding4Fun (http://msdn.microsoft.com/coding4fun/)
//	Visit http://blogs.msdn.com/coding4fun/archive/2007/03/14/1879033.aspx
//  and http://www.codeplex.com/WiimoteLib
//	for more information
//////////////////////////////////////////////////////////////////////////////////

using System;
using WiimoteLib.Geometry;

namespace WiimoteLib {
	

	

	

	

	

	

	

	///// <summary>
	///// Current state of the Guitar controller
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct GuitarState {
	//	/// <summary>
	//	/// Guitar type
	//	/// </summary>
	//	[DataMember]
	//	public GuitarType GuitarType;
	//	/// <summary>
	//	/// Current button state of the Guitar
	//	/// </summary>
	//	[DataMember]
	//	public GuitarButtonState ButtonState;
	//	/// <summary>
	//	/// Current fret button state of the Guitar
	//	/// </summary>
	//	[DataMember]
	//	public GuitarFretButtonState FretButtonState;
	//	/// <summary>
	//	/// Current touchbar state of the Guitar
	//	/// </summary>
	//	[DataMember]
	//	public GuitarFretButtonState TouchbarState;
	//	/// <summary>
	//	/// Raw joystick position.  Values range between 0 - 63.
	//	/// </summary>
	//	[DataMember]
	//	public Point2I RawJoystick;
	//	/// <summary>
	//	/// Normalized value of joystick position.  Values range between 0.0 - 1.0.
	//	/// </summary>
	//	[DataMember]
	//	public Point2F Joystick;
	//	/// <summary>
	//	/// Raw whammy bar position.  Values range between 0 - 10.
	//	/// </summary>
	//	[DataMember]
	//	public byte RawWhammyBar;
	//	/// <summary>
	//	/// Normalized value of whammy bar position.  Values range between 0.0 - 1.0.
	//	/// </summary>
	//	[DataMember]
	//	public float WhammyBar;
	//}

	///// <summary>
	///// Current fret button state of the Guitar controller
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct GuitarFretButtonState {
	//	/// <summary>
	//	/// Fret buttons
	//	/// </summary>
	//	[DataMember]
	//	public bool Green, Red, Yellow, Blue, Orange;
	//}


	///// <summary>
	///// Current button state of the Guitar controller
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct GuitarButtonState {
	//	/// <summary>
	//	/// Strum bar
	//	/// </summary>
	//	[DataMember]
	//	public bool StrumUp, StrumDown;
	//	/// <summary>
	//	/// Other buttons
	//	/// </summary>
	//	[DataMember]
	//	public bool Minus, Plus;
	//}

	///// <summary>
	///// Current state of the Drums controller
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct DrumsState {
	//	/// <summary>
	//	/// Drum pads
	//	/// </summary>
	//	public bool Red, Green, Blue, Orange, Yellow, Pedal;
	//	/// <summary>
	//	/// Speed at which the pad is hit.  Values range from 0 (very hard) to 6 (very soft)
	//	/// </summary>
	//	public int RedVelocity, GreenVelocity, BlueVelocity, OrangeVelocity, YellowVelocity, PedalVelocity;
	//	/// <summary>
	//	/// Other buttons
	//	/// </summary>
	//	public bool Plus, Minus;
	//	/// <summary>
	//	/// Raw value of analong joystick.  Values range from 0 - 15
	//	/// </summary>
	//	public Point2I RawJoystick;
	//	/// <summary>
	//	/// Normalized value of analog joystick.  Values range from 0.0 - 1.0
	//	/// </summary>
	//	public Point2F Joystick;
	//}

	///// <summary>
	///// Current state of the Wii Fit Balance Board controller
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct BalanceBoardState {
	//	/// <summary>
	//	/// Calibration information for the Balance Board
	//	/// </summary>
	//	[DataMember]
	//	public BalanceBoardCalibrationInfo CalibrationInfo;
	//	/// <summary>
	//	/// Raw values of each sensor
	//	/// </summary>
	//	[DataMember]
	//	public BalanceBoardSensors SensorValuesRaw;
	//	/// <summary>
	//	/// Kilograms per sensor
	//	/// </summary>
	//	[DataMember]
	//	public BalanceBoardSensorsF SensorValuesKg;
	//	/// <summary>
	//	/// Pounds per sensor
	//	/// </summary>
	//	[DataMember]
	//	public BalanceBoardSensorsF SensorValuesLb;
	//	/// <summary>
	//	/// Total kilograms on the Balance Board
	//	/// </summary>
	//	[DataMember]
	//	public float WeightKg;
	//	/// <summary>
	//	/// Total pounds on the Balance Board
	//	/// </summary>
	//	[DataMember]
	//	public float WeightLb;
	//	/// <summary>
	//	/// Center of gravity of Balance Board user
	//	/// </summary>
	//	[DataMember]
	//	public Point2F CenterOfGravity;
	//}

	///// <summary>
	///// Current state of the Taiko Drum (TaTaCon) controller
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct TaikoDrumState {
	//	/// <summary>
	//	/// Drum hit location
	//	/// </summary>
	//	[DataMember]
	//	public bool InnerLeft, InnerRight, OuterLeft, OuterRight;
	//}

	


	///// <summary>
	///// Calibration information
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct BalanceBoardCalibrationInfo {
	//	/// <summary>
	//	/// Calibration information at 0kg
	//	/// </summary>
	//	[DataMember]
	//	public BalanceBoardSensors Kg0;
	//	/// <summary>
	//	/// Calibration information at 17kg
	//	/// </summary>
	//	[DataMember]
	//	public BalanceBoardSensors Kg17;
	//	/// <summary>
	//	/// Calibration information at 34kg
	//	/// </summary>
	//	[DataMember]
	//	public BalanceBoardSensors Kg34;
	//}

	///// <summary>
	///// The 4 sensors on the Balance Board (short values)
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct BalanceBoardSensors {
	//	/// <summary>
	//	/// Sensor at top right
	//	/// </summary>
	//	[DataMember]
	//	public short TopRight;
	//	/// <summary>
	//	/// Sensor at top left
	//	/// </summary>
	//	[DataMember]
	//	public short TopLeft;
	//	/// <summary>
	//	/// Sensor at bottom right
	//	/// </summary>
	//	[DataMember]
	//	public short BottomRight;
	//	/// <summary>
	//	/// Sensor at bottom left
	//	/// </summary>
	//	[DataMember]
	//	public short BottomLeft;
	//}

	///// <summary>
	///// The 4 sensors on the Balance Board (float values)
	///// </summary>
	//[Serializable]
	//[DataContract]
	//public struct BalanceBoardSensorsF {
	//	/// <summary>
	//	/// Sensor at top right
	//	/// </summary>
	//	[DataMember]
	//	public float TopRight;
	//	/// <summary>
	//	/// Sensor at top left
	//	/// </summary>
	//	[DataMember]
	//	public float TopLeft;
	//	/// <summary>
	//	/// Sensor at bottom right
	//	/// </summary>
	//	[DataMember]
	//	public float BottomRight;
	//	/// <summary>
	//	/// Sensor at bottom left
	//	/// </summary>
	//	[DataMember]
	//	public float BottomLeft;
	//}

	

	

	

	

	

	

	

	/// <summary>
	/// The report format in which the Wiimote should return data
	/// </summary>
	//public enum InputReport : byte {
	//	/// <summary>
	//	/// Status report
	//	/// </summary>
	//	Status = 0x20,
	//	/// <summary>
	//	/// Read data from memory location
	//	/// </summary>
	//	ReadData = 0x21,
	//	/// <summary>
	//	/// Register write complete
	//	/// </summary>
	//	AcknowledgeOutputReport = 0x22,
	//	/// <summary>
	//	/// Button data only
	//	/// </summary>
	//	Buttons = 0x30,
	//	/// <summary>
	//	/// Button and accelerometer data
	//	/// </summary>
	//	ButtonsAccel = 0x31,
	//	/// <summary>
	//	/// IR sensor and accelerometer data
	//	/// </summary>
	//	ButtonsAccelIR12 = 0x33,
	//	/// <summary>
	//	/// Button and extension controller data
	//	/// </summary>
	//	ButtonsExt19 = 0x34,
	//	/// <summary>
	//	/// Extension and accelerometer data
	//	/// </summary>
	//	ButtonsAccelExt16 = 0x35,
	//	/// <summary>
	//	/// IR sensor, extension controller and accelerometer data
	//	/// </summary>
	//	ButtonsAccelIR10Ext6 = 0x37,
	//}

	

	///// <summary>
	///// Type of guitar extension: Guitar Hero 3 or Guitar Hero World Tour
	///// </summary>
	//public enum GuitarType {
	//	/// <summary>
	//	///  Guitar Hero 3 guitar controller
	//	/// </summary>
	//	GuitarHero3,
	//	/// <summary>
	//	/// Guitar Hero: World Tour guitar controller
	//	/// </summary>
	//	GuitarHeroWorldTour
	//}

	
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteLib.Native;

namespace WiimoteLib {
	/// <summary>Specifies the size of the input or output report enum.</summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal class ReportSizeAttribute : Attribute {

		/// <summary>The suze of the report in bytes.</summary>
		public int Size { get; }

		/// <summary>Construcots the report size attribute.</summary>
		public ReportSizeAttribute(int size) {
			Size = size;
		}
	}

	/// <summary>Specifies information about a data input report.</summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal class DataReportAttribute : Attribute {


		public bool HasButtons { get; set; } = true;
		public int ButtonsSize => HasButtons ? 2 : 0;
		public int ButtonsOffset => 0;

		public bool HasAccel {
			get => AccelSize != 0;
			set => AccelSize = (value ? 3 : 0);
		}
		public int AccelSize { get; set; } = 0;
		public int AccelOffset => 0;

		public bool HasIR => IRSize != 0;
		public int IRSize { get; set; } = 0;
		public int IROffset => ButtonsSize + AccelSize;

		public bool HasExt => ExtSize != 0;
		public int ExtSize { get; set; } = 0;
		public int ExtOffset => ButtonsSize + AccelSize + IRSize;


		public Interleave Interleave { get; set; } = Interleave.None;
		public int InterleaveIndex => (int) Interleave;
		public bool IsInterleaved => Interleave == Interleave.None;

		public int TotalSize => ButtonsSize + AccelSize + ExtSize + IRSize;
	}

	internal enum Interleave {
		None = -1,
		A = 0,
		B = 1,
	}

	public enum OutputReport : byte {
		[ReportSize(1)]
		Rumble = 0x10,
		[ReportSize(1)]
		LEDs = 0x11,
		[ReportSize(2)]
		InputReportType = 0x12,
		[ReportSize(1)]
		IRPixelClock = 0x13,
		[ReportSize(1)]
		SpeakerEnable = 0x14,
		[ReportSize(1)]
		Status = 0x15,
		[ReportSize(21)]
		WriteMemory = 0x16,
		[ReportSize(6)]
		ReadMemory = 0x17,
		[ReportSize(21)]
		SpeakerData = 0x18,
		[ReportSize(1)]
		SpeakerMute = 0x19,
		[ReportSize(1)]
		IRLogic = 0x1A,
	}
	public enum InputReport : byte {
		[ReportSize(6)]
		Status = 0x20,
		[ReportSize(21)]
		ReadData = 0x21,
		[ReportSize(4)]
		AcknowledgeOutputReport = 0x22,

		[ReportSize(2)]
		[DataReport(HasButtons = true)]
		Buttons = 0x30,

		[ReportSize(5)]
		[DataReport(HasButtons = true, HasAccel = true)]
		ButtonsAccel = 0x31,

		[ReportSize(10)]
		[DataReport(HasButtons = true, ExtSize = 8)]
		ButtonsExt8 = 0x32,

		[ReportSize(17)]
		[DataReport(HasButtons = true, HasAccel = true, IRSize = 12)]
		ButtonsAccelIR12 = 0x33,

		[ReportSize(21)]
		[DataReport(HasButtons = true, ExtSize = 19)]
		ButtonsExt19 = 0x34,

		[ReportSize(21)]
		[DataReport(HasButtons = true, HasAccel = true, ExtSize = 16)]
		ButtonsAccelExt16 = 0x35,

		[ReportSize(21)]
		[DataReport(HasButtons = true, IRSize = 10, ExtSize = 9)]
		ButtonsIR10Ext9 = 0x36,

		[ReportSize(21)]
		[DataReport(HasButtons = true, HasAccel = true, IRSize = 10, ExtSize = 6)]
		ButtonsAccelIR10Ext6 = 0x37,

		[ReportSize(21)]
		[DataReport(ExtSize = 21)]
		Ext21 = 0x3D,

		[ReportSize(21)]
		[DataReport(HasButtons = true, AccelSize = 1, IRSize = 18, Interleave = Interleave.A)]
		InterleavedButtonsAccelIR36_A = 0x3E,
		[ReportSize(21)]
		[DataReport(HasButtons = true, AccelSize = 1, IRSize = 18, Interleave = Interleave.B)]
		InterleavedButtonsAccelIR36_B = 0x3F,
	}

	public enum ReportType {
		Buttons = 0x30,
		ButtonsAccel = 0x31,
		ButtonsExt8 = 0x32,
		ButtonsAccelIR12 = 0x33,
		ButtonsExt19 = 0x34,
		ButtonsAccelExt16 = 0x35,
		ButtonsIR10Ext9 = 0x36,
		ButtonsAccelIR10Ext6 = 0x37,
		Ext21 = 0x3D,
		Interleaved = 0x3E,
	}

	internal class WriteRequest {
		public Wiimote Wiimote { get; }
		public OutputReport Type { get; }
		public byte[] Buffer { get; }
		public int Length { get; }

		public IntPtr Handle => Wiimote.Device.DangerousHandle;
		public FileStream Stream => Wiimote.Device.Stream;

		public WriteRequest(Wiimote wiimote, byte[] buff)
			: this(wiimote, buff, buff.Length)
		{
		}

		public WriteRequest(Wiimote wiimote, byte[] buff, int length) {
			Wiimote = wiimote;
			Type = (OutputReport) buff[0];
			Buffer = buff;
			Length = length;
		}

		public override string ToString() => $"{Wiimote} Write: {Type}";

		public bool Send() {
			if (Wiimote.AltWriteMethod) {
				if (!NativeMethods.HidD_SetOutputReport(Handle, Buffer, Length)) {
					Debug.WriteLine($"Failed to send: {this} HIdD");
					return false;
				}
				return true;
			}
			else {
				try {
					Stream.Write(Buffer, 0, Length);
					return true;
				}
				catch (Exception ex) {
					Debug.WriteLine($"Failed to send: {this} FileWrite: {ex}");
					return false;
				}
			}
		}
	}

	/// <summary>Results from an output report.</summary>
	internal enum WriteResult : byte {
		/// <summary>The output report was successful.</summary>
		Success = 0x00,
		/// <summary>An error occurred.</summary>
		Error = 0x03,
		/// <summary>Possibly returned bt report 16H, 17H, or 18H (WriteMemory, ReadMemory, SpeakerData).</summary>
		Unknown4 = 0x04,
		/// <summary>Possibly returned bt report 12H (DataReportType).</summary>
		Unknown5 = 0x05,
		/// <summary>Possibly returned bt report 16H (WriteMemory).</summary>
		Unknown8 = 0x08,

		/// <summary>Not an real error code.</summary>
		WriteFailed = 0xFD,
		/// <summary>Not an real error code.</summary>
		TimedOut = 0xFE,
		/// <summary>Not an real error code.</summary>
		Unfinished = 0xFF,
	}
}

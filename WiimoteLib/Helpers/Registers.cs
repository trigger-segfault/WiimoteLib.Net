using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib {
	internal static class Registers {
		public const int WiimoteCalibration = 0x00000016;

		public const int IR = 0x04b00030;
		public const int IRSensitivity1 = 0x04b00000;
		public const int IRSensitivity2 = 0x04b0001a;
		public const int IRMode = 0x04b00033;

		public const int Extension = 0x04a40000;
		public const int ExtensionData = 0x04a40008;
		public const int ExtensionInit1 = 0x04a400f0;
		public const int ExtensionInit2 = 0x04a400fb;
		public const int ExtensionType1 = 0x04a400fa;
		public const int ExtensionType2 = 0x04a400fe;
		public const int ExtensionCalibration = 0x04a40020;
		public const int PassthroughCalibration = 0x04a40040;

		public const int MotionPlusEnable = 0x04a600fe;

		public const int MotionPlusDisable = 0x04a400f0;
		public const int MotionPlusCalibration = 0x04a60020;

		public const int Speaker1 = 0x04a20009;
		public const int Speaker2 = 0x04a20008;
		public const int SpeakerConfig = 0x04a20001;
	}
}

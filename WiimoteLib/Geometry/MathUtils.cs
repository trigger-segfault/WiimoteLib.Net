using System;
using System.Collections.Generic;
using System.Text;

namespace WiimoteLib.Geometry {
	internal static class MathUtils {
		public static double DegToRad(double degrees) {
			return degrees * Math.PI / 180;
		}

		public static double RadToDeg(double radians) {
			return radians / Math.PI * 180;
		}
	}
}

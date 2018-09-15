using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Geometry {
	/// <summary>Pitch/Yaw/Roll in raw values.</summary>
	[Serializable]
	public struct PitchYawRollI {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>Returns a pitch/yaw/roll positioned at (0, 0, 0).</summary>
		public static readonly PitchYawRollI Zero = new PitchYawRollI(0, 0, 0);


		//-----------------------------------------------------------------------------
		// Members
		//-----------------------------------------------------------------------------

		/// <summary>The pitch angle.</summary>
		public int Pitch;
		/// <summary>The yaw angle.</summary>
		public int Yaw;
		/// <summary>The roll angle.</summary>
		public int Roll;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		/// <summary>Constructs a <see cref="PitchYawRollI"/> from the specified angles.</summary>
		/// <param name="pitch">The pitch angle to use.</param>
		/// <param name="yaw">The yaw angle to use.</param>
		/// <param name="roll">The roll angle to use.</param>
		public PitchYawRollI(int pitch, int yaw, int roll = 0) {
			Pitch = pitch;
			Yaw   = yaw;
			Roll  = roll;
		}

		/// <summary>Constructs a <see cref="PitchYawRollI"/> from the same angle.</summary>
		/// <param name="uniform">The pitch/yaw/roll angles to use.</param>
		public PitchYawRollI(int uniform) {
			Pitch = uniform;
			Yaw   = uniform;
			Roll  = uniform;
		}


		//-----------------------------------------------------------------------------
		// General
		//-----------------------------------------------------------------------------

		/// <summary>Convert to a human-readable string.</summary>
		/// <returns>A string that represents the pitch/yaw/roll</returns>
		public override string ToString() => $"(Pitch={Pitch} Yaw={Yaw} Roll={Roll})";

		/// <summary>Returns the hash code of this point.</summary>
		public override int GetHashCode() => Pitch.GetHashCode() ^ Yaw.GetHashCode() ^ Roll.GetHashCode();

		/// <summary>Checks if the point is equal to the other point.</summary>
		public override bool Equals(object obj) {
			switch (obj) {
			case PitchYawRollI pyri: return this == pyri;
			case PitchYawRollF pyrf: return this == pyrf;
			default:
				return false;
			}
		}


		//-----------------------------------------------------------------------------
		// Operators
		//-----------------------------------------------------------------------------

		public static PitchYawRollI operator +(PitchYawRollI a) => a;

		public static PitchYawRollI operator -(PitchYawRollI a) => new PitchYawRollI(-a.Pitch, -a.Yaw, -a.Roll);

		public static PitchYawRollI operator ++(PitchYawRollI a) => new PitchYawRollI(++a.Pitch, ++a.Yaw, ++a.Roll);

		public static PitchYawRollI operator --(PitchYawRollI a) => new PitchYawRollI(--a.Pitch, --a.Yaw, --a.Roll);

		//--------------------------------

		public static PitchYawRollI operator +(PitchYawRollI a, PitchYawRollI b) {
			return new PitchYawRollI(a.Pitch + b.Pitch, a.Yaw + b.Yaw, a.Roll + b.Roll);
		}

		public static PitchYawRollI operator +(int a, PitchYawRollI b) {
			return new PitchYawRollI(a + b.Pitch, a + b.Yaw, a + b.Roll);
		}

		public static PitchYawRollI operator +(PitchYawRollI a, int b) {
			return new PitchYawRollI(a.Pitch + b, a.Yaw + b, a.Roll + b);
		}

		public static PitchYawRollF operator +(float a, PitchYawRollI b) {
			return new PitchYawRollF(a + b.Pitch, a + b.Yaw, a + b.Roll);
		}

		public static PitchYawRollF operator +(PitchYawRollI a, float b) {
			return new PitchYawRollF(a.Pitch + b, a.Yaw + b, a.Roll + b);
		}


		public static PitchYawRollI operator -(PitchYawRollI a, PitchYawRollI b) {
			return new PitchYawRollI(a.Pitch - b.Pitch, a.Yaw - b.Yaw, a.Roll - b.Roll);
		}

		public static PitchYawRollI operator -(int a, PitchYawRollI b) {
			return new PitchYawRollI(a - b.Pitch, a - b.Yaw, a - b.Roll);
		}

		public static PitchYawRollI operator -(PitchYawRollI a, int b) {
			return new PitchYawRollI(a.Pitch - b, a.Yaw - b, a.Roll - b);
		}

		public static PitchYawRollF operator -(float a, PitchYawRollI b) {
			return new PitchYawRollF(a - b.Pitch, a - b.Yaw, a - b.Roll);
		}

		public static PitchYawRollF operator -(PitchYawRollI a, float b) {
			return new PitchYawRollF(a.Pitch - b, a.Yaw - b, a.Roll - b);
		}


		public static PitchYawRollI operator *(PitchYawRollI a, PitchYawRollI b) {
			return new PitchYawRollI(a.Pitch * b.Pitch, a.Yaw * b.Yaw, a.Roll * b.Roll);
		}

		public static PitchYawRollI operator *(int a, PitchYawRollI b) {
			return new PitchYawRollI(a * b.Pitch, a * b.Yaw, a * b.Roll);
		}

		public static PitchYawRollI operator *(PitchYawRollI a, int b) {
			return new PitchYawRollI(a.Pitch * b, a.Yaw * b, a.Roll * b);
		}

		public static PitchYawRollF operator *(float a, PitchYawRollI b) {
			return new PitchYawRollF(a * b.Pitch, a * b.Yaw, a * b.Roll);
		}

		public static PitchYawRollF operator *(PitchYawRollI a, float b) {
			return new PitchYawRollF(a.Pitch * b, a.Yaw * b, a.Roll * b);
		}


		public static PitchYawRollI operator /(PitchYawRollI a, PitchYawRollI b) {
			return new PitchYawRollI(a.Pitch / b.Pitch, a.Yaw / b.Yaw, a.Roll / b.Roll);
		}

		public static PitchYawRollI operator /(int a, PitchYawRollI b) {
			return new PitchYawRollI(a / b.Pitch, a / b.Yaw, a / b.Roll);
		}

		public static PitchYawRollI operator /(PitchYawRollI a, int b) {
			return new PitchYawRollI(a.Pitch / b, a.Yaw / b, a.Roll / b);
		}

		public static PitchYawRollF operator /(float a, PitchYawRollI b) {
			return new PitchYawRollF(a / b.Pitch, a / b.Yaw, a / b.Roll);
		}

		public static PitchYawRollF operator /(PitchYawRollI a, float b) {
			return new PitchYawRollF(a.Pitch / b, a.Yaw / b, a.Roll / b);
		}


		public static PitchYawRollI operator %(PitchYawRollI a, PitchYawRollI b) {
			return new PitchYawRollI(a.Pitch % b.Pitch, a.Yaw % b.Yaw, a.Roll % b.Roll);
		}

		public static PitchYawRollI operator %(int a, PitchYawRollI b) {
			return new PitchYawRollI(a % b.Pitch, a % b.Yaw, a % b.Roll);
		}

		public static PitchYawRollI operator %(PitchYawRollI a, int b) {
			return new PitchYawRollI(a.Pitch % b, a.Yaw % b, a.Roll % b);
		}

		public static PitchYawRollF operator %(float a, PitchYawRollI b) {
			return new PitchYawRollF(a % b.Pitch, a % b.Yaw, a % b.Roll);
		}

		public static PitchYawRollF operator %(PitchYawRollI a, float b) {
			return new PitchYawRollF(a.Pitch % b, a.Yaw % b, a.Roll % b);
		}

		//--------------------------------

		public static bool operator ==(PitchYawRollI a, PitchYawRollI b) {
			return (a.Pitch == b.Pitch && a.Yaw == b.Yaw && a.Roll == b.Yaw);
		}

		public static bool operator !=(PitchYawRollI a, PitchYawRollI b) {
			return (a.Pitch != b.Pitch || a.Yaw != b.Yaw || a.Roll != b.Yaw);
		}

		public static bool operator <(PitchYawRollI a, PitchYawRollI b) {
			return (a.Pitch < b.Pitch && a.Yaw < b.Yaw && a.Roll < b.Yaw);
		}

		public static bool operator >(PitchYawRollI a, PitchYawRollI b) {
			return (a.Pitch > b.Pitch && a.Yaw > b.Yaw && a.Roll > b.Yaw);
		}

		public static bool operator <=(PitchYawRollI a, PitchYawRollI b) {
			return (a.Pitch <= b.Pitch && a.Yaw <= b.Yaw && a.Roll <= b.Yaw);
		}

		public static bool operator >=(PitchYawRollI a, PitchYawRollI b) {
			return (a.Pitch >= b.Pitch && a.Yaw >= b.Yaw && a.Roll >= b.Yaw);
		}


		//-----------------------------------------------------------------------------
		// Casting
		//-----------------------------------------------------------------------------

		/*public static implicit operator PitchYawRollRaw(Point2I point) {
			return new PitchYawRollRaw(point, 0);
		}

		public static explicit operator Point2I(PitchYawRollRaw point) {
			return point.XY;
		}*/


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		/// <summary>Gets the coordinate at the specified index.</summary>
		public int this[int index] {
			get {
				switch (index) {
				case 0: return Pitch;
				case 1: return Yaw;
				case 2: return Roll;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
			set {
				switch (index) {
				case 0: Pitch = value; break;
				case 1: Yaw = value; break;
				case 2: Roll = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
		}

		/// <summary>Gets just the 2D coordinates of the 3D point.</summary>
		/*public Point2I XY {
			get => new Point2I(Pitch, Yaw);
			set {
				Pitch = value.Pitch;
				Yaw = value.Yaw;
			}
		}*/

		/// <summary>Returns true if the point is positioned at (0, 0, 0).</summary>
		public bool IsZero => (Pitch == 0 && Yaw == 0 && Roll == 0);

		/// <summary>Returns true if either Pitch, Yaw, or Roll is positioned at 0.</summary>
		public bool IsAnyZero => (Pitch == 0 || Yaw == 0 || Roll == 0);
	}
}

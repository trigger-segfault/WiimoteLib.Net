using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.Geometry {
	/// <summary>Pitch/Yaw/Roll in floating-point angles.</summary>
	[Serializable]
	public struct PitchYawRollF {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>Returns a pitch/yaw/roll positioned at (0, 0, 0).</summary>
		public static readonly PitchYawRollF Zero = new PitchYawRollF(0, 0, 0);


		//-----------------------------------------------------------------------------
		// Members
		//-----------------------------------------------------------------------------

		/// <summary>The pitch angle.</summary>
		public float Pitch;
		/// <summary>The yaw angle.</summary>
		public float Yaw;
		/// <summary>The roll angle.</summary>
		public float Roll;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		/// <summary>Constructs a <see cref="PitchYawRollF"/> from the specified angles.</summary>
		/// <param name="pitch">The pitch angle to use.</param>
		/// <param name="yaw">The yaw angle to use.</param>
		/// <param name="roll">The roll angle to use.</param>
		public PitchYawRollF(float pitch, float yaw, float roll = 0) {
			Pitch = pitch;
			Yaw   = yaw;
			Roll  = roll;
		}

		/// <summary>Constructs a <see cref="PitchYawRollF"/> from the same angle.</summary>
		/// <param name="uniform">The pitch/yaw/roll angles to use.</param>
		public PitchYawRollF(float uniform) {
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
			default: return false;
			}
		}


		//-----------------------------------------------------------------------------
		// Operators
		//-----------------------------------------------------------------------------

		public static PitchYawRollF operator +(PitchYawRollF a) => a;

		public static PitchYawRollF operator -(PitchYawRollF a) => new PitchYawRollF(-a.Pitch, -a.Yaw, -a.Roll);

		public static PitchYawRollF operator ++(PitchYawRollF a) => new PitchYawRollF(++a.Pitch, ++a.Yaw, ++a.Roll);

		public static PitchYawRollF operator --(PitchYawRollF a) => new PitchYawRollF(--a.Pitch, --a.Yaw, --a.Roll);

		//--------------------------------

		public static PitchYawRollF operator +(PitchYawRollF a, PitchYawRollF b) {
			return new PitchYawRollF(a.Pitch + b.Pitch, a.Yaw + b.Yaw, a.Roll + b.Roll);
		}

		public static PitchYawRollF operator +(float a, PitchYawRollF b) {
			return new PitchYawRollF(a + b.Pitch, a + b.Yaw, a + b.Roll);
		}

		public static PitchYawRollF operator +(PitchYawRollF a, float b) {
			return new PitchYawRollF(a.Pitch + b, a.Yaw + b, a.Roll + b);
		}

		public static PitchYawRollF operator -(PitchYawRollF a, PitchYawRollF b) {
			return new PitchYawRollF(a.Pitch - b.Pitch, a.Yaw - b.Yaw, a.Roll - b.Roll);
		}

		public static PitchYawRollF operator -(float a, PitchYawRollF b) {
			return new PitchYawRollF(a - b.Pitch, a - b.Yaw, a - b.Roll);
		}

		public static PitchYawRollF operator -(PitchYawRollF a, float b) {
			return new PitchYawRollF(a.Pitch - b, a.Yaw - b, a.Roll - b);
		}

		public static PitchYawRollF operator *(PitchYawRollF a, PitchYawRollF b) {
			return new PitchYawRollF(a.Pitch * b.Pitch, a.Yaw * b.Yaw, a.Roll * b.Roll);
		}

		public static PitchYawRollF operator *(float a, PitchYawRollF b) {
			return new PitchYawRollF(a * b.Pitch, a * b.Yaw, a * b.Roll);
		}

		public static PitchYawRollF operator *(PitchYawRollF a, float b) {
			return new PitchYawRollF(a.Pitch * b, a.Yaw * b, a.Roll * b);
		}

		public static PitchYawRollF operator /(PitchYawRollF a, PitchYawRollF b) {
			return new PitchYawRollF(a.Pitch / b.Pitch, a.Yaw / b.Yaw, a.Roll / b.Roll);
		}

		public static PitchYawRollF operator /(float a, PitchYawRollF b) {
			return new PitchYawRollF(a / b.Pitch, a / b.Yaw, a / b.Roll);
		}

		public static PitchYawRollF operator /(PitchYawRollF a, float b) {
			return new PitchYawRollF(a.Pitch / b, a.Yaw / b, a.Roll / b);
		}

		public static PitchYawRollF operator %(PitchYawRollF a, PitchYawRollF b) {
			return new PitchYawRollF(a.Pitch % b.Pitch, a.Yaw % b.Yaw, a.Roll % b.Roll);
		}

		public static PitchYawRollF operator %(float a, PitchYawRollF b) {
			return new PitchYawRollF(a % b.Pitch, a % b.Yaw, a % b.Roll);
		}

		public static PitchYawRollF operator %(PitchYawRollF a, float b) {
			return new PitchYawRollF(a.Pitch % b, a.Yaw % b, a.Roll % b);
		}

		//--------------------------------

		public static bool operator ==(PitchYawRollF a, PitchYawRollF b) {
			return (a.Pitch == b.Pitch && a.Yaw == b.Yaw && a.Roll == b.Yaw);
		}

		public static bool operator !=(PitchYawRollF a, PitchYawRollF b) {
			return (a.Pitch != b.Pitch || a.Yaw != b.Yaw || a.Roll != b.Yaw);
		}

		public static bool operator <(PitchYawRollF a, PitchYawRollF b) {
			return (a.Pitch < b.Pitch && a.Yaw < b.Yaw && a.Roll < b.Yaw);
		}

		public static bool operator >(PitchYawRollF a, PitchYawRollF b) {
			return (a.Pitch > b.Pitch && a.Yaw > b.Yaw && a.Roll > b.Yaw);
		}

		public static bool operator <=(PitchYawRollF a, PitchYawRollF b) {
			return (a.Pitch <= b.Pitch && a.Yaw <= b.Yaw && a.Roll <= b.Yaw);
		}

		public static bool operator >=(PitchYawRollF a, PitchYawRollF b) {
			return (a.Pitch >= b.Pitch && a.Yaw >= b.Yaw && a.Roll >= b.Yaw);
		}


		//-----------------------------------------------------------------------------
		// Casting
		//-----------------------------------------------------------------------------

		/// <summary>Casts the <see cref="PitchYawRollI"/> to a <see cref="PitchYawRollF"/>.</summary>
		public static implicit operator PitchYawRollF(PitchYawRollI pyr) {
			return new PitchYawRollF(pyr.Pitch, pyr.Yaw, pyr.Roll);
		}

		/// <summary>Casts the <see cref="PitchYawRollF"/> to a <see cref="PitchYawRollI"/>.</summary>
		public static explicit operator PitchYawRollI(PitchYawRollF pyr) {
			return new PitchYawRollI((int) pyr.Pitch, (int) pyr.Yaw, (int) pyr.Roll);
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		/// <summary>Gets the angles at the specified index.</summary>
		public float this[int index] {
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

		/// <summary>Gets the coordinates in 3D space (assuming Forward is +Y).</summary>
		public Point3F XYZ {
			get {
				double pitch = MathUtils.DegToRad(Pitch);
				double yaw = MathUtils.DegToRad(Yaw);
				return new Point3F(
					(float) (Math.Cos(yaw) * Math.Cos(pitch)),
					(float) (Math.Sin(yaw) * Math.Cos(pitch)),
					(float) (Math.Sin(pitch)));
			}
		}

		/// <summary>Gets the pitch/yaw/roll in radians.</summary>
		public PitchYawRollF Radians {
			get {
				return new PitchYawRollF(
					(float) MathUtils.DegToRad(Pitch),
					(float) MathUtils.DegToRad(Yaw),
					(float) MathUtils.DegToRad(Roll));
			}
		}

		/// <summary>Gets the pitch/yaw/roll in degrees.</summary>
		public PitchYawRollF Degrees {
			get {
				return new PitchYawRollF(
					(float) MathUtils.RadToDeg(Pitch),
					(float) MathUtils.RadToDeg(Yaw),
					(float) MathUtils.RadToDeg(Roll));
			}
		}
		
		/// <summary>Returns true if the point is positioned at (0, 0, 0).</summary>
		public bool IsZero => (Pitch == 0 && Yaw == 0 && Roll == 0);

		/// <summary>Returns true if either Pitch, Yaw, or Roll is positioned at 0.</summary>
		public bool IsAnyZero => (Pitch == 0 || Yaw == 0 || Roll == 0);
	}
}

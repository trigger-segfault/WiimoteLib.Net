using System;

namespace WiimoteLib.Geometry {
	/// <summary>Point structure for floating point 3D positions (X, Y, Z).</summary>
	[Serializable]
	public struct Point3F {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>Returns a point positioned at (0, 0, 0).</summary>
		public static readonly Point3F Zero = new Point3F(0f, 0f, 0f);
		/// <summary>Returns a point positioned at (0.5, 0.5, 0.5).</summary>
		public static readonly Point3F Half = new Point3F(0.5f, 0.5f, 0.5f);
		/// <summary>Returns a point positioned at (0.5, 0, 0).</summary>
		public static readonly Point3F HalfX = new Point3F(0.5f, 0f, 0f);
		/// <summary>Returns a point positioned at (0, 0.5, 0).</summary>
		public static readonly Point3F HalfY = new Point3F(0f, 0.5f, 0f);
		/// <summary>Returns a point positioned at (1, 1, 1).</summary>
		public static readonly Point3F One = new Point3F(1f, 1f, 1f);
		/// <summary>Returns a point positioned at (1, 0, 0).</summary>
		public static readonly Point3F OneX = new Point3F(1f, 0f, 0f);
		/// <summary>Returns a point positioned at (0, 1, 0).</summary>
		public static readonly Point3F OneY = new Point3F(0f, 1f, 0f);


		//-----------------------------------------------------------------------------
		// Members
		//-----------------------------------------------------------------------------

		/// <summary>X coordinate of this point.</summary>
		public float X;
		/// <summary>Y coordinate of this point.</summary>
		public float Y;
		/// <summary>Z coordinate of this point.</summary>
		public float Z;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		/// <summary>Constructs a <see cref="Point3F"/> from the X, Y, and Z coordinates.</summary>
		/// <param name="x">The X coordinate to use.</param>
		/// <param name="y">The Y coordinate to use.</param>
		/// <param name="z">The Z coordinate to use.</param>
		public Point3F(float x, float y, float z) {
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>Constructs a <see cref="Point3F"/> from the same coordinates.</summary>
		/// <param name="uniform">The X, Y, and Z coordinate to use.</param>
		public Point3F(float uniform) {
			X = uniform;
			Y = uniform;
			Z = uniform;
		}

		/// <summary>Constructs a <see cref="Point3F"/> from a <see cref="Point2F"/>
		/// and Z coordinate.</summary>
		/// <param name="xy">The X and Y coordinates as a point.</param>
		/// <param name="z">The Z coordinate to use.</param>
		public Point3F(Point2F xy, float z) {
			X = xy.X;
			Y = xy.Y;
			Z = z;
		}


		//-----------------------------------------------------------------------------
		// General
		//-----------------------------------------------------------------------------

		/// <summary>Convert to a human-readable string.</summary>
		/// <returns>A string that represents the point</returns>
		public override string ToString() => $"(X={X} Y={Y})";

		/// <summary>Returns the hash code of this point.</summary>
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
		
		/// <summary>Checks if the point is equal to the other point.</summary>
		public override bool Equals(object obj) {
			switch (obj) {
			case Point2I pt2i: return this == pt2i;
			case Point2F pt2f: return this == pt2f;
			case Point3I pt3i: return this == pt3i;
			case Point3F pt3f: return this == pt3f;
			default: return false;
			}
		}


		//-----------------------------------------------------------------------------
		// Operators
		//-----------------------------------------------------------------------------

		public static Point3F operator +(Point3F a) => a;

		public static Point3F operator -(Point3F a) => new Point3F(-a.X, -a.Y, -a.Z);

		public static Point3F operator ++(Point3F a) => new Point3F(++a.X, ++a.Y, ++a.Z);

		public static Point3F operator --(Point3F a) => new Point3F(--a.X, --a.Y, --a.Z);

		//--------------------------------

		public static Point3F operator +(Point3F a, Point3F b) {
			return new Point3F(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Point3F operator +(float a, Point3F b) {
			return new Point3F(a + b.X, a + b.Y, a + b.Z);
		}

		public static Point3F operator +(Point3F a, float b) {
			return new Point3F(a.X + b, a.Y + b, a.Z + b);
		}

		public static Point3F operator -(Point3F a, Point3F b) {
			return new Point3F(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Point3F operator -(float a, Point3F b) {
			return new Point3F(a - b.X, a - b.Y, a - b.Z);
		}

		public static Point3F operator -(Point3F a, float b) {
			return new Point3F(a.X - b, a.Y - b, a.Z - b);
		}

		public static Point3F operator *(Point3F a, Point3F b) {
			return new Point3F(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}

		public static Point3F operator *(float a, Point3F b) {
			return new Point3F(a * b.X, a * b.Y, a * b.Z);
		}

		public static Point3F operator *(Point3F a, float b) {
			return new Point3F(a.X * b, a.Y * b, a.Z * b);
		}

		public static Point3F operator /(Point3F a, Point3F b) {
			return new Point3F(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
		}

		public static Point3F operator /(float a, Point3F b) {
			return new Point3F(a / b.X, a / b.Y, a / b.Z);
		}

		public static Point3F operator /(Point3F a, float b) {
			return new Point3F(a.X / b, a.Y / b, a.Z / b);
		}

		public static Point3F operator %(Point3F a, Point3F b) {
			return new Point3F(a.X % b.X, a.Y % b.Y, a.Z % b.Z);
		}

		public static Point3F operator %(float a, Point3F b) {
			return new Point3F(a % b.X, a % b.Y, a % b.Z);
		}

		public static Point3F operator %(Point3F a, float b) {
			return new Point3F(a.X % b, a.Y % b, a.Z % b);
		}

		//--------------------------------

		public static bool operator ==(Point3F a, Point3F b) {
			return (a.X == b.X && a.Y == b.Y && a.Z == b.Y);
		}

		public static bool operator !=(Point3F a, Point3F b) {
			return (a.X != b.X || a.Y != b.Y || a.Z != b.Y);
		}

		public static bool operator <(Point3F a, Point3F b) {
			return (a.X < b.X && a.Y < b.Y && a.Z < b.Y);
		}

		public static bool operator >(Point3F a, Point3F b) {
			return (a.X > b.X && a.Y > b.Y && a.Z > b.Y);
		}

		public static bool operator <=(Point3F a, Point3F b) {
			return (a.X <= b.X && a.Y <= b.Y && a.Z <= b.Y);
		}

		public static bool operator >=(Point3F a, Point3F b) {
			return (a.X >= b.X && a.Y >= b.Y && a.Z >= b.Y);
		}


		//-----------------------------------------------------------------------------
		// Casting
		//-----------------------------------------------------------------------------

		/// <summary>Casts the <see cref="Point3I"/> to a <see cref="Point3F"/>.</summary>
		public static implicit operator Point3F(Point3I point) {
			return new Point3F(point.X, point.Y, point.Z);
		}

		/// <summary>Casts the <see cref="Point3F"/> to a <see cref="Point3I"/>.</summary>
		public static explicit operator Point3I(Point3F point) {
			return new Point3I((int) point.X, (int) point.Y, (int) point.Z);
		}

		/// <summary>Casts the <see cref="Point2I"/> to a <see cref="Point3F"/>.</summary>
		public static implicit operator Point3F(Point2I point) {
			return new Point3F(point, 0);
		}

		/// <summary>Casts the <see cref="Point3F"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(Point3F point) {
			return new Point2I((int) point.X, (int) point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a <see cref="Point3F"/>.</summary>
		public static implicit operator Point3F(Point2F point) {
			return new Point3F(point, 0);
		}

		/// <summary>Casts the <see cref="Point3F"/> to a <see cref="Point2F"/>.</summary>
		public static explicit operator Point2F(Point3F point) {
			return new Point2F(point.X, point.Y);
		}

		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------
		

		/// <summary>Gets or sets the length of the point.</summary>
		public float Length {
			get { return (float) Math.Sqrt((X * X) + (Y * Y) + (Z * Z)); }
			set {
				if (!IsZero) {
					float oldLength = Length;
					X *= value / oldLength;
					Y *= value / oldLength;
					Z *= value / oldLength;
				}
				else {
					X = value;
					Y = 0f;
					Z = 0f;
				}
			}
		}

		/// <summary>Gets the squared length of the point.</summary>
		public float LengthSquared {
			get { return ((X * X) + (Y * Y) + (Z * Z)); }
		}

		/// <summary>Gets the coordinate at the specified index.</summary>
		public float this[int index] {
			get {
				switch (index) {
				case 0: return X;
				case 1: return Y;
				case 2: return Z;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
			set {
				switch (index) {
				case 0: X = value; break;
				case 1: Y = value; break;
				case 2: Z = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
		}

		/// <summary>Gets just the 2D coordinates of the 3D point.</summary>
		public Point2F XY {
			get => new Point2F(X, Y);
			set {
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>Returns true if the point is positioned at (0, 0, 0).</summary>
		public bool IsZero => (X == 0f && Y == 0f && Z == 0f);

		/// <summary>Returns true if either X, Y, or Z is positioned at 0.</summary>
		public bool IsAnyZero => (X == 0f || Y == 0f || Z == 0f);
	}
}

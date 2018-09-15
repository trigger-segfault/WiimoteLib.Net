using System;

namespace WiimoteLib.Geometry {
	/// <summary>Point structure for integer 3D positions (X, Y, Z).</summary>
	[Serializable]
	public struct Point3I {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>Returns a point positioned at (0, 0, 0).</summary>
		public static readonly Point3I Zero = new Point3I(0, 0, 0);
		/// <summary>Returns a point positioned at (1, 1, 1).</summary>
		public static readonly Point3I One = new Point3I(1, 1, 1);
		/// <summary>Returns a point positioned at (1, 0, 0).</summary>
		public static readonly Point3I OneX = new Point3I(1, 0, 0);
		/// <summary>Returns a point positioned at (0, 1, 0).</summary>
		public static readonly Point3I OneY = new Point3I(0, 1, 0);


		//-----------------------------------------------------------------------------
		// Members
		//-----------------------------------------------------------------------------

		/// <summary>X coordinate of this point.</summary>
		public int X;
		/// <summary>Y coordinate of this point.</summary>
		public int Y;
		/// <summary>Z coordinate of this point.</summary>
		public int Z;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		/// <summary>Constructs a <see cref="Point3I"/> from the X, Y, and Z coordinates.</summary>
		/// <param name="x">The X coordinate to use.</param>
		/// <param name="y">The Y coordinate to use.</param>
		/// <param name="z">The Z coordinate to use.</param>
		public Point3I(int x, int y, int z) {
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>Constructs a <see cref="Point3I"/> from the same coordinates.</summary>
		/// <param name="uniform">The X, Y, and Z coordinate to use.</param>
		public Point3I(int uniform) {
			X = uniform;
			Y = uniform;
			Z = uniform;
		}

		/// <summary>Constructs a <see cref="Point3I"/> from a <see cref="Point2I"/>
		/// and Z coordinate.</summary>
		/// <param name="xy">The X and Y coordinates as a point.</param>
		/// <param name="z">The Z coordinate to use.</param>
		public Point3I(Point2I xy, int z) {
			X = xy.X;
			Y = xy.Y;
			Z = z;
		}


		//-----------------------------------------------------------------------------
		// General
		//-----------------------------------------------------------------------------

		/// <summary>Convert to a human-readable string.</summary>
		/// <returns>A string that represents the point</returns>
		public override string ToString() => $"(X={X} Y={Y} Z={Z})";

		/// <summary>Returns the hash code of this point.</summary>
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
		
		/// <summary>Checks if the point is equal to the other point.</summary>
		public override bool Equals(object obj) {
			switch (obj) {
			case Point2I pt2i: return this == pt2i;
			case Point2F pt2f: return this == (Point3F) pt2f;
			case Point3I pt3i: return this == pt3i;
			case Point3F pt3f: return this == pt3f;
			default: return false;
			}
		}


		//-----------------------------------------------------------------------------
		// Operators
		//-----------------------------------------------------------------------------

		public static Point3I operator +(Point3I a) => a;

		public static Point3I operator -(Point3I a) => new Point3I(-a.X, -a.Y, -a.Z);

		public static Point3I operator ++(Point3I a) => new Point3I(++a.X, ++a.Y, ++a.Z);

		public static Point3I operator --(Point3I a) => new Point3I(--a.X, --a.Y, --a.Z);

		//--------------------------------

		public static Point3I operator +(Point3I a, Point3I b) {
			return new Point3I(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Point3I operator +(int a, Point3I b) {
			return new Point3I(a + b.X, a + b.Y, a + b.Z);
		}

		public static Point3I operator +(Point3I a, int b) {
			return new Point3I(a.X + b, a.Y + b, a.Z + b);
		}

		public static Point3F operator +(float a, Point3I b) {
			return new Point3F(a + b.X, a + b.Y, a + b.Z);
		}

		public static Point3F operator +(Point3I a, float b) {
			return new Point3F(a.X + b, a.Y + b, a.Z + b);
		}


		public static Point3I operator -(Point3I a, Point3I b) {
			return new Point3I(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Point3I operator -(int a, Point3I b) {
			return new Point3I(a - b.X, a - b.Y, a - b.Z);
		}

		public static Point3I operator -(Point3I a, int b) {
			return new Point3I(a.X - b, a.Y - b, a.Z - b);
		}

		public static Point3F operator -(float a, Point3I b) {
			return new Point3F(a - b.X, a - b.Y, a - b.Z);
		}

		public static Point3F operator -(Point3I a, float b) {
			return new Point3F(a.X - b, a.Y - b, a.Z - b);
		}


		public static Point3I operator *(Point3I a, Point3I b) {
			return new Point3I(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}

		public static Point3I operator *(int a, Point3I b) {
			return new Point3I(a * b.X, a * b.Y, a * b.Z);
		}

		public static Point3I operator *(Point3I a, int b) {
			return new Point3I(a.X * b, a.Y * b, a.Z * b);
		}

		public static Point3F operator *(float a, Point3I b) {
			return new Point3F(a * b.X, a * b.Y, a * b.Z);
		}

		public static Point3F operator *(Point3I a, float b) {
			return new Point3F(a.X * b, a.Y * b, a.Z * b);
		}


		public static Point3I operator /(Point3I a, Point3I b) {
			return new Point3I(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
		}

		public static Point3I operator /(int a, Point3I b) {
			return new Point3I(a / b.X, a / b.Y, a / b.Z);
		}

		public static Point3I operator /(Point3I a, int b) {
			return new Point3I(a.X / b, a.Y / b, a.Z / b);
		}

		public static Point3F operator /(float a, Point3I b) {
			return new Point3F(a / b.X, a / b.Y, a / b.Z);
		}

		public static Point3F operator /(Point3I a, float b) {
			return new Point3F(a.X / b, a.Y / b, a.Z / b);
		}


		public static Point3I operator %(Point3I a, Point3I b) {
			return new Point3I(a.X % b.X, a.Y % b.Y, a.Z % b.Z);
		}

		public static Point3I operator %(int a, Point3I b) {
			return new Point3I(a % b.X, a % b.Y, a % b.Z);
		}

		public static Point3I operator %(Point3I a, int b) {
			return new Point3I(a.X % b, a.Y % b, a.Z % b);
		}

		public static Point3F operator %(float a, Point3I b) {
			return new Point3F(a % b.X, a % b.Y, a % b.Z);
		}

		public static Point3F operator %(Point3I a, float b) {
			return new Point3F(a.X % b, a.Y % b, a.Z % b);
		}

		//--------------------------------

		public static bool operator ==(Point3I a, Point3I b) {
			return (a.X == b.X && a.Y == b.Y && a.Z == b.Y);
		}

		public static bool operator !=(Point3I a, Point3I b) {
			return (a.X != b.X || a.Y != b.Y || a.Z != b.Y);
		}

		public static bool operator <(Point3I a, Point3I b) {
			return (a.X < b.X && a.Y < b.Y && a.Z < b.Y);
		}

		public static bool operator >(Point3I a, Point3I b) {
			return (a.X > b.X && a.Y > b.Y && a.Z > b.Y);
		}

		public static bool operator <=(Point3I a, Point3I b) {
			return (a.X <= b.X && a.Y <= b.Y && a.Z <= b.Y);
		}

		public static bool operator >=(Point3I a, Point3I b) {
			return (a.X >= b.X && a.Y >= b.Y && a.Z >= b.Y);
		}


		//-----------------------------------------------------------------------------
		// Casting
		//-----------------------------------------------------------------------------

		/// <summary>Casts the <see cref="Point2I"/> to a <see cref="Point3I"/>.</summary>
		public static implicit operator Point3I(Point2I point) {
			return new Point3I(point, 0);
		}

		/// <summary>Casts the <see cref="Point3I"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(Point3I point) {
			return point.XY;
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		/// <summary>Gets the coordinate at the specified index.</summary>
		public int this[int index] {
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
		public Point2I XY {
			get => new Point2I(X, Y);
			set {
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>Returns true if the point is positioned at (0, 0, 0).</summary>
		public bool IsZero => (X == 0 && Y == 0 && Z == 0);

		/// <summary>Returns true if either X, Y, or Z is positioned at 0.</summary>
		public bool IsAnyZero => (X == 0 || Y == 0 || Z == 0);
	}
}

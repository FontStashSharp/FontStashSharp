// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace FontStashSharp
{
	/// <summary>
	/// Describes a 32-bit packed color.
	/// </summary>
	[DataContract]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct FssColor : IEquatable<FssColor>
	{
		// Stored as RGBA with R in the least significant octet:
		// |-------|-------|-------|-------
		// A       B       G       R
		private uint _packedValue;

		/// <summary>
		/// Constructs an RGBA color from a packed value.
		/// The value is a 32-bit unsigned integer, with R in the least significant octet.
		/// </summary>
		/// <param name="packedValue">The packed value.</param>
		[CLSCompliant(false)]
		public FssColor(uint packedValue)
		{
			_packedValue = packedValue;
		}

		/// <summary>
		/// Constructs an RGBA color from a <see cref="FssColor"/> and an alpha value.
		/// </summary>
		/// <param name="color">A <see cref="FssColor"/> for RGB values of new <see cref="FssColor"/> instance.</param>
		/// <param name="alpha">The alpha component value from 0 to 255.</param>
		public FssColor(FssColor color, int alpha)
		{
			if ((alpha & 0xFFFFFF00) != 0)
			{
				var clampedA = (uint)alpha.Clamp(Byte.MinValue, Byte.MaxValue);

				_packedValue = (color._packedValue & 0x00FFFFFF) | (clampedA << 24);
			}
			else
			{
				_packedValue = (color._packedValue & 0x00FFFFFF) | ((uint)alpha << 24);
			}
		}

		/// <summary>
		/// Constructs an RGBA color from color and alpha value.
		/// </summary>
		/// <param name="color">A <see cref="FssColor"/> for RGB values of new <see cref="FssColor"/> instance.</param>
		/// <param name="alpha">Alpha component value from 0.0f to 1.0f.</param>
		public FssColor(FssColor color, float alpha) :
			this(color, (int)(alpha * 255))
		{
		}

		/// <summary>
		/// Constructs an RGBA color from scalars representing red, green and blue values. Alpha value will be opaque.
		/// </summary>
		/// <param name="r">Red component value from 0.0f to 1.0f.</param>
		/// <param name="g">Green component value from 0.0f to 1.0f.</param>
		/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
		public FssColor(float r, float g, float b)
			: this((int)(r * 255), (int)(g * 255), (int)(b * 255))
		{
		}

		/// <summary>
		/// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
		/// </summary>
		/// <param name="r">Red component value from 0.0f to 1.0f.</param>
		/// <param name="g">Green component value from 0.0f to 1.0f.</param>
		/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
		/// <param name="alpha">Alpha component value from 0.0f to 1.0f.</param>
		public FssColor(float r, float g, float b, float alpha)
			: this((int)(r * 255), (int)(g * 255), (int)(b * 255), (int)(alpha * 255))
		{
		}

		/// <summary>
		/// Constructs an RGBA color from scalars representing red, green and blue values. Alpha value will be opaque.
		/// </summary>
		/// <param name="r">Red component value from 0 to 255.</param>
		/// <param name="g">Green component value from 0 to 255.</param>
		/// <param name="b">Blue component value from 0 to 255.</param>
		public FssColor(int r, int g, int b)
		{
			_packedValue = 0xFF000000; // A = 255

			if (((r | g | b) & 0xFFFFFF00) != 0)
			{
				var clampedR = (uint)r.Clamp(Byte.MinValue, Byte.MaxValue);
				var clampedG = (uint)g.Clamp(Byte.MinValue, Byte.MaxValue);
				var clampedB = (uint)b.Clamp(Byte.MinValue, Byte.MaxValue);

				_packedValue |= (clampedB << 16) | (clampedG << 8) | (clampedR);
			}
			else
			{
				_packedValue |= ((uint)b << 16) | ((uint)g << 8) | ((uint)r);
			}
		}

		/// <summary>
		/// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
		/// </summary>
		/// <param name="r">Red component value from 0 to 255.</param>
		/// <param name="g">Green component value from 0 to 255.</param>
		/// <param name="b">Blue component value from 0 to 255.</param>
		/// <param name="alpha">Alpha component value from 0 to 255.</param>
		public FssColor(int r, int g, int b, int alpha)
		{
			if (((r | g | b | alpha) & 0xFFFFFF00) != 0)
			{
				var clampedR = (uint)r.Clamp(Byte.MinValue, Byte.MaxValue);
				var clampedG = (uint)g.Clamp(Byte.MinValue, Byte.MaxValue);
				var clampedB = (uint)b.Clamp(Byte.MinValue, Byte.MaxValue);
				var clampedA = (uint)alpha.Clamp(Byte.MinValue, Byte.MaxValue);

				_packedValue = (clampedA << 24) | (clampedB << 16) | (clampedG << 8) | (clampedR);
			}
			else
			{
				_packedValue = ((uint)alpha << 24) | ((uint)b << 16) | ((uint)g << 8) | ((uint)r);
			}
		}

		/// <summary>
		/// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
		/// </summary>
		/// <remarks>
		/// This overload sets the values directly without clamping, and may therefore be faster than the other overloads.
		/// </remarks>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <param name="alpha"></param>
		public FssColor(byte r, byte g, byte b, byte alpha)
		{
			_packedValue = ((uint)alpha << 24) | ((uint)b << 16) | ((uint)g << 8) | (r);
		}

		/// <summary>
		/// Gets or sets the blue component.
		/// </summary>
		[DataMember]
		public byte B
		{
			get
			{
				unchecked
				{
					return (byte)(this._packedValue >> 16);
				}
			}
			set
			{
				this._packedValue = (this._packedValue & 0xff00ffff) | ((uint)value << 16);
			}
		}

		/// <summary>
		/// Gets or sets the green component.
		/// </summary>
		[DataMember]
		public byte G
		{
			get
			{
				unchecked
				{
					return (byte)(this._packedValue >> 8);
				}
			}
			set
			{
				this._packedValue = (this._packedValue & 0xffff00ff) | ((uint)value << 8);
			}
		}

		/// <summary>
		/// Gets or sets the red component.
		/// </summary>
		[DataMember]
		public byte R
		{
			get
			{
				unchecked
				{
					return (byte)this._packedValue;
				}
			}
			set
			{
				this._packedValue = (this._packedValue & 0xffffff00) | value;
			}
		}

		/// <summary>
		/// Gets or sets the alpha component.
		/// </summary>
		[DataMember]
		public byte A
		{
			get
			{
				unchecked
				{
					return (byte)(this._packedValue >> 24);
				}
			}
			set
			{
				this._packedValue = (this._packedValue & 0x00ffffff) | ((uint)value << 24);
			}
		}

		/// <summary>
		/// Compares whether two <see cref="FssColor"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="FssColor"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="FssColor"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(FssColor a, FssColor b)
		{
			return (a._packedValue == b._packedValue);
		}

		/// <summary>
		/// Compares whether two <see cref="FssColor"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="FssColor"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="FssColor"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
		public static bool operator !=(FssColor a, FssColor b)
		{
			return (a._packedValue != b._packedValue);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="FssColor"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="FssColor"/>.</returns>
		public override int GetHashCode()
		{
			return this._packedValue.GetHashCode();
		}

		/// <summary>
		/// Compares whether current instance is equal to specified object.
		/// </summary>
		/// <param name="obj">The <see cref="FssColor"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return ((obj is FssColor) && this.Equals((FssColor)obj));
		}

		/// <summary>
		/// Multiply <see cref="FssColor"/> by value.
		/// </summary>
		/// <param name="value">Source <see cref="FssColor"/>.</param>
		/// <param name="scale">Multiplicator.</param>
		/// <returns>Multiplication result.</returns>
		public static FssColor Multiply(FssColor value, float scale)
		{
			return new FssColor((int)(value.R * scale), (int)(value.G * scale), (int)(value.B * scale), (int)(value.A * scale));
		}

		/// <summary>
		/// Multiply <see cref="FssColor"/> by value.
		/// </summary>
		/// <param name="value">Source <see cref="FssColor"/>.</param>
		/// <param name="scale">Multiplicator.</param>
		/// <returns>Multiplication result.</returns>
		public static FssColor operator *(FssColor value, float scale)
		{
			return new FssColor((int)(value.R * scale), (int)(value.G * scale), (int)(value.B * scale), (int)(value.A * scale));
		}

		public static FssColor operator *(float scale, FssColor value)
		{
			return new FssColor((int)(value.R * scale), (int)(value.G * scale), (int)(value.B * scale), (int)(value.A * scale));
		}

		/// <summary>
		/// Gets or sets packed value of this <see cref="FssColor"/>.
		/// </summary>
		[CLSCompliant(false)]
		public UInt32 PackedValue
		{
			get { return _packedValue; }
			set { _packedValue = value; }
		}


		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					this.R.ToString(), "  ",
					this.G.ToString(), "  ",
					this.B.ToString(), "  ",
					this.A.ToString()
				);
			}
		}


		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="FssColor"/> in the format:
		/// {R:[red] G:[green] B:[blue] A:[alpha]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="FssColor"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(25);
			sb.Append("{R:");
			sb.Append(R);
			sb.Append(" G:");
			sb.Append(G);
			sb.Append(" B:");
			sb.Append(B);
			sb.Append(" A:");
			sb.Append(A);
			sb.Append("}");
			return sb.ToString();
		}

		/// <summary>
		/// Translate a non-premultipled alpha <see cref="FssColor"/> to a <see cref="FssColor"/> that contains premultiplied alpha.
		/// </summary>
		/// <param name="r">Red component value.</param>
		/// <param name="g">Green component value.</param>
		/// <param name="b">Blue component value.</param>
		/// <param name="a">Alpha component value.</param>
		/// <returns>A <see cref="FssColor"/> which contains premultiplied alpha data.</returns>
		public static FssColor FromNonPremultiplied(int r, int g, int b, int a)
		{
			return new FssColor(r * a / 255, g * a / 255, b * a / 255, a);
		}

		#region IEquatable<Color> Members

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="FssColor"/>.
		/// </summary>
		/// <param name="other">The <see cref="FssColor"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(FssColor other)
		{
			return this.PackedValue == other.PackedValue;
		}

		#endregion

		/// <summary>
		/// Deconstruction method for <see cref="FssColor"/>.
		/// </summary>
		/// <param name="r">Red component value from 0 to 255.</param>
		/// <param name="g">Green component value from 0 to 255.</param>
		/// <param name="b">Blue component value from 0 to 255.</param>
		public void Deconstruct(out byte r, out byte g, out byte b)
		{
			r = R;
			g = G;
			b = B;
		}

		/// <summary>
		/// Deconstruction method for <see cref="FssColor"/>.
		/// </summary>
		/// <param name="r">Red component value from 0.0f to 1.0f.</param>
		/// <param name="g">Green component value from 0.0f to 1.0f.</param>
		/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
		public void Deconstruct(out float r, out float g, out float b)
		{
			r = R / 255f;
			g = G / 255f;
			b = B / 255f;
		}

		/// <summary>
		/// Deconstruction method for <see cref="FssColor"/> with Alpha.
		/// </summary>
		/// <param name="r">Red component value from 0 to 255.</param>
		/// <param name="g">Green component value from 0 to 255.</param>
		/// <param name="b">Blue component value from 0 to 255.</param>
		/// <param name="a">Alpha component value from 0 to 255.</param>
		public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
		{
			r = R;
			g = G;
			b = B;
			a = A;
		}

		/// <summary>
		/// Deconstruction method for <see cref="FssColor"/> with Alpha.
		/// </summary>
		/// <param name="r">Red component value from 0.0f to 1.0f.</param>
		/// <param name="g">Green component value from 0.0f to 1.0f.</param>
		/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
		/// <param name="a">Alpha component value from 0.0f to 1.0f.</param>
		public void Deconstruct(out float r, out float g, out float b, out float a)
		{
			r = R / 255f;
			g = G / 255f;
			b = B / 255f;
			a = A / 255f;
		}
	}
}
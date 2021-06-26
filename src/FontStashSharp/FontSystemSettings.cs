using System;

namespace FontStashSharp
{
	public enum FontSystemEffect
	{
		None,
		Blurry,
		Stroked
	}

	public class FontSystemSettings
	{
		private int _effectAmount = 0;
		public FontSystemEffect Effect { get; set; } = FontSystemEffect.None;

		private int _textureWidth = 1024, _textureHeight = 1024;
		private float _fontResolutionFactor = 1.0f;
		private int _kernelWidth = 0, _kernelHeight = 0;

		public int EffectAmount
		{
			get => _effectAmount;
			set
			{
				if (value < 0 || value > 20)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_effectAmount = value;
			}
		}

		public int TextureWidth
		{
			get => _textureWidth;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));

				}

				_textureWidth = value;
			}
		}

		public int TextureHeight
		{
			get => _textureHeight;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));

				}

				_textureHeight = value;
			}
		}

		public bool PremultiplyAlpha { get; set; } = true;

		public float FontResolutionFactor
		{
			get => _fontResolutionFactor;
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "This cannot be smaller than 0");
				}

				_fontResolutionFactor = value;
			}
		}

		public int KernelWidth
		{
			get => _kernelWidth;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "This cannot be smaller than 0");
				}

				_kernelWidth = value;
			}
		}

		public int KernelHeight
		{
			get => _kernelHeight;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "This cannot be smaller than 0");
				}

				_kernelHeight = value;
			}
		}

		public FontSystemSettings Clone()
		{
			return new FontSystemSettings
			{
				Effect = Effect,
				EffectAmount = EffectAmount,
				TextureWidth = TextureWidth,
				TextureHeight = TextureHeight,
				PremultiplyAlpha = PremultiplyAlpha,
				FontResolutionFactor = FontResolutionFactor,
				KernelWidth = KernelWidth,
				KernelHeight = KernelHeight
			};
		}
	}
}
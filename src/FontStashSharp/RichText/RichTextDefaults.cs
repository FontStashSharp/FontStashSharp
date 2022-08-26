using System;

namespace FontStashSharp.RichText
{
	public static class RichTextDefaults
	{
		public static Func<string, SpriteFontBase> FontResolver { get; set; }
		public static Func<string, TextureInfo> ImageResolver { get; set; }
	}
}

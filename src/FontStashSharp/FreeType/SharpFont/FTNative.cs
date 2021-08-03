using System;
using System.Runtime.InteropServices;

namespace SharpFontInternal
{
	internal static class FTNative
	{
		private const string FreetypeDll = "freetype";

		/// <summary>
		/// Defines the calling convention for P/Invoking the native freetype methods.
		/// </summary>
		private const CallingConvention CallConvention = CallingConvention.Cdecl;

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		public static extern Error FT_Init_FreeType(out IntPtr alibrary);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		public static extern Error FT_New_Memory_Face(IntPtr library, IntPtr memoryHandle, int file_size, int face_index, out IntPtr aface);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		public static extern Error FT_Done_Face(IntPtr face);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		public static extern uint FT_Get_Char_Index(IntPtr face, uint charcode);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		public static extern Error FT_Set_Pixel_Sizes(IntPtr face, uint pixel_width, uint pixel_height);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Load_Glyph(IntPtr face, uint glyph_index, int load_flags);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Render_Glyph(IntPtr slot, RenderMode render_mode);
	}
}
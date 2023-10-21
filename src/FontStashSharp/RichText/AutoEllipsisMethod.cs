namespace FontStashSharp.RichText
{
	public enum AutoEllipsisMethod
	{
		/// <summary>
		/// Autoellipsis is disabled.
		/// </summary>
		None,

		/// <summary>
		/// The text can be cut at any character.
		/// </summary>
		Character,

		/// <summary>
		/// The text will be cut at spaces.
		/// </summary>
		Word
	}
}
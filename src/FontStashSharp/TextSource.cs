using System.Text;

namespace FontStashSharp
{
	internal struct TextSource
	{
		public string StringText;
		public StringBuilder StringBuilderText;
		private int Position;

		public TextSource(string text)
		{
			StringText = text;
			StringBuilderText = null;
			Position = 0;
		}

		public TextSource(StringBuilder text)
		{
			StringText = null;
			StringBuilderText = text;
			Position = 0;
		}

		public bool IsNull
		{
			get
			{
				return StringText == null && StringBuilderText == null;
			}
		}

		public bool GetNextCodepoint(out int result)
		{
			result = 0;

			if (StringText != null)
			{
				if (Position >= StringText.Length)
				{
					return false;
				}

				result = char.ConvertToUtf32(StringText, Position);
				Position += char.IsSurrogatePair(StringText, Position) ? 2 : 1;
				return true;
			}

			if (StringBuilderText != null)
			{
				if (Position >= StringBuilderText.Length)
				{
					return false;
				}

				result = StringBuilderConvertToUtf32(StringBuilderText, Position);
				Position += StringBuilderIsSurrogatePair(StringBuilderText, Position) ? 2 : 1;
				return true;
			}

			return false;
		}

		public void Reset()
		{
			Position = 0;
		}

		private static bool StringBuilderIsSurrogatePair(StringBuilder sb, int index)
		{
			if (index + 1 < sb.Length)
				return char.IsSurrogatePair(sb[index], sb[index + 1]);
			return false;
		}

		private static int StringBuilderConvertToUtf32(StringBuilder sb, int index)
		{
			if (!char.IsHighSurrogate(sb[index]))
				return sb[index];

			return char.ConvertToUtf32(sb[index], sb[index + 1]);
		}
	}
}
namespace JsonDiffPatchDotNet
{
	public sealed class Options
	{
		public Options()
		{
			ArrayDiff = ArrayDiffMode.Simple;
			TextDiff = TextDiffMode.Efficient;
			MinEfficientTextDiffLength = 50;
		}

		/// <summary>
		/// Specifies how arrays are diffed. The default is Simple.
		/// </summary>
		public ArrayDiffMode ArrayDiff { get; set; }

		/// <summary>
		/// Specifies how string values are diffed. The default is Efficient.
		/// </summary>
		public TextDiffMode TextDiff { get; set; }

		/// <summary>
		/// The minimum string length required to use Efficient text diff. If the minimum
		/// length is not met, simple text diff will be used. The default length is 50 characters.
		/// </summary>
		public long MinEfficientTextDiffLength { get; set; }
	}
}

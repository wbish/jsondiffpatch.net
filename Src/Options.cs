namespace JsonDiffPatchDotNet
{
	public sealed class Options
	{
		public Options()
		{
			ArrayDiff = ArrayDiffMode.Efficient;
			TextDiff = TextDiffMode.Efficient;
			MinEfficientTextDiffLength = 50;
		}

		public ArrayDiffMode ArrayDiff
		{ get; set; }

		public TextDiffMode TextDiff
		{ get; set; }

		public long MinEfficientTextDiffLength
		{ get; set; }
	}
}

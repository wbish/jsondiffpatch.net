namespace JsonDiffPatchDotNet
{
	public sealed class Options
	{
		public Options()
		{
			Patch = PatchMode.Lenient;
			ArrayDiff = ArrayDiffMode.Efficient;
			TextDiff = TextDiffMode.Efficient;
			MinEfficientTextDiffLength = 50;
		}

		public PatchMode Patch { get; set; }

		public ArrayDiffMode ArrayDiff { get; set; }

		public TextDiffMode TextDiff { get; set; }

		public long MinEfficientTextDiffLength { get; set; }
	}
}

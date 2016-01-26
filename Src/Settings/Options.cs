namespace JsonDiffPatchDotNet.Settings
{
	public sealed class Options
	{
		public Options()
		{
			Patch = PatchMode.Lenient;
			ArrayDiff = ArrayDiffMode.Efficient;
			TextDiff = TextDiffMode.Efficient;
		}

		public PatchMode Patch { get; set; }

		public ArrayDiffMode ArrayDiff { get; set; }

		public TextDiffMode TextDiff { get; set; }
	}
}

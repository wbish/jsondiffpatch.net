namespace JsonDiffPatchDotNet
{
	public sealed class Options
	{
		public Options()
		{
			ArrayDiff = ArrayDiffMode.Efficient;
			TextDiff = TextDiffMode.Efficient;
			MinEfficientTextDiffLength = 50;
		    OmitLeftSideOnDiff = false;
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

        /// <summary>
        /// If true, will provide an empty string in serialized diff document
        /// representing the left side object (original).  Useful for creating a
        /// smaller package where unpatch is not needed.
        /// </summary>
        /// <remarks>
        /// DANGER: Unpatch will not be supported.
        /// </remarks>
        public bool OmitLeftSideOnDiff { get; set; }
	}
}

namespace JsonDiffPatchDotNet
{
	public enum TextDiffMode
	{
		/// <summary>
		/// Efficient string diff uses google-diff-match-patch to produce a patchable diff. For small string 
		/// lengths, it is more efficient to use simple TextDiffMode as the less bytes are necessary to 
		/// do a string replace.
		/// </summary>
		Efficient,

		/// <summary>
		/// Simple string diff is a simple case sensitive string comparison. The patch document will contain
		/// the the entirity both strings in the event they are different as stored as a simple JSON token
		/// replace.
		/// </summary>
		Simple,
	}
}

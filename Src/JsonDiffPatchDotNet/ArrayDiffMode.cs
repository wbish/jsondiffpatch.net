namespace JsonDiffPatchDotNet
{
	public enum ArrayDiffMode
	{
		/// <summary>
		/// Efficient array diff does a deep examination of the contents of an array and 
		/// produces a patch document that only contains elements in the array that were 
		/// added or removed. Efficient array diff can only patch and unpatch the original 
		/// JSON array values used to produce the patch or there will be unintended 
		/// consequences.
		/// </summary>
		Efficient,

		/// <summary>
		/// Simple array diff does an exact match comparison on two arrays. If they are different
		/// the entire left and entire right arrays are added to the patch document as a simple
		/// JSON token replace. If they are the same, then token is skipped in the patch document.
		/// </summary>
		Simple,
	}
}

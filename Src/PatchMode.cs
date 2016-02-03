namespace JsonDiffPatchDotNet
{
	public enum PatchMode
	{
		/// <summary>
		/// Strict patch mode enforces the requirement that the left or right objects match
		/// the values specified in the patch object. If a property cannot be patched an exception
		/// is thrown
		/// </summary>
		StrictAbort,
		
		/// <summary>
		/// Lenient patch mode does not require the pre-patch and pre-unpatch state to match the
		/// values specified in the patch object.
		/// </summary>
		Lenient,
	}
}

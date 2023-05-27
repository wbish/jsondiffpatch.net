namespace JsonDiffPatchDotNet
{
	public enum PatchBehavior
	{
		None,

		/// <summary>
		/// If left json value is not equals with patch[0] value then throw exception on patch action
		/// </summary>
		LeftMatchValidation
	}
}

namespace JsonDiffPatchDotNet.Formatters
{
	public enum DeltaType
	{
		Unknown,
		Unchanged,
		Added,
		Moved,
		Deleted,
		MoveDestination,
		Modified,
		Node,
		TextDiff
	}
}

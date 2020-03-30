namespace JsonDiffPatchDotNet.Formatters
{
	public interface IFormatContext<out TResult>
	{
		TResult Result();
	}
}

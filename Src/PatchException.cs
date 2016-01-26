using System;

namespace JsonDiffPatchDotNet
{
	public class PatchException : Exception
	{
		public PatchException(string property, string expected, string actual)
			: base($"Cannot patch '{property}'. Expected value is: '{expected}'. Actual value is: '{actual}'")
		{
		}
	}
}

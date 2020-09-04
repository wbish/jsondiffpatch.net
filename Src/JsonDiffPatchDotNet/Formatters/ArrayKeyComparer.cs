using System.Collections.Generic;

namespace JsonDiffPatchDotNet.Formatters
{
	internal class ArrayKeyComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			return ArrayKeyToSortNumber(x) - ArrayKeyToSortNumber(y);
		}

		private static int ArrayKeyToSortNumber(string key)
		{
			if (key == "_t")
				return -1;

			if (key.Length > 1 && key[0] == '_')
				return int.Parse(key.Substring(1)) * 10;

			return (int.Parse(key) * 10) + 1;
		}
	}
}

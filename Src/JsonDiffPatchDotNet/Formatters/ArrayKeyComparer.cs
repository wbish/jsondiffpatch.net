using System.Collections.Generic;

namespace JsonDiffPatchDotNet.Formatters
{
    internal class ArrayKeyComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // This purposefully REVERSED from benjamine/jsondiffpatch,
            // In order to match logic found in JsonDiffPatch.ArrayPatch,
            // which applies operations in reverse order to avoid shifting floor
            return ArrayKeyToSortNumber(y) - ArrayKeyToSortNumber(x);
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

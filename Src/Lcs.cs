using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet
{
	internal class Lcs
	{
		internal List<JToken> Sequence { get; set; }

		internal List<int> Indices1 { get; set; }

		internal List<int> Indices2 { get; set; }

		private Lcs()
		{
			Sequence = new List<JToken>();
			Indices1 = new List<int>();
			Indices2 = new List<int>();
		}

		internal static Lcs Get(List<JToken> left, List<JToken> right)
		{
			var matrix = LcsInternal(left, right);
			var result = Backtrack(matrix, left, right, left.Count(), right.Count());
			return result;
		}

		private static int[,] LcsInternal(List<JToken> left, List<JToken> right)
		{
			var arr = new int[left.Count() + 1, right.Count() + 1];

			for (int i = 0; i <= right.Count(); i++)
			{
				arr[0, i] = 0;
			}
			for (int i = 0; i <= left.Count(); i++)
			{
				arr[i, 0] = 0;
			}

			for (int i = 1; i <= left.Count(); i++)
			{
				for (int j = 1; j <= right.Count(); j++)
				{
					if (left[i - 1].Equals(right[j - 1]))
					{
						arr[i, j] = arr[i - 1, j - 1] + 1;
					}
					else
					{
						arr[i, j] = Math.Max(arr[i - 1, j], arr[i, j - 1]);
					}
				}
			}

			return arr;
		}

        private static Lcs Backtrack(int[,] matrix, List<JToken> left, List<JToken> right, int li, int ri)
        {
            var result = new Lcs();
            for (int i = li, j = ri; i > 0 && j > 0;)
            {
                // If the JSON tokens at the same position are both Objects or both Arrays, we just say they 
                // are the same even if they are not, because we can package smaller deltas than an entire 
                // object or array replacement by doing object to object or array to array diff.
                if (left[i - 1].Equals(right[j - 1])
                || (left[i - 1].Type == JTokenType.Object && right[j - 1].Type == JTokenType.Object)
                || (left[i - 1].Type == JTokenType.Array && right[j - 1].Type == JTokenType.Array))
                {
                    result.Sequence.Insert(0, left[i - 1]);
                    result.Indices1.Insert(0, i - 1);
                    result.Indices2.Insert(0, j - 1);
                    i--;
                    j--;
                    continue;
                }

                if (matrix[i, j - 1] > matrix[i - 1, j])
                {
                    j--;
                }
                else
                {
                    i--;
                }
            }

            return result;
        }
    }
}
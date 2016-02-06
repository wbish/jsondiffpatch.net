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
			if (li == 0 || ri == 0)
			{
				return new Lcs();
			}

			if (left[li - 1].Equals(right[ri - 1]) 
				|| (left[li - 1].Type == JTokenType.Object && right[ri - 1].Type == JTokenType.Object))
			{
				var subsequence = Backtrack(matrix, left, right, li - 1, ri - 1);
				subsequence.Sequence.Add(left[li - 1]);
				subsequence.Indices1.Add(li - 1);
				subsequence.Indices2.Add(ri - 1);
				return subsequence;
			}

			if (matrix[li, ri - 1] > matrix[li - 1, ri])
			{
				return Backtrack(matrix, left, right, li, ri - 1);
			}
			else
			{
				return Backtrack(matrix, left, right, li - 1, ri);
			}
		}
	}
}
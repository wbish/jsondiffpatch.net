using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonDiffPatchDotNet
{
	public sealed class Options
	{		
		public Options()
		{
			ArrayDiff = ArrayDiffMode.Efficient;
			TextDiff = TextDiffMode.Efficient;
			MinEfficientTextDiffLength = 50;
		}

		/// <summary>
		/// Specifies how arrays are diffed. The default is Efficient.
		/// </summary>
		public ArrayDiffMode ArrayDiff { get; set; }

		/// <summary>
		/// Specifies how string values are diffed. The default is Efficient.
		/// </summary>
		public TextDiffMode TextDiff { get; set; }

		/// <summary>
		/// The minimum string length required to use Efficient text diff. If the minimum
		/// length is not met, simple text diff will be used. The default length is 50 characters.
		/// </summary>
		public long MinEfficientTextDiffLength { get; set; }

		/// <summary>
		/// Specifies which paths to exclude from the diff patch set
		/// </summary>
		public List<string> ExcludePaths { get; set; } = new List<string>();

		/// <summary>
		/// Specifies behaviors to apply to the diff patch set
		/// </summary>
		public DiffBehavior DiffBehaviors { get; set; }
	}
}

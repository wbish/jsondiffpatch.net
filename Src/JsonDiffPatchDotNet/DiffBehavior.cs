using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDiffPatchDotNet
{
	[Flags]
	public enum DiffBehavior
	{
		/// <summary>
		/// Default behavior
		/// </summary>
		None,
		/// <summary>
		/// If the patch document is missing properties that are in the source document, leave the existing properties in place instead of deleting them
		/// </summary>
		IgnoreMissingProperties,
		/// <summary>
		/// If the patch document contains properties that aren't defined in the source document, ignore them instead of adding them
		/// </summary>
		IgnoreNewProperties
	}
}

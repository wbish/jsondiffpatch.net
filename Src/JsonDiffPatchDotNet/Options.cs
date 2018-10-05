using System;
using Newtonsoft.Json.Linq;

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
        /// Specifies how arrays are diffed. The default is Simple.
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
        /// for LCS to work, it needs a way to match items between previous/original (or left/right) arrays. In traditional text diff tools this is trivial, as two lines of text are compared char 
        /// char.
        /// When no matches by reference or value are found, array diffing fallbacks to a dumb behavior: matching items by position.
        /// Matching by position is not the most efficient option (eg. if an item is added at the first position, all the items below will be considered modified), but it produces expected results
        /// in most trivial cases.This is good enough as soon as movements/insertions/deletions only happen near the bottom of the array.
        /// This is because if 2 objects are not equal by reference(ie.the same object) both objects are considered different values, as there is no trivial solution to compare two arbitrary objects
        /// in JavaScript.
        /// To improve the results leveraging the power of LCS(and position move detection) you need to provide a way to compare 2 objects.
        /// </summary>
        public Func<JToken, object> ObjectHash { get; set; }
    }
}

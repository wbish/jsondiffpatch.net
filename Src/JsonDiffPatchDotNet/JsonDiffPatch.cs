using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DiffMatchPatch;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet
{
	public class JsonDiffPatch
	{
		private readonly Options _options;

		public JsonDiffPatch()
			: this(new Options())
		{
		}

		public JsonDiffPatch(Options options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_options = options;
		}

		/// <summary>
		/// Diff two JSON objects.
		/// 
		/// The output is a JObject that contains enough information to represent the
		/// delta between the two objects and to be able perform patch and reverse operations.
		/// </summary>
		/// <param name="left">The base JSON object</param>
		/// <param name="right">The JSON object to compare against the base</param>
		/// <returns>JSON Patch Document</returns>
		public JToken Diff(JToken left, JToken right)
		{
			if (left == null)
				left = new JValue("");
			if (right == null)
				right = new JValue("");

			if (left.Type == JTokenType.Object && right.Type == JTokenType.Object)
			{
				return ObjectDiff((JObject)left, (JObject)right);
			}

			if (_options.ArrayDiff == ArrayDiffMode.Efficient
				&& left.Type == JTokenType.Array
				&& right.Type == JTokenType.Array)
			{
				return ArrayDiff((JArray)left, (JArray)right);
			}

			if (_options.TextDiff == TextDiffMode.Efficient
				&& left.Type == JTokenType.String
				&& right.Type == JTokenType.String
				&& (left.ToString().Length > _options.MinEfficientTextDiffLength || right.ToString().Length > _options.MinEfficientTextDiffLength))
			{
				var dmp = new diff_match_patch();
				List<Patch> patches = dmp.patch_make(left.ToObject<string>(), right.ToObject<string>());
				return patches.Any()
					? new JArray(dmp.patch_toText(patches), 0, (int)DiffOperation.TextDiff)
					: null;
			}

			if (!JToken.DeepEquals(left, right))
			{
				return new JArray(left, right);
			}				

			return null;
		}

		/// <summary>
		/// Patch a JSON object
		/// </summary>
		/// <param name="left">Unpatched JSON object</param>
		/// <param name="patch">JSON Patch Document</param>
		/// <returns>Patched JSON object</returns>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		public JToken Patch(JToken left, JToken patch)
		{
			if (patch == null)
				return left;

			if (patch.Type == JTokenType.Object)
			{
				var patchObj = (JObject)patch;
				JProperty arrayDiffCanary = patchObj.Property("_t");

				if (left != null
					&& left.Type == JTokenType.Array
					&& arrayDiffCanary != null
					&& arrayDiffCanary.Value.Type == JTokenType.String
					&& arrayDiffCanary.Value.ToObject<string>() == "a")
				{
					return ArrayPatch((JArray)left, patchObj);
				}

				return ObjectPatch(left as JObject, patchObj);
			}

			if (patch.Type == JTokenType.Array)
			{
				var patchArray = (JArray)patch;

				if (patchArray.Count == 1)	// Add
				{
					return patchArray[0];
				}

				if (patchArray.Count == 2)	// Replace
				{
					return patchArray[1];
				}

				if (patchArray.Count == 3)	// Delete, Move or TextDiff
				{
					if (patchArray[2].Type != JTokenType.Integer)
						throw new InvalidDataException("Invalid patch object");

					int op = patchArray[2].Value<int>();

					if (op == 0)
					{
						return null;
					}

					if (op == 2)
					{
						if (left.Type != JTokenType.String)
							throw new InvalidDataException("Invalid patch object");

						var dmp = new diff_match_patch();
						List<Patch> patches = dmp.patch_fromText(patchArray[0].ToObject<string>());

						if (!patches.Any())
							throw new InvalidDataException("Invalid textline");

						object[] result = dmp.patch_apply(patches, left.Value<string>());
						var patchResults = (bool[])result[1];
						if (patchResults.Any(x => !x))
							throw new InvalidDataException("Text patch failed");

						string right = (string)result[0];
						return right;
					}

					throw new InvalidDataException("Invalid patch object");
				}

				throw new InvalidDataException("Invalid patch object");
			}

			return null;
		}

		/// <summary>
		/// Unpatch a JSON object
		/// </summary>
		/// <param name="right">Patched JSON object</param>
		/// <param name="patch">JSON Patch Document</param>
		/// <returns>Unpatched JSON object</returns>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		public JToken Unpatch(JToken right, JToken patch)
		{
			if (patch == null)
				return right;

			if (patch.Type == JTokenType.Object)
			{
				var patchObj = (JObject)patch;
				JProperty arrayDiffCanary = patchObj.Property("_t");

				if (right != null
					&& right.Type == JTokenType.Array
					&& arrayDiffCanary != null
					&& arrayDiffCanary.Value.Type == JTokenType.String
					&& arrayDiffCanary.Value.ToObject<string>() == "a")
				{
					return ArrayUnpatch((JArray)right, patchObj);
				}

				return ObjectUnpatch(right as JObject, patchObj);
			}

			if (patch.Type == JTokenType.Array)
			{
				var patchArray = (JArray)patch;

				if (patchArray.Count == 1)	// Add (we need to remove the property)
				{
					return null;
				}

				if (patchArray.Count == 2)	// Replace
				{
					return patchArray[0];
				}

				if (patchArray.Count == 3)	// Delete, Move or TextDiff
				{
					if (patchArray[2].Type != JTokenType.Integer)
						throw new InvalidDataException("Invalid patch object");

					int op = patchArray[2].Value<int>();

					if (op == 0)
					{
						return patchArray[0];
					}
					if (op == 2)
					{
						if (right.Type != JTokenType.String)
							throw new InvalidDataException("Invalid patch object");

						var dmp = new diff_match_patch();
						List<Patch> patches = dmp.patch_fromText(patchArray[0].ToObject<string>());

						if (!patches.Any())
							throw new InvalidDataException("Invalid textline");

						var unpatches = new List<Patch>();
						for (int i = patches.Count - 1; i >= 0; --i)
						{
							Patch p = patches[i];
							var u = new Patch
							{
								length1 = p.length1,
								length2 = p.length2,
								start1 = p.start1,
								start2 = p.start2
							};

							foreach (Diff d in p.diffs)
							{
								if (d.operation == Operation.DELETE)
								{
									u.diffs.Add(new Diff(Operation.INSERT, d.text));
								}
								else if (d.operation == Operation.INSERT)
								{
									u.diffs.Add(new Diff(Operation.DELETE, d.text));
								}
								else
								{
									u.diffs.Add(d);
								}
							}
							unpatches.Add(u);
						}

						object[] result = dmp.patch_apply(unpatches, right.Value<string>());
						var unpatchResults = (bool[])result[1];
						if (unpatchResults.Any(x => !x))
							throw new InvalidDataException("Text patch failed");

						string left = (string)result[0];
						return left;
					}
					throw new InvalidDataException("Invalid patch object");
				}

				throw new InvalidDataException("Invalid patch object");
			}

			return null;
		}

		#region String Overrides

		/// <summary>
		/// Diff two JSON objects.
		/// 
		/// The output is a JObject that contains enough information to represent the
		/// delta between the two objects and to be able perform patch and reverse operations.
		/// </summary>
		/// <param name="left">The base JSON object</param>
		/// <param name="right">The JSON object to compare against the base</param>
		/// <returns>JSON Patch Document</returns>
		public string Diff(string left, string right)
		{
			JToken obj = Diff(JToken.Parse(left ?? ""), JToken.Parse(right ?? ""));
			return obj?.ToString();
		}

		/// <summary>
		/// Patch a JSON object
		/// </summary>
		/// <param name="left">Unpatched JSON object</param>
		/// <param name="patch">JSON Patch Document</param>
		/// <returns>Patched JSON object</returns>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		public string Patch(string left, string patch)
		{
			JToken patchedObj = Patch(JToken.Parse(left ?? ""), JToken.Parse(patch ?? ""));
			return patchedObj?.ToString();
		}

		/// <summary>
		/// Unpatch a JSON object
		/// </summary>
		/// <param name="right">Patched JSON object</param>
		/// <param name="patch">JSON Patch Document</param>
		/// <returns>Unpatched JSON object</returns>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		public string Unpatch(string right, string patch)
		{
			JToken unpatchedObj = Unpatch(JToken.Parse(right ?? ""), JToken.Parse(patch ?? ""));
			return unpatchedObj?.ToString();
		}

		#endregion

		private JObject ObjectDiff(JObject left, JObject right)
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));
			if (right == null)
				throw new ArgumentNullException(nameof(right));

			var diffPatch = new JObject();

			// Find properties modified or deleted
			foreach (var lp in left.Properties())
			{
				//Skip property if in path exclustions
				if (_options.ExcludePaths.Count > 0 && _options.ExcludePaths.Any(p => p.Equals(lp.Path, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}

				JProperty rp = right.Property(lp.Name);

				// Property deleted
				if (rp == null && (_options.DiffBehaviors & DiffBehavior.IgnoreMissingProperties) == DiffBehavior.IgnoreMissingProperties)
				{
					continue;
				}

				if (rp == null)
				{
					diffPatch.Add(new JProperty(lp.Name, new JArray(lp.Value, 0, (int)DiffOperation.Deleted)));
					continue;
				}

				JToken d = Diff(lp.Value, rp.Value);
				if (d != null)
				{
					diffPatch.Add(new JProperty(lp.Name, d));
				}
			}

			// Find properties that were added 
			foreach (var rp in right.Properties())
			{
				if (left.Property(rp.Name) != null || (_options.DiffBehaviors & DiffBehavior.IgnoreNewProperties) == DiffBehavior.IgnoreNewProperties)
					continue;

				diffPatch.Add(new JProperty(rp.Name, new JArray(rp.Value)));
			}

			if (diffPatch.Properties().Any())
				return diffPatch;

			return null;
		}

		private JObject ArrayDiff(JArray left, JArray right)
		{
			var result = JObject.Parse(@"{ ""_t"": ""a"" }");

			int commonHead = 0;
			int commonTail = 0;

			if (JToken.DeepEquals(left, right))
				return null;

			// Find common head
			while (commonHead < left.Count
				&& commonHead < right.Count
				&& JToken.DeepEquals(left[commonHead], right[commonHead]))
			{
				commonHead++;
			}

			// Find common tail
			while (commonTail + commonHead < left.Count
				&& commonTail + commonHead < right.Count
				&& JToken.DeepEquals(left[left.Count - 1 - commonTail], right[right.Count - 1 - commonTail]))
			{
				commonTail++;
			}

			if (commonHead + commonTail == left.Count)
			{
				// Trivial case, a block (1 or more consecutive items) was added
				for (int index = commonHead; index < right.Count - commonTail; ++index)
				{
					result[$"{index}"] = new JArray(right[index]);
				}

				return result;
			}
			if (commonHead + commonTail == right.Count)
			{
				// Trivial case, a block (1 or more consecutive items) was removed
				for (int index = commonHead; index < left.Count - commonTail; ++index)
				{
					result[$"_{index}"] = new JArray(left[index], 0, (int)DiffOperation.Deleted);
				}

				return result;
			}

			// Complex Diff, find the LCS (Longest Common Subsequence)
			List<JToken> trimmedLeft = left.ToList().GetRange(commonHead, left.Count - commonTail - commonHead);
			List<JToken> trimmedRight = right.ToList().GetRange(commonHead, right.Count - commonTail - commonHead);
			Lcs lcs = Lcs.Get(trimmedLeft, trimmedRight);

			for (int index = commonHead; index < left.Count - commonTail; ++index)
			{
				if (lcs.Indices1.IndexOf(index - commonHead) < 0)
				{
					// Removed
					result[$"_{index}"] = new JArray(left[index], 0, (int)DiffOperation.Deleted);
				}
			}

			for (int index = commonHead; index < right.Count - commonTail; index++)
			{
				int indexRight = lcs.Indices2.IndexOf(index - commonHead);

				if (indexRight < 0)
				{
					// Added
					result[$"{index}"] = new JArray(right[index]);
				}
				else
				{
					int li = lcs.Indices1[indexRight] + commonHead;
					int ri = lcs.Indices2[indexRight] + commonHead;

					JToken diff = Diff(left[li], right[ri]);

					if (diff != null)
					{
						result[$"{index}"] = diff;
					}
				}
			}

			return result;
		}

		private JObject ObjectPatch(JObject obj, JObject patch)
		{
			if (obj == null)
				obj = new JObject();
			if (patch == null)
				return obj;

			var target = (JObject)obj.DeepClone();

			foreach (var diff in patch.Properties())
			{
				JProperty property = target.Property(diff.Name);
				JToken patchValue = diff.Value;

				// We need to special case deletion when doing objects since a delete is a removal of a property
				// not a null assignment
				if (patchValue.Type == JTokenType.Array && ((JArray)patchValue).Count == 3 && patchValue[2].Value<int>() == 0)
				{
					target.Remove(diff.Name);
				}
				else
				{
					if (property == null)
					{
						target.Add(new JProperty(diff.Name, Patch(null, patchValue)));
					}
					else
					{
						property.Value = Patch(property.Value, patchValue);
					}
				}
			}

			return target;
		}

		private JArray ArrayPatch(JArray left, JObject patch)
		{
			var toRemove = new List<JProperty>();
			var toInsert = new List<JProperty>();
			var toModify = new List<JProperty>();

			foreach (JProperty op in patch.Properties())
			{
				if (op.Name == "_t")
					continue;

				var value = op.Value as JArray;

				if (op.Name.StartsWith("_"))
				{
					// removed item from original array
					if (value != null && value.Count == 3 && (value[2].ToObject<int>() == (int)DiffOperation.Deleted || value[2].ToObject<int>() == (int)DiffOperation.ArrayMove))
					{
						toRemove.Add(new JProperty(op.Name.Substring(1), op.Value));

						if (value[2].ToObject<int>() == (int)DiffOperation.ArrayMove)
							toInsert.Add(new JProperty(value[1].ToObject<int>().ToString(), new JArray(left[int.Parse(op.Name.Substring(1))].DeepClone())));
					}
					else
					{
						throw new Exception($"Only removal or move can be applied at original array indices. Context: {value}");
					}
				}
				else
				{
					if (value != null && value.Count == 1)
					{
						toInsert.Add(op);
					}
					else
					{
						toModify.Add(op);
					}
				}
			}


			// remove items, in reverse order to avoid sawing our own floor
			toRemove.Sort((x, y) => int.Parse(x.Name).CompareTo(int.Parse(y.Name)));
			for (int i = toRemove.Count - 1; i >= 0; --i)
			{
				JProperty op = toRemove[i];
				left.RemoveAt(int.Parse(op.Name));
			}

			// insert items, in reverse order to avoid moving our own floor
			toInsert.Sort((x, y) => int.Parse(y.Name).CompareTo(int.Parse(x.Name)));
			for (int i = toInsert.Count - 1; i >= 0; --i)
			{
				JProperty op = toInsert[i];
				left.Insert(int.Parse(op.Name), ((JArray)op.Value)[0]);
			}

			foreach (var op in toModify)
			{
				JToken p = Patch(left[int.Parse(op.Name)], op.Value);
				left[int.Parse(op.Name)] = p;
			}

			return left;
		}

		private JObject ObjectUnpatch(JObject obj, JObject patch)
		{
			if (obj == null)
				obj = new JObject();
			if (patch == null)
				return obj;

			var target = (JObject)obj.DeepClone();

			foreach (var diff in patch.Properties())
			{
				JProperty property = target.Property(diff.Name);
				JToken patchValue = diff.Value;

				// We need to special case addition when doing objects since an undo add is a removal of a property
				// not a null assignment
				if (patchValue.Type == JTokenType.Array && ((JArray)patchValue).Count == 1)
				{
					target.Remove(property.Name);
				}
				else
				{
					if (property == null)
					{
						target.Add(new JProperty(diff.Name, Unpatch(null, patchValue)));
					}
					else
					{
						property.Value = Unpatch(property.Value, patchValue);
					}
				}
			}

			return target;
		}

		private JArray ArrayUnpatch(JArray right, JObject patch)
		{
			var toRemove = new List<JProperty>();
			var toInsert = new List<JProperty>();
			var toModify = new List<JProperty>();

			foreach (JProperty op in patch.Properties())
			{
				if (op.Name == "_t")
					continue;

				var value = op.Value as JArray;

				if (op.Name.StartsWith("_"))
				{
					// removed item from original array
					if (value != null && value.Count == 3 && (value[2].ToObject<int>() == (int)DiffOperation.Deleted || value[2].ToObject<int>() == (int)DiffOperation.ArrayMove))
					{
						var newOp = new JProperty(value[1].ToObject<int>().ToString(), op.Value);

						if (value[2].ToObject<int>() == (int)DiffOperation.ArrayMove)
						{
							toInsert.Add(new JProperty(op.Name.Substring(1), new JArray(right[value[1].ToObject<int>()].DeepClone())));
							toRemove.Add(newOp);
						}
						else
						{
							toInsert.Add(new JProperty(op.Name.Substring(1), new JArray(value[0])));
						}
					}
					else
					{
						throw new Exception($"Only removal or move can be applied at original array indices. Context: {value}");
					}
				}
				else
				{
					if (value != null && value.Count == 1)
					{
						toRemove.Add(op);
					}
					else
					{
						toModify.Add(op);
					}
				}
			}

			// first modify entries
			foreach (var op in toModify)
			{
				JToken p = Unpatch(right[int.Parse(op.Name)], op.Value);
				right[int.Parse(op.Name)] = p;
			}

			// remove items, in reverse order to avoid sawing our own floor
			toRemove.Sort((x, y) => int.Parse(x.Name).CompareTo(int.Parse(y.Name)));
			for (int i = toRemove.Count - 1; i >= 0; --i)
			{
				JProperty op = toRemove[i];
				right.RemoveAt(int.Parse(op.Name));
			}

			// insert items, in reverse order to avoid moving our own floor
			toInsert.Sort((x, y) => int.Parse(x.Name).CompareTo(int.Parse(y.Name)));
			foreach (var op in toInsert)
			{
				right.Insert(int.Parse(op.Name), ((JArray)op.Value)[0]);
			}

			return right;
		}
	}
}

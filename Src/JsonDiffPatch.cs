using System;
using System.Collections.Generic;
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
				throw new ArgumentNullException(nameof(options));

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
		/// <exception cref="System.NotImplementedException">Thrown if patch document contains an array diff</exception>
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
				throw new NotImplementedException("Array Diff");
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

			if (!left.Equals(right))
				return new JArray(left, right);

			return null;
		}

		/// <summary>
		/// Patch a JSON object
		/// </summary>
		/// <param name="left">Unpatched JSON object</param>
		/// <param name="patch">JSON Patch Document</param>
		/// <returns>Patched JSON object</returns>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		/// <exception cref="System.NotImplementedException">Thrown if patch document contains an array diff</exception>
		public JToken Patch(JToken left, JToken patch)
		{
			if (patch == null)
				return left;

			if (patch.Type == JTokenType.Object)
			{
				var patchObj = (JObject)patch;
				JProperty arrayDiffCanary = patchObj.Property("_t");

				if (arrayDiffCanary != null && arrayDiffCanary.ToObject<string>() == "a")
				{
					throw new NotImplementedException("Array Diff");
				}

				return ObjectPatch(left as JObject, patchObj);
			}

			if (patch.Type == JTokenType.Array)
			{
				var patchArray = (JArray)patch;

				if (patchArray.Count == 1) // Add
				{
					return patchArray[0];
				}
				else if (patchArray.Count == 2) // Replace
				{
					return patchArray[1];
				}
				else if (patchArray.Count == 3) // Delete, Move or TextDiff
				{
					if (patchArray[2].Type != JTokenType.Integer)
						throw new InvalidDataException("Invalid patch object");

					int op = patchArray[2].Value<int>();

					if (op == 0)
					{
						return null;
					}
					else if (op == 2)
					{
						var dmp = new diff_match_patch();
						List<Patch> patches = dmp.patch_fromText(patchArray[0].ToObject<string>());

						if (patches.Count != 1)
							throw new InvalidDataException("Invalid textline");

						string right = dmp.diff_text2(patches[0].diffs);
						return right;
					}
					else
					{
						throw new InvalidDataException("Invalid patch object");
					}
				}
				else
				{
					throw new InvalidDataException("Invalid patch object");
				}
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
		/// <exception cref="System.NotImplementedException">Thrown if patch document contains an array diff</exception>
		public JToken Unpatch(JToken right, JToken patch)
		{
			if (patch == null)
				return right;

			if (patch.Type == JTokenType.Object)
			{
				var patchObj = (JObject)patch;
				JProperty arrayDiffCanary = patchObj.Property("_t");

				if (arrayDiffCanary != null && arrayDiffCanary.ToObject<string>() == "a")
				{
					throw new NotImplementedException("Array Diff");
				}

				return ObjectUnpatch(right as JObject, patchObj);
			}

			if (patch.Type == JTokenType.Array)
			{
				var patchArray = (JArray)patch;

				if (patchArray.Count == 1) // Add (we need to remove the property)
				{
					return null;
				}
				else if (patchArray.Count == 2) // Replace
				{
					return patchArray[0];
				}
				else if (patchArray.Count == 3) // Delete, Move or TextDiff
				{
					if (patchArray[2].Type != JTokenType.Integer)
						throw new InvalidDataException("Invalid patch object");

					int op = patchArray[2].Value<int>();

					if (op == 0)
					{
						return patchArray[0];
					}
					else if (op == 2)
					{
						var dmp = new diff_match_patch();
						List<Patch> patches = dmp.patch_fromText(patchArray[0].ToObject<string>());

						if (patches.Count != 1)
							throw new InvalidDataException("Invalid textline");

						string left = dmp.diff_text1(patches[0].diffs);
						return left;
					}
					else
					{
						throw new InvalidDataException("Invalid patch object");
					}
				}
				else
				{
					throw new InvalidDataException("Invalid patch object");
				}
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
		/// <exception cref="System.NotImplementedException">Thrown if patch document contains an array diff</exception>
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
		/// <exception cref="System.NotImplementedException">Thrown if patch document contains an array diff</exception>
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
		/// <exception cref="System.NotImplementedException">Thrown if patch document contains an array diff</exception>
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
				JProperty rp = right.Property(lp.Name);

				// Property deleted
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
				if (left.Property(rp.Name) != null)
					continue;

				diffPatch.Add(new JProperty(rp.Name, new JArray(rp.Value)));
			}

			if (diffPatch.Properties().Any())
				return diffPatch;

			return null;
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
				var property = target.Property(diff.Name);
				var patchValue = diff.Value;

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

		private JObject ObjectUnpatch(JObject obj, JObject patch)
		{
			if (obj == null)
				obj = new JObject();
			if (patch == null)
				return obj;

			var target = (JObject)obj.DeepClone();

			foreach (var diff in patch.Properties())
			{
				var property = target.Property(diff.Name);
				var patchValue = diff.Value;

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
	}
}

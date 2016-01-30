using System;
using System.Collections.Generic;
using System.IO;
using DiffMatchPatch;
using JsonDiffPatchDotNet.Settings;
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
		/// <param name="left">The base object</param>
		/// <param name="right">The object to comprare against the base</param>
		/// <returns>JObject in JsonDiffPatch format</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if left or right input arguments are null</exception>
		public JObject Diff(JObject left, JObject right)
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

				if (rp == null)
				{
					diffPatch.Add(new JProperty(lp.Name, new JArray(lp.Value, 0, 0)));
				}
				else
				{
					if (_options.ArrayDiff == ArrayDiffMode.Efficient
						&& lp.Value.Type == JTokenType.Array
						&& rp.Value.Type == JTokenType.Array)
					{
						// Efficient Array diffing is is NYI.
						throw new NotImplementedException();
					}
					else if (_options.TextDiff == TextDiffMode.Efficient
						&& lp.Value.Type == JTokenType.String
						&& rp.Value.Type == JTokenType.String)
					{
						var dmp = new diff_match_patch();
						List<Patch> patches = dmp.patch_make(
							lp.Value.ToObject<string>(),
							rp.Value.ToObject<string>());

						if (patches.Count > 0)
						{
							string patch = dmp.patch_toText(patches);
							diffPatch.Add(new JProperty(rp.Name, new JArray(patch, 0, 2)));
						}
					}
					else if (!lp.Value.Equals(rp.Value))
					{
						diffPatch.Add(new JProperty(lp.Name, new JArray(lp.Value, rp.Value)));
					}
				}
			}

			// Find properties that were added 
			foreach (var rp in right.Properties())
			{
				if (left.Property(rp.Name) != null)
					continue;

				diffPatch.Add(new JProperty(rp.Name, new JArray(rp.Value)));
			}

			return diffPatch;
		}

		/// <summary>
		/// Patch a JSON object
		/// 
		/// The output is a patched JObject.
		/// </summary>
		/// <param name="obj">Unpatched object</param>
		/// <param name="patch">Patch document</param>
		/// <returns>Patched JObject</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if obj or patch arguments are null</exception>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		/// <exception cref="JsonDiffPatchDotNet.PatchException">Thrown if PatchMode is StrictAbort and an unexpected value is encountered</exception>
		public JObject Patch(JObject obj, JObject patch)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			if (patch == null)
				throw new ArgumentNullException(nameof(patch));

			if (_options.Patch != PatchMode.Lenient && _options.Patch != PatchMode.StrictAbort)
				throw new NotImplementedException($"PatchMode: {_options.Patch}");

			var target = (JObject)obj.DeepClone();

			foreach (var diff in patch.Properties())
			{
				if (diff.Type == JTokenType.Object && ((JObject)diff.Value["_a"]) != null)
					throw new NotImplementedException("Efficient Array Diff");

				if (diff.Type != JTokenType.Array)
					throw new InvalidDataException("Invalid patch object");

				var property = target.Property(diff.Name);
				var value = (JArray)diff.Value;

				if (value.Count == 1) // Add
				{
					ValidatePatchStrict(diff.Name, property, null);

					if (property == null)
					{
						target.Add(new JProperty(diff.Name, value[0]));
					}
					else
					{
						property.Value = value[0];
					}
				}
				else if (value.Count == 2) // Replace
				{
					ValidatePatchStrict(diff.Name, property, value[0]);

					if (property == null)
					{
						target.Add(new JProperty(diff.Name, value[1]));
					}
					else
					{
						property.Value = value[1];
					}
				}
				else if (value.Count == 3) // Delete, Move or TextDiff
				{
					if (value[2].Type != JTokenType.Integer || value[2].Value<int>() != 0)
						throw new NotImplementedException($"Diff Operation: {value[2]}");

					ValidatePatchStrict(diff.Name, property, value[0]);

					target.Remove(property.Name);
				}
				else
				{
					throw new InvalidDataException("Invalid patch object");
				}
			}

			return target;
		}

		/// <summary>
		/// Unpatch a JSON object
		/// 
		/// The output is an unpatched JObject.
		/// </summary>
		/// <param name="obj">Patched object</param>
		/// <param name="patch">Patch document</param>
		/// <returns>Unpatched JObject</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if obj or patch arguments are null</exception>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		/// <exception cref="JsonDiffPatchDotNet.PatchException">Thrown if PatchMode is StrictAbort and an unexpected value is encountered</exception>
		public JObject Unpatch(JObject obj, JObject patch)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			if (patch == null)
				throw new ArgumentNullException(nameof(patch));

			if (_options.Patch != PatchMode.Lenient && _options.Patch != PatchMode.StrictAbort)
				throw new NotImplementedException($"PatchMode: {_options.Patch}");

			var target = (JObject)obj.DeepClone();

			foreach (var diff in patch.Properties())
			{
				if (diff.Type == JTokenType.Object && ((JObject)diff.Value["_a"]) != null)
					throw new NotImplementedException("Efficient Array Diff");

				if (diff.Type != JTokenType.Array)
					throw new InvalidDataException("Invalid patch object");

				var property = target.Property(diff.Name);
				var value = (JArray)diff.Value;

				if (value.Count == 1) // Add (we need to remove the property)
				{
					// The unpatched object should either not contain this property or
					// the value of the property should be null
					ValidatePatchStrict(diff.Name, property, value[0]);

					target.Remove(property.Name);
				}
				else if (value.Count == 2) // Replace
				{
					ValidatePatchStrict(diff.Name, property, value[1]);

					if (property == null)
					{
						target.Add(new JProperty(diff.Name, value[0]));
					}
					else
					{
						property.Value = value[0];
					}
				}
				else if (value.Count == 3) // Delete, Move or TextDiff
				{
					if (value[2].Type != JTokenType.Integer || value[2].Value<int>() != 0)
						throw new NotImplementedException($"Diff Operation: {value[2]}");

					ValidatePatchStrict(diff.Name, property, null);

					if (property == null)
					{
						target.Add(new JProperty(diff.Name, value[0]));
					}
					else
					{
						property.Value = value[0];
					}
				}
				else
				{
					throw new InvalidDataException("Invalid patch object");
				}
			}

			return target;
		}

		#region String Overrides

		/// <summary>
		/// Diff two JSON objects.
		/// 
		/// The output is a JObject that contains enough information to represent the
		/// delta between the two objects and to be able perform patch and reverse operations.
		/// </summary>
		/// <param name="left">The base object</param>
		/// <param name="right">The object to comprare against the base</param>
		/// <returns>JObject in JsonDiffPatch format</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if left or right input arguments are null</exception>
		public string Diff(string left, string right)
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));
			if (right == null)
				throw new ArgumentNullException(nameof(right));

			JObject obj = Diff(JObject.Parse(left), JObject.Parse(right));

			return obj.ToString();
		}

		/// <summary>
		/// Patch a JSON object
		/// 
		/// The output is a patched JObject.
		/// </summary>
		/// <param name="obj">Unpatched object</param>
		/// <param name="patch">Patch document</param>
		/// <returns>Patched JObject</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if obj or patch arguments are null</exception>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		/// <exception cref="JsonDiffPatchDotNet.PatchException">Thrown if PatchMode is StrictAbort and an unexpected value is encountered</exception>
		public string Patch(string obj, string patch)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			if (patch == null)
				throw new ArgumentNullException(nameof(patch));

			JObject patchedObj = Patch(JObject.Parse(obj), JObject.Parse(patch));

			return patchedObj.ToString();
		}

		/// <summary>
		/// Unpatch a JSON object
		/// 
		/// The output is an unpatched JObject.
		/// </summary>
		/// <param name="obj">Patched object</param>
		/// <param name="patch">Patch document</param>
		/// <returns>Unpatched JObject</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if obj or patch arguments are null</exception>
		/// <exception cref="System.IO.InvalidDataException">Thrown if the patch document is invalid</exception>
		/// <exception cref="JsonDiffPatchDotNet.PatchException">Thrown if PatchMode is StrictAbort and an unexpected value is encountered</exception>
		public string Unpatch(string obj, string patch)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			if (patch == null)
				throw new ArgumentNullException(nameof(patch));

			JObject unpatchedObj = Unpatch(JObject.Parse(obj), JObject.Parse(patch));

			return unpatchedObj.ToString();
		}

		#endregion

		private void ValidatePatchStrict(string name, JProperty property, JToken oldValue)
		{
			if (_options.Patch == PatchMode.StrictAbort)
			{
				if (property == null && oldValue != null)
				{
					throw new PatchException(name, oldValue.ToString(), string.Empty);
				}

				if (property != null && oldValue == null)
				{
					throw new PatchException(name, string.Empty, property.Value.ToString());
				}

				if (property != null && !property.Value.Equals(oldValue))
				{
					throw new PatchException(name, oldValue.ToString(), property.Value.ToString());
				}
			}
		}
	}
}

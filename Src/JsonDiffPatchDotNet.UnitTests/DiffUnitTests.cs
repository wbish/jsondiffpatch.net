using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class DiffUnitTests
	{
		[Test]
		public void Diff_EmptyObjects_EmptyPatch()
		{
			var jdp = new JsonDiffPatch();
			var empty = JObject.Parse(@"{}");

			JToken result = jdp.Diff(empty, empty);

			Assert.IsNull(result);
		}

		[Test]
		public void Diff_EqualBooleanProperty_NoDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": true }");

			JToken result = jdp.Diff(left, right);

			Assert.IsNull(result);
		}

		[Test]
		public void Diff_DiffBooleanProperty_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": false }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(2, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Old Value");
			Assert.IsFalse(((JArray)obj.Property("p").Value)[1].ToObject<bool>(), "Array New Value");
		}

		[Test]
		public void Diff_BooleanPropertyDeleted_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"": true }");
			var right = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Old Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[1].ToObject<int>(), "Array New Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[2].ToObject<int>(), "Array Deleted Indicator");
		}

		[Test]
		public void Diff_BooleanPropertyAdded_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"": true }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(1, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Added Value");
		}

		[Test]
		public void Diff_EfficientStringDiff_ValidPatch()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""blah1"" }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.AreEqual("@@ -1,64 +1,5 @@\n-lp.Value.ToString().Length %3e _options.MinEfficientTextDiffLength\n+blah1\n", ((JArray)obj.Property("p").Value)[0].ToString(), "Array Added Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[1].ToObject<int>(), "Array Added Value");
			Assert.AreEqual(2, ((JArray)obj.Property("p").Value)[2].ToObject<int>(), "Array String Diff Indicator");
		}

		[Test]
		public void Diff_EfficientStringDiff_NoChanges()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");

			JToken result = jdp.Diff(left, right);

			Assert.IsNull(result, "No Changes");
		}

		[Test]
		public void Diff_LeftNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(null, obj);

			Assert.AreEqual(JTokenType.Array, result.Type);
		}

		[Test]
		public void Diff_RightNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(obj, null);

			Assert.AreEqual(JTokenType.Array, result.Type);
		}

		[Test]
		public void Diff_EfficientArrayDiffSame_NullDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var array = JToken.Parse(@"[1,2,3]");

			JToken diff = jdp.Diff(array, array);

			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentHeadRemoved_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[2,3,4]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["_0"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentTailRemoved_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[1,2,3]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["_3"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentHeadAdded_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[0,1,2,3,4]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["0"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffTailMovedToHead_IgnoreMove_NoChange()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = false } });
			var left = JToken.Parse(@"[1,2,3,4,5,6,7,8,9,10]");
			var right = JToken.Parse(@"[4,1,2,3,7,5,6,8,10,9]");

			var diff = jdp.Diff(left, right);

			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_EfficientArrayDiffTailHeadMovedToTail_IncludeMove_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = true } });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[2,3,4,1]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.AreEqual(diff["_0"], JToken.Parse("['', 3, 3]"));
		}

		[Test]
		public void Diff_EfficientArrayDiffWithComplexObjectHeadAdded_NoObjectHash_DisabledMove_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = false, IncludeValueOnMove = false } });
			var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");
			var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC7"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(5, diff.Properties().Count());
			Assert.AreEqual(diff["0"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC7""]"));
			Assert.AreEqual(diff["1"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8""]"));
			Assert.AreEqual(diff["2"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9""]"));
			Assert.AreEqual(diff["3"], JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]"));
		}

		[Test]
		public void Diff_EfficientArrayDiffWithComplexObjectHeadAdded_NoObjectHash_EnableddMove_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = true } });
			var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");
			var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC7"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(5, diff.Properties().Count());
			Assert.AreEqual(diff["0"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC7""]"));
			Assert.AreEqual(diff["1"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8""]"));
			Assert.AreEqual(diff["2"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9""]"));
			Assert.AreEqual(diff["3"], JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]"));
		}

		[Test]
		public void Diff_EfficientArrayDiffWithComplexObjectMiddleAdded_NoObjectHash_EnabledMove_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = true } });
			var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}]");
			var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""NEW ID VALUE"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(3, diff.Properties().Count());
			Assert.AreEqual(diff["2"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""NEW ID VALUE""]"));
			Assert.AreEqual(diff["3"], JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}]"));
		}

		[Test]
		public void Diff_EfficientArrayDiffWithComplexObjectMiddleRemoved_NoObjectHash_EnabledMove_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = true } });
			var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");
			var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(3, diff.Properties().Count());
			Assert.AreEqual(diff["1"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10""]"));
			Assert.IsNotNull(diff["_2"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffWithComplexObject_NoObjectHash_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");
			var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""NEW ID VALUE"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.AreEqual(diff["1"]["Id"], JToken.Parse(@"[""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""NEW ID VALUE""]"));
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentTailAdded_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[1,2,3,4,5]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["4"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentHeadTailAdded_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[0,1,2,3,4,5]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(3, diff.Properties().Count());
			Assert.IsNotNull(diff["0"]);
			Assert.IsNotNull(diff["5"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffSameLengthNested_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, ObjectHash = (jObj) => jObj["Id"].Value<string>() });
			var left = JToken.Parse(@"[1,2,{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false},4]");
			var right = JToken.Parse(@"[1,2,{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":true},4]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["2"]);
		}

        [Test]
        public void Diff_EfficientArrayDiffWithComplexObject_ValidDiff()
        {
            var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, ObjectHash = (jObj) => jObj["Id"].Value<string>() });
            //var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
            var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}]");
            var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":false}]");

            JObject diff = jdp.Diff(left, right) as JObject;

            Assert.IsNotNull(diff);
            Assert.AreEqual(4, diff.Properties().Count());
        }

		[Test]
		public void Diff_EfficientArrayDiffSameWithObject_NoDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"
{
	""@context"": [
		""http://www.w3.org/ns/csvw"",
		{
			""@language"": ""en"",
			""@base"": ""http://example.org""
		}
	]
}");
			var right = left.DeepClone();

			JToken diff = jdp.Diff(left, right);

			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_EfficientArrayDiffHugeArrays_NoStackOverflow()
		{
			const int arraySize = 1000;
			Func<int, int, JToken> hugeArrayFunc = (startIndex, count) =>
			{
				var builder = new StringBuilder("[");
				foreach (var i in Enumerable.Range(startIndex, count))
				{
					builder.Append($"{i},");
				}
				builder.Append("]");

				return JToken.Parse(builder.ToString());
			};

			var jdp = new JsonDiffPatch();
			var left = hugeArrayFunc(0, arraySize);
			var right = hugeArrayFunc(arraySize / 2, arraySize);

			JToken diff = jdp.Diff(left, right);
			var restored = jdp.Patch(left, diff);

			Assert.That(JToken.DeepEquals(restored, right));
		}

		[Test]
		public void Diff_EfficientArrayDiffHugeArrays_OnlyIgnoredMoves_NoStackOverflow()
		{
			const int arraySize = 1000;

			Func<JToken> shuffledArrayFunc = () =>
			{
				Random rng = new Random();
				var builder = new StringBuilder("[");

				var randomList = new List<int>();
				for (int i = 0; i < arraySize; i++)
				{
					randomList.Add(i);
				}

				// Shuffle array
				int n = arraySize - 1;
				while (n > 1)
				{
					n--;
					int k = rng.Next(n + 1);
					int val = randomList[k];
					randomList[k] = randomList[n];
					randomList[n] = val;
				}

				foreach (var i in randomList)
				{
					builder.Append($"{i},");
				}
				builder.Append("]");

				return JToken.Parse(builder.ToString());
			};

			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = false } });
			var left = shuffledArrayFunc();
			var right = shuffledArrayFunc();

			JToken diff = jdp.Diff(left, right);
			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_IntStringDiff_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JToken.Parse(@"1");
			var right = JToken.Parse(@"""hello""");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Array, result.Type);
			JArray array = (JArray)result;
			Assert.AreEqual(2, array.Count);
			Assert.AreEqual(left, array[0]);
			Assert.AreEqual(right, array[1]);
		}		
	}
}

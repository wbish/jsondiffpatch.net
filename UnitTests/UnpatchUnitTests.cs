using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class UnpatchUnitTests
	{
		[Test]
		public void Unpatch_ObjectApplyDelete_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"" : true }");
			var right = JObject.Parse(@"{ }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Unpatched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property Undeleted");
			Assert.AreEqual(JTokenType.Boolean, unpatched.Property("p").Value.Type);
			Assert.IsTrue(unpatched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[Test]
		public void Unpatch_ObjectApplyAdd_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(0, unpatched.Properties().Count(), "Property Deleted");
		}

		[Test]
		public void Unpatch_ObjectApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, unpatched.Property("p").Value.Type);
			Assert.IsFalse(unpatched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[Test]
		public void Unpatch_ObjectApplyEditText_Success()
		{
			var jdp = new JsonDiffPatch();
			const string value = @"bla1h111111111111112312weldjidjoijfoiewjfoiefjefijfoejoijfiwoejfiewjfiwejfowjwifewjfejdewdwdewqwertyqwertifwiejifoiwfei";
			var left = JObject.Parse(@"{ ""p"" : """ + value + @""" }");
			var right = JObject.Parse(@"{ ""p"" : ""blah1"" }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.String, unpatched.Property("p").Value.Type, "String Type");
			Assert.AreEqual(value, unpatched.Property("p").Value.ToObject<string>(), "String value");
		}

		[Test]
		public void Unpatch_NestedObjectApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""i"": { ""p"" : false } }");
			var right = JObject.Parse(@"{ ""i"": { ""p"" : true } }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Object, patched.Property("i").Value.Type);
			Assert.AreEqual(1, ((JObject)patched.Property("i").Value).Properties().Count());
			Assert.AreEqual(JTokenType.Boolean, ((JObject)patched.Property("i").Value).Property("p").Value.Type);
			Assert.IsFalse(((JObject)patched.Property("i").Value).Property("p").Value.ToObject<bool>());
		}

		[Test]
		public void Unpatch_ArrayUnpatchAdd_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3]");
			var right = JToken.Parse(@"[1,2,3,4]");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchRemove_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3]");
			var right = JToken.Parse(@"[1,2]");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchModify_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,3,{""p"":false}]");
			var right = JToken.Parse(@"[1,4,{""p"": [1] }]");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchComplex_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"{""p"": [1,2,[1],false,""11111"",3,{""p"":false},10,10] }");
			var right = JToken.Parse(@"{""p"": [1,2,[1,3],false,""11112"",3,{""p"":true},10,10] }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchMoving_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[0,1,2,3,4,5,6,7,8,9,10]");
			var right = JToken.Parse(@"[10,0,1,7,2,4,5,6,88,9,3]");
			var patch = JToken.Parse(@"{ ""8"": [88], ""_t"": ""a"", ""_3"": ["""", 10, 3], ""_7"": ["""", 3, 3], ""_8"": [8, 0, 0], ""_10"": ["""", 0, 3] }");

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayPatchMovingNonConsecutive_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[0,1,3,4,5]");
			var right = JToken.Parse(@"[0,4,3,1,5]");
			var patch = JToken.Parse(@"{""_t"": ""a"", ""_2"": ["""", 2, 3],""_3"": ["""", 1, 3]}");

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayPatchMoveDeletingNonConsecutive_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[0,1,3,4,5]");
			var right = JToken.Parse(@"[0,5,3]");
			var patch = JToken.Parse(@"{""_t"": ""a"", ""_1"": [ 1, 0, 0], ""_3"": [4,0, 0],""_4"": [ """", 1, 3 ]}");

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}
	}
}

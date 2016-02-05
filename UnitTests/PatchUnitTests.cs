using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class PatchUnitTests
	{

		[Test]
		public void Patch_ObjectApplyDelete_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"" : true }");
			var right = JObject.Parse(@"{ }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(0, patched.Properties().Count(), "Property Deleted");
		}

		[Test]
		public void Patch_ObjectApplyAdd_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, patched.Property("p").Value.Type);
			Assert.IsTrue(patched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[Test]
		public void Patch_ObjectApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, patched.Property("p").Value.Type);
			Assert.IsTrue(patched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[Test]
		public void Patch_ObjectApplyEditText_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"" : ""bla1h111111111111112312weldjidjoijfoiewjfoiefjefijfoejoijfiwoejfiewjfiwejfowjwifewjfejdewdwdewqwertyqwertifwiejifoiwfei"" }");
			var right = JObject.Parse(@"{ ""p"" : ""blah1"" }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.String, patched.Property("p").Value.Type, "String Type");
			Assert.AreEqual("blah1", patched.Property("p").Value, "String value");
		}

		[Test]
		public void Patch_NestedObjectApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""i"": { ""p"" : false } }");
			var right = JObject.Parse(@"{ ""i"": { ""p"" : true } }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Object, patched.Property("i").Value.Type);
			Assert.AreEqual(1, ((JObject)patched.Property("i").Value).Properties().Count());
			Assert.AreEqual(JTokenType.Boolean, ((JObject)patched.Property("i").Value).Property("p").Value.Type);
			Assert.IsTrue(((JObject)patched.Property("i").Value).Property("p").Value.ToObject<bool>());
		}

		[Test]
		public void Patch_NestedComplexEdit_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""i"": { ""1"" : 1, ""2"": 2 }, ""j"": [0, 2, 4], ""k"": [1] }");
			var right = JObject.Parse(@"{ ""i"": { ""1"" : 1, ""2"": 3 }, ""j"": [0, 2, 3], ""k"": null }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch);

			Assert.AreEqual(right.ToString(), patched.ToString());
		}

		[Test]
		public void Patch_NestedComplexEditDifferentLeft_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""i"": { ""1"" : 1, ""2"": 2 }, ""j"": [0, 2, 4], ""k"": [1] }");
			var right = JObject.Parse(@"{ ""i"": { ""1"" : 1, ""2"": 3 }, ""j"": [0, 2, 3], ""k"": null }");
			var patch = jdp.Diff(JObject.Parse(@"{ ""k"": { ""i"": [1] } }"), right);

			var patched = jdp.Patch(left, patch);

			Assert.AreEqual(right.ToString(), patched.ToString());
		}

		[Test]
		public void Patch_NullLeft_Exception()
		{
			var jdp = new JsonDiffPatch();
			var patch = JToken.Parse(@"[true]");

			JToken result = jdp.Patch(null, patch);

			Assert.IsNotNull(result);
			Assert.AreEqual(JTokenType.Boolean, result.Type);
			Assert.AreEqual(true, result.ToObject<bool>());
		}

		[Test]
		public void Patch_ArrayDiff_NotImplementedException()
		{
			var jdp = new JsonDiffPatch();

			Assert.Throws<NotImplementedException>(
				() => jdp.Patch(JObject.Parse(@"{}"), JObject.Parse(@"{ ""p"" : { ""_t"" : ""a""} }")));
		}
	}
}

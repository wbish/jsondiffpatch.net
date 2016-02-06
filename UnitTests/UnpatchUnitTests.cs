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
		public void Unpatch_ArrayDiff_NotImplementedException()
		{
			var jdp = new JsonDiffPatch();

			Assert.Throws<NotImplementedException>(
				() => jdp.Unpatch(JObject.Parse(@"{ ""p"": [] }"), JObject.Parse(@"{ ""p"" : { ""_t"" : ""a""} }")));
		}
	}
}

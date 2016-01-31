using System;
using System.Linq;
using JsonDiffPatchDotNet.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestClass]
	public class UnpatchUnitTests
	{
		[TestMethod]
		public void Unpatch_ApplyDelete_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : true }");
			var right = JObject.Parse(@"{ }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch);

			Assert.IsNotNull(unpatched, "Unpatched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property Undeleted");
			Assert.AreEqual(JTokenType.Boolean, unpatched.Property("p").Value.Type);
			Assert.IsTrue(unpatched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[TestMethod]
		public void Unpatch_ApplyAdd_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch);

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(0, unpatched.Properties().Count(), "Property Deleted");
		}

		[TestMethod]
		public void Unpatch_ApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch);

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, unpatched.Property("p").Value.Type);
			Assert.IsFalse(unpatched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[TestMethod]
		public void Unpatch_ApplyEditText_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			const string value = @"bla1h111111111111112312weldjidjoijfoiewjfoiefjefijfoejoijfiwoejfiewjfiwejfowjwifewjfejdewdwdewqwertyqwertifwiejifoiwfei";
            var left = JObject.Parse(@"{ ""p"" : """ + value + @""" }");
			var right = JObject.Parse(@"{ ""p"" : ""blah1"" }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch);

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.String, unpatched.Property("p").Value.Type, "String Type");
			Assert.AreEqual(value, unpatched.Property("p").Value.ToObject<string>(), "String value");
		}

		[TestMethod]
		[ExpectedException(typeof(PatchException))]
		public void Unpatch_ApplyEditStrict_PatchException()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			jdp.Unpatch(JObject.Parse("{}"), patch);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Unpatch_NullLeft_Exception()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });

			jdp.Unpatch(null, JObject.Parse(@"{}"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Unpatch_NullPatch_Exception()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });

			jdp.Unpatch(JObject.Parse(@"{}"), null);
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void Unpatch_ArrayDiff_NotImplementedException()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });

			jdp.Unpatch(JObject.Parse(@"{}"), JObject.Parse(@"{ ""p"" : { ""_t"" : ""a""} }"));
		}
	}
}

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestClass]
	public class PatchUnitTests
	{
		
		[TestMethod]
		public void Patch_ObjectApplyDelete_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : true }");
			var right = JObject.Parse(@"{ }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(0, patched.Properties().Count(), "Property Deleted");
		}

		[TestMethod]
		public void Patch_ObjectApplyAdd_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, patched.Property("p").Value.Type);
			Assert.IsTrue(patched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[TestMethod]
		public void Patch_ObjectApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch(new Options {Patch = PatchMode.StrictAbort});
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, patched.Property("p").Value.Type);
			Assert.IsTrue(patched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[TestMethod]
		public void Patch_ObjctApplyEditText_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : ""bla1h111111111111112312weldjidjoijfoiewjfoiefjefijfoejoijfiwoejfiewjfiwejfowjwifewjfejdewdwdewqwertyqwertifwiejifoiwfei"" }");
			var right = JObject.Parse(@"{ ""p"" : ""blah1"" }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.String, patched.Property("p").Value.Type, "String Type");
			Assert.AreEqual("blah1", patched.Property("p").Value, "String value");
		}

		/*
		[TestMethod]
		[ExpectedException(typeof(PatchException))]
		public void Patch_ObjectApplyEditStrict_PatchException()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			jdp.Patch(JObject.Parse("{}"), patch);
		}
		*/

		[TestMethod]
		public void Patch_NullLeft_Exception()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var patch = JToken.Parse(@"[true]");

            JToken result = jdp.Patch(null, patch);

			Assert.IsNotNull(result);
			Assert.AreEqual(JTokenType.Boolean, result.Type);
			Assert.AreEqual(true, result.ToObject<bool>());
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void Patch_ArrayDiff_NotImplementedException()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });

			jdp.Patch(JObject.Parse(@"{}"), JObject.Parse(@"{ ""p"" : { ""_t"" : ""a""} }"));
		}
	}
}

using System.Linq;
using JsonDiffPatchDotNet.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestClass]
	public class PatchUnitTests
	{
		[TestMethod]
		public void Patch_ApplyDelete_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : true }");
			var right = JObject.Parse(@"{ }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch);

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(0, patched.Properties().Count(), "Property Deleted");
		}

		[TestMethod]
		public void Patch_ApplyAdd_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch);

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, patched.Property("p").Value.Type);
			Assert.IsTrue(patched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[TestMethod]
		public void Patch_ApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch(new Options {Patch = PatchMode.StrictAbort});
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch);

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, patched.Property("p").Value.Type);
			Assert.IsTrue(patched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[TestMethod]
		public void Patch_ApplyEditText_Success()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : ""bla1h"" }");
			var right = JObject.Parse(@"{ ""p"" : ""blah1"" }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Patch(left, patch);

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.String, patched.Property("p").Value.Type, "String Type");
			Assert.AreEqual("blah1", patched.Property("p").Value, "String is: blah1");
		}

		[TestMethod]
		[ExpectedException(typeof(PatchException))]
		public void Patch_ApplyEditStrict_PatchException()
		{
			var jdp = new JsonDiffPatch(new Options { Patch = PatchMode.StrictAbort });
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			jdp.Patch(JObject.Parse("{}"), patch);
		}
	}
}

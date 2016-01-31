using System;
using System.Linq;
using JsonDiffPatchDotNet.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestClass]
	public class DiffUnitTests
	{
		[TestMethod]
		public void Diff_EmptyObjects_EmptyPatch()
		{
			var jdp = new JsonDiffPatch();
			var empty = JObject.Parse(@"{}");

			JObject result = jdp.Diff(empty, empty);

			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Properties().Count(), "No properties");
		}

		[TestMethod]
		public void Diff_EqualBooleanProperty_NoDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": true }");

			JObject result = jdp.Diff(left, right);

			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Properties().Count(), "No properties");
		}

		[TestMethod]
		public void Diff_DiffBooleanProperty_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": false }");

			JObject result = jdp.Diff(left, right);
			
			Assert.IsNotNull(result.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, result.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(2, ((JArray)result.Property("p").Value).Count, "Array Length");
			Assert.AreEqual(true, ((JArray)result.Property("p").Value)[0], "Array Old Value");
			Assert.AreEqual(false, ((JArray)result.Property("p").Value)[1], "Array New Value");
		}

		[TestMethod]
		public void Diff_BooleanPropertyDeleted_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"": true }");
			var right = JObject.Parse(@"{ }");

			JObject result = jdp.Diff(left, right);

			Assert.IsNotNull(result.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, result.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)result.Property("p").Value).Count, "Array Length");
			Assert.AreEqual(true, ((JArray)result.Property("p").Value)[0], "Array Old Value");
			Assert.AreEqual(0, ((JArray)result.Property("p").Value)[1], "Array New Value");
			Assert.AreEqual(0, ((JArray)result.Property("p").Value)[2], "Array Deleted Indicator");
		}

		[TestMethod]
		public void Diff_BooleanPropertyAdded_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"": true }");

			JObject result = jdp.Diff(left, right);

			Assert.IsNotNull(result.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, result.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(1, ((JArray)result.Property("p").Value).Count, "Array Length");
			Assert.AreEqual(true, ((JArray)result.Property("p").Value)[0], "Array Added Value");
		}

		[TestMethod]
		public void Diff_EfficientStringDiff_ValidPatch()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""blah1"" }");

			JObject result = jdp.Diff(left, right);

			Assert.IsNotNull(result.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, result.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)result.Property("p").Value).Count, "Array Length");
			Assert.AreEqual("@@ -1,64 +1,5 @@\n-lp.Value.ToString().Length %3e _options.MinEfficientTextDiffLength\n+blah1\n", ((JArray)result.Property("p").Value)[0], "Array Added Value");
			Assert.AreEqual(0, ((JArray)result.Property("p").Value)[1], "Array Added Value");
			Assert.AreEqual(2, ((JArray)result.Property("p").Value)[2 ], "Array String Diff Indicator");
		}

		[TestMethod]
		public void Diff_EfficientStringDiff_NoChanges()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");

			JObject result = jdp.Diff(left, right);

			Assert.AreEqual(0, result.Properties().Count(), "No Changes");
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Diff_LeftNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			jdp.Diff(null, obj);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Diff_RightNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			jdp.Diff(obj, null);
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void Diff_EfficientArrayDiff_Exception()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var array = JObject.Parse(@"{ ""p"": [] }");

			jdp.Diff(array, array);
		}
	}
}

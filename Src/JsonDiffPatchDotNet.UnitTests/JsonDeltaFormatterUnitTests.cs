using System.Linq;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class JsonDeltaFormatterUnitTests
	{
		private static readonly JsonDiffPatch Differ = new JsonDiffPatch();
		private static readonly JsonDeltaFormatter Formatter = new JsonDeltaFormatter();

		[Test]
		public void Format_EmptyDiffIsEmpty_Success()
		{
			var left = JObject.Parse(@"{}");
			var right = JObject.Parse(@"{}");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(0, operations.Count);
		}

		[Test]
		public void Format_SupportsRemove_Success()
		{
			var left = JObject.Parse(@"{ ""a"": ""a"", ""b"": ""b"", ""c"" : ""c"" }");
			var right = JObject.Parse(@"{ ""a"": ""a"", ""b"": ""b"" }");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Remove, "/c");
		}

		[Test]
		public void Format_SupportsAdd_Success()
		{
			var left = JObject.Parse(@"{ ""a"": ""a"", ""b"": ""b"" }");
			var right = JObject.Parse(@"{ ""a"": ""a"", ""b"": ""b"", ""c"" : ""c"" }");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Add, "/c", new JValue("c"));
		}

		[Test]
		public void Format_SupportsReplace_Success()
		{
			var left = JObject.Parse(@"{ ""a"": ""a"", ""b"": ""b"" }");
			var right = JObject.Parse(@"{ ""a"": ""a"", ""b"": ""c"" }");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Replace, "/b", new JValue("c"));
		}

		[Test]
		public void Format_SupportsArrayAdd_Success()
		{
			var left = JObject.Parse(@"{ ""items"" : [""car"", ""bus""] }");
			var right = JObject.Parse(@"{ ""items"" : [""bike"", ""car"", ""bus""] }");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Add, "/items/0", JValue.CreateString("bike"));
		}

		[Test]
		public void Format_SupportsArrayRemove_Success()
		{
			var left = JObject.Parse(@"{ ""items"" : [""bike"", ""car"", ""bus""] }");
			var right = JObject.Parse(@"{ ""items"" : [""car"", ""bus""] }");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Remove, "/items/0");
		}

		[Test]
		public void Format_SupportsArrayMove_Success()
		{
			var left = JObject.Parse(@"{ ""items"" : [""bike"", ""car"", ""bus""] }");
			var right = JObject.Parse(@"{ ""items"" : [""bike"", ""bus"", ""car""] }");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(2, operations.Count);
			AssertOperation(operations[0], OperationTypes.Remove, "/items/2");
			AssertOperation(operations[1], OperationTypes.Add, "/items/1", JValue.CreateString("bus"));
		}

		[Test]
		public void Format_ArrayAddsInAscOrder_Success()
		{
			var left = JArray.Parse(@"[]");
			var right = JArray.Parse(@"[1, 2, 3]");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(3, operations.Count);
			AssertOperation(operations[0], OperationTypes.Add, "/0", new JValue(1));
			AssertOperation(operations[1], OperationTypes.Add, "/1", new JValue(2));
			AssertOperation(operations[2], OperationTypes.Add, "/2", new JValue(3));
		}

		[Test]
		public void Format_ArrayRemoveInDescOrder_Success()
		{
			var left = JArray.Parse("[1, 2, 3]");
			var right = JArray.Parse("[1]");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.AreEqual(2, operations.Count);
			AssertOperation(operations[0], OperationTypes.Remove, "/2");
			AssertOperation(operations[1], OperationTypes.Remove, "/1");
		}

		[Test]
		public void Format_SortsRemoveOperations_Success()
		{
			const string patchJson = @"
{
	""a"": {
		""a"": [0,0,0],
		""b"": [0,0,0],
		""c"": {
			""a"": {
				""a"": [0,0,0],
				""b"": [0,0,0],
				""c"": [0,0,0],
				""d"": [0,0,0],
				""e"": [0,0,0],
				""f"": [0,0,0]
			}
		}
	},
	""b"": [0,0,0],
	""c"": [0,0,0],
	""d"": [0,0,0],
	""e"": [0,0,0],
	""f"": [0,0,0],
	""g"": [0,0,0],
	""h"": [0,0,0],
	""i"": {
		""a"": {
			""a"": {
				""_t"": ""a"",
				""_0"": [0,0,0],
				""_1"": [0,0,0]
			}
		}
	}
}
";
			var patch = JToken.Parse(patchJson);
			var operations = Formatter.Format(patch);

			var paths = operations.Select(o => o.Path).ToList();
			// removal of the array item at index 1 should come before the item at index 0
			Assert.Less(paths.IndexOf("/i/a/a/1"), paths.IndexOf("/i/a/a/0"));
		}

		public void Format_EscapeOfJsonPointer_Success()
		{
			var left = JObject.Parse(@"{ ""a/b"": ""a"", ""a~b"": ""ab"", ""a/~b"": ""abb"",""a/b/c~"": ""abc"" }");
			var right = JObject.Parse(@"{ ""a/b"": ""ab"", ""a~b"": ""ba"", ""a/~b"": ""bba"",""a/b/c~"": ""cba""  }");
			var patch = Differ.Diff(left, right);
			var operations = Formatter.Format(patch);

			Assert.IsTrue(operations.Any(x => x.Path.Equals("/a~1b") && x.Value.ToString().Equals("ab")));
			Assert.IsTrue(operations.Any(x => x.Path.Equals("/a~0b") && x.Value.ToString().Equals("ba")));
			Assert.IsTrue(operations.Any(x => x.Path.Equals("/a~1~0b") && x.Value.ToString().Equals("bba")));
			Assert.IsTrue(operations.Any(x => x.Path.Equals("/a~1b~1c~0") && x.Value.ToString().Equals("cba")));
		}

		private void AssertOperation(Operation operation, string expectedOp, string expectedPath, JValue expectedValue = null)
		{
			Assert.AreEqual(expectedOp, operation.Op);
			Assert.AreEqual(expectedPath, operation.Path);

			if (expectedValue != null)
			{
				var value = operation.Value as JValue;
				Assert.IsNotNull(value, "Operation value was expected to be a JValue");
				Assert.AreEqual(expectedValue, value);
			}
			else
			{
				Assert.IsNull(operation.Value);
			}
		}
	}
}

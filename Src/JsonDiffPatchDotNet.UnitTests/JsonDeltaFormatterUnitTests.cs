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

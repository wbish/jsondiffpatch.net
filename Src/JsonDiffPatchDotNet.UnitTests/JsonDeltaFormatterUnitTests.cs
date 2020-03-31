using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class JsonDeltaFormatterUnitTests
	{
		[Test]
		public void Format_SupportsRemove_Success()
		{
			var jdp = new JsonDiffPatch();
			var formatter = new JsonDeltaFormatter();
			var left = JObject.Parse(@"{ ""p"" : true }");
			var right = JObject.Parse(@"{ }");
			var patch = jdp.Diff(left, right);
			var operations = formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Remove, "/p");
		}

		[Test]
		public void Format_SupportsAdd_Success()
		{
			var jdp = new JsonDiffPatch();
			var formatter = new JsonDeltaFormatter();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);
			var operations = formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Add, "/p", new JValue(true));
		}

		[Test]
		public void Format_SupportsReplace_Success()
		{
			var jdp = new JsonDiffPatch();
			var formatter = new JsonDeltaFormatter();
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);
			var operations = formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Replace, "/p", new JValue(true));
		}

		[Test]
		public void Format_SupportsArrayAdd_Success()
		{
			var jdp = new JsonDiffPatch();
			var formatter = new JsonDeltaFormatter();
			var left = JObject.Parse(@"{ ""items"" : [""car"", ""bus""] }");
			var right = JObject.Parse(@"{ ""items"" : [""bike"", ""car"", ""bus""] }");
			var patch = jdp.Diff(left, right);
			var operations = formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Add, "/items/0", JValue.CreateString("bike"));
		}

		[Test]
		public void Format_SupportsArrayRemove_Success()
		{
			var jdp = new JsonDiffPatch();
			var formatter = new JsonDeltaFormatter();
			var left = JObject.Parse(@"{ ""items"" : [""bike"", ""car"", ""bus""] }");
			var right = JObject.Parse(@"{ ""items"" : [""car"", ""bus""] }");
			var patch = jdp.Diff(left, right);
			var operations = formatter.Format(patch);

			Assert.AreEqual(1, operations.Count);
			AssertOperation(operations[0], OperationTypes.Remove, "/items/0");
		}

		[Test]
		public void Format_SupportsArrayMove_Success()
		{
			var jdp = new JsonDiffPatch();
			var formatter = new JsonDeltaFormatter();
			var left = JObject.Parse(@"{ ""items"" : [""bike"", ""car"", ""bus""] }");
			var right = JObject.Parse(@"{ ""items"" : [""bike"", ""bus"", ""car""] }");
			var patch = jdp.Diff(left, right);
			var operations = formatter.Format(patch);

			Assert.AreEqual(2, operations.Count);
			AssertOperation(operations[0], OperationTypes.Remove, "/items/2");
			AssertOperation(operations[1], OperationTypes.Add, "/items/1", JValue.CreateString("bus"));
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

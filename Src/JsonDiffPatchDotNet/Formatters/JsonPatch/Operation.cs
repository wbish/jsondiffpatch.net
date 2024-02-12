using Newtonsoft.Json;

namespace JsonDiffPatchDotNet.Formatters.JsonPatch
{
	public class Operation
	{
		public Operation() { }

		public Operation(string op, string path, string from)
		{
			Op = op;
			Path = path;
			From = from;
		}

		public Operation(string op, string path, string from, object value, object oldValue)
		{
			Op = op;
			Path = path;
			From = from;
			Value = value;
			OldValue = oldValue;
		}

		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("op")]
		public string Op { get; set; }

		[JsonProperty("from")]
		public string From { get; set; }

		[JsonProperty("value")]
		public object Value { get; set; }
		
		[JsonProperty("old", NullValueHandling = NullValueHandling.Ignore)]
		public object OldValue { get; set; }
	}
}

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

		public Operation(string op, string path, string from, object value)
		{
			Op = op;
			Path = path;
			From = from;
			Value = value;
		}

		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("op")]
		public string Op { get; set; }

		[JsonProperty("from")]
		public string From { get; set; }

		[JsonProperty("value")]
		public object Value { get; set; }
	}
}

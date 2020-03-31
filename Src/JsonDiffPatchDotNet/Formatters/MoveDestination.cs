using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.Formatters
{
	public class MoveDestination
	{
		public MoveDestination(string key, JToken value)
		{
			Key = key;
			Value = value;
		}

		public string Key { get; }

		public JToken Value { get; }
	}
}

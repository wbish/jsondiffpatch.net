using System.Collections.Generic;
using System.Linq;

namespace JsonDiffPatchDotNet.Formatters.JsonPatch
{
	public class JsonFormatContext : IFormatContext<IList<Operation>>
	{
		public JsonFormatContext()
		{
			Operations = new List<Operation>();
			Path = new List<string>();
		}

		public IList<Operation> Operations { get; }

		public IList<string> Path { get; }

		public IList<Operation> Result()
		{
			return Operations;
		}

		public void PushCurrentOp(string op)
		{
			Operations.Add(new Operation(op, CurrentPath(), null));
		}

		public void PushCurrentOp(string op, object value)
		{
			Operations.Add(new Operation(op, CurrentPath(), null, value));
		}

		public void PushMoveOp(string to)
		{
			Operations.Add(new Operation(OperationTypes.Move, ToPath(to), CurrentPath()));
		}

		private string CurrentPath()
		{
			return $"/{string.Join("/", Path)}";
		}

		private string ToPath(string toPath)
		{
			var to = Path.ToList();
			to[to.Count - 1] = toPath;
			return $"/{string.Join("/", to)}";
		}
	}
}

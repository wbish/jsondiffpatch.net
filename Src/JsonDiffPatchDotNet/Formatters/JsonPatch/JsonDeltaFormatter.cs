using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.Formatters.JsonPatch
{
	public class JsonDeltaFormatter : BaseDeltaFormatter<JsonFormatContext, IList<Operation>>
	{
		protected override bool IncludeMoveDestinations => true;

		public override IList<Operation> Format(JToken delta)
		{
			var result = base.Format(delta);
			return ReorderOps(result);
		}

		protected override void Format(DeltaType type, JsonFormatContext context, JToken delta, JToken leftValue, string key, string leftKey, MoveDestination movedFrom)
		{
			switch (type)
			{
				case DeltaType.Added:
					FormatAdded(context, delta);
					break;

				case DeltaType.Node:
					FormatNode(context, delta, leftValue);
					break;

				case DeltaType.Modified:
					FormatModified(context, delta);
					break;

				case DeltaType.Deleted:
					FormatDeleted(context);
					break;

				case DeltaType.Moved:
					FormatMoved(context, delta);
					break;

				case DeltaType.Unknown:
				case DeltaType.Unchanged:
				case DeltaType.MoveDestination:
					break;

				case DeltaType.TextDiff:
					throw new InvalidOperationException("JSON RFC 6902 does not support TextDiff.");
			}
		}

		protected override void NodeBegin(JsonFormatContext context, string key, string leftKey, DeltaType type, NodeType nodeType, bool isLast)
		{
			context.Path.Add(leftKey);
		}

		protected override void NodeEnd(JsonFormatContext context, string key, string leftKey, DeltaType type, NodeType nodeType, bool isLast)
		{
			if (context.Path.Count > 0)
				context.Path.RemoveAt(context.Path.Count - 1);
		}

		protected override void RootBegin(JsonFormatContext context, DeltaType type, NodeType nodeType) { }

		protected override void RootEnd(JsonFormatContext context, DeltaType type, NodeType nodeType) { }

		private void FormatNode(JsonFormatContext context, JToken delta, JToken left)
		{
			FormatDeltaChildren(context, delta, left);
		}

		private void FormatAdded(JsonFormatContext context, JToken delta)
		{
			context.PushCurrentOp(OperationTypes.Add, delta[0]);
		}

		private void FormatModified(JsonFormatContext context, JToken delta)
		{
			context.PushCurrentOp(OperationTypes.Replace, delta[1]);
		}

		private void FormatDeleted(JsonFormatContext context)
		{
			context.PushCurrentOp(OperationTypes.Remove);
		}

		private void FormatMoved(JsonFormatContext context, JToken delta)
		{
			context.PushMoveOp(delta[1].ToString());
		}

		private IList<Operation> ReorderOps(IList<Operation> result)
		{
			var removeOpsOtherOps = PartitionRemoveOps(result);
			var removeOps = removeOpsOtherOps[0];
			var otherOps = removeOpsOtherOps[1];
			Array.Sort(removeOps, new RemoveOperationComparer());
			return removeOps.Concat(otherOps).ToList();
		}

		private IList<Operation[]> PartitionRemoveOps(IList<Operation> result)
		{
			var left = new List<Operation>();
			var right = new List<Operation>();

			foreach (var op in result)
				(op.Op.Equals("remove", StringComparison.Ordinal) ? left : right).Add(op);

			return new List<Operation[]> {left.ToArray(), right.ToArray()};
		}

		private class RemoveOperationComparer : IComparer<Operation>
		{
			public int Compare(Operation a, Operation b)
			{
				if (a == null) throw new ArgumentNullException(nameof(a));
				if (b == null) throw new ArgumentNullException(nameof(b));

				var splitA = a.Path.Split('/');
				var splitB = b.Path.Split('/');

				return splitA.Length != splitB.Length
					? splitA.Length - splitB.Length
					: CompareByIndexDesc(splitA.Last(), splitB.Last());
			}

			private static int CompareByIndexDesc(string indexA, string indexB)
				=> int.TryParse(indexA, out var a) && int.TryParse(indexB, out var b) ? b - a : 0;
		}
	}
}

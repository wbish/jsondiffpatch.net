using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.Formatters
{
	public abstract class BaseDeltaFormatter<TContext, TResult>
		where TContext : IFormatContext<TResult>, new()
	{
		public delegate void DeltaKeyIterator(string key, string leftKey, MoveDestination movedFrom, bool isLast);

		private static readonly IComparer<string> s_arrayKeyComparer = new ArrayKeyComparer();

		public virtual TResult Format(JToken delta)
		{
			var context = new TContext();
			Recurse(context, delta, left: null, key: null, leftKey: null, movedFrom: null, isLast: false);
			return context.Result();
		}

		protected abstract bool IncludeMoveDestinations { get; }

		protected abstract void NodeBegin(TContext context, string key, string leftKey, DeltaType type, NodeType nodeType, bool isLast);

		protected abstract void NodeEnd(TContext context, string key, string leftKey, DeltaType type, NodeType nodeType, bool isLast);

		protected abstract void RootBegin(TContext context, DeltaType type, NodeType nodeType);

		protected abstract void RootEnd(TContext context, DeltaType type, NodeType nodeType);

		protected abstract void Format(DeltaType type, TContext context, JToken delta, JToken leftValue, string key, string leftKey, MoveDestination movedFrom);

		protected void Recurse(TContext context, JToken delta, JToken left, string key, string leftKey, MoveDestination movedFrom, bool isLast)
		{
			var useMoveOriginHere = delta != null && movedFrom != null;
			var leftValue = useMoveOriginHere ? movedFrom.Value : left;

			if (delta == null && string.IsNullOrEmpty(key))
				return;

			var type = GetDeltaType(delta, movedFrom);
			var nodeType = type == DeltaType.Node ? (delta["_t"]?.Value<string>() == "a" ? NodeType.Array : NodeType.Object) : NodeType.Unknown;

			if (!string.IsNullOrEmpty(key))
				NodeBegin(context, key, leftKey, type, nodeType, isLast);
			else
				RootBegin(context, type, nodeType);

			Format(type, context, delta, leftValue, key, leftKey, movedFrom);

			if (!string.IsNullOrEmpty(key))
				NodeEnd(context, key, leftKey, type, nodeType, isLast);
			else
				RootEnd(context, type, nodeType);
		}

		protected void FormatDeltaChildren(TContext context, JToken delta, JToken left)
		{
			ForEachDeltaKey(delta, left, Iterator);

			void Iterator(string key, string leftKey, MoveDestination movedFrom, bool isLast)
			{
				Recurse(context, delta[key], left?[leftKey], key, leftKey, movedFrom, isLast);
			}
		}

		protected void ForEachDeltaKey(JToken delta, JToken left, DeltaKeyIterator iterator)
		{
			var keys = new List<string>();
			var arrayKeys = false;
			var movedDestinations = new Dictionary<string, MoveDestination>();

			if (delta is JObject jObject)
			{
				keys = jObject.Properties().Select(p => p.Name).ToList();
				arrayKeys = jObject["_t"]?.Value<string>() == "a";
			}

			if (left != null && left is JObject leftObject)
			{
				foreach (var kvp in leftObject)
				{
					if (delta[kvp.Key] == null && (!arrayKeys || delta["_" + kvp.Key] == null))
					{
						keys.Add(kvp.Key);
					}
				}
			}

			if (delta is JObject deltaObject)
			{
				foreach (var kvp in deltaObject)
				{
					var value = kvp.Value;
					if (value is JArray valueArray && valueArray.Count == 3)
					{
						var diffOp = valueArray[2].Value<int>();
						if (diffOp == (int)DiffOperation.ArrayMove)
						{
							var moveKey = valueArray[1].ToString();
							movedDestinations[moveKey] = new MoveDestination(kvp.Key, left?[kvp.Key.Substring(1)]);

							if (IncludeMoveDestinations && left == null && deltaObject.Property(moveKey) == null)
								keys.Add(moveKey);
						}
					}
				}
			}

			if (arrayKeys)
				keys.Sort(s_arrayKeyComparer);
			else
				keys.Sort();

			for (var index = 0; index < keys.Count; index++)
			{
				var key = keys[index];
				if (arrayKeys && key == "_t")
					continue;

				var leftKey = arrayKeys
					? key.TrimStart('_')
					: key;

				var isLast = index == keys.Count - 1;
				var movedFrom = movedDestinations.ContainsKey(leftKey) ? movedDestinations[leftKey] : null;
				iterator(key, leftKey, movedFrom, isLast);
			}
		}

		protected static DeltaType GetDeltaType(JToken delta = null, MoveDestination movedFrom = null)
		{
			if (delta == null)
				return movedFrom != null ? DeltaType.MoveDestination : DeltaType.Unchanged;

			switch (delta.Type)
			{
				case JTokenType.Array:
				{
					var deltaArray = (JArray)delta;
					switch (deltaArray.Count)
					{
						case 1: return DeltaType.Added;
						case 2: return DeltaType.Modified;
						case 3:
						{
							switch ((DiffOperation)deltaArray[2].Value<int>())
							{
								case DiffOperation.Deleted: return DeltaType.Deleted;
								case DiffOperation.TextDiff: return DeltaType.TextDiff;
								case DiffOperation.ArrayMove: return DeltaType.Moved;
							}
							break;
						}
					}

					break;
				}

				case JTokenType.Object:
					return DeltaType.Node;
			}

			return DeltaType.Unknown;
		}
	}
}

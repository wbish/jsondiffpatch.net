using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet.Formatters.JsonPatch
{
    public class JsonDeltaFormatter : BaseDeltaFormatter<JsonFormatContext, IList<Operation>>
    {
        protected override bool IncludeMoveDestinations => true;

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
    }
}

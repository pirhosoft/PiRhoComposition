using PiRhoSoft.Expressions;
using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Branch", 1)]
	[OutputDictionaryNode(nameof(Outputs))]
	public class BranchNode : GraphNode
	{
		public ReadOnlyExpression Switch = new ReadOnlyExpression();

		[Dictionary]
		public GraphNodeDictionary Outputs = new GraphNodeDictionary();

		public GraphNode Default;

		public override Color NodeColor => Colors.Branch;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var name = Switch.Execute(variables, VariableType.String).AsString;

			if (Outputs.TryGetValue(name, out var output))
				graph.GoTo(output, GetConnectionName(nameof(Outputs), name));
			else
				graph.GoTo(Default, nameof(Default));

			yield break;
		}
	}
}

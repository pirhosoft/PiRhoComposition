using PiRhoSoft.Expressions;
using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Branch List", 1)]
	[OutputListNode(nameof(Outputs))]
	public class BranchListNode : GraphNode
	{
		public ReadOnlyExpression Switch = new ReadOnlyExpression();

		[List]
		public GraphNodeList Outputs = new GraphNodeList();

		public GraphNode Default;

		public override Color NodeColor => Colors.Branch;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var index = Switch.Execute(variables, VariableType.Int).AsInt;

			if (index >= 0 && index < Outputs.Count)
				graph.GoTo(Outputs[index], GetConnectionName(nameof(Outputs), index));
			else
				graph.GoTo(Default, nameof(Default));

			yield break;
		}
	}
}

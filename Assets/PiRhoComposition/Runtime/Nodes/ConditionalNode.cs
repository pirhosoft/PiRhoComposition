using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Conditional", 0)]
	public class ConditionalNode : GraphNode
	{
		public GraphNode OnTrue;
		public GraphNode OnFalse;

		public ReadOnlyExpression Condition = new ReadOnlyExpression();

		public override Color NodeColor => Colors.Branch;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var condition = Condition.Execute(variables, VariableType.Bool);

			if (condition.AsBool)
				graph.GoTo(OnTrue, nameof(OnTrue));
			else
				graph.GoTo(OnFalse, nameof(OnFalse));

			yield break;
		}
	}
}

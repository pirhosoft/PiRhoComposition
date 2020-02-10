using PiRhoSoft.Expressions;
using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Composition/Expression", 0)]
	public class ExpressionNode : GraphNode
	{
		public GraphNode Next;

		[Stretch]
		public Expression Expression = new Expression();

		public override Color NodeColor => Colors.ExecutionDark;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			Expression.Execute(variables);
			graph.GoTo(Next, nameof(Next));
			yield break;
		}
	}
}

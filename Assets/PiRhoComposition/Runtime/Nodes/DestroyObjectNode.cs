using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Object Manipulation/Destroy Object", 2)]
	public class DestroyObjectNode : GraphNode
	{
		public GraphNode Next;

		[VariableConstraint(typeof(Object))]
		public ReadOnlyExpression Target = new ReadOnlyExpression();

		public override Color NodeColor => Colors.SequencingDark;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			if (Target.IsValid)
			{
				var target = Target.Execute<Object>(variables);
				Destroy(target);
			}

			graph.GoTo(Next, nameof(Next));

			yield break;
		}
	}
}

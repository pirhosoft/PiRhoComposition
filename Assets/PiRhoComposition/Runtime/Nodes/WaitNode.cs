using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Wait", 24)]
	public class WaitNode : GraphNode
	{
		public GraphNode Next;

		[VariableConstraint(0.0f, true)]
		public VariableSource Time = new VariableSource(Variable.Float(1.0f));

		public bool UseScaledTime = true;

		public override Color NodeColor => Colors.Sequencing;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var variable = Time.Resolve(variables, VariableType.Float);

			if (UseScaledTime)
				yield return new WaitForSeconds(variable.AsFloat);
			else
				yield return new WaitForSecondsRealtime(variable.AsFloat);

			graph.GoTo(Next, nameof(Next));
		}
	}
}

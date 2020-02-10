using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Loop", 20)]
	public class LoopNode : GraphNode
	{
		public GraphNode Loop;

		public AssignmentExpression Index = new AssignmentExpression();
		public ReadOnlyExpression Condition = new ReadOnlyExpression();

		public override Color NodeColor => Colors.Loop;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			//var index = 0;

			while (true)
			{
				if (Index.IsValid)
				{
					// TODO: this in expression execute
					// TODO: assign to Variable.Int(index++)
					Index.Execute(variables);
				}

				var condition = Condition.Execute(variables, VariableType.Bool);

				if (!condition.AsBool || Loop == null)
					break;

				yield return graph.Run(Loop, variables, nameof(Loop));
			}
		}
	}
}
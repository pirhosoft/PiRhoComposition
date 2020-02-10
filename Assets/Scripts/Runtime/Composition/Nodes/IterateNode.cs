using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Iterate", 21)]
	public class IterateNode : GraphNode
	{
		public GraphNode Loop;

		public ReadOnlyExpression Container = new ReadOnlyExpression();
		public AssignmentExpression Index = new AssignmentExpression();
		public AssignmentExpression Value = new AssignmentExpression();

		public override Color NodeColor => Colors.Loop;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			if (Loop != null)
			{
				var variable = Container.Execute(variables);

				if (variable.TryGetList(out var list))
				{
					for (var i = 0; i < list.VariableCount; i++)
					{
						var item = list.GetVariable(i);
						yield return SetValues(graph, variables, i, item);
					}
				}
				else if (variable.TryGetDictionary(out var dictionary))
				{
					var i = 0;
					var names = dictionary.VariableNames;

					foreach (var name in names)
					{
						var item = dictionary.GetVariable(name);
						yield return SetValues(graph, variables, i++, item);
					}
				}
				else
				{
					throw new VariableSourceException();
				}
			}

			yield break;
		}

		private IEnumerator SetValues(IGraphRunner graph, IVariableDictionary variables, int iteration, Variable item)
		{
			if (Index.IsValid)
				Index.Execute(variables); // Variable.Int(iteration)

			if (Value.IsValid)
				Value.Execute(variables); // item

			yield return graph.Run(Loop, variables, nameof(Loop));
		}
	}
}
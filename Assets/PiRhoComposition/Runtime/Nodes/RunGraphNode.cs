using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Composition/Run Graph", 1)]
	public class RunGraphNode : GraphNode
	{
		public GraphNode Next;

		[VariableConstraint(typeof(Graph))]
		public VariableSource TargetGraph = new VariableSource();

		public bool WaitForCompletion = true;

		public override Color NodeColor => Colors.ExecutionLight;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			// TODO: inputs
			// TODO: add a RunParallel to IGraphRunner instead of CompositionManager.Instance.StartCoroutine

			var target = TargetGraph.Resolve<Graph>(variables);

			if (WaitForCompletion)
				yield return target.Execute();
			else
				CompositionManager.Instance.StartCoroutine(target.Execute());

			graph.GoTo(Next, nameof(Next));
		}
	}
}

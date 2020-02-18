using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Object Manipulation/Unload Scene", 101)]
	public class UnloadSceneNode : GraphNode
	{
		public GraphNode Next;

		[VariableConstraint(VariableType.Asset)]
		public VariableSource Scene = new VariableSource();

		public bool WaitForCompletion = true;
		public bool Cleanup = true;

		public override Color NodeColor => Colors.ExecutionDark;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			if (WaitForCompletion)
				yield return UnloadScene(variables);
			else
				CompositionManager.Instance.StartCoroutine(UnloadScene(variables));

			graph.GoTo(Next, nameof(Next));
		}

		private IEnumerator UnloadScene(IVariableDictionary variables)
		{
			var variable = Scene.Resolve(variables, VariableType.Asset);

			yield return variable.AsAsset.UnLoadScene();

			if (Cleanup)
				yield return Resources.UnloadUnusedAssets();
		}
	}
}

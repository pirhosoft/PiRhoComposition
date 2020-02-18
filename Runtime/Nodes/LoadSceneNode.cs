using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Object Manipulation/Load Scene", 100)]
	public class LoadSceneNode : GraphNode
	{
		public GraphNode Next;

		[VariableConstraint(VariableType.Asset)]
		public VariableSource Scene = new VariableSource();

		public bool WaitForCompletion = true;
		public bool Cleanup = true;
		public bool Additive = true;

		public override Color NodeColor => Colors.ExecutionLight;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			if (WaitForCompletion)
				yield return LoadScene(variables);
			else
				CompositionManager.Instance.StartCoroutine(LoadScene(variables));

			graph.GoTo(Next, nameof(Next));
		}

		private IEnumerator LoadScene(IVariableDictionary variables)
		{
			var variable = Scene.Resolve(variables, VariableType.Asset);

			yield return variable.AsAsset.LoadSceneAsync(Additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

			if (Cleanup)
				yield return Resources.UnloadUnusedAssets();
		}
	}
}

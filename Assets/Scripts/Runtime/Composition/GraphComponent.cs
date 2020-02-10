using PiRhoSoft.Utilities;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[ExecuteInEditMode]
	public class GraphComponent : MonoBehaviour
	{
		[ReadOnly]
		[Button(nameof(OpenGraph), "Open Graph Window", Location = TraitLocation.Above)]
		public Graph Graph;

		private void OnEnable()
		{
			if (!Graph)
			{
				Graph = ScriptableObject.CreateInstance<Graph>();
				Graph.name = $"{name}Graph";
			}
		}

		private void OpenGraph()
		{
#if UNITY_EDITOR
			if (Graph)
				UnityEditor.AssetDatabase.OpenAsset(Graph);
#endif
		}
	}
}

using PiRhoSoft.Variables;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	public class GraphTrigger : MonoBehaviour
	{
		public const string ThisName = "this";

		public GraphCaller Graph = new GraphCaller();

		protected IVariableDictionary _variables;

		void Awake()
		{
			var parent = GetComponentInParent<IVariableHierarchy>() as IVariableDictionary;
			_variables = new ChildDictionary(parent ?? VariableContext.Default);
			_variables.AddVariable(ThisName, Variable.Object(this));
		}

		public void Run()
		{
			if (!Graph.IsRunning)
				CompositionManager.Instance.RunGraph(Graph, _variables);
		}
	}
}

using PiRhoSoft.Variables;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PiRhoSoft.Composition
{
	[Serializable]
	public class GraphCaller
	{
		public Graph Graph;
		public List<GraphInput> Inputs = new List<GraphInput>();
		public List<GraphOutput> Outputs = new List<GraphOutput>();

		public bool IsRunning => Graph && Graph.IsRunning;

		public IEnumerator Execute(IVariableDictionary variables)
		{
			if (Graph)
			{
				var store = GraphDictionary.Reserve();
				store.WriteInputs(this, Inputs, variables);
				store.WriteOutputs(Outputs);

				yield return Graph.Execute(store);

				store.ReadOutputs(this, Outputs, variables);
				GraphDictionary.Release(store);
			}
		}
	}
}

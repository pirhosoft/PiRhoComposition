using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Yield", 23)]
	public class YieldNode : GraphNode
	{
		public GraphNode Next = null;

		public override Color NodeColor => Colors.Break;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			yield return null;
			graph.GoTo(Next, nameof(Next));
		}
	}
}

using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Exit", 23)]
	public class ExitNode : GraphNode
	{
		public override Color NodeColor => Colors.Break;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			graph.Exit();
			yield break;
		}
	}
}

using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Control Flow/Sequence", 10)]
	[OutputListNode(nameof(Sequence))]
	public class SequenceNode : GraphNode
	{
		[List]
		public GraphNodeList Sequence = new GraphNodeList();

		public override Color NodeColor => Colors.Sequence;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			for (var i = 0; i < Sequence.Count; i++)
			{
				var node = Sequence[i];

				if (node)
					yield return graph.Run(node, variables, GetConnectionName(nameof(Sequence), i));
			}
		}
	}
}
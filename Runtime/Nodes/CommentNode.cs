using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;
using MultilineAttribute = PiRhoSoft.Utilities.MultilineAttribute;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Debug/Comment", 400)]
	public class CommentNode : GraphNode
	{
		public override Color NodeColor => new Color(0.13f, 0.24f, 0.14f, 0.8f);

		[Stretch]
		[Multiline]
		public string Comment = "Double click to insert comment";

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			yield break;
		}
	}
}

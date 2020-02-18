using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Debug/Log", 400)]
	public class LogNode : GraphNode
	{
		public GraphNode Next;

		public StringExpression Message = new StringExpression();

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var text = Message.Execute(variables, VariableType.String);
			Debug.Log(text.AsString);
			graph.GoTo(Next, nameof(Next));
			yield break;
		}
	}
}

using PiRhoSoft.Expressions;
using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Object Manipulation/Create Scriptable Object", 1)]
	public class CreateScriptableObjectNode : GraphNode
	{
		public GraphNode Next;

		[TypePicker(typeof(ScriptableObject), false)]
		public string ScriptableObjectType;

		public AssignmentExpression ObjectVariable = new AssignmentExpression();

		public override Color NodeColor => Colors.SequencingLight;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var type = Type.GetType(ScriptableObjectType, false);
			var obj = CreateInstance(type);

			if (ObjectVariable.IsValid)
				ObjectVariable.Assign(variables, Variable.Object(obj));

			graph.GoTo(Next, nameof(Next));

			yield break;
		}
	}
}

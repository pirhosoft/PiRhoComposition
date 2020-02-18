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
		private const string _invalidTypeError = "Failed to create object in node '{0}': the type '{1}' could not be found";
		private const string _invalidObjectError = "Failed to create object in node '{0}': an object of type '{1}' could not be instantiated";

		public GraphNode Next;

		[TypePicker(typeof(ScriptableObject), false)]
		public string ScriptableObjectType;

		public AssignmentExpression ObjectVariable = new AssignmentExpression();

		public override Color NodeColor => Colors.SequencingLight;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			Type type;

			try
			{
				type = Type.GetType(ScriptableObjectType, false); // still throws in some cases
			}
			catch
			{
				type = null;
			}

			if (type != null)
			{
				var obj = CreateInstance(type);

				if (obj != null)
				{
					if (ObjectVariable.IsValid)
						ObjectVariable.Execute(variables); // Variable.Object(obj)
				}
				else
				{
					Debug.LogErrorFormat(this, _invalidObjectError, name, ScriptableObjectType);
				}
			}
			else
			{
				Debug.LogErrorFormat(this, _invalidTypeError, name, ScriptableObjectType);
			}

			graph.GoTo(Next, nameof(Next));

			yield break;
		}
	}
}

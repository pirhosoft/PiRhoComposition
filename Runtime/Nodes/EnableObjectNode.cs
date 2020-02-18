using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Object Manipulation/Enable Object", 20)]
	public class EnableObjectNode : GraphNode
	{
		private const string _invalidObjectWarning = "Unable to enable object for node '{0)': the object '{1}' is not a GameObject, Behaviour, or Renderer";

		public GraphNode Next;

		[VariableConstraint(typeof(Object))]
		public ReadOnlyExpression Target = new ReadOnlyExpression();

		public override Color NodeColor => Colors.SequencingLight;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			if (Target.IsValid)
			{
				var target = Target.Execute<Object>(variables);

				if (target is GameObject gameObject)
					gameObject.SetActive(true);
				else if (target is Behaviour behaviour)
					behaviour.enabled = true;
				else if (target is Renderer renderer)
					renderer.enabled = true;
				else
					Debug.LogWarningFormat(this, _invalidObjectWarning, name, Target);
			}

			graph.GoTo(Next, nameof(Next));

			yield break;
		}
	}
}

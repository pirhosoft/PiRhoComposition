using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Object Manipulation/Disable Object", 21)]
	public class DisableObjectNode : GraphNode
	{
		private const string _invalidObjectWarning = "Unable to disable object for node '{0)': the object '{1}' is not a GameObject, Behaviour, or Renderer";

		public GraphNode Next;

		[VariableConstraint(typeof(Object))]
		public ReadOnlyExpression Target = new ReadOnlyExpression();

		public override Color NodeColor => Colors.SequencingDark;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			if (Target.IsValid)
			{
				var target = Target.Execute<Object>(variables);

				if (target is GameObject gameObject)
					gameObject.SetActive(false);
				else if (target is Behaviour behaviour)
					behaviour.enabled = false;
				else if (target is Renderer renderer)
					renderer.enabled = false;
				else
					Debug.LogWarningFormat(this, _invalidObjectWarning, name, Target);
			}

			graph.GoTo(Next, nameof(Next));

			yield break;
		}
	}
}

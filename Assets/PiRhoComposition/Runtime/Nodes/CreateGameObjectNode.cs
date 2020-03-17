using PiRhoSoft.Expressions;
using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	[CreateGraphNodeMenu("Object Manipulation/Create Game Object", 0)]
	public class CreateGameObjectNode : GraphNode
	{
		public enum ObjectPositioning
		{
			Absolute,
			Relative,
			Child
		}

		public GraphNode Next;

		[VariableConstraint(typeof(GameObject))]
		public VariableSource Prefab = new VariableSource();

		[VariableConstraint(VariableType.String)]
		public VariableSource ObjectName = new VariableSource(Variable.String("SpawnedObject"));

		public AssignmentExpression ObjectVariable = new AssignmentExpression();

		[EnumButtons]
		public ObjectPositioning Positioning = ObjectPositioning.Absolute;

		[Conditional(nameof(Positioning), (int)ObjectPositioning.Relative, EnumTest.ShowIfEqual)]
		public ReadOnlyExpression Object = new ReadOnlyExpression();

		[Conditional(nameof(Positioning), (int)ObjectPositioning.Child, EnumTest.ShowIfEqual)]
		public ReadOnlyExpression Parent = new ReadOnlyExpression();

		[VariableConstraint(VariableType.Vector3)]
		public VariableSource Position = new VariableSource();

		[VariableConstraint(VariableType.Quaternion)]
		public VariableSource Rotation = new VariableSource();

		public override Color NodeColor => Colors.SequencingLight;

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var prefabVariable = Prefab.Resolve(variables);

			if (prefabVariable.TryGetObject(out GameObject prefab))
			{
				GameObject spawned = null;

				var position = Position.Resolve(variables, VariableType.Vector3);
				var rotation = Rotation.Resolve(variables, VariableType.Quaternion);

				if (Positioning == ObjectPositioning.Absolute)
				{
					spawned = Instantiate(prefab, position.AsVector3, rotation.AsQuaternion);
				}
				else if (Positioning == ObjectPositioning.Relative)
				{
					if (Object.IsValid)
					{
						var obj = Object.Execute<GameObject>(variables);
						spawned = Instantiate(prefab, obj.transform.position + position.AsVector3, rotation.AsQuaternion);
					}
				}
				else if (Positioning == ObjectPositioning.Child)
				{
					if (Parent.IsValid)
					{
						var parent = Parent.Execute<GameObject>(variables);
						spawned = Instantiate(prefab, parent.transform.position + position.AsVector3, rotation.AsQuaternion, parent.transform);
					}
				}

				if (spawned)
				{
					var objectName = ObjectName.Resolve(variables, VariableType.String).AsString;

					if (!string.IsNullOrEmpty(objectName))
						spawned.name = objectName;

					if (ObjectVariable.IsValid)
						ObjectVariable.Assign(variables, Variable.Object(spawned));
				}
			}

			graph.GoTo(Next, nameof(Next));

			yield break;
		}
	}
}

using PiRhoSoft.Utilities.Editor;
using PiRhoSoft.Variables.Editor;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Composition.Editor
{
	public class GraphCallerField : BindableElement
	{
		public const string Stylesheet = "GraphCallerStyle.uss";
		public const string UssClassName = "pirho-graph-caller";
		public const string InputsUssClassName = UssClassName + "__inputs";
		public const string OutputsUssClassName = UssClassName + "__outputs";
		public const string NoInputsUssClassName = UssClassName + "--no-inputs";
		public const string NoOutputsUssClassName = UssClassName + "--no-outputs";
		public const string NameUssClassName = UssClassName + "__name";
		public const string ValueUssClassName = UssClassName + "__value";

		private readonly SerializedProperty _rootProperty;
		private readonly SerializedProperty _graphProperty;
		private readonly SerializedProperty _inputsProperty;
		private readonly SerializedProperty _outputsProperty;

		private ListField _inputsList;
		private ListField _outputsList;

		public GraphCallerField(SerializedProperty property)
		{
			_rootProperty = property;
			_graphProperty = property.FindPropertyRelative(nameof(GraphCaller.Graph));
			_inputsProperty = property.FindPropertyRelative(nameof(GraphCaller.Inputs));
			_outputsProperty = property.FindPropertyRelative(nameof(GraphCaller.Outputs));

			var graphField = new ObjectPickerField(typeof(Graph)).ConfigureProperty(_graphProperty);
			graphField.SetFieldLabel(property.displayName);
			graphField.RegisterCallback<ChangeEvent<Object>>(evt => UpdateVariables(evt.newValue as Graph));

			_inputsList = new ListField
			{
				bindingPath = _inputsProperty.propertyPath,
				AllowAdd = false,
				AllowRemove = false,
				AllowReorder = false,
				Tooltip = "The input values to set for the Graph"
			};

			_outputsList = new ListField
			{
				bindingPath = _outputsProperty.propertyPath,
				AllowAdd = false,
				AllowRemove = false,
				AllowReorder = false,
				Tooltip = "The output values to resolve from this Graph"
			};

			Add(graphField);
			Add(_inputsList);
			Add(_outputsList);

			_outputsList.AddToClassList(OutputsUssClassName);
			_inputsList.AddToClassList(InputsUssClassName);

			AddToClassList(UssClassName);
			this.AddStyleSheet(Stylesheet);

			UpdateVariables(_graphProperty.objectReferenceValue as Graph);
		}

		private void UpdateVariables(Graph graph)
		{
			EnableInClassList(NoInputsUssClassName, graph == null || graph.Inputs.Count == 0);
			EnableInClassList(NoOutputsUssClassName, graph == null || graph.Outputs.Count == 0);

			var inputsProxy = new GraphInputsProxy(_inputsProperty, graph);
			var outputsProxy = new GraphOutputsProxy(_outputsProperty, graph);

			_inputsList.SetProxy(inputsProxy, null, false);
			_outputsList.SetProxy(outputsProxy, null, false);

			//_rootProperty.serializedObject.ApplyModifiedProperties();
			//this.Bind(_rootProperty.serializedObject);
		}

		private class GraphInputsProxy : IListProxy
		{
			public SerializedProperty Property;
			public Graph Graph;

			public int Count => Graph != null ? Graph.Inputs.Count : 0;

			public bool CanAdd() => false;
			public bool CanAdd(Type type) => false;
			public bool CanRemove(int index) => false;
			public bool AddItem(Type type) { return false; }
			public void RemoveItem(int index) { }
			public void ReorderItem(int from, int to) { }

			public GraphInputsProxy(SerializedProperty property, Graph graph)
			{
				Property = property;
				Graph = graph;
			}

			public VisualElement CreateElement(int index)
			{
				if (Property.arraySize <= index)
					Property.ResizeArray(index + 1);

				var inputProperty = Property.GetArrayElementAtIndex(index);
				var nameProperty = inputProperty.FindPropertyRelative(nameof(GraphInput.Name));
				var typeProperty = inputProperty.FindPropertyRelative(nameof(GraphInput.Type));
				var referenceProperty = inputProperty.FindPropertyRelative(nameof(GraphInput.Reference));
				var valueProperty = inputProperty.FindPropertyRelative(nameof(GraphInput.Value));

				var definition = Graph.Inputs[index];

				nameProperty.stringValue = definition.Name;
				var label = new Label { bindingPath = nameProperty.propertyPath };
				label.AddToClassList(NameUssClassName);

				var referenceField = new PropertyField(referenceProperty);
				referenceField.SetFieldLabel(null);

				var valueField = new SerializedVariableField(valueProperty, definition);
				valueField.AddToClassList(ValueUssClassName);
				
				if (!definition.IsValid(valueField.Control.Value))
				{
					var value = definition.Generate();
					valueField.Inject(value);
				}

				var typeValue = (GraphInputType)typeProperty.intValue;
				var typeField = new EnumButtonsField { Type = typeof(GraphInputType), value = typeValue }.ConfigureProperty(typeProperty);
				typeField.SetFieldLabel(null);
				typeField.RegisterCallback<ChangeEvent<Enum>>(evt =>
				{
					var type = (GraphInputType)evt.newValue;

					referenceField.SetDisplayed(type == GraphInputType.Reference);
					valueField.SetDisplayed(type == GraphInputType.Value);
				});

				
				referenceField.SetDisplayed(typeValue == GraphInputType.Reference);
				valueField.SetDisplayed(typeValue == GraphInputType.Value);

				var container = new VisualElement();
				container.Add(label);
				container.Add(typeField);
				container.Add(referenceField);
				container.Add(valueField);

				return container;
			}
		}

		private class GraphOutputsProxy : IListProxy
		{
			public SerializedProperty Property;
			public Graph Graph;

			public int Count => Graph != null ? Graph.Outputs.Count : 0;

			public bool CanAdd() => false;
			public bool CanAdd(Type type) => false;
			public bool CanRemove(int index) => false;
			public bool AddItem(Type type) { return false; }
			public void RemoveItem(int index) { }
			public void ReorderItem(int from, int to) { }

			public GraphOutputsProxy(SerializedProperty property, Graph graph)
			{
				Property = property;
				Graph = graph;
			}

			public VisualElement CreateElement(int index)
			{
				if (Property.arraySize <= index)
					Property.ResizeArray(index + 1);

				var outputProperty = Property.GetArrayElementAtIndex(index);
				var nameProperty = outputProperty.FindPropertyRelative(nameof(GraphOutput.Name));
				var typeProperty = outputProperty.FindPropertyRelative(nameof(GraphOutput.Type));
				var referenceProperty = outputProperty.FindPropertyRelative(nameof(GraphOutput.Reference));

				var definition = Graph.Outputs[index];

				nameProperty.stringValue = definition.Name;
				var label = new Label { bindingPath = nameProperty.propertyPath };
				label.AddToClassList(NameUssClassName);

				var referenceField = new PropertyField(referenceProperty);
				referenceField.SetFieldLabel(null);

				var typeValue = (GraphOutputType)typeProperty.intValue;
				var typeField = new EnumButtonsField { Type = typeof(GraphOutputType), value = typeValue }.ConfigureProperty(typeProperty);
				typeField.SetFieldLabel(null);
				typeField.RegisterCallback<ChangeEvent<Enum>>(evt =>
				{
					referenceField.SetDisplayed((GraphOutputType)evt.newValue == GraphOutputType.Reference);
				});

				referenceField.SetDisplayed(typeValue == GraphOutputType.Reference);

				var container = new VisualElement();
				container.Add(label);
				container.Add(typeField);
				container.Add(referenceField);

				return container;
			}
		}
	}
}

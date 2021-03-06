﻿using PiRhoSoft.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Composition.Editor
{
	public interface IInputNode
	{
		GraphNode.NodeData Data { get; }
		GraphViewInputPort Input { get; }

		void UpdateColors(bool active);
	}

	public interface IOutputNode
	{
		GraphNode.NodeData Data { get; }
		List<GraphViewOutputPort> Outputs { get; }

		void UpdateColors(bool active);
	}

	public class GraphViewNode : Node
	{
		public const string UssClassName = GraphViewEditor.UssClassName + "__node";
		public const string NodeDeleteButtonUssClassName = UssClassName + "__delete-button";
		public const string NodeEditableLabelUssClassName = UssClassName + "__editable-label";

		private static readonly CustomStyleProperty<Color> _nodeColorProperty = new CustomStyleProperty<Color>("--node-color");

		public GraphNode.NodeData Data { get; private set; }

		public override bool IsAscendable() => true;
		public override bool IsDroppable() => false;
		public override bool IsMovable() => true;
		public override bool IsResizable() => false;
		public override bool IsSelectable() => true;

		protected SerializedObject _serializedObject;

		protected override void ToggleCollapse() { }

		public GraphViewNode(GraphNode node)
		{
			AddToClassList(UssClassName);

			Data = new GraphNode.NodeData(node);

			_serializedObject = new SerializedObject(node);

			titleContainer.style.backgroundColor = node.NodeColor;
			titleContainer.style.unityFontStyleAndWeight = FontStyle.Bold;

			m_CollapseButton.SetDisplayed(false);

			SetPosition(new Rect(node.GraphPosition, Vector2.zero));
		}

		public virtual void BindNode(SerializedObject serializedObject)
		{
			this.Bind(serializedObject);
		}

		protected void CreateDeleteButton()
		{
			var deleteButton = new IconButton(DeleteNode) { image = Icon.Close.Texture, tooltip = "Delete this node" };
			deleteButton.AddToClassList(NodeDeleteButtonUssClassName);
			titleButtonContainer.Add(deleteButton);
		}

		protected void DeleteNode()
		{
			var graph = GetFirstAncestorOfType<GraphView>();
			graph.RemoveNode(this, Enumerable.Empty<GraphViewNode>());
		}

		protected void ViewDocumentation(Type type)
		{
			var help = type.GetAttribute<HelpURLAttribute>();
			if (help != null)
				Application.OpenURL(help.URL);
		}

		protected override void OnCustomStyleResolved(ICustomStyle style)
		{
			base.OnCustomStyleResolved(style);

			titleContainer.style.backgroundColor = style.TryGetValue(_nodeColorProperty, out var nodeColor) ? nodeColor : Data.Node.NodeColor;
		}

		#region Editable Label
		
		public static TextField CreateEditableLabel(TextElement container, SerializedProperty property, bool multiline = false)
		{
			var edit = new TextField { multiline = multiline };
			edit.BindProperty(property);
			edit.AddToClassList(NodeEditableLabelUssClassName);
			edit.Q(TextField.textInputUssName).RegisterCallback<FocusOutEvent>(evt => HideEditableText(edit));

			container.BindProperty(property);
			container.RegisterCallback<MouseDownEvent>(evt => OnEditEvent(evt, edit));
			container.RegisterValueChangedCallback(e => container.text = e.newValue);
			container.Add(edit);

			HideEditableText(edit);

			return edit;
		}

		public static void ShowEditableText(TextField edit)
		{
			edit.style.visibility = Visibility.Visible;
			edit.Focus();
		}

		public static void HideEditableText(TextField edit)
		{
			if (edit != null) // this needs to be here for when it is stupidly called after node is destroyed
				edit.style.visibility = Visibility.Hidden;
		}

		public static void OnEditEvent(MouseDownEvent evt, TextField edit)
		{
			if (evt.clickCount == 2 && evt.button == (int)MouseButton.LeftMouse)
			{
				ShowEditableText(edit);
				evt.PreventDefault();
			}
		}

		#endregion
	}
}

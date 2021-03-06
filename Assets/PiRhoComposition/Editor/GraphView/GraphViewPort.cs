﻿using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Composition.Editor
{
	public class GraphViewPort : Port
	{
		public const string UssClassName = GraphViewNode.UssClassName + "__port";
		public const string InCallstackUssClassName = UssClassName + "--in-callstack";

		public GraphViewNode Node { get; private set; }

		public GraphViewPort(GraphViewNode node, GraphViewConnector edgeListener, bool isInput) : base(Orientation.Horizontal, isInput ? Direction.Input : Direction.Output, isInput ? Capacity.Multi : Capacity.Single, null)
		{
			AddToClassList(UssClassName);
			Node = node;
			m_EdgeConnector = new EdgeConnector<Edge>(edgeListener);
			this.AddManipulator(m_EdgeConnector);
		}
	}

	public class GraphViewInputPort : GraphViewPort
	{
		public const string UssInputClassName = UssClassName + "--input";

		public GraphViewInputPort(GraphViewNode node, GraphViewConnector edgeListener) : base(node, edgeListener, true)
		{
			AddToClassList(UssInputClassName);

			tooltip = "Drag an output to this port to create a connection";

			m_ConnectorText.style.marginLeft = 0;
			m_ConnectorText.style.marginRight = 0;
		}

		public void UpdateColor(bool inCallstack)
		{
			EnableInClassList(InCallstackUssClassName, inCallstack);
		}
	}

	public class GraphViewOutputPort : GraphViewPort
	{
		public const string UssOutputClassName = UssClassName + "--output";
		public const string EditableLabelUssClassName = UssClassName + "__editable-label";

		public GraphNode.ConnectionData Connection { get; private set; }

		public GraphViewOutputPort(GraphViewNode node, GraphNode.ConnectionData connection, GraphViewConnector edgeListener, SerializedProperty nameProperty) : base(node, edgeListener, false)
		{
			AddToClassList(UssOutputClassName);

			Connection = connection;

			tooltip = "Click and drag to make a connection from this output";

			m_ConnectorText.style.flexGrow = 1;
			m_ConnectorText.style.unityTextAlign = TextAnchor.MiddleLeft;

			if (nameProperty != null)
				GraphViewNode.CreateEditableLabel(m_ConnectorText, nameProperty);
		}

		public override void OnStartEdgeDragging()
		{
			base.OnStartEdgeDragging();

			var output = m_EdgeConnector.edgeDragHelper.edgeCandidate?.output;
			if (output == this)
			{
				var graph = GetFirstAncestorOfType<GraphView>();
				graph.DeleteElements(connections);
			}
		}

		public void UpdateColor()
		{
			var inCallstack = Connection.From != null && Connection.From.Graph.IsInCallStack(Connection);
			EnableInClassList(InCallstackUssClassName, inCallstack);
		}
	}
}

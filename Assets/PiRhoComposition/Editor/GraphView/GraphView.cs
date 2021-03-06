﻿using PiRhoSoft.Utilities;
using PiRhoSoft.Utilities.Editor;
using PiRhoSoft.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Composition.Editor
{
	public class GraphViewNodeProvider : PickerProvider<Type> { }
	public class GraphProvider : PickerProvider<Graph> { }
	public class ObjectProvider : PickerProvider<Object> { }

	public class GraphView : UnityEditor.Experimental.GraphView.GraphView
	{
		#region Members

		private const string UssClassName = GraphViewEditor.UssClassName + "__graph";

		private static readonly List<GraphNode> _copiedNodes = new List<GraphNode>();

		public Graph Graph { get; private set; }

		public bool CanCut => canCutSelection;
		public bool CanCopy => canCopySelection;
		public bool CanPaste => canPaste;
		public bool CanDuplicate => canDuplicateSelection;
		public bool CanDelete => canDeleteSelection;

		private readonly GraphViewWindow _window;
		private readonly GraphViewNodeProvider _nodeProvider;
		private readonly GraphViewConnector _nodeConnector;

		private readonly UQueryState<GraphViewInputPort> _inputs;
		private readonly UQueryState<GraphViewOutputPort> _outputs;

		private StartNode _start = null;
		private Vector2 _createPosition;

		#endregion

		#region Initialization

		public GraphView(GraphViewWindow window, Graph graph)
		{
			Graph = graph;

			_window = window;
			_nodeProvider = ScriptableObject.CreateInstance<GraphViewNodeProvider>();
			_nodeConnector = new GraphViewConnector(_nodeProvider);

			_inputs = this.Query<GraphViewInputPort>().Build();
			_outputs = this.Query<GraphViewOutputPort>().Build();

			AddToClassList(UssClassName);
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			SetupNodeProvider();
			SetupNodes();
			SetupConnections();
			RegisterCallback<KeyDownEvent>(OnKeyDown);
			RegisterCallback<GeometryChangedEvent>(evt => ShowAll()); // Use this event because the layout isn't build right away so ShowAll won't work immediately

			nodeCreationRequest = OnShowCreateNode;
			graphViewChanged = OnGraphChanged;
			canPasteSerializedData = data => canPaste;
			serializeGraphElements = OnCopy;
			unserializeAndPaste = OnPaste;

			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new ClickSelector());

			Undo.undoRedoPerformed += Rebuild;
			EditorApplication.playModeStateChanged += PlayStateChanged;

			if (Application.isPlaying) // Make sure the callback still gets set if the window was opened during play mode
			{
				PlayStateChanged(PlayModeStateChange.EnteredPlayMode);
				FrameChanged(null);
			}
		}

		private void SetupNodeProvider()
		{
			var types = TypeHelper.GetDerivedTypes<GraphNode>(false)
				.Where(type => type != typeof(StartNode))
				.OrderBy(type => type.GetAttribute<CreateGraphNodeMenuAttribute>()?.MenuName ?? type.Name);

			var paths = types.Select(type => type.GetAttribute<CreateGraphNodeMenuAttribute>()?.MenuName ?? type.Name);

			_nodeProvider.Setup("Create Node", paths.ToList(), types.ToList(), type => AssetPreview.GetMiniTypeThumbnail(type), selectedType => CreateNode(selectedType));
		}

		#endregion

		#region Overrides

		protected override bool canCutSelection => selection.OfType<GraphViewNode>().Where(node => !(node is StartGraphViewNode)).Any();
		protected override bool canCopySelection => canCutSelection;
		protected override bool canPaste => _copiedNodes.Count > 0;
		protected override bool canDuplicateSelection => canCopySelection;
		protected override bool canDeleteSelection => canCutSelection;

		public override List<Port> GetCompatiblePorts(Port start, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();

			if (start is GraphViewPort startNode)
			{
				var type = start.GetType();

				ports.ForEach(port =>
				{
					if (port is GraphViewPort portNode && startNode.Node.Data != portNode.Node.Data && type != port.GetType())
						compatiblePorts.Add(port);
				});
			}

			return compatiblePorts;
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
			_createPosition = evt.mousePosition + _window.position.position;
		}

		private Vector2 MouseToGraphPosition(Vector2 position)
		{
			var windowPosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, position - _window.position.position);
			return contentViewContainer.WorldToLocal(windowPosition);
		}

		#endregion

		#region Node Management

		private class StartNode : GraphNode
		{
			public override Color NodeColor => Colors.Start;
			public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables) { yield break; }

			public override void GetConnections(NodeData data) => data.AddConnections(Graph);
			public override void SetConnection(ConnectionData connection, GraphNode target) => connection.ApplyConnection(Graph, target);
		}

		private void SetupNodes()
		{
			Graph.SyncNodes();

			if (_start == null)
			{
				_start = ScriptableObject.CreateInstance<StartNode>();
				_start.name = "Start";
				_start.Graph = Graph;
				_start.GraphPosition = Vector2.zero;
			}

			AddNode(_start);

			foreach (var node in Graph.Nodes)
				AddNode(node);
		}

		private void SetupConnections()
		{
			_outputs.ForEach(output => SetupConnection(output));
		}

		private void SetupConnection(GraphViewOutputPort output)
		{
			_inputs.ForEach(input =>
			{
				if (output.Connection.To == input.Node.Data.Node)
				{
					output.Connection.SetTarget(input.Node.Data);

					var edge = new Edge
					{
						output = output,
						input = input
					};

					AddEdge(edge);
				}
			});
		}

		private void CreateNode(Type type)
		{
			var position = MouseToGraphPosition(_createPosition);
			var node = CreateNode(type, type.Name, position);

			AddNode(node);
		}

		private GraphViewNode CreateNodeView(GraphNode node)
		{
			if (node.GetType().TryGetAttribute<OutputCollectionNodeAttribute>(out var outputCollection))
			{
				return new OutputCollectionGraphViewNode(node, _nodeConnector, outputCollection);
			}
			else
			{
				switch (node)
				{
					case StartNode start: return new StartGraphViewNode(start, _nodeConnector);
					case CommentNode comment: return new CommentGraphViewNode(comment);
					default: return new DefaultGraphViewNode(node, _nodeConnector);
				}
			}
		}

		private GraphViewNode AddNode(GraphNode node)
		{
			var nodeElement = CreateNodeView(node);
			nodeElement.BindNode(new SerializedObject(node));
			AddElement(nodeElement);
			return nodeElement;
		}

		public void RemoveNode(GraphViewNode node, IEnumerable<GraphViewNode> removedNodes)
		{
			if (node is IInputNode inputNode)
			{
				foreach (var edge in inputNode.Input.connections.ToList()) // must use ToList() because internal enumerable is modified
					RemoveEdge(edge, edge.output is GraphViewOutputPort output && !removedNodes.Contains(output.Node));
			}

			if (node is IOutputNode outputNode)
			{
				foreach (var output in outputNode.Outputs)
				{
					foreach (var edge in output.connections.ToList()) // must use ToList() because internal enumerable is modified
						RemoveEdge(edge, false);
				}
			}
	
			DestroyNode(node.Data.Node);

			RemoveElement(node);
		}

		private void AddEdge(Edge edge)
		{
			if (edge.output is GraphViewOutputPort output && edge.input is GraphViewInputPort input)
			{
				SetConnectionTarget(output.Connection, input.Node.Data);

				edge.output.Connect(edge);
				edge.input.Connect(edge);
				edge.capabilities &= ~Capabilities.Selectable;

				AddElement(edge);
			}
		}

		public void RemoveEdge(Edge edge, bool removeConnection)
		{
			if (edge.output is GraphViewOutputPort output && edge.input is GraphViewInputPort input)
			{
				if (removeConnection) // We only want to remove connections that are external of all edges being removed
					SetConnectionTarget(output.Connection, null);

				edge.output.Disconnect(edge);
				edge.input.Disconnect(edge);

				RemoveElement(edge);
			}
		}

		#endregion

		#region Callbacks

		private void Rebuild()
		{
			var elements = graphElements.ToList(); // Can't use DeleteElements because it uses the graphChanged callback to actually delete stuff
			foreach (var element in elements)
				RemoveElement(element);

			SetupNodes();
			SetupConnections();
			MarkDirtyRepaint();
		}

		private void PlayStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredPlayMode)
				Graph.OnProcessFrame += FrameChanged;
			else if (state == PlayModeStateChange.ExitingPlayMode)
				Graph.OnProcessFrame -= FrameChanged;
			else if (state == PlayModeStateChange.EnteredEditMode)
			{
				Graph.Reset();
				FrameChanged(null);
			}
		}

		private void FrameChanged(GraphNode graphNode)
		{
			nodes.ForEach(node =>
			{
				if (node is IOutputNode outputNode)
					outputNode.UpdateColors(outputNode.Data.Node == graphNode);
				else if (node is IInputNode inputNode)
					inputNode.UpdateColors(inputNode.Data.Node == graphNode);
				else if (node is StartGraphViewNode startNode)
					startNode.Output.UpdateColor();
			});
		}

		private void OnShowCreateNode(NodeCreationContext context)
		{
			_createPosition = context.screenMousePosition;
			SearchWindow.Open(new SearchWindowContext(_createPosition), _nodeProvider);
		}

		private string OnCopy(IEnumerable<GraphElement> elements)
		{
			Copy();
			return string.Empty;
		}

		private void OnPaste(string operationName, string data)
		{
			Paste(_createPosition);
		}

		private GraphViewChange OnGraphChanged(GraphViewChange graphViewChange)
		{
			if (graphViewChange.elementsToRemove != null)
			{
				var nodes = graphViewChange.elementsToRemove.OfType<GraphViewNode>();
				var edges = graphViewChange.elementsToRemove.OfType<Edge>();

				foreach (var node in nodes)
					RemoveNode(node, nodes);

				foreach (var edge in edges)
					RemoveEdge(edge, true);

				graphViewChange.elementsToRemove.Clear();
			}

			if (graphViewChange.edgesToCreate != null)
			{
				foreach (var edge in graphViewChange.edgesToCreate)
					AddEdge(edge);

				graphViewChange.edgesToCreate.Clear();
			}

			if (graphViewChange.movedElements != null)
			{
				foreach (var element in graphViewChange.movedElements)
				{
					if (element is GraphViewNode node && node.IsMovable())
						SetNodePosition(node.Data.Node, node.Data.Node.GraphPosition + graphViewChange.moveDelta);
				}

				graphViewChange.movedElements.Clear();
			}

			return graphViewChange;
		}

		private void OnKeyDown(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.Home)
				GoToStart();
			else if (evt.keyCode == KeyCode.Tab)
				GoToSelection();
			else if (evt.keyCode == KeyCode.End)
				ShowAll();
		}

		#endregion

		#region View Menu

		public void ShowAll()
		{
			FrameAll();
		}

		public void GoToStart()
		{
			FrameOrigin();
		}

		public void GoToSelection()
		{
			FrameSelection();
		}

		public void GoToNode(GraphNode node)
		{
			var target = nodes.ToList().OfType<GraphViewNode>().Where(graphNode => graphNode.Data.Node == node).FirstOrDefault();
			if (target != null)
			{
				ClearSelection();
				AddToSelection(target);
				FrameSelection();
			}
		}

		#endregion

		#region Edit Menu

		public void Cut()
		{
			Copy();
			Delete();
		}

		public void Copy()
		{
			_copiedNodes.Clear();

			var sourceNodes = selection.OfType<GraphViewNode>().Where(node => !(node is StartGraphViewNode)).Select(node => node.Data.Node).ToList();
			var copiedData = new List<GraphNode.NodeData>();

			foreach (var node in sourceNodes)
			{
				var copy = CloneNode(node);
				var data = new GraphNode.NodeData(copy);
				copiedData.Add(data);
				_copiedNodes.Add(copy);
			}

			foreach (var data in copiedData)
			{
				foreach (var connection in data.Connections)
				{
					var index = connection.To != null ? sourceNodes.IndexOf(connection.To) : -1;
					connection.SetTarget(index >= 0 ? copiedData[index] : null);
				}
			}
		}

		public void Paste(Vector2 position)
		{
			ClearSelection();
			AddClonedNodes(_copiedNodes, MouseToGraphPosition(position));

			foreach (var copy in _copiedNodes)
			{
				var node = AddNode(copy);
				AddToSelection(node);
			}

			foreach (var node in selection.OfType<IOutputNode>())
			{
				foreach (var output in node.Outputs)
					SetupConnection(output);
			}

			Copy(); // re-copy so the same set of nodes can be pasted twice
		}

		public void Duplicate(Vector2 position)
		{
			Copy();
			Paste(position);
		}

		public void Delete()
		{
			DeleteSelectionCallback(AskUser.DontAskUser);
		}

		#endregion

		#region Graph Modification

		private GraphNode CreateNode(Type type, string name, Vector2 position)
		{
			using (new ChangeScope(Graph))
			{
				var node = ScriptableObject.CreateInstance(type) as GraphNode;
				node.hideFlags = HideFlags.HideInHierarchy;
				node.name = name;
				node.Graph = Graph;
				node.GraphPosition = position;

				Graph.Nodes.Add(node);

				Undo.RegisterCreatedObjectUndo(node, $"Create Node: {name}");
				AssetDatabase.AddObjectToAsset(node, Graph);
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Graph));
				return node;
			}
		}

		private GraphNode CloneNode(GraphNode node)
		{
			var clone = Object.Instantiate(node);
			clone.name = node.name;
			clone.hideFlags = HideFlags.HideInHierarchy;
			return clone;
		}

		private void AddClonedNodes(List<GraphNode> nodes, Vector2 position)
		{
			var minimum = nodes.First().GraphPosition;

			foreach (var node in nodes)
				minimum = Vector2.Min(minimum, node.GraphPosition);

			var offset = position - minimum;

			using (new ChangeScope(Graph))
			{
				foreach (var node in nodes)
				{
					node.GraphPosition += offset;
					node.Graph = Graph;
					Graph.Nodes.Add(node);
					Undo.RegisterCreatedObjectUndo(node, "Paste Node");
					AssetDatabase.AddObjectToAsset(node, Graph);
				}

				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Graph));
			}
		}

		private void DestroyNode(GraphNode node)
		{
			using (new ChangeScope(Graph))
			{
				Graph.Nodes.Remove(node);
				Undo.DestroyObjectImmediate(node);
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Graph));
			}
		}

		private void SetNodePosition(GraphNode node, Vector2 position)
		{
			using (new ChangeScope(Graph))
				node.GraphPosition = position;
		}

		private void SetConnectionTarget(GraphNode.ConnectionData connection, GraphNode.NodeData target)
		{
			using (new ChangeScope(Graph))
				connection.SetTarget(target);
		}

		#endregion
	}

	public class GraphViewEditor : VisualElement
	{
		#region Members

		public const string Stylesheet = "GraphViewStyle.uss";
		public const string UssClassName = "pirho-graph-view";
		public const string ToolbarUssClassName = UssClassName + "__toolbar";
		public const string ToolbarMenuUssClassName = ToolbarUssClassName + "__menu";
		public const string ToolbarButtonUssClassName = ToolbarUssClassName + "__button";
		public const string ToolbarButtonLargeUssClassName = ToolbarButtonUssClassName + "--large";
		public const string ToolbarButtonSmallUssClassName = ToolbarButtonUssClassName + "--small";
		public const string ToolbarButtonLockUssClassName = ToolbarButtonUssClassName + "--lock";
		public const string ToolbarButtonActiveUssClassName = ToolbarButtonUssClassName + "--active";
		public const string ToolbarButtonFirstUssClassName = ToolbarButtonUssClassName + "--first";
		public const string ToolbarButtonGraphUssClassName = ToolbarButtonUssClassName + "--graph";
		public const string ToolbarButtonBreakpointUssClassName = ToolbarButtonUssClassName + "__breakpoint";
		public const string BreakpointsDisabledUssClassName = UssClassName + "--breakpoints-disabled";

		public static WeakEvent AutocompleteChanged = new WeakEvent();

		private static readonly Icon _playIcon = Icon.BuiltIn("Animation.Play");
		private static readonly Icon _pauseIcon = Icon.BuiltIn("PauseButton");
		private static readonly Icon _stopIcon = Icon.BuiltIn("PreMatQuad");
		private static readonly Icon _stepIcon = Icon.BuiltIn("Animation.NextKey");
		private static readonly Icon _logIcon = Icon.BuiltIn("UnityEditor.ConsoleWindow");

		private static readonly BoolPreference _breakpointsEnabled = new BoolPreference("PiRhoSoft.Composition.Graph.BreakpointsEnabled", true);
		private static readonly BoolPreference _loggingEnabled = new BoolPreference("PiRhoSoft.Composition.Graph.LoggingEnabled", false);

		private readonly GraphViewWindow _window;
		private readonly GraphProvider _graphProvider;
		private readonly ObjectProvider _objectProvider;

		private ToolbarMenu _editButton;
		private ToolbarMenu _viewButton;

		private TextElement _graphButton;
		private VisualElement _breakButton;
		private Image _loggingButton;
		private Image _lockButton;
		private Image _playButton;
		private Image _pauseButton;
		private Image _stepButton;
		private Image _stopButton;

		public Graph CurrentGraph => GraphView?.Graph;
		public GraphView GraphView { get; private set; }
		public bool IsLocked { get; private set; }

		#endregion

		public GraphViewEditor(GraphViewWindow window)
		{
			_window = window;
			_graphProvider = ScriptableObject.CreateInstance<GraphProvider>();
			_objectProvider = ScriptableObject.CreateInstance<ObjectProvider>();

			this.AddStyleSheet(Stylesheet);
			AddToClassList(UssClassName);

			CreateToolbar();

			// this has to be here because Unity doesn't allow EditorPrefs access in a static constructor
			Graph.IsDebugBreakEnabled = _breakpointsEnabled.Value;
			Graph.IsDebugLoggingEnabled = _loggingEnabled.Value;
		}

		public void SetGraph(Graph graph)
		{
			if (GraphView == null || graph != GraphView.Graph)
			{
				if (GraphView != null)
					GraphView.RemoveFromHierarchy();

				var serializedObject = new SerializedObject(graph);
				this.Bind(serializedObject);

				GraphView = new GraphView(_window, graph);

				Add(GraphView);
				RefreshToolbar();

				AutocompleteChanged.Trigger();
			}
		}

		private void CreateToolbar()
		{
			_graphButton = new TextElement { tooltip = "Select a graph to edit" };
			_graphButton.AddToClassList(ToolbarButtonUssClassName);
			_graphButton.AddToClassList(ToolbarButtonLargeUssClassName);
			_graphButton.AddToClassList(ToolbarButtonFirstUssClassName);
			_graphButton.AddToClassList(ToolbarButtonGraphUssClassName);
			_graphButton.AddManipulator(new Clickable(() => ShowGraphPicker(GUIUtility.GUIToScreenPoint(_graphButton.worldBound.position))));

			_editButton = CreateEditMenu();
			_editButton.AddToClassList(ToolbarMenuUssClassName);

			_viewButton = CreateViewMenu();
			_viewButton.AddToClassList(ToolbarMenuUssClassName);

			_playButton = new Image { image = _playIcon.Texture, tooltip = "Resume execution of the graph" };
			_playButton.AddToClassList(ToolbarButtonUssClassName);
			_playButton.AddToClassList(ToolbarButtonSmallUssClassName);
			_playButton.AddManipulator(new Clickable(() => CurrentGraph.DebugPlay()));

			_pauseButton = new Image { image = _pauseIcon.Texture, tooltip = "Pause the execution of the graph" };
			_pauseButton.AddToClassList(ToolbarButtonUssClassName);
			_pauseButton.AddToClassList(ToolbarButtonSmallUssClassName);
			_pauseButton.AddManipulator(new Clickable(() => CurrentGraph.DebugPause()));

			_stepButton = new Image { image = _stepIcon.Texture, tooltip = "Step forward one node in the graph" };
			_stepButton.AddToClassList(ToolbarButtonUssClassName);
			_stepButton.AddToClassList(ToolbarButtonSmallUssClassName);
			_stepButton.AddManipulator(new Clickable(() => CurrentGraph.DebugStep()));

			_stopButton = new Image { image = _stopIcon.Texture, tooltip = "Stop executing the graph", tintColor = Color.gray };
			_stopButton.AddToClassList(ToolbarButtonUssClassName);
			_stopButton.AddToClassList(ToolbarButtonSmallUssClassName);
			_stopButton.AddManipulator(new Clickable(() => CurrentGraph.DebugStop()));

			_breakButton = new VisualElement { tooltip = "Enable/Disable node breakpoints for all graphs" };
			_breakButton.AddToClassList(ToolbarButtonUssClassName);
			_breakButton.AddToClassList(ToolbarButtonSmallUssClassName);
			_breakButton.AddToClassList(ToolbarButtonFirstUssClassName);
			_breakButton.AddManipulator(new Clickable(ToggleBreakpointsEnabled));

			var breakButton = new VisualElement();
			breakButton.AddToClassList(ToolbarButtonBreakpointUssClassName);

			_breakButton.Add(breakButton);

			_loggingButton = new Image { image = _logIcon.Texture, tooltip = "Enable/Disable logging of graph execution for all graphs" };
			_loggingButton.AddToClassList(ToolbarButtonUssClassName);
			_loggingButton.AddToClassList(ToolbarButtonSmallUssClassName);
			_loggingButton.AddManipulator(new Clickable(ToggleLoggingEnabled));

			_lockButton = new Image { tintColor = Color.black, tooltip = "Lock/Unlock this window so it won't be used when other graphs are opened" };
			_lockButton.AddToClassList(ToolbarButtonUssClassName);
			_lockButton.AddToClassList(ToolbarButtonSmallUssClassName);
			_lockButton.AddToClassList(ToolbarButtonLockUssClassName);
			_lockButton.AddManipulator(new Clickable(ToggleLockingEnabled));

			var watchButton = new Image { image = Icon.View.Texture, tooltip = "Open the Watch Window" };
			watchButton.AddToClassList(ToolbarButtonUssClassName);
			watchButton.AddToClassList(ToolbarButtonSmallUssClassName);
			watchButton.AddManipulator(new Clickable(WatchWindow.ShowWindow));

			RefreshToolbar();

			var toolbar = new Toolbar();
			toolbar.AddToClassList(ToolbarUssClassName);
			toolbar.Add(_graphButton);
			toolbar.Add(_editButton);
			toolbar.Add(_viewButton);
			toolbar.Add(_playButton);
			toolbar.Add(_pauseButton);
			toolbar.Add(_stepButton);
			toolbar.Add(_stopButton);
			toolbar.Add(new ToolbarSpacer { flex = true });
			toolbar.Add(_breakButton);
			toolbar.Add(_loggingButton);
			toolbar.Add(watchButton);
			toolbar.Add(_lockButton);

			Add(toolbar);
		}

		private ToolbarMenu CreateEditMenu()
		{
			var menu = new ToolbarMenu { text = "Edit" };

			menu.menu.AppendAction("Create Node _SPACE", evt => GraphView.nodeCreationRequest(new NodeCreationContext { screenMousePosition = _window.position.position }));
			menu.menu.AppendSeparator();
			menu.menu.AppendAction("Cut %x", evt => GraphView.Cut(), evt => GraphView.CanCut ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			menu.menu.AppendAction("Copy %c", evt => GraphView.Copy(), evt => GraphView.CanCopy ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			menu.menu.AppendAction("Paste %v", evt => GraphView.Paste(_window.position.center), evt => GraphView.CanPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			menu.menu.AppendAction("Duplicate %d", evt => GraphView.Duplicate(_window.position.center), evt => GraphView.CanDuplicate ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			menu.menu.AppendSeparator();
			menu.menu.AppendAction("Delete _DELETE", evt => GraphView.Delete(), evt => GraphView.CanDelete ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

			return menu;
		}

		private ToolbarMenu CreateViewMenu()
		{
			var menu = new ToolbarMenu { text = "View" };

			menu.menu.AppendAction("Show All _END", evt => GraphView.ShowAll());
			menu.menu.AppendAction("Go To Start _HOME", evt => GraphView.GoToStart());
			menu.menu.AppendAction("Zoom To Selection _TAB", evt => GraphView.GoToSelection());
			menu.style.marginLeft = -1;

			return menu;
		}

		private void RefreshToolbar()
		{
			var isEnabled = Application.isPlaying && CurrentGraph != null && CurrentGraph.IsRunning;
			var isPlaying = isEnabled && CurrentGraph.DebugState == Graph.PlaybackState.Running;
			var isPaused = isEnabled && CurrentGraph.DebugState == Graph.PlaybackState.Paused;
			var isStepping = isEnabled && CurrentGraph.DebugState == Graph.PlaybackState.Step;
			var isStopping = isEnabled && CurrentGraph.DebugState == Graph.PlaybackState.Stopped;

			_graphButton.text = CurrentGraph == null ? "No Graph Selected" : CurrentGraph.name;
			_lockButton.image = IsLocked ? Icon.Locked.Texture : Icon.Unlocked.Texture;

			_editButton.SetEnabled(CurrentGraph != null);
			_viewButton.SetEnabled(CurrentGraph != null);

			_playButton.SetEnabled(isEnabled);
			_pauseButton.SetEnabled(isEnabled);
			_stepButton.SetEnabled(isEnabled);
			_stopButton.SetEnabled(isEnabled);

			_playButton.EnableInClassList(ToolbarButtonActiveUssClassName, isEnabled && isPlaying);
			_pauseButton.EnableInClassList(ToolbarButtonActiveUssClassName, isEnabled && isPaused);
			_stepButton.EnableInClassList(ToolbarButtonActiveUssClassName, isEnabled && isStepping);
			_stopButton.EnableInClassList(ToolbarButtonActiveUssClassName, isEnabled && isStopping);
			_breakButton.EnableInClassList(ToolbarButtonActiveUssClassName, Graph.IsDebugBreakEnabled);
			_loggingButton.EnableInClassList(ToolbarButtonActiveUssClassName, Graph.IsDebugLoggingEnabled);
			_lockButton.EnableInClassList(ToolbarButtonActiveUssClassName, IsLocked);
			EnableInClassList(BreakpointsDisabledUssClassName, !Graph.IsDebugBreakEnabled);
		}

		private void ShowGraphPicker(Vector2 position)
		{
			var graphs = AssetHelper.GetAssetList<Graph>();

			_graphProvider.Setup("Select Graph", graphs.Paths, graphs.Assets.Cast<Graph>().ToList(), graph => AssetPreview.GetMiniThumbnail(graph), selectedGraph => SetGraph(selectedGraph));

			SearchWindow.Open(new SearchWindowContext(position), _graphProvider);
		}

		private void ToggleBreakpointsEnabled()
		{
			Graph.IsDebugBreakEnabled = !Graph.IsDebugBreakEnabled;
			_breakpointsEnabled.Value = Graph.IsDebugBreakEnabled;

			RefreshToolbar();
		}

		private void ToggleLoggingEnabled()
		{
			Graph.IsDebugLoggingEnabled = !Graph.IsDebugLoggingEnabled;
			_loggingEnabled.Value = Graph.IsDebugLoggingEnabled;

			RefreshToolbar();
		}

		private void ToggleLockingEnabled()
		{
			IsLocked = !IsLocked;
			RefreshToolbar();
		}
	}
}
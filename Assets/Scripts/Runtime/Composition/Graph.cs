using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	public interface IGraphRunner
	{
		void Exit();
		void GoTo(GraphNode node, string source);
		IEnumerator Run(GraphNode node, IVariableDictionary variables, string source);
	}

	[CreateAssetMenu(menuName = "PiRho Composition/Graph", fileName = nameof(Graph), order = 100)]
	public class Graph : ScriptableObject
	{
		// TODO: need to ensure a node isn't run by two different runners at the same time

		private const string _graphAlreadyRunningError = "Failed to run graph '{0}': the graph is already running";
		private const string _nodeAlreadyRunningError = "Failed to run GraphNode '{0}' on Graph '{1}': the node is already running";

		[InspectTrigger(nameof(SyncNodes))]

		[List]
		public VariableDefinitionList Inputs = new VariableDefinitionList();

		[List]
		public VariableDefinitionList Outputs = new VariableDefinitionList();

		[HideInInspector] public GraphNode StartNode = null;
		[HideInInspector] public List<GraphNode> Nodes = new List<GraphNode>();

		public bool IsRunning { get; private set; }
		public bool IsExiting { get; private set; }
		public IVariableDictionary Variables { get; private set; }
		private readonly List<GraphRunner> _runners = new List<GraphRunner>();

		#region Definition

		public VariableDefinition GetInputDefinition(GraphInput input)
		{
			foreach (var definition in Inputs)
			{
				if (input.Name == definition.Name)
					return definition;
			}

			return new VariableDefinition(input.Name);
		}

		public VariableDefinition GetOutputDefinition(GraphOutput output)
		{
			foreach (var definition in Outputs)
			{
				if (output.Name == definition.Name)
					return definition;
			}

			return new VariableDefinition(output.Name);
		}

		#endregion

		#region Reset

		void OnEnable() => Reset();
		void OnDisable() => Reset();

		public void Reset()
		{
			// in case the editor exits play mode while the graph is running
			Variables = null;
			IsRunning = false;
			DebugState = PlaybackState.Running;

			foreach (var runner in _runners)
				_graphRunnerPool.Release(runner as GraphRunner);

			_runners.Clear();
		}

		#endregion

		#region Playback

		public IEnumerator Execute()
		{
			var store = GraphStore.Reserve();
			yield return Execute(store);
			GraphStore.Release(store);
		}

		public IEnumerator Execute(GraphStore variables)
		{
			if (IsRunning)
			{
				Debug.LogErrorFormat(this, _graphAlreadyRunningError, name);
			}
			else
			{
				Variables = variables;
				IsRunning = true;

				yield return CompositionManager.Track(this, Run(StartNode, Variables, "Start"));

				IsRunning = false;
				Variables = null;
			}
		}

		private IEnumerator Run(GraphNode node, IVariableDictionary variables, string source)
		{
			var runner = _graphRunnerPool.Reserve();
			_runners.Add(runner);

#if UNITY_EDITOR
			NodeStarting(source);
#endif

			yield return runner.Run(this, node, variables, source);

#if UNITY_EDITOR
			NodeFinished(source);
#endif

			_runners.Remove(runner);
			_graphRunnerPool.Release(runner);
		}

		#endregion

		#region Debugging

#if UNITY_EDITOR

		public enum PlaybackState
		{
			Running,
			Paused,
			Step,
			Stopped
		}

		public PlaybackState DebugState { get; private set; }

		public static bool IsDebugBreakEnabled = true;
		public static bool IsDebugLoggingEnabled = false;
		public static Action<Graph, GraphNode> OnBreakpointHit;

		public Action<GraphNode> OnProcessFrame;

		public bool CanDebugPlay => IsRunning && DebugState == PlaybackState.Paused;
		public bool CanDebugPause => IsRunning && DebugState == PlaybackState.Running;
		public bool CanDebugStep => IsRunning && DebugState == PlaybackState.Paused;
		public bool CanDebugStop => IsRunning && DebugState != PlaybackState.Stopped;

		public void DebugPlay()
		{
			if (CanDebugPlay)
				DebugState = PlaybackState.Running;
		}

		public void DebugPause()
		{
			if (CanDebugPause)
				DebugState = PlaybackState.Paused;
		}

		public void DebugStep()
		{
			if (CanDebugStep)
				DebugState = PlaybackState.Step;
		}

		public void DebugStop()
		{
			if (CanDebugStop)
				DebugState = PlaybackState.Stopped;
		}

		public bool IsInCallStack(GraphNode node)
		{
			foreach (var runner in _runners)
			{
				if (runner.IsInCallStack(node))
					return true;
			}

			return false;
		}

		public bool IsInCallStack(GraphNode.ConnectionData connection)
		{
			foreach (var runner in _runners)
			{
				if (runner.IsInCallStack(connection))
					return true;
			}

			return false;
		}

		private void NodeStarting(string source)
		{
			if (IsDebugLoggingEnabled)
				Debug.Log($"(Frame {Time.frameCount}) Graph {name}: running '{source}'", this);
		}

		private IEnumerator ProcessNode(IGraphRunner runner, GraphNode node, IVariableDictionary variables, string source)
		{
			if (node.IsBreakpoint && IsDebugBreakEnabled)
			{
				DebugPause();
				OnBreakpointHit?.Invoke(this, node);
			}

			if (DebugState == PlaybackState.Paused && IsDebugLoggingEnabled)
				Debug.Log($"(Frame {Time.frameCount}) Graph {name}: pausing at node '{node.name}'");

			while (DebugState == PlaybackState.Paused)
				yield return null;

			if (DebugState == PlaybackState.Stopped)
				yield break;

			if (IsDebugLoggingEnabled)
				Debug.Log($"(Frame {Time.frameCount}) Graph {name}: following '{source}' to node '{node.name}'");

			OnProcessFrame?.Invoke(node);

			yield return node.Run(runner, variables);

			if (DebugState == PlaybackState.Step)
				DebugPause();
		}

		private void NodeFinished(string source)
		{
			if (IsDebugLoggingEnabled)
			{
				if (DebugState == PlaybackState.Stopped)
					Debug.Log($"(Frame {Time.frameCount}) Graph {name}: halting '{source}'", this);
				else
					Debug.Log($"(Frame {Time.frameCount}) Graph {name}: finished '{source}'", this);
			}
		}

#endif

		#endregion

		#region GraphRunner

		private static ClassPool<GraphRunner> _graphRunnerPool = new ClassPool<GraphRunner>(() => new GraphRunner(), 10, 5);

		private class GraphRunner : IGraphRunner
		{
			private struct NodeFrame
			{
				public GraphNode Node;
				public string Source;
			}

			private Graph _graph;
			private NodeFrame _nextNode;

			public void GoTo(GraphNode node, string source)
			{
				if (!_graph.IsExiting)
				{
					_nextNode.Node = node;
					_nextNode.Source = source;
				}
			}

			public IEnumerator Run(GraphNode node, IVariableDictionary variables, string source)
			{
				if (!_graph.IsExiting)
					yield return CompositionManager.Instance.GetEnumerator(_graph.Run(node, variables, source));
			}

			public IEnumerator Run(Graph graph, GraphNode root, IVariableDictionary variables, string source)
			{
				_graph = graph;
				_graph.IsExiting = false;
				GoTo(root, source);

#if UNITY_EDITOR
				while (Iterate())
#else
				while(_nextNode.Node != null)
#endif
				{
					var node = _nextNode;
					_nextNode.Node = null;

					node.Node.IsRunning = true;

#if UNITY_EDITOR
					yield return _graph.ProcessNode(this, node.Node, variables, node.Source);
#else
					yield return node.Node.Run(this, variables);
#endif

					node.Node.IsRunning = false;
				}
			}

			public void Reset()
			{
				_graph = null;
				_nextNode = new NodeFrame();

#if UNITY_EDITOR
				_callstack.Clear();
#endif
			}

			public void Exit()
			{
				GoTo(null, string.Empty);
				_graph.IsExiting = true;
			}

#if UNITY_EDITOR
			private readonly Stack<NodeFrame> _callstack = new Stack<NodeFrame>();

			private bool Iterate()
			{
				if (_graph.DebugState != PlaybackState.Stopped && _nextNode.Node != null)
				{
					_callstack.Push(_nextNode);
					return true;
				}

				return false;
			}

			public bool IsInCallStack(GraphNode node)
			{
				foreach (var frame in _callstack)
				{
					if (frame.Node == node)
						return true;
				}

				return false;
			}

			public bool IsInCallStack(GraphNode.ConnectionData connection)
			{
				foreach (var frame in _callstack)
				{
					if (frame.Node == connection.To && frame.Source == connection.Name)
						return true;
				}

				return false;
			}
#endif
		}

		#endregion

		#region Editor Interface

		public void SyncNodes()
		{
#if UNITY_EDITOR
			var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this))
				.Select(asset => asset as GraphNode)
				.Where(node => node != null)
				.ToList();

			for (var i = 0; i < Nodes.Count; i++)
			{
				if (!assets.Contains(Nodes[i]))
				{
					Debug.LogWarningFormat(this, "Syncing nodes for Graph {0}: removed a node that was not in the asset list", name);
					DestroyImmediate(Nodes[i], true);
					Nodes.RemoveAt(i--);
					UnityEditor.EditorUtility.SetDirty(this);
				}
			}

			foreach (var asset in assets)
			{
				if (asset is GraphNode node && !Nodes.Contains(node))
				{
					Nodes.Add(node);
					Debug.LogWarningFormat(this, "Syncing nodes for Graph {0}: added the node {1} that was an asset but was not contained in the list", name, node.name);
					UnityEditor.EditorUtility.SetDirty(this);
				}
			}
#endif
		}

		#endregion
	}
}

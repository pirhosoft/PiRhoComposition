﻿using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.Composition
{
	public class CompositionManager : GlobalBehaviour<CompositionManager>
	{
		public void RunGraph(Graph graph)
		{
			var enumerator = graph.Execute();
			RunEnumerator(enumerator);
		}

		public void RunGraph(GraphCaller caller)
		{
			RunGraph(caller, VariableContext.Default);
		}

		public void RunGraph(GraphCaller caller, IVariableDictionary variables)
		{
			var enumerator = caller.Execute(variables);
			RunEnumerator(enumerator);
		}

		public IEnumerator GetEnumerator(IEnumerator enumerator)
		{
			return new JoinEnumerator(enumerator);
		}

		public Coroutine RunEnumerator(IEnumerator enumerator)
		{
			var joined = new JoinEnumerator(enumerator);
			return StartCoroutine(joined);
		}

		#region Enumerator

		private class JoinEnumerator : IEnumerator
		{
			private const string _iterationLimitWarning = "Cancelling JoinEnumerator after {0} unyielding iterations";

			public static int MaximumIterations = 10000;

			private IEnumerator _root;
			private Stack<IEnumerator> _enumerators = new Stack<IEnumerator>(10);

			public object Current
			{
				get { return _enumerators.Peek().Current; }
			}

			public bool IsComplete => _enumerators.Count == 0;

			public JoinEnumerator(IEnumerator coroutine)
			{
				_root = coroutine;
				Push(coroutine);
			}

			private bool MoveNext_()
			{
				while (_enumerators.Count > 0 && Track())
				{
					var enumerator = _enumerators.Peek();
					var next = enumerator.MoveNext();

					// three scenarios
					//  - enumerator has no next: resume the parent (unless this is the root)
					//  - enumerator has a next and it is an IEnumerator: process that enumerator
					//  - enumerator has a next and it is something else: yield

					if (!next)
						Pop();
					else if (enumerator.Current is IEnumerator child && !(child is CustomYieldInstruction)) // CustomYieldInstruction implements IEnumerator
						Push(child);
					else
						break;
				}

				return _enumerators.Count > 0;
			}

			private void Reset_()
			{
				while (_enumerators.Count > 0)
					_enumerators.Pop();

				_enumerators.Push(_root);
				_root.Reset();
			}

#if UNITY_EDITOR

			private int _iterations = 0;
			private readonly Stack<TrackingEnumerator> _trackers = new Stack<TrackingEnumerator>(5);

			public bool MoveNext()
			{
				_iterations = 0;
				return MoveNext_();
			}

			public void Reset()
			{
				Reset_();
				_trackers.Clear();
				_iterations = 0;
			}

			private void Push(IEnumerator enumerator)
			{
				_enumerators.Push(enumerator);

				if (enumerator is TrackingEnumerator tracker)
					_trackers.Push(tracker);
			}

			private void Pop()
			{
				if (_trackers.Count > 0 && _enumerators.Peek() == _trackers.Peek())
				{
					_trackers.Peek().Finish();
					_trackers.Pop();
				}

				_enumerators.Pop();
			}

			private bool Track()
			{
				_iterations++;

				if (_iterations >= MaximumIterations)
				{
					// this is a protection against infinite loops that can crash or hang Unity

					Debug.LogWarningFormat(_iterationLimitWarning, MaximumIterations);
					_enumerators.Clear();
					_trackers.Clear();
					return false;
				}

				if (_trackers.Count > 0)
					_trackers.Peek().Continue();

				return true;
			}
#else
		
			public bool MoveNext() => MoveNext_();
			public void Reset() => Reset_();
			private void Push(IEnumerator enumerator) => _enumerators.Push(enumerator);
			private void Pop() => _enumerators.Pop();
			private bool Track() => true;

#endif
		}

		#endregion

		#region Tracking

#if UNITY_EDITOR

		public static bool LogTracking = false;

		private const string _trackingStartFormat = "{0} started";
		private const string _trackingCompleteFormat = "{0} complete: ran {1} iterations in {2} frames and {3:F3} seconds\n";

		public static Dictionary<Graph, TrackingData> TrackingState { get; } = new Dictionary<Graph, TrackingData>();

		private class TrackingEnumerator : IEnumerator
		{
			private IEnumerator _trackee;
			private TrackingData _data;

			public TrackingEnumerator(Graph graph, IEnumerator trackee)
			{
				_data = new TrackingData(graph);
				_trackee = trackee;
			}

			public void Continue()
			{
				_data.TotalIterations++;
			}

			public void Finish()
			{
				_data.Finish();
			}

			public object Current => _trackee.Current;
			public bool MoveNext() => _trackee.MoveNext();
			public void Reset() => _trackee.Reset();
		}

		public class TrackingData
		{
			public Graph Graph;
			public bool IsComplete;
			public int StartFrame;
			public float StartSeconds;
			public int EndFrame;
			public float EndSeconds;
			public int TotalIterations;

			public int TotalFrames => IsComplete ? EndFrame - StartFrame : Time.frameCount - StartFrame;
			public float TotalSeconds => IsComplete ? EndSeconds - StartSeconds : Time.realtimeSinceStartup - StartSeconds;

			public TrackingData(Graph graph)
			{
				TrackingState.Add(graph, this);

				Graph = graph;
				StartFrame = Time.frameCount;
				StartSeconds = Time.realtimeSinceStartup;

				if (LogTracking)
					Debug.LogFormat(Graph, _trackingStartFormat, Graph);
			}

			public void Finish()
			{
				IsComplete = true;
				EndFrame = Time.frameCount;
				EndSeconds = Time.realtimeSinceStartup;

				if (LogTracking)
					Debug.LogFormat(Graph, _trackingCompleteFormat, Graph, TotalIterations, TotalFrames, TotalSeconds);

				TrackingState.Remove(Graph);
			}
		}

		internal static IEnumerator Track(Graph graph, IEnumerator enumerator) => new TrackingEnumerator(graph, enumerator);

#else

		internal static IEnumerator Track(Graph graph, IEnumerator enumerator) => enumerator;

#endif

		#endregion
	}
}

using PiRhoSoft.Expressions;
using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Composition
{
	public enum GraphInputType
	{
		Expression,
		Value
	}

	public enum GraphOutputType
	{
		Ignore,
		Expression
	}

	[Serializable]
	public class GraphInput
	{
		public string Name;
		public GraphInputType Type;
		public ReadOnlyExpression Expression = new ReadOnlyExpression();
		public SerializedVariable Value = new SerializedVariable();
	}

	[Serializable]
	public class GraphOutput
	{
		public string Name;
		public GraphOutputType Type;
		public AssignmentExpression Expression = new AssignmentExpression();
	}

	public class GraphDictionary : IVariableDictionary
	{
		private const string _invalidInputError = "(CISII) Failed to create input '{0}' for graph '{1}': the value '{2}' does not satisfy the constraint";
		private const string _missingInputError = "(CISMI) Failed to read input '{0}' for graph '{1}': the variable '{2}' could not be found";

		public const string InputName = "input";
		public const string OutputName = "output";
		public const string LocalName = "local";
		public const string GlobalName = "global";

		public VariableDictionary Input { get; } = new VariableDictionary();
		public VariableDictionary Output { get; } = new VariableDictionary();
		public VariableDictionary Local { get; } = new VariableDictionary();

		private readonly string[] _variableNames = new string[] { InputName, OutputName, LocalName, GlobalName };

		#region Pooling

		private static ClassPool<GraphDictionary> _pool = new ClassPool<GraphDictionary>(() => new GraphDictionary(), 10, 5);

		internal static GraphDictionary Reserve()
		{
			return _pool.Reserve();
		}

		internal static void Release(GraphDictionary store)
		{
			store.Input.ClearVariables();
			store.Output.ClearVariables();
			store.Local.ClearVariables();
			_pool.Release(store);
		}

		#endregion

		#region Inputs and Outputs

		public void WriteInputs(GraphCaller graph, IList<GraphInput> inputs, IVariableDictionary variables)
		{
			foreach (var input in inputs)
			{
				if (input.Type == GraphInputType.Expression)
				{
					var value = input.Expression.Execute(variables);
					var definition = graph?.Graph.GetInputDefinition(input);

					value = ResolveValue(definition, value, graph.Graph, _invalidInputError, definition.Name);

					if (!value.IsEmpty)
					{
						if (Input.AddVariable(input.Name, value) == SetVariableResult.NotFound)
							Input.SetVariable(input.Name, value);
					}
					else
					{
						Debug.LogWarningFormat(_missingInputError, input.Name, graph.Graph, input.Expression);
					}
				}
				else if (input.Type == GraphInputType.Value)
				{
					if (Input.AddVariable(input.Name, input.Value.Variable) == SetVariableResult.NotFound)
						Input.SetVariable(input.Name, input.Value.Variable);
				}
			}
		}

		public void WriteOutputs(IList<GraphOutput> outputs)
		{
			foreach (var output in outputs)
			{
				if (Output.AddVariable(output.Name, Variable.Empty) == SetVariableResult.NotFound)
					Output.SetVariable(output.Name, Variable.Empty);
			}
		}

		public void ReadOutputs(GraphCaller graph, IList<GraphOutput> outputs, IVariableDictionary variables)
		{
			foreach (var output in outputs)
			{
				if (output.Type == GraphOutputType.Expression)
				{
					var value = Output.GetVariable(output.Name);
					output.Expression.Assign(variables, value);
				}
			}
		}

		private static Variable ResolveValue(VariableDefinition definition, Variable value, Object errorContext, string invalidError, string variableName)
		{
			if (definition.Type == VariableType.Object && definition.Constraint is ObjectConstraint constraint && value.TryGetObject<Object>(out var obj))
			{
				var resolved = obj.GetAsObject(constraint.ObjectType);
				value = Variable.Object(resolved);
			}

			if (definition.Type != VariableType.Empty && !definition.IsValid(value))
				Debug.LogWarningFormat(invalidError, variableName, errorContext, value);

			return value;
		}

		#endregion

		#region IVariableDictionary Implementation

		public IReadOnlyCollection<string> VariableNames => _variableNames;

		public Variable GetVariable(string name)
		{
			switch (name)
			{
				case InputName: return Variable.Dictionary(Input);
				case OutputName: return Variable.Dictionary(Output);
				case LocalName: return Variable.Dictionary(Local);
				case GlobalName: return Variable.Dictionary(VariableContext.Default);
				default: return Variable.Empty;
			}
		}

		public SetVariableResult SetVariable(string name, Variable value)
		{
			switch (name)
			{
				case InputName: return SetVariableResult.ReadOnly;
				case OutputName: return SetVariableResult.ReadOnly;
				case LocalName: return SetVariableResult.ReadOnly;
				case GlobalName: return SetVariableResult.ReadOnly;
				default: return SetVariableResult.NotFound;
			}
		}

		public SetVariableResult AddVariable(string name, Variable variable) => SetVariableResult.ReadOnly;
		public SetVariableResult RemoveVariable(string name) => SetVariableResult.ReadOnly;
		public SetVariableResult ClearVariables() => SetVariableResult.ReadOnly;

		#endregion
	}
}

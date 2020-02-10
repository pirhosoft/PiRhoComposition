using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System;

namespace PiRhoSoft.Bindings
{
	public static class ExpressionBinding
	{
		public static IBindingInput CreateInput(IVariableDictionary variables, Expression expression)
		{
			// TODO: Pooling
			return new ExpressionBindingInput(variables, expression);
		}
	}

	public class ExpressionBindingInput : IBindingInput<Variable>
	{
		public Type InputType => typeof(Variable);
		public bool IsValid => _expression.IsValid;

		private IVariableDictionary _variables;
		private Expression _expression;

		public ExpressionBindingInput(IVariableDictionary variables, Expression expression)
		{
			_variables = variables;
			_expression = expression;
		}

		public Variable Lookup() => _expression.Execute(_variables);
		public void Release() { }
	}

	public class ExpressionBindingOutput : IBindingOutput<Variable>
	{
		public Type OutputType => typeof(Variable);
		public bool IsValid => _expression.IsValid;

		private IVariableDictionary _variables;
		private string _name;
		private Expression _expression;

		public ExpressionBindingOutput(IVariableDictionary variables, string name, Expression expression)
		{
			_variables = variables;
			_name = name;
			_expression = expression;
		}

		public void Assign(Variable value)
		{
			_variables.SetVariable(_name, value);
			_expression.Execute(_variables);
		}

		public void Release() { }
	}

	public class ExpressionBindingInputWrapper : BindingInputWrapper<Variable, Variable>
	{
		private IVariableDictionary _variables;
		private string _name;
		private Expression _expression;

		public ExpressionBindingInputWrapper(IVariableDictionary variables, string name, Expression expression)
		{
			_variables = variables;
			_name = name;
			_expression = expression;
		}

		protected override Variable Wrap(Variable value)
		{
			_variables.SetVariable(_name, value);
			return _expression.Execute(_variables);
		}
	}

	public class ExpressionBindingOutputWrapper : BindingInputWrapper<Variable, Variable>
	{
		private IVariableDictionary _variables;
		private string _name;
		private Expression _expression;

		public ExpressionBindingOutputWrapper(IVariableDictionary variables, string name, Expression expression)
		{
			_variables = variables;
			_name = name;
			_expression = expression;
		}

		protected override Variable Wrap(Variable value)
		{
			_variables.SetVariable(_name, value);
			return _expression.Execute(_variables);
		}
	}
}

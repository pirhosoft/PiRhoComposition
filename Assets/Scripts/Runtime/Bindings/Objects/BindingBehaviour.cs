using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Bindings
{
	public class BindingBehaviour : MonoBehaviour
	{
		public ReadOnlyExpression SourceExpression;
		public Object SourceObject;
		public string SourceProperty;

		public Object DestinationObject;
		public string DestinationProperty;

		public bool Animate = false;
		public string Format = string.Empty;

		private IBinding _binding;

		private void Start()
		{
			UpdateBinding();
		}

		private void OnDestroy()
		{
			RemoveBinding();
		}

		private void OnEnable()
		{
			if (_binding != null)
				_binding.IsEnabled = true;
		}

		private void OnDisable()
		{
			if (_binding != null)
				_binding.IsEnabled = false;
		}

		public void UpdateBinding()
		{
			RemoveBinding();

			if (BindingProcessor.Manager != null)
			{
				var input = CreateInput();
				var output = CreateOutput();
				var transition = CreateTransition(input);

				_binding = input != null && output != null
					? BindingProcessor.Manager.Bind(input, output, transition)
					: null;
			}
		}

		public void RemoveBinding()
		{
			if (_binding != null && BindingProcessor.Manager != null)
				BindingProcessor.Manager.Remove(_binding);
		}

		private IBindingInput CreateInput()
		{
			if (SourceExpression.IsValid)
			{
				var variables = GetComponentInParent<IVariableHierarchy>() as IVariableDictionary;
				var wrapper = new ChildDictionary(variables ?? VariableContext.Default);

				return ExpressionBinding.CreateInput(wrapper, SourceExpression);
			}

			if (SourceObject && !string.IsNullOrEmpty(SourceProperty))
			{
				return PropertyBinding.CreateInput(SourceObject, SourceProperty);
			}

			return null;
		}

		private IBindingOutput CreateOutput()
		{
			var output = PropertyBinding.CreateOutput(DestinationObject, DestinationProperty);

			return !string.IsNullOrEmpty(Format) && output.OutputType == typeof(string)
				? BindingFormatter.CreateOutput(output, Format)
				: output;
		}

		private IBindingTransition CreateTransition(IBindingInput input)
		{
			return Animate && input != null ? BindingAnimation.Create(input.InputType) : null;
		}
	}
}

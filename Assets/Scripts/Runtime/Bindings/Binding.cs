using System;
using System.Collections.Generic;

namespace PiRhoSoft.Bindings
{
	public interface IBinding
	{
		Type BindingType { get; }
		bool IsValid { get; }
		bool IsEnabled { get; set; }

		void Create();
		void Destroy();
		void Update(float elapsed);
	}

	public interface IBinding<Type> : IBinding
	{
		// PENDING: C#8 will allow BindingType to be defaulted here to typeof(Type) rather than requiring it in all implementors.
	}

	public abstract class Binding
	{
		internal abstract void Setup(IBindingInput input, IBindingOutput output, IBindingTransition transition);
	}

	// TODO: Is there a use case for making this unsealed with a public constructor and virtual Create, Destroy, and
	// Update? Each instance would need to track whether it came from the BindingManager's pool or not.

	public sealed class Binding<Type> : Binding, IBinding<Type>
	{
		// This binding implementation looks up a value from an input, optionally performs a transition (usually an
		// animation) on that value, and assigns it to an output. The input and output are designed to be chainable so
		// values can be converted (which the BindingManager can handle automatically) or manipulated (e.g applied to a
		// format string). On update a comparison is used to determine if the input value has changed. If it has, or if
		// a transition is running, the output will be updated.

		public static IEqualityComparer<Type> DefaultComparison = EqualityComparer<Type>.Default;

		// PROFILE: Transition = null vs InertTransition
		private static IBindingTransition<Type> _inertTransition = new InertTransition();

		private Type _currentValue;
		private Type _previousValue;

		public IBindingInput<Type> Input { get; private set; }
		public IBindingOutput<Type> Output { get; private set; }
		public IBindingTransition<Type> Transition { get; private set; }
		public IEqualityComparer<Type> Comparison { get; set; }

		public System.Type BindingType => typeof(Type);

		public bool IsValid => Input.IsValid && Output.IsValid;
		public bool IsEnabled { get; set; } = true;

		internal Binding() { }

		internal override void Setup(IBindingInput input, IBindingOutput output, IBindingTransition transition)
		{
			Input = input as IBindingInput<Type>;
			Output = output as IBindingOutput<Type>;
			Transition = transition as IBindingTransition<Type>;
			Comparison = DefaultComparison;
		}

		public override string ToString()
		{
			return $"From {Input} to {Output}";
		}

		public void Create()
		{
			Transition = Transition ?? _inertTransition;
		}

		public void Destroy()
		{
			Input.Release();
			Output.Release();
			Transition.Release();
		}

		public void Update(float elapsed)
		{
			var value = Input.Lookup();
			var hasChanged = !Comparison.Equals(_currentValue, value); // TODO: Needs to always bind on first update (value and _currentValue might both be the default value).
			var isTransitioning = Transition.IsInProgress;

			if (hasChanged)
			{
				_previousValue = _currentValue;
				_currentValue = value;

				Transition.Start(_previousValue, _currentValue);
			}
			else if (isTransitioning)
			{
				Transition.Update(elapsed, _previousValue, _currentValue);
			}

			if (hasChanged || isTransitioning)
			{
				// This block needs to run for *was* transitioning but the value is retrieved based on *is*
				// transitioning.

				var current = Transition.IsInProgress
					? Transition.CurrentValue
					: _currentValue;

				Output.Assign(current);
			}
		}

		private class InertTransition : IBindingTransition<Type>
		{
			public bool IsInProgress => false;
			public Type CurrentValue { get; set; }
			public System.Type TransitionType => typeof(Type);

			public void Start(Type from, Type to) => CurrentValue = to;
			public void Update(float elapsed, Type from, Type to) { }
			public void Release() { }
		}
	}
}

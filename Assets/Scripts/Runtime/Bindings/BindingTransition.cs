using System;

namespace PiRhoSoft.Bindings
{
	public interface IBindingTransition
	{
		Type TransitionType { get; }
		bool IsInProgress { get; }
		void Release();
	}

	public interface IBindingTransition<Type> : IBindingTransition
	{
		// PENDING: C#8 will allow TransitionType to be defaulted here to typeof(Type) rather than requiring it in all implementors.
		Type CurrentValue { get; }

		void Start(Type from, Type to);
		void Update(float elapsed, Type from, Type to);
	}
}

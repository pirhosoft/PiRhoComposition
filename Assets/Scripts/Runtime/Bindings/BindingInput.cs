using System;

namespace PiRhoSoft.Bindings
{
	public interface IBindingInput
	{
		Type InputType { get; }
		bool IsValid { get; }
		void Release();
	}

	public interface IBindingInput<Type> : IBindingInput
	{
		// PENDING: C#8 will allow InputType to be defaulted here to typeof(Type) rather than requiring it in all implementors.
		Type Lookup();
	}
}

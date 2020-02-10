using System;

namespace PiRhoSoft.Bindings
{
	public interface IBindingOutput
	{
		Type OutputType { get; }
		bool IsValid { get; }
		void Release();
	}

	public interface IBindingOutput<Type> : IBindingOutput
	{
		// PENDING: C#8 will allow OutputType to be defaulted here to typeof(Type) rather than requiring it in all implementors.
		void Assign(Type value);
	}
}

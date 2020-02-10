using System;

namespace PiRhoSoft.Bindings
{
	public interface IBindingWrapperPool
	{
		void ReleaseInput(IBindingInputWrapper input);
		void ReleaseOutput(IBindingOutputWrapper output);
	}

	public interface IBindingInputWrapper : IBindingInput
	{
		void Setup(IBindingWrapperPool pool, IBindingInput input);
	}

	public interface IBindingOutputWrapper : IBindingOutput
	{
		void Setup(IBindingWrapperPool pool, IBindingOutput output);
	}

	public abstract class BindingInputWrapper<FromType, ToType> : IBindingInput<ToType>, IBindingInputWrapper
	{
		Type IBindingInput.InputType => typeof(ToType);
		bool IBindingInput.IsValid => _input.IsValid;

		ToType IBindingInput<ToType>.Lookup()
		{
			var from = _input.Lookup();
			return Wrap(from);
		}

		void IBindingInput.Release()
		{
			_input.Release();
			_pool.ReleaseInput(this);
		}

		private IBindingWrapperPool _pool;
		private IBindingInput<FromType> _input;

		void IBindingInputWrapper.Setup(IBindingWrapperPool pool, IBindingInput input)
		{
			_pool = pool;
			_input = input as IBindingInput<FromType>;
		}

		protected abstract ToType Wrap(FromType value);
	}

	public abstract class BindingOutputWrapper<FromType, ToType> : IBindingOutput<FromType>, IBindingOutputWrapper
	{
		Type IBindingOutput.OutputType => typeof(FromType);
		bool IBindingOutput.IsValid => _output.IsValid;

		void IBindingOutput<FromType>.Assign(FromType value)
		{
			var wrapped = Wrap(value);
			_output.Assign(wrapped);
		}

		void IBindingOutput.Release()
		{
			_output.Release();
			_pool.ReleaseOutput(this);
		}

		private IBindingWrapperPool _pool;
		private IBindingOutput<ToType> _output;

		void IBindingOutputWrapper.Setup(IBindingWrapperPool pool, IBindingOutput output)
		{
			_pool = pool;
			_output = output as IBindingOutput<ToType>;
		}

		protected abstract ToType Wrap(FromType value);
	}
}

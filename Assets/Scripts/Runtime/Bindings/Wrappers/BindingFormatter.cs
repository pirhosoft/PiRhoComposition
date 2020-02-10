using PiRhoSoft.Utilities;
using System.Collections.Generic;

namespace PiRhoSoft.Bindings
{
	public static class BindingFormatter
	{
		#region Pooling

		private class BindingFormatterPool : IBindingWrapperPool
		{
			public HierarchyPool<IBindingInputWrapper> Inputs = new HierarchyPool<IBindingInputWrapper>();
			public HierarchyPool<IBindingOutputWrapper> Outputs = new HierarchyPool<IBindingOutputWrapper>();

			public void ReleaseInput(IBindingInputWrapper input) => Inputs.Release(input);
			public void ReleaseOutput(IBindingOutputWrapper output) => Outputs.Release(output);
		}

		private static BindingFormatterPool _pool = new BindingFormatterPool();

		#endregion

		#region Registration

		public static void Register<Type>()
		{
			_pool.Inputs.Register<Type, BindingInputFormatter<Type>>(() => new BindingInputFormatter<Type>());
			_pool.Outputs.Register<Type, BindingOutputFormatter<Type>>(() => new BindingOutputFormatter<Type>());
		}

		#endregion

		#region Creation

		public static IBindingInput CreateInput(IBindingInput input, string format)
		{
			var wrapper = _pool.Inputs.Reserve(input.InputType);
			wrapper.Setup(_pool, input);

			if (wrapper is IBindingFormatter formatter)
				formatter.Format = format;

			return wrapper;
		}

		public static IBindingOutput CreateOutput(IBindingOutput output, string format)
		{
			var wrapper = _pool.Outputs.Reserve(output.OutputType);
			wrapper.Setup(_pool, output);

			if (wrapper is IBindingFormatter formatter)
				formatter.Format = format;

			return wrapper;
		}

		#endregion

		#region Implementation Classes

		private interface IBindingFormatter
		{
			string Format { set; }
		}

		private class BindingInputFormatter<Type> : BindingInputWrapper<Type, string>, IBindingFormatter
		{
			public string Format { get; set; }
			protected override string Wrap(Type value) => string.Format(Format, value);
		}

		private class BindingOutputFormatter<Type> : BindingOutputWrapper<Type, string>, IBindingFormatter
		{
			public string Format { get; set; }
			protected override string Wrap(Type value) => string.Format(Format, value);
		}

		#endregion

		#region Tools Interface

		public static List<ClassPoolInfo> InputPool => _pool.Inputs.GetPoolInfo();
		public static List<ClassPoolInfo> OutputPool => _pool.Outputs.GetPoolInfo();

		#endregion
	}
}

using PiRhoSoft.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiRhoSoft.Bindings
{
	public interface IBindingConversion<FromType, ToType>
	{
		ToType Convert(FromType from);
	}

	public static class BindingConverter
	{
		#region Pooling

		private class BindingConverterPool : IBindingWrapperPool
		{
			public HierarchyPool<IBindingInputWrapper> Inputs = new HierarchyPool<IBindingInputWrapper>();
			public HierarchyPool<IBindingOutputWrapper> Outputs = new HierarchyPool<IBindingOutputWrapper>();

			public void ReleaseInput(IBindingInputWrapper input) => Inputs.Release(input);
			public void ReleaseOutput(IBindingOutputWrapper output) => Outputs.Release(output);
		}

		private static Dictionary<Type, BindingConverterPool> _pool = new Dictionary<Type, BindingConverterPool>();

		#endregion

		#region Registration

		public static void Register<FromType, ToType>(IBindingConversion<FromType, ToType> conversion)
		{
			var fromType = typeof(FromType);

			if (!_pool.TryGetValue(fromType, out var pool))
			{
				pool = new BindingConverterPool();
				_pool.Add(fromType, pool);
			}

			pool.Inputs.Register<ToType, BindingInputConverter<FromType, ToType>>(() => new BindingInputConverter<FromType, ToType>(conversion));
			pool.Outputs.Register<ToType, BindingOutputConverter<FromType, ToType>>(() => new BindingOutputConverter<FromType, ToType>(conversion));
		}

		public static void Register<FromType, ToType>(Func<FromType, ToType> conversion)
		{
			Register(new LambdaBindingConversion<FromType, ToType>(conversion));
		}

		#endregion

		#region Creation

		public static IBindingInput CreateInput(IBindingInput input, Type toType)
		{
			if (_pool.TryGetValue(input.InputType, out var pool))
			{
				var converter = pool.Inputs.Reserve(toType);
				converter.Setup(pool, input);
				return converter;
			}

			return null;
		}

		public static IBindingOutput CreateOutput(IBindingOutput output, Type fromType)
		{
			if (_pool.TryGetValue(fromType, out var pool))
			{
				var converter = pool.Outputs.Reserve(output.OutputType);
				converter.Setup(pool, output);
				return converter;
			}

			return null;
		}

		#endregion

		#region Implementation Classes

		private class BindingInputConverter<FromType, ToType> : BindingInputWrapper<FromType, ToType>
		{
			private IBindingConversion<FromType, ToType> _conversion;
			public BindingInputConverter(IBindingConversion<FromType, ToType> conversion) => _conversion = conversion;
			protected override ToType Wrap(FromType value) => _conversion.Convert(value);
		}

		private class BindingOutputConverter<FromType, ToType> : BindingOutputWrapper<FromType, ToType>
		{
			private IBindingConversion<FromType, ToType> _conversion;
			public BindingOutputConverter(IBindingConversion<FromType, ToType> conversion) => _conversion = conversion;
			protected override ToType Wrap(FromType value) => _conversion.Convert(value);
		}

		private class LambdaBindingConversion<FromType, ToType> : IBindingConversion<FromType, ToType>
		{
			public Func<FromType, ToType> Lambda;
			public ToType Convert(FromType from) => Lambda(from);
			public LambdaBindingConversion(Func<FromType, ToType> lambda) => Lambda = lambda;
		}

		#endregion

		#region Tools Interface

		public static Dictionary<Type, List<ClassPoolInfo>> InputPool => _pool.ToDictionary(pool => pool.Key, pool => pool.Value.Inputs.GetPoolInfo());
		public static Dictionary<Type, List<ClassPoolInfo>> OutputPool => _pool.ToDictionary(pool => pool.Key, pool => pool.Value.Outputs.GetPoolInfo());

		#endregion
	}
}

using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.Bindings
{
	public class BindingManager
	{
		#region Registration

		// TODO: Figure out the how this attribute is handled (if at all) by code stripping.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		public static void RegisterDefaults()
		{
			BindingAnimation.RegisterDefaults();

			// TODO: What else should be included by default?

			Register<bool>();
			Register<int>();
			Register<float>();
			Register<string>();
			Register<Vector2Int>();
			Register<Vector3Int>();
			Register<RectInt>();
			Register<BoundsInt>();
			Register<Vector2>();
			Register<Vector3>();
			Register<Vector4>();
			Register<Quaternion>();
			Register<Rect>();
			Register<Bounds>();
			Register<Color>();
			Register<Variable>();
		}

		// Binding types that are only accessed via the non-generic methods will fail in builds that strip unused types
		// unless they are registered.

		public static void Register<Type>(int capacity = ClassPool.DefaultCapacity, int growth = ClassPool.DefaultGrowth)
		{
			_pool.Register<Type, Binding<Type>>(() => new Binding<Type>(), capacity, growth);
			PropertyBinding.RegisterType<Type>();
			BindingFormatter.Register<Type>();

			if (typeof(Type) != typeof(string))
				BindingConverter.Register(new StringConversion<Type>());

			if (typeof(Type) != typeof(Variable))
			{
				BindingConverter.Register(new ToVariableConversion<Type>());

				if (typeof(Type) != typeof(string))
					BindingConverter.Register(new FromVariableConversion<Type>());
			}
		}

		private class StringConversion<Type> : IBindingConversion<Type, string>
		{
			public string Convert(Type from) => from.ToString();
		}

		private class ToVariableConversion<Type> : IBindingConversion<Type, Variable>
		{
			public Variable Convert(Type from) => Variable.Create(from);
		}

		private class FromVariableConversion<Type> : IBindingConversion<Variable, Type>
		{
			public Type Convert(Variable from) => from.As<Type>();
		}

		#endregion

		#region Binding

		private static HierarchyPool<Binding> _pool = new HierarchyPool<Binding>();
		private List<IBinding> _bindings = new List<IBinding>();

		public IBinding Bind(IBindingInput input, IBindingOutput output, IBindingTransition transition)
		{
			// Releasing from the pool is a little bit awkward since it is not nicely mirrored. The idea is consumers only
			// need to release the IBinding and everything it owns will be correctly released automatically (so calling
			// Bind effectively transfers ownership of input, output, and transition). Things to note:
			// - Conversion objects are also potentially created from a pool. Since these are just wrappers for an input
			//   or output they become the owner of the input/output and are released as if they were the original.
			// - If a binding reservation fails (i.e a valid conversion couldn't be created) everything will be released.
			// - If a binding is added directly (i.e with Add rather than Bind) that binding is responsible for cleaning
			//   itself up in its Destroy method.
			// - When a binding becomes invalid (i.e IsValid returns false) it will be released. Remove can still be called
			//   manually in this case but it will do nothing.

			var bindingType = transition != null ? transition.TransitionType : input.InputType;
			var isValid = true;

			if (input.InputType != bindingType)
			{
				var conversion = BindingConverter.CreateInput(input, bindingType);

				if (conversion != null)
					input = conversion;
				else
					isValid = false;
			}

			if (output.OutputType != bindingType)
			{
				var conversion = BindingConverter.CreateOutput(output, bindingType);

				if (conversion != null)
					output = conversion;
				else
					isValid = false;
			}

			// TODO: The reflection fallback on HierarchyPool doesn't work here because the pooled type is different
			// from the key type.

			var binding = isValid
				? _pool.Reserve(bindingType)
				: null;

			if (binding != null)
			{
				binding.Setup(input, output, transition);
				Add(binding as IBinding);
				return binding as IBinding;
			}
			else
			{
				input.Release();
				output.Release();
				transition?.Release();

				return null;
			}
		}

		public void Add(IBinding binding)
		{
			binding.Create();
			_bindings.Add(binding);
		}

		public void Remove(IBinding binding)
		{
			// The binding may have already been removed by becoming invalid.
			if (_bindings.Remove(binding))
				Release(binding);
		}

		private void Remove(IBinding binding, int index)
		{
			_bindings.RemoveAt(index);
			Release(binding);
		}

		private void Release(IBinding binding)
		{
			binding.Destroy();

			if (binding is Binding pooled)
				_pool.Release(pooled);
		}

		public void Update(float elapsed)
		{
			for (var i = 0; i < _bindings.Count; i++)
			{
				var binding = _bindings[i];

				if (!binding.IsValid)
					Remove(binding, i--);
				else if (binding.IsEnabled)
					binding.Update(elapsed);
			}
		}

		#endregion

		#region Tools Interface

		public List<ClassPoolInfo> Pool => _pool.GetPoolInfo();
		public List<IBinding> Bindings => _bindings;

		#endregion
	}
}

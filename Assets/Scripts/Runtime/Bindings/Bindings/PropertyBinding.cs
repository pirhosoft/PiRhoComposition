using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System;
using System.Collections.Generic;

namespace PiRhoSoft.Bindings
{
	public static class PropertyBinding
	{
		#region Registration

		public static void RegisterType<PropertyType>()
		{
			_inputPools.Register<PropertyType, PropertyBindingInput<PropertyType>>(() => new PropertyBindingInput<PropertyType>());
			_outputPools.Register<PropertyType, PropertyBindingOutput<PropertyType>>(() => new PropertyBindingOutput<PropertyType>());
		}

		#endregion

		#region Pooling

		private static HierarchyPool<IPropertyBindingInput> _inputPools = new HierarchyPool<IPropertyBindingInput>();
		private static HierarchyPool<IPropertyBindingOutput> _outputPools = new HierarchyPool<IPropertyBindingOutput>();

		public static IBindingInput CreateInput(object obj, string propertyName)
		{
			var map = ObjectMap.Get(obj.GetType());
			var property = map.GetProperty(propertyName);

			var binding = _inputPools.Reserve(property.PropertyType);
			binding.Setup(obj, property);
			return binding;
		}

		public static IBindingOutput CreateOutput(object obj, string propertyName)
		{
			var map = ObjectMap.Get(obj.GetType());
			var property = map.GetProperty(propertyName);

			var binding = _outputPools.Reserve(property.PropertyType);
			binding.Setup(obj, property);
			return binding;
		}

		private static void ReleaseInput(IPropertyBindingInput input)
		{
			_inputPools.Release(input);
		}

		private static void ReleaseOutput(IPropertyBindingOutput output)
		{
			_outputPools.Release(output);
		}

		#endregion

		#region Tools Interface

		public static List<ClassPoolInfo> InputPool => _inputPools.GetPoolInfo();
		public static List<ClassPoolInfo> OutputPool => _outputPools.GetPoolInfo();

		#endregion

		#region Binding Classes

		private interface IPropertyBindingInput : IBindingInput
		{
			void Setup(object obj, IMappedProperty property);
		}

		private class PropertyBindingInput<PropertyType> : IPropertyBindingInput, IBindingInput<PropertyType>
		{
			public object Object;
			public IMappedProperty<PropertyType> Property;

			public Type InputType => typeof(PropertyType);
			public bool IsValid => !Object.Equals(null);

			public void Setup(object obj, IMappedProperty property)
			{
				Object = obj;
				Property = property as IMappedProperty<PropertyType>;
			}

			public void Release()
			{
				ReleaseInput(this);
			}

			public PropertyType Lookup()
			{
				return Property.Lookup(Object);
			}
		}

		private interface IPropertyBindingOutput : IBindingOutput
		{
			void Setup(object obj, IMappedProperty property);
		}

		private class PropertyBindingOutput<PropertyType> : IPropertyBindingOutput, IBindingOutput<PropertyType>
		{
			public object Object;
			public IMappedProperty<PropertyType> Property;

			public Type OutputType => typeof(PropertyType);
			public bool IsValid => !Object.Equals(null);

			public void Setup(object obj, IMappedProperty property)
			{
				Object = obj;
				Property = property as IMappedProperty<PropertyType>;
			}

			public void Release()
			{
				ReleaseOutput(this);
			}

			public void Assign(PropertyType value)
			{
				Property.Assign(Object, value);
			}
		}

		#endregion
	}
}

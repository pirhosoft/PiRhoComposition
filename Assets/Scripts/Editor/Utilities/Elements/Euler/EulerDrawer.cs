﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(EulerAttribute))]
	class EulerDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "(PUEEDIT) invalid type for EulerAttribute on field '{0}': Euler can only be applied to Quaternion fields";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.Quaternion)
				return new EulerField().ConfigureProperty(property);
			else
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);

			return new FieldContainer(property.displayName);
		}
	}
}
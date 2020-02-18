using PiRhoSoft.Utilities.Editor;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Composition.Editor
{
	public class GraphNodeField : BaseField<Object>
	{
		public const string Stylesheet = "GraphNodeStyle.uss";
		public const string UssClassName = "pirho-graph-node";
		public const string LabelUssClassName = UssClassName + "__label";
		public const string InputUssClassName = UssClassName + "__input";
		public const string InputLabelUssClassName = InputUssClassName + "__label";
		public const string IconUssClassName = InputUssClassName + "__icon";

		private readonly GraphNodeControl _control;

		public GraphNodeField() : base(null, null)
		{
			_control = new GraphNodeControl();
			_control.AddToClassList(InputUssClassName);
			_control.RegisterCallback<ChangeEvent<GraphNode>>(evt =>
			{
				base.value = evt.newValue;
				evt.StopImmediatePropagation();
			});

			labelElement.AddToClassList(LabelUssClassName);

			AddToClassList(UssClassName);
			this.SetVisualInput(_control);
			this.AddStyleSheet(Stylesheet);
		}

		public override void SetValueWithoutNotify(Object newValue)
		{
			var node = newValue as GraphNode;
			base.SetValueWithoutNotify(node);
			_control.SetValueWithoutNotify(node);
		}

		#region Visual Input

		public class GraphNodeControl : VisualElement
		{
			public GraphNode Value { get; private set; }

			private readonly Label _label;
			private readonly IconButton _icon;

			public GraphNodeControl()
			{
				_label = new Label();
				_label.AddToClassList(InputLabelUssClassName);

				_icon = new IconButton(() => Selection.activeObject = Value) { image = Icon.Inspect.Texture, tooltip = "Select and edit this node" };
				_icon.AddToClassList(IconUssClassName);

				Add(_label);
				Add(_icon);

				Refresh();
			}

			public void SetValueWithoutNotify(GraphNode newValue)
			{
				Value = newValue;
				Refresh();
			}

			private void Refresh()
			{
				_label.text = Value ? Value.name : "Unconnected";
				_icon.SetEnabled(Value);
			}
		}
		#endregion
	}
}

﻿namespace PiRhoSoft.Utilities
{
	public class ListAttribute : PropertyTraitAttribute
	{
		public const string Always = "";
		public const string Never = null;

		public string AllowAdd = Always;
		public string AllowRemove = Always;

		public bool AllowReorder = true;
		public bool IsCollapsable = true;

		public string EmptyLabel = null;

		public string AddCallback = null;
		public string RemoveCallback = null;
		public string ReorderCallback = null;
		public string ChangeCallback = null;

		public ListAttribute() : base(ContainerPhase, 0)
		{
		}
	}
}
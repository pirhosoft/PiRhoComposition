using System.Collections.Generic;

namespace PiRhoSoft.Composition.Editor
{
	public class GlobalAutocompleteItem : AutocompleteItem
	{
		public GlobalAutocompleteItem()
		{
			AllowsCustomFields = true;
			IsCastable = false;
			IsIndexable = false;
			Fields = new List<IAutocompleteItem>();
			Types = null;
		}

		public override void Setup(object obj)
		{
		}
	}
}

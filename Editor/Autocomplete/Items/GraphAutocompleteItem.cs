using PiRhoSoft.Variables;
using System.Collections.Generic;

namespace PiRhoSoft.Composition.Editor
{
	[Autocomplete(typeof(Graph))]
	public class GraphAutocompleteItem : AutocompleteItem<Graph>
	{
		private Graph _graph;

		private readonly InputAutocompleteItem _input = new InputAutocompleteItem { Name = GraphDictionary.InputStoreName };
		private readonly OutputAutocompleteItem _output = new OutputAutocompleteItem { Name = GraphDictionary.OutputStoreName };
		private readonly LocalAutocompleteItem _local = new LocalAutocompleteItem { Name = GraphDictionary.LocalStoreName };
		private readonly GlobalAutocompleteItem _global = new GlobalAutocompleteItem { Name = GraphDictionary.GlobalStoreName };
		//private readonly SceneAutocompleteItem _scene = new SceneAutocompleteItem { Name = CompositionManager.SceneStoreName };

		protected override void Setup(Graph graph)
		{
			_graph = graph;

			AllowsCustomFields = false;
			IsCastable = false;
			IsIndexable = false;
			Fields = new List<IAutocompleteItem> { _input, _output, _local, _global/*, _scene*/ };
			Types = null;

			Reset();

			GraphViewEditor.AutocompleteChanged.Subscribe(this, item => item.Reset());
		}

		private void Reset()
		{
			_input.Setup(_graph);
			_output.Setup(_graph);
			_local.Setup(_graph);
			_global.Setup(_graph);
			//_scene.Setup(_graph);
		}

		public class InputAutocompleteItem : AutocompleteItem
		{
			public InputAutocompleteItem()
			{
				AllowsCustomFields = true;
				IsCastable = false;
				IsIndexable = false;
				Fields = new List<IAutocompleteItem>();
				Types = null;
			}

			public override void Setup(object obj)
			{
				Fields.Clear();

				var graph = obj as Graph;

				foreach (var input in graph.Inputs)
					Fields.Add(new DefinitionAutocompleteItem(input)); // if GraphEditor.AutocompleteCaller is assigned, see if there is a value assigned and use that instead
			}
		}

		public class OutputAutocompleteItem : AutocompleteItem
		{
			public OutputAutocompleteItem()
			{
				AllowsCustomFields = true;
				IsCastable = false;
				IsIndexable = false;
				Fields = new List<IAutocompleteItem>();
				Types = null;
			}

			public override void Setup(object obj)
			{
				Fields.Clear();

				var graph = obj as Graph;
				
				foreach (var output in graph.Outputs)
					Fields.Add(new DefinitionAutocompleteItem(output));
			}
		}

		public class LocalAutocompleteItem : AutocompleteItem
		{
			public LocalAutocompleteItem()
			{
				AllowsCustomFields = true;
				IsCastable = false;
				IsIndexable = false;
				Fields = new List<IAutocompleteItem>();
				Types = null;
			}

			public override void Setup(object obj)
			{
				Fields.Clear();

				var graph = obj as Graph;
				var locals = new VariableDefinitionList();

				foreach (var local in locals)
					Fields.Add(new DefinitionAutocompleteItem(local));
			}
		}
	}

	[Autocomplete(typeof(GraphNode))]
	public class GraphNodeAutocompleteItem : GraphAutocompleteItem
	{
		public override void Setup(object obj)
		{
			base.Setup((obj as GraphNode).Graph);
		}
	}
}

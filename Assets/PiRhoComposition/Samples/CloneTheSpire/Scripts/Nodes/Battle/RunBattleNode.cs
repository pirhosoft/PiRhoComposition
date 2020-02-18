using PiRhoSoft.Composition;
using PiRhoSoft.Expressions;
using PiRhoSoft.Variables;
using System.Collections;

namespace CloneTheSpire
{
	public class RunBattleNode : GraphNode
	{
		public ReadOnlyExpression Battle = new ReadOnlyExpression();

		public override IEnumerator Run(IGraphRunner graph, IVariableDictionary variables)
		{
			var battle = Battle.Execute<BattleRoom>(variables);
			yield return null;//battle.RunBattle();
		}
	}
}

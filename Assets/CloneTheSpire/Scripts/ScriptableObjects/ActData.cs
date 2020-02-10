using UnityEngine;

namespace CloneTheSpire
{
	[CreateAssetMenu(menuName = "Clone The Spire/Act", fileName = "Act")]
	public class ActData : ScriptableObject
	{
		public virtual Act CreateAct(Game game)
		{
			return new Act(this);
		}
	}
}

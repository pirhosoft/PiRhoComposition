using UnityEngine;

namespace CloneTheSpire
{
	[CreateAssetMenu(menuName = "Clone The Spire/Enemy")]
	public class EnemyData : ScriptableObject
	{
		public virtual Enemy Generate(Game game)
		{
			return new Enemy();
		}
	}
}

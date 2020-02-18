using System.Collections;
using UnityEngine;

namespace CloneTheSpire
{
	public class Enemy : MonoBehaviour
	{
		public int MaxHealth;
		public int Health;

		public IEnumerator DoTurn(Game game, BattleRoom battle)
		{
			yield break;
		}
	}
}

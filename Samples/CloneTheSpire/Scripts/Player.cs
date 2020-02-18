using UnityEngine;

namespace CloneTheSpire
{
	public class Player : MonoBehaviour
	{
		public CharacterData Character;

		public int Block;
		public int Health;
		public int MaxHealth;

		public void AddedToGame(Game game)
		{
			Block = 0;
			Health = Character.Health;
			MaxHealth = Character.Health;
		}
	}
}

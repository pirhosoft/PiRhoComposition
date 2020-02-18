using PiRhoSoft.Composition;
using UnityEngine;

namespace CloneTheSpire
{
	[CreateAssetMenu(menuName = "Clone The Spire/Character", fileName = "Character")]
	public class CharacterData : ScriptableObject
	{
		public int Health;

		public Graph TurnGraph;
	}
}

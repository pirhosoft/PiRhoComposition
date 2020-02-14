using PiRhoSoft.Composition;
using System;
using UnityEngine;

namespace CloneTheSpire
{
	public enum CardType
	{
		Attack,
		Skill,
		Power,
		Status,
		Curse
	}

	public class CardData : ScriptableObject
	{
		public CardData Data;

		public string DisplayName;

		public int Damage = 0;
		public int Block = 0;
		public int Heal = 0;
		public int SelfDamage = 0;

		public CardData Upgrade;

		public CardType Type = CardType.Attack;
		public CardDestination AfterPlayDestination = CardDestination.Discard;
		public CardDestination EndOfTurnDestination = CardDestination.Discard;

		public Graph OnDraw;
		public Graph OnAddToDeck;
		public Graph OnAddToHand;
		public Graph OnPlay;
		public Graph AfterPlay;

		public CardData Clone()
		{
			return (CardData)MemberwiseClone();
		}
	}
}

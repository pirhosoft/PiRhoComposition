using PiRhoSoft.Expressions;
using UnityEngine;

namespace CloneTheSpire
{
	public enum CardLocation
	{
		Deck,
		Hand,
		Discard,
		Exhaust,
		Character,
		Draw
	}

	public enum CardDestination
	{
		Deck,
		Hand,
		Discard,
		Exhaust,
		Character,
		DrawRandom,
		DrawTop,
		DrawBottom
	}

	public class Card : MonoBehaviour
	{
		public CardDestination StartDestination = CardDestination.DrawRandom;
		public CardDestination PlayDestination = CardDestination.Discard;
		public CardDestination ShuffleDestination = CardDestination.DrawRandom;

		public bool IsTargetable;
		public int Cost;
		public int Attack;
		public int Block;

		public StringExpression Description;

		public virtual void Play(Enemy target)
		{
		}

		public virtual void OnAddedToDeck()
		{
		}

		public virtual void OnRemovedFromDeck()
		{
		}

		public virtual void OnMovedToDraw()
		{
		}

		public virtual void OnMovedToHand()
		{
		}

		public virtual void OnMovedToDiscard()
		{
		}

		public virtual void OnMovedToExhaust()
		{
		}

		public virtual void OnMovedToCharacter()
		{
		}
	}
}

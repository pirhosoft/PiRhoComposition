using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloneTheSpire
{
	public interface IGameAction
	{
		IEnumerator Process();
	}

	public class Game : MonoBehaviour
	{
		#region Game Events

		public delegate void GameEvent(Game game);
		public event GameEvent GameStarting;
		public event GameEvent GameStarted;
		public event GameEvent GameFinishing;
		public event GameEvent GameFinished;
		public void InvokeGameStarting() => GameStarting?.Invoke(this);
		public void InvokeGameStarted() => GameStarted?.Invoke(this);
		public void InvokeGameFinishing() => GameFinishing?.Invoke(this);
		public void InvokeGameFinished() => GameFinished?.Invoke(this);

		#endregion

		#region Map Events

		public delegate void ActEvent(Game game, Act act);
		public event ActEvent ActStarting;
		public event ActEvent ActStarted;
		public event ActEvent ActFinishing;
		public event ActEvent ActFinished;
		public void InvokeActStarting(Act act) => ActStarting?.Invoke(this, act);
		public void InvokeActStarted(Act act) => ActStarted?.Invoke(this, act);
		public void InvokeActFinishing(Act act) => ActFinishing?.Invoke(this, act);
		public void InvokeActFinished(Act act) => ActFinished?.Invoke(this, act);

		public delegate void RoomEvent(Game game, Room room);
		public event RoomEvent RoomEntering;
		public event RoomEvent RoomEntered;
		public event RoomEvent RoomLeaving;
		public event RoomEvent RoomLeft;
		public void InvokeRoomEntering(Room room) => RoomEntering?.Invoke(this, room);
		public void InvokeRoomEntered(Room room) => RoomEntered?.Invoke(this, room);
		public void InvokeRoomLeaving(Room room) => RoomLeaving?.Invoke(this, room);
		public void InvokeRoomLeft(Room room) => RoomLeft?.Invoke(this, room);

		#endregion

		#region Player Events

		public delegate void CardEvent(Game game, Card card);
		public event CardEvent CardAcquiring;
		public event CardEvent CardAcquired;
		public event CardEvent CardRemoving;
		public event CardEvent CardRemoved;
		public void InvokeCardAcquiring(Card card) => CardAcquiring?.Invoke(this, card);
		public void InvokeCardAcquired(Card card) => CardAcquired?.Invoke(this, card);
		public void InvokeCardRemoving(Card card) => CardRemoving?.Invoke(this, card);
		public void InvokeCardRemoved(Card card) => CardRemoved?.Invoke(this, card);

		public delegate void PotionEvent(Game game, Potion Potion);
		public event PotionEvent PotionAcquiring;
		public event PotionEvent PotionAcquired;
		public event PotionEvent PotionUsing;
		public event PotionEvent PotionUsed;
		public event PotionEvent PotionRemoving;
		public event PotionEvent PotionRemoved;
		public void InvokePotionAcquiring(Potion potion) => PotionAcquiring?.Invoke(this, potion);
		public void InvokePotionAcquired(Potion potion) => PotionAcquired?.Invoke(this, potion);
		public void InvokePotionUsing(Potion potion) => PotionUsing?.Invoke(this, potion);
		public void InvokePotionUsed(Potion potion) => PotionUsed?.Invoke(this, potion);
		public void InvokePotionRemoving(Potion potion) => PotionRemoving?.Invoke(this, potion);
		public void InvokePotionRemoved(Potion potion) => PotionRemoved?.Invoke(this, potion);

		public delegate void RelicEvent(Relic relic);
		public event RelicEvent RelicAcquiring;
		public event RelicEvent RelicAcquired;
		public event RelicEvent RelicRemoving;
		public event RelicEvent RelicRemoved;
		public void InvokeRelicAcquiring(Relic relic) => RelicAcquiring?.Invoke(relic);
		public void InvokeRelicAcquired(Relic relic) => RelicAcquired?.Invoke(relic);
		public void InvokeRelicRemoving(Relic relic) => RelicRemoving?.Invoke(relic);
		public void InvokeRelicRemoved(Relic relic) => RelicRemoved?.Invoke(relic);

		public delegate void GoldEvent(int gold);
		public event GoldEvent GoldAcquiring;
		public event GoldEvent GoldAcquired;
		public event GoldEvent GoldSpending;
		public event GoldEvent GoldSpent;
		public event GoldEvent GoldRemoving;
		public event GoldEvent GoldRemoved;
		public void InvokeGoldAcquiring(int gold) => GoldAcquiring?.Invoke(gold);
		public void InvokeGoldAcquired(int gold) => GoldAcquired?.Invoke(gold);
		public void InvokeGoldSpending(int gold) => GoldSpending?.Invoke(gold);
		public void InvokeGoldSpent(int gold) => GoldSpent?.Invoke(gold);
		public void InvokeGoldRemoving(int gold) => GoldRemoving?.Invoke(gold);
		public void InvokeGoldRemoved(int gold) => GoldRemoved?.Invoke(gold);

		#endregion

		#region Battle Events

		public delegate void BattleEvent(BattleRoom battle);
		public event BattleEvent BattleStarting;
		public event BattleEvent BattleStarted;
		public event BattleEvent BattleFinishing;
		public event BattleEvent BattleFinished;
		public void InvokeBattleStarting(BattleRoom battle) => BattleStarting?.Invoke(battle);
		public void InvokeBattleStarted(BattleRoom battle) => BattleStarted?.Invoke(battle);
		public void InvokeBattleFinishing(BattleRoom battle) => BattleFinishing?.Invoke(battle);
		public void InvokeBattleFinished(BattleRoom battle) => BattleFinished?.Invoke(battle);

		public delegate void TurnEvent();
		public event TurnEvent TurnStarting;
		public event TurnEvent TurnStarted;
		public event TurnEvent TurnFinishing;
		public event TurnEvent TurnFinished;
		public void InvokeTurnStarting() => TurnStarting?.Invoke();
		public void InvokeTurnStarted() => TurnStarted?.Invoke();
		public void InvokeTurnFinishing() => TurnFinishing?.Invoke();
		public void InvokeTurnFinished() => TurnFinished?.Invoke();

		public delegate void PlayerTurnEvent();
		public event PlayerTurnEvent PlayerTurnStarting;
		public event PlayerTurnEvent PlayerTurnStarted;
		public event PlayerTurnEvent PlayerTurnFinishing;
		public event PlayerTurnEvent PlayerTurnFinished;
		public void InvokePlayerTurnStarting() => PlayerTurnStarting?.Invoke();
		public void InvokePlayerTurnStarted() => PlayerTurnStarted?.Invoke();
		public void InvokePlayerTurnFinishing() => PlayerTurnFinishing?.Invoke();
		public void InvokePlayerTurnFinished() => PlayerTurnFinished?.Invoke();

		public delegate void EnemiesTurnEvent();
		public event EnemiesTurnEvent EnemiesTurnStarting;
		public event EnemiesTurnEvent EnemiesTurnStarted;
		public event EnemiesTurnEvent EnemiesTurnFinishing;
		public event EnemiesTurnEvent EnemiesTurnFinished;
		public void InvokeEnemiesTurnStarting() => EnemiesTurnStarting?.Invoke();
		public void InvokeEnemiesTurnStarted() => EnemiesTurnStarted?.Invoke();
		public void InvokeEnemiesTurnFinishing() => EnemiesTurnFinishing?.Invoke();
		public void InvokeEnemiesTurnFinished() => EnemiesTurnFinished?.Invoke();

		public delegate void EnemyTurnEvent(Enemy enemy);
		public event EnemyTurnEvent EnemyTurnStarting;
		public event EnemyTurnEvent EnemyTurnStarted;
		public event EnemyTurnEvent EnemyTurnFinishing;
		public event EnemyTurnEvent EnemyTurnFinished;
		public void InvokeEnemyTurnStarting(Enemy enemy) => EnemyTurnStarting?.Invoke(enemy);
		public void InvokeEnemyTurnStarted(Enemy enemy) => EnemyTurnStarted?.Invoke(enemy);
		public void InvokeEnemyTurnFinishing(Enemy enemy) => EnemyTurnFinishing?.Invoke(enemy);
		public void InvokeEnemyTurnFinished(Enemy enemy) => EnemyTurnFinished?.Invoke(enemy);

		#endregion

		public GameData Data;

		public Player Player { get; private set; }
		public Act Act { get; private set; }
		public int ActIndex { get; private set; }
		public bool IsRunning { get; private set; }

		private Queue<IGameAction> _actions = new Queue<IGameAction>();

		public void Setup(Player player)
		{
			Player = player;
			Act = null;
			ActIndex = 0;
			IsRunning = true;

			StartCoroutine(RunActions());

			player.AddedToGame(this);

			InvokeGameStarting();
			Data.Start();
			InvokeGameStarted();
		}

		public void Teardown()
		{
			InvokeGameFinishing();
			Data.Finish();
			InvokeGameFinished();

			IsRunning = false;
		}

		#region Actions

		public void QueueAction(IGameAction action)
		{
			_actions.Enqueue(action);
		}

		public IEnumerator RunActions()
		{
			while (IsRunning)
			{
				while (_actions.Count > 0)
				{
					var action = _actions.Dequeue();
					yield return action.Process();
				}
				
				yield return null;
			}
		}

		public IEnumerator WaitForActions()
		{
			while (_actions.Count > 0)
				yield return null;
		}

		#endregion
	}
}

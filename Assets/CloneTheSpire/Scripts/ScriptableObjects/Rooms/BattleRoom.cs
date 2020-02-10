using PiRhoSoft.Composition;
using PiRhoSoft.Variables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CloneTheSpire
{
	public abstract class BattleRoom : RoomData
	{
		public List<EnemyData> Enemies = new List<EnemyData>();

		protected Game Game;

		public virtual IEnumerator RunBattle(Game game)
		{
			Game = game;

			var variables = new GraphStore();
			var enemies = new List<Variable>();
			var aliveEnemies = enemies.Where(v => v.GetObject<Enemy>().Health > 0);

			foreach (var enemyData in Enemies)
			{
				var enemy = enemyData.Generate(game);
				enemies.Add(Variable.Object(enemy));
			}

			variables.Input.SetVariable("Game", Variable.Object(game));
			variables.Input.SetVariable("Player", Variable.Object(game.Player));
			variables.Input.SetVariable("Enemies", Variable.List(new VariableList(enemies)));

			DoStartBattle();
			yield return game.WaitForActions();
			
			while (game.Player.Health > 0 && aliveEnemies.Any())
			{
				DoStartTurn();

				DoStartPlayerTurn();
				yield return game.Player.Character.TurnGraph.Execute(variables);
				DoFinishPlayerTurn();
			
				DoStartEnemiesTurn();
				foreach (var enemyVariable in aliveEnemies)
				{
					var enemy = enemyVariable.GetObject<Enemy>();
					DoStartEnemyTurn(enemy);
					yield return enemy.DoTurn(game, this);
					DoFinishEnemyTurn(enemy);
				}
				DoFinishEnemiesTurn();
			
				DoFinishTurn();
				yield return game.WaitForActions();
			}

			DoFinishBattle();
			yield return game.WaitForActions();
		}

		protected virtual void StartBattle() { }
		protected virtual void FinishBattle() { }
		protected virtual void StartTurn() { }
		protected virtual void FinishTurn() { }
		protected virtual void StartPlayerTurn() { }
		protected virtual void FinishPlayerTurn() { }
		protected virtual void StartEnemiesTurn() { }
		protected virtual void FinishEnemiesTurn() { }
		protected virtual void StartEnemyTurn(Enemy enemy) { }
		protected virtual void FinishEnemyTurn(Enemy enemy) { }

		protected virtual void DoStartBattle()
		{
			Game.InvokeBattleStarting(this);
			StartBattle();
			Game.InvokeBattleStarted(this);
		}

		protected virtual void DoFinishBattle()
		{
			Game.InvokeBattleFinishing(this);
			FinishBattle();
			Game.InvokeBattleFinished(this);
		}

		protected virtual void DoStartTurn()
		{
			Game.InvokeTurnStarting();
			StartTurn();
			Game.InvokeTurnStarted();
		}

		protected virtual void DoFinishTurn()
		{
			Game.InvokeTurnFinishing();
			FinishTurn();
			Game.InvokeTurnFinished();
		}

		protected virtual void DoStartPlayerTurn()
		{
			Game.InvokePlayerTurnStarting();
			StartPlayerTurn();
			Game.InvokePlayerTurnStarted();
		}

		protected virtual void DoFinishPlayerTurn()
		{
			Game.InvokePlayerTurnFinishing();
			FinishPlayerTurn();
			Game.InvokePlayerTurnFinished();
		}

		protected virtual void DoStartEnemiesTurn()
		{
			Game.InvokeEnemiesTurnStarting();
			StartEnemiesTurn();
			Game.InvokeEnemiesTurnStarted();
		}

		protected virtual void DoFinishEnemiesTurn()
		{
			Game.InvokeEnemiesTurnFinishing();
			FinishEnemiesTurn();
			Game.InvokeEnemiesTurnFinished();
		}

		protected virtual void DoStartEnemyTurn(Enemy enemy)
		{
			Game.InvokeEnemyTurnStarting(enemy);
			StartEnemyTurn(enemy);
			Game.InvokeEnemyTurnStarted(enemy);
		}

		protected virtual void DoFinishEnemyTurn(Enemy enemy)
		{
			Game.InvokeEnemyTurnFinishing(enemy);
			FinishEnemyTurn(enemy);
			Game.InvokeEnemyTurnFinished(enemy);
		}
	}
}

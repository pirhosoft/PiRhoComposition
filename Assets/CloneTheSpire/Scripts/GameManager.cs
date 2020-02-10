using PiRhoSoft.Utilities;
using System;
using UnityEngine;

namespace CloneTheSpire
{
	[Serializable]
	public class GameSettings
	{
		public Game Game;
		public Player Player;
	}

	public class GameManager : MonoBehaviour
	{
		[Button(nameof(Create), "Start Game")]
		[Frame]
		public GameSettings Settings = new GameSettings();

		private Game _game;
		private Player _player;

		public void Create()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Enter play mode to start a game");
				return;
			}

			if (Settings.Game == null)
			{
				Debug.LogWarning("Set the 'Game' prefab before starting a game");
				return;
			}

			if (Settings.Player == null)
			{
				Debug.LogWarning("Set the 'Player' prefab before starting a game");
				return;
			}

			_game = Instantiate(Settings.Game);
			_player = Instantiate(Settings.Player);

			_game.Setup(_player);
		}

		public void Quit()
		{
			_game.Teardown();

			Destroy(_player);
			Destroy(_game);
		}

		public void EnterRoom(RoomData room)
		{
			if (room is BattleRoom battle)
				StartCoroutine(battle.RunBattle(_game));
		}
	}
}

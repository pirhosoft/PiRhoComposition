using PiRhoSoft.Utilities;
using System;
using UnityEngine;

namespace CloneTheSpire
{
	[Serializable] public class ActDataList : SerializedList<ActData> { }

	[CreateAssetMenu(menuName = "Clone The Spire/Game", fileName = "Game")]
	public class GameData : ScriptableObject
	{
		[List] public ActDataList Acts = new ActDataList();

		public virtual void Start()
		{
		}

		public virtual void Finish()
		{
		}
	}
}

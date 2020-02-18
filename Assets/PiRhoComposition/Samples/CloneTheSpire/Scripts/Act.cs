using System.Collections.Generic;

namespace CloneTheSpire
{
	public class RoomNode
	{
		public RoomData Room { get; private set; }
		public List<RoomNode> Next { get; private set; }
	}

	public class Act
	{
		public ActData Data { get; private set; }

		public RoomNode StartNode { get; private set; }
		public RoomNode CurrentNode { get; private set; }
		public Room CurrentRoom { get; private set; }

		private List<Room> _previousRooms = new List<Room>();

		public IReadOnlyList<Room> PreviousRooms => _previousRooms;

		public Act(ActData data)
		{
			Data = data;
		}
	}
}

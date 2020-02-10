using PiRhoSoft.Utilities;
using UnityEngine.EventSystems;

namespace PiRhoSoft.Composition
{
	public class ClickGraphTrigger : GraphTrigger, IPointerDownHandler, IPointerUpHandler
	{
		public enum ClickState
		{
			OnUp,
			OnDown
		}

		[EnumButtons]
		public ClickState State = ClickState.OnUp;

		public void OnPointerDown(PointerEventData eventData)
		{
			if (State == ClickState.OnDown)
				Run();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (State == ClickState.OnDown)
				Run();
		}
	}
}

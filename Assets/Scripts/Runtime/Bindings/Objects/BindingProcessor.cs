using UnityEngine;

namespace PiRhoSoft.Bindings
{
	public class BindingProcessor : MonoBehaviour
	{
		public static BindingManager Manager { get; private set; }

		private BindingManager _manager = new BindingManager();

		private void OnEnable()
		{
			if (Manager == null)
				Manager = _manager;
		}

		private void OnDisable()
		{
			if (Manager == _manager)
				Manager = null;
		}

		private void Update() => _manager.Update(Time.deltaTime);
	}
}

using PiRhoSoft.Variables;
using UnityEngine;

namespace CloneTheSpire
{
	public static class Mappings
	{
		[RuntimeInitializeOnLoadMethod]
		private static void Setup()
		{
			ObjectMap<GameObject>.Add("activeSelf", obj => obj.activeSelf);
			ObjectMap<GameObject>.Add("activeInHierarchy", obj => obj.activeInHierarchy);
			ObjectMap<GameObject>.Add<bool>("SetActive", null, (obj, value) => obj.SetActive(value));
		}
	}
}

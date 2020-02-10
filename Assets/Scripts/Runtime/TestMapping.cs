using PiRhoSoft.Variables;
using UnityEngine;

public class TransformMap : ObjectMap<Transform>
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Map()
	{
		Add(nameof(Transform.position), o => o.position, (o, value) => o.position = value);
	}
}

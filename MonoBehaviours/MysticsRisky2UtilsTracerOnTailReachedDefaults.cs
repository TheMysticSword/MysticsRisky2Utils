using RoR2;
using UnityEngine;

namespace MysticsRisky2Utils.MonoBehaviours
{
	public class MysticsRisky2UtilsTracerOnTailReachedDefaults : MonoBehaviour
	{
		public Tracer tracer;
		public EventFunctions eventFunctions;
		public Transform[] transformsToUnparent;
		public bool destroySelf;

		public void Awake()
		{
			tracer = GetComponent<Tracer>();
			eventFunctions = GetComponent<EventFunctions>();
			if (tracer && eventFunctions)
			{
				tracer.onTailReachedDestination.AddListener(() =>
				{
					if (transformsToUnparent != null) foreach (Transform transform in transformsToUnparent)
					{
						eventFunctions.UnparentTransform(transform);
					}
					if (destroySelf) eventFunctions.DestroySelf();
				});
			}
		}
	}
}

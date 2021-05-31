using RoR2.Orbs;
using UnityEngine;

namespace MysticsRisky2Utils.MonoBehaviours
{
	public class MysticsRisky2UtilsOrbEffectOnArrivalDefaults : MonoBehaviour
	{
		public OrbEffect orbEffect;
		public Transform[] transformsToUnparentChildren;
		public MonoBehaviour[] componentsToEnable;

		public void Awake()
		{
			if (transformsToUnparentChildren == null) transformsToUnparentChildren = new Transform[] { };
			if (componentsToEnable == null) componentsToEnable = new MonoBehaviour[] { };
			if (orbEffect)
			{
				orbEffect.onArrival.AddListener(() =>
				{
					foreach (Transform transform in transformsToUnparentChildren)
					{
						transform.DetachChildren();
					}
					foreach (MonoBehaviour monoBehaviour in componentsToEnable)
                    {
						monoBehaviour.enabled = true;
					}
				});
			}
		}
	}
}

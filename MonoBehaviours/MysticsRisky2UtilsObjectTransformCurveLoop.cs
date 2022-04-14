using RoR2;
using UnityEngine;

namespace MysticsRisky2Utils.MonoBehaviours
{
    public class MysticsRisky2UtilsObjectTransformCurveLoop : MonoBehaviour
    {
        public ObjectTransformCurve objectTransformCurve;

        public void Awake()
        {
            objectTransformCurve = GetComponent<ObjectTransformCurve>();
        }

        public void LateUpdate()
        {
            if (objectTransformCurve.time >= objectTransformCurve.timeMax)
            {
                objectTransformCurve.time = objectTransformCurve.time % objectTransformCurve.timeMax;
            }
        }
    }
}

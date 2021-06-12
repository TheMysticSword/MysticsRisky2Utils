using RoR2;
using UnityEngine;

namespace MysticsRisky2Utils.MonoBehaviours
{
	public class MysticsRisky2UtilsLineWidthOverTime : MonoBehaviour
	{
		public LineRenderer lineRenderer;
		public AnimationCurve animationCurve;
		public float age = 0f;
		public float maxDuration = 1f;

		public void Awake()
		{
			lineRenderer = GetComponent<LineRenderer>();
		}

		public void Update()
        {
			age += Time.deltaTime;
			if (lineRenderer && animationCurve != null)
            {
				lineRenderer.widthMultiplier = animationCurve.Evaluate(age / maxDuration);
			}
        }
	}
}

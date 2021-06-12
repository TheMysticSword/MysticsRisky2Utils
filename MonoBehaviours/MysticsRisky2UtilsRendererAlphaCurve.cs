using RoR2;
using UnityEngine;

namespace MysticsRisky2Utils.MonoBehaviours
{
	public class MysticsRisky2UtilsRendererAlphaCurve : MonoBehaviour
	{
		public Renderer renderer;
		public MaterialPropertyBlock materialPropertyBlock;
		public AnimationCurve animationCurve;
		public float age = 0f;
		public float maxDuration = 1f;
		public string colorPropertyName = "_Color";

		public void Awake()
		{
			renderer = GetComponent<Renderer>();
		}

		public void Update()
        {
			age += Time.deltaTime;
			if (renderer && animationCurve != null)
            {
				renderer.GetPropertyBlock(materialPropertyBlock);
				Color color = materialPropertyBlock.GetColor(colorPropertyName);
				color.a = animationCurve.Evaluate(age / maxDuration);
				materialPropertyBlock.SetColor(colorPropertyName, color);
				renderer.SetPropertyBlock(materialPropertyBlock);
			}
        }
	}
}

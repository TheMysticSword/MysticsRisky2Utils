using TMPro;
using UnityEngine;
using RoR2.UI;

namespace MysticsRisky2Utils.MonoBehaviours
{
    public class MysticsRisky2UtilsParticleSystemToggle : MonoBehaviour
    {
        public ParticleSystem[] particleSystemsToToggle;

        public void Start()
        {
            if (particleSystemsToToggle == null) particleSystemsToToggle = new ParticleSystem[] { };
        }

        public void OnEnable()
        {
            foreach (ParticleSystem particleSystem in particleSystemsToToggle)
            {
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
            }
        }

        public void OnDisable()
        {
            foreach (ParticleSystem particleSystem in particleSystemsToToggle)
            {
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = false;
            }
        }
    }
}

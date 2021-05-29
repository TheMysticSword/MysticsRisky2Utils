using TMPro;
using UnityEngine;
using RoR2.UI;

namespace MysticsRisky2Utils.MonoBehaviours
{
    public class MysticsRisky2UtilsClothReenabler : MonoBehaviour
    {
        public Cloth[] clothToReenable;
        public Vector3 lastScale;

        public void Start()
        {
            if (clothToReenable == null) clothToReenable = new Cloth[] { };
        }

        public void FixedUpdate()
        {
            if (lastScale != transform.localScale)
            {
                foreach (Cloth cloth in clothToReenable)
                {
                    cloth.enabled = false;
                    cloth.gameObject.SetActive(false);
                    cloth.gameObject.SetActive(true);
                    cloth.enabled = true;
                }
            }
            lastScale = transform.localScale;
        }
    }
}

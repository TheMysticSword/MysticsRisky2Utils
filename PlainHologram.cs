using UnityEngine;
using TMPro;
using R2API;
using RoR2;

namespace MysticsRisky2Utils
{
    public static class PlainHologram
    {
        public static GameObject hologramContentPrefab; // for use with IHologramContentProvider.GetHologramContentPrefab()

        internal static void Init()
        {
            hologramContentPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CostHologramContent"), "MysticsRisky2UtilsPlainHologramContent", false);
            CostHologramContent costHologramContent = hologramContentPrefab.GetComponent<CostHologramContent>();
            MysticsRisky2UtilsPlainHologramContent plainHologramContent = hologramContentPrefab.AddComponent<MysticsRisky2UtilsPlainHologramContent>();
            plainHologramContent.targetTextMesh = costHologramContent.targetTextMesh;
            Object.Destroy(costHologramContent);
        }

        public class MysticsRisky2UtilsPlainHologramContent : MonoBehaviour
        {
            public void FixedUpdate()
            {
                if (targetTextMesh)
                {
                    targetTextMesh.SetText(text);
                    targetTextMesh.color = color;
                }
            }

            public string text;
            public Color color = Color.white;
            public TextMeshPro targetTextMesh;
        }
    }
}
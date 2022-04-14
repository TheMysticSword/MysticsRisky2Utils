using UnityEngine;
using TMPro;
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;

namespace MysticsRisky2Utils
{
    public static class PlainHologram
    {
        private static GameObject _hologramContentPrefab;
        public static GameObject hologramContentPrefab // for use with IHologramContentProvider.GetHologramContentPrefab()
        {
            get
            {
                if (!_hologramContentPrefab)
                {
                    _hologramContentPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/CostHologramContent.prefab").WaitForCompletion(), "MysticsRisky2UtilsPlainHologramContent", false);
                    CostHologramContent costHologramContent = hologramContentPrefab.GetComponent<CostHologramContent>();
                    MysticsRisky2UtilsPlainHologramContent plainHologramContent = hologramContentPrefab.AddComponent<MysticsRisky2UtilsPlainHologramContent>();
                    plainHologramContent.targetTextMesh = costHologramContent.targetTextMesh;
                    Object.Destroy(costHologramContent);
                }
                return _hologramContentPrefab;
            }
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
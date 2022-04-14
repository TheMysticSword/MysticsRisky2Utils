using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using System.Collections.Generic;
using RoR2;
using UnityEngine.AddressableAssets;

namespace MysticsRisky2Utils
{
    public static class Utils
    {
        internal static void Init()
        {
            NetworkingAPI.RegisterMessageType<SyncForceRecalculateStats>();
        }

        public static GameObject CreateBlankPrefab(string name = "GameObject", bool network = false)
        {
            GameObject gameObject = PrefabAPI.InstantiateClone(new GameObject(name), name, false);
            if (network)
            {
                gameObject.AddComponent<NetworkIdentity>();
                gameObject.AddComponent<NetworkHelper.MysticsRisky2UtilsNetworkHelper>();
                PrefabAPI.RegisterNetworkPrefab(gameObject);
            }
            return gameObject;
        }

        public static void CopyChildren(GameObject from, GameObject to, bool cloneFromThenDestroy = true)
        {
            string trueName = to.name;
            if (cloneFromThenDestroy) from = PrefabAPI.InstantiateClone(from, from.name + "Copy", false);

            Transform parent = to.transform.parent;

            int childCount = from.transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                from.transform.GetChild(0).SetParent(to.transform);
            }
            foreach (Component fromComponent in from.GetComponents<Component>())
            {
                System.Type componentType = fromComponent.GetType();

                Component toComponent = to.GetComponent(componentType);
                if (!toComponent) toComponent = to.AddComponent(componentType);

                bool isAnimator = typeof(Animator).IsAssignableFrom(fromComponent.GetType());
                bool animatorLogWarnings = false;

                if (isAnimator)
                {
                    Animator fromAnimator = (Animator)fromComponent;
                    Animator toAnimator = (Animator)toComponent;
                    animatorLogWarnings = fromAnimator.logWarnings;
                    fromAnimator.logWarnings = false;
                    toAnimator.logWarnings = false;
                }

                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
                foreach (PropertyInfo propertyInfo in componentType.GetProperties(flags))
                {
                    if (propertyInfo.CanWrite)
                    {
                        try
                        {
                            propertyInfo.SetValue(toComponent, propertyInfo.GetValue(fromComponent));
                        }
                        catch { }
                    }
                }
                foreach (FieldInfo fieldInfo in componentType.GetFields(flags))
                {
                    fieldInfo.SetValue(toComponent, fieldInfo.GetValue(fromComponent));
                }

                if (isAnimator)
                {
                    Animator fromAnimator = (Animator)fromComponent;
                    Animator toAnimator = (Animator)toComponent;
                    fromAnimator.logWarnings = animatorLogWarnings;
                    toAnimator.logWarnings = animatorLogWarnings;
                }
            }

            to.transform.SetParent(parent);
            to.name = trueName;
            to.layer = from.layer;

            if (cloneFromThenDestroy) Object.Destroy(from);
        }

        public static string TrimCloneFromString(string originalString)
        {
            if (originalString.EndsWith("(Clone)")) originalString = originalString.Remove(originalString.Length - "(Clone)".Length);
            return originalString;
        }

        public static void ForceRecalculateStats(RoR2.CharacterBody body)
        {
            body.RecalculateStats();
            if (NetworkServer.active) new SyncForceRecalculateStats(body.netId);
        }

        private class SyncForceRecalculateStats : INetMessage
        {
            NetworkInstanceId objID;

            public SyncForceRecalculateStats()
            {
            }

            public SyncForceRecalculateStats(NetworkInstanceId objID)
            {
                this.objID = objID;
            }

            public void Deserialize(NetworkReader reader)
            {
                objID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                GameObject obj = RoR2.Util.FindNetworkObject(objID);
                if (obj)
                {
                    RoR2.CharacterBody component = obj.GetComponent<RoR2.CharacterBody>();
                    if (component)
                    {
                        component.RecalculateStats();
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(objID);
            }
        }

        public enum ItemIconBackgroundType
        {
            Tier1,
            Tier2,
            Tier3,
            Boss,
            Equipment,
            Lunar,
            Survivor
        }

        public static Sprite AddItemIconBackgroundToSprite(Sprite originalSprite, ItemIconBackgroundType bgType)
        {
            Texture2D loadedOriginalTexture = originalSprite.texture;
            
            Texture2D originalTexture = new Texture2D(loadedOriginalTexture.width, loadedOriginalTexture.height, TextureFormat.ARGB32, false);
            Graphics.ConvertTexture(loadedOriginalTexture, originalTexture);
            RenderTexture renderTexture = RenderTexture.GetTemporary(originalTexture.width, originalTexture.height, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            RenderTexture.active = renderTexture;
            Graphics.Blit(originalTexture, renderTexture);
            originalTexture.ReadPixels(new Rect(0, 0, originalTexture.width, originalTexture.height), 0, 0);
            originalTexture.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            Sprite loadedBackground = null;
            switch (bgType)
            {
                case ItemIconBackgroundType.Tier1:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier1BGIcon.png").WaitForCompletion();
                    break;
                case ItemIconBackgroundType.Tier2:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier2BGIcon.png").WaitForCompletion();
                    break;
                case ItemIconBackgroundType.Tier3:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier3BGIcon.png").WaitForCompletion();
                    break;
                case ItemIconBackgroundType.Boss:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBossBGIcon.png").WaitForCompletion();
                    break;
                case ItemIconBackgroundType.Equipment:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texEquipmentBGIcon.png").WaitForCompletion();
                    break;
                case ItemIconBackgroundType.Lunar:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texLunarBGIcon.png").WaitForCompletion();
                    break;
                case ItemIconBackgroundType.Survivor:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texSurvivorBGIcon.png").WaitForCompletion();
                    break;
                default:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier1BGIcon.png").WaitForCompletion();
                    break;
            }

            Texture2D backgroundTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.ARGB32, false);
            Graphics.ConvertTexture(loadedBackground.texture, backgroundTexture);
            renderTexture = RenderTexture.GetTemporary(backgroundTexture.width, backgroundTexture.height, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            RenderTexture.active = renderTexture;
            Graphics.Blit(backgroundTexture, renderTexture);
            backgroundTexture.ReadPixels(new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), 0, 0);
            backgroundTexture.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, false);
            newTexture.wrapMode = originalTexture.wrapMode;
            newTexture.filterMode = originalTexture.filterMode;
            for (var x = 0; x < newTexture.width; x++)
                for (var y = 0; y < newTexture.height; y++)
                {
                    Color backgroundPixel = backgroundTexture.GetPixel(x, y);
                    Color originalPixel = originalTexture.GetPixel(x, y);
                    newTexture.SetPixel(x, y, new Color(
                        backgroundPixel.r * (1 - originalPixel.a) + originalPixel.r * originalPixel.a,
                        backgroundPixel.g * (1 - originalPixel.a) + originalPixel.g * originalPixel.a,
                        backgroundPixel.b * (1 - originalPixel.a) + originalPixel.b * originalPixel.a,
                        Mathf.Clamp01(backgroundPixel.a + originalPixel.a)
                    ));
                }
            newTexture.Apply();

            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f), 25f, 1u, SpriteMeshType.Tight);
            return newSprite;
        }

        public static string FormatStringByDict(string str, Dictionary<string, string> dict)
        {
            foreach (var kvp in dict) str = str.Replace("{" + kvp.Key + "}", kvp.Value);
            return str;
        }
    }
}
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MysticsRisky2Utils
{
    public static class CustomTempVFXManagement
    {
        public struct VFXInfo
        {
            public GameObject prefab;
            public string child;
            public System.Func<CharacterBody, bool> condition;
            public System.Func<CharacterBody, float> radius;
        }

        public static float DefaultRadiusCall(CharacterBody body) { return body.radius; }
        public static float DefaultBestFitRadiusCall(CharacterBody body) { return body.bestFitRadius; }

        public static List<VFXInfo> allVFX = new List<VFXInfo>();

        internal static void Init()
        {
            On.RoR2.CharacterBody.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<MysticsRisky2UtilsCharacterCustomTempVFXHolder>();
            };

            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (orig, self) =>
            {
                orig(self);
                if (MysticsRisky2UtilsCharacterCustomTempVFXHolder.bodyToVFXHolder.TryGetValue(self, out var component))
                {
                    foreach (var vfxInfo in allVFX)
                    {
                        var active = vfxInfo.condition(self);
                        var tempVFX = component.dictionary[vfxInfo.prefab];
                        if (active)
                        {
                            if (!tempVFX)
                            {
                                var gameObject = Object.Instantiate(vfxInfo.prefab, self.corePosition, Quaternion.identity);

                                tempVFX = gameObject.GetComponent<MysticsRisky2UtilsTempVFX>();
                                component.dictionary[vfxInfo.prefab] = tempVFX;
                                tempVFX.parentTransform = self.coreTransform;
                                tempVFX.visualState = MysticsRisky2UtilsTempVFX.VisualState.Enter;
                                tempVFX.healthComponent = self.healthComponent;
                                tempVFX.radius = vfxInfo.radius(self);

                                var localCameraEffect = gameObject.GetComponent<LocalCameraEffect>();
                                if (localCameraEffect) localCameraEffect.targetCharacter = self.gameObject;

                                if (!string.IsNullOrEmpty(vfxInfo.child))
                                {
                                    var modelLocator = self.modelLocator;
                                    if (modelLocator)
                                    {
                                        var modelTransform = modelLocator.modelTransform;
                                        if (modelTransform)
                                        {
                                            var childLocator = modelTransform.GetComponent<ChildLocator>();
                                            if (childLocator)
                                            {
                                                var transform = childLocator.FindChild(vfxInfo.child);
                                                if (transform)
                                                {
                                                    tempVFX.parentTransform = transform;
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else tempVFX.visualState = MysticsRisky2UtilsTempVFX.VisualState.Enter;
                        }
                        else
                        {
                            if (tempVFX) tempVFX.visualState = MysticsRisky2UtilsTempVFX.VisualState.Exit;
                        }
                    }
                }
            };
        }

        public class MysticsRisky2UtilsTempVFX : MonoBehaviour
        {
            public Transform parentTransform;
            public bool rotateWithParent = false;
            public HealthComponent healthComponent;
            public GameObject[] enterObjects = new GameObject[] { };
            public MonoBehaviour[] enterBehaviours = new MonoBehaviour[] { };
            public GameObject[] exitObjects = new GameObject[] { };
            public MonoBehaviour[] exitBehaviours = new MonoBehaviour[] { };
            public VisualState visualState;
            public VisualState prevVisualState;
            public float radius;
            private Transform cachedTransform;

            public void Awake()
            {
                cachedTransform = transform;
            }

            public void Start()
            {
                RebuildVisuals();
            }

            public void Update()
            {
                if (!healthComponent || !healthComponent.alive) visualState = VisualState.Exit;
                if (parentTransform)
                {
                    cachedTransform.position = parentTransform.position;
                    if (rotateWithParent) cachedTransform.rotation = parentTransform.rotation;
                    cachedTransform.localScale = Vector3.one * radius;
                    if (visualState != prevVisualState)
                    {
                        prevVisualState = visualState;
                        RebuildVisuals();
                    }
                }
                else
                {
                    Object.Destroy(gameObject);
                }
            }

            public void RebuildVisuals()
            {
                bool enterState = visualState == VisualState.Enter;
                foreach (var obj in enterObjects) obj.SetActive(enterState);
                foreach (var behaviour in enterBehaviours) behaviour.enabled = enterState;
                foreach (var obj in exitObjects) obj.SetActive(!enterState);
                foreach (var behaviour in exitBehaviours) behaviour.enabled = !enterState;
            }

            public enum VisualState
            {
                Enter,
                Exit
            }
        }

        public class MysticsRisky2UtilsCharacterCustomTempVFXHolder : MonoBehaviour
        {
            public CharacterBody characterBody;
            public Dictionary<GameObject, MysticsRisky2UtilsTempVFX> dictionary = new Dictionary<GameObject, MysticsRisky2UtilsTempVFX>();
            
            public void Awake()
            {
                characterBody = GetComponent<CharacterBody>();
                bodyToVFXHolder[characterBody] = this;

                foreach (var vfxInfo in allVFX) dictionary.Add(vfxInfo.prefab, default);
            }

            public void OnDestroy()
            {
                if (bodyToVFXHolder.ContainsKey(characterBody))
                    bodyToVFXHolder.Remove(characterBody);
            }

            public static Dictionary<CharacterBody, MysticsRisky2UtilsCharacterCustomTempVFXHolder> bodyToVFXHolder = new Dictionary<CharacterBody, MysticsRisky2UtilsCharacterCustomTempVFXHolder>();
        }
    }
}

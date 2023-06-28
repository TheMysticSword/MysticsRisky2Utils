using RoR2;
using R2API;
using R2API.Utils;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MysticsRisky2Utils.ContentManagement;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseItemLike : BaseLoadableAsset
    {
        /// <summary>
        /// Default item display prefab. Shortcut setter and getter for itemDisplayPrefabs["default"].
        /// </summary>
        public GameObject itemDisplayPrefab
        {
            get
            {
                if (itemDisplayPrefabs.ContainsKey("default")) return itemDisplayPrefabs["default"];
                return null;
            }
            set
            {
                if (itemDisplayPrefabs.ContainsKey("default")) itemDisplayPrefabs["default"] = value;
                else itemDisplayPrefabs.Add("default", value);
            }
        }
        /// <summary>
        /// Dictionary of named item display prefabs. Can be used for storing and retrieving multiple displays.
        /// </summary>
        public Dictionary<string, GameObject> itemDisplayPrefabs = new Dictionary<string, GameObject>();
        public static event System.Action onSetupIDRS;
        
        public GameObject PrepareModel(GameObject model)
        {
            model.AddComponent<MysticsRisky2UtilsItemFollowerVisualScaling>();

            // Automatically set up a ModelPanelParameters component if camera points are present
            Transform focusPoint = model.transform.Find("FocusPoint");
            if (focusPoint)
            {
                Transform cameraPosition = focusPoint.Find("CameraPosition");
                if (cameraPosition)
                {
                    ModelPanelParameters component = model.GetComponent<ModelPanelParameters>();
                    if (!component) component = model.AddComponent<ModelPanelParameters>();
                    component.focusPointTransform = focusPoint;
                    component.cameraPositionTransform = cameraPosition;
                }
            }

            // Apply HG shader
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null && material.shader.name == "Standard" && material.shader != HopooShaderToMaterial.Standard.shader)
                    {
                        HopooShaderToMaterial.Standard.Apply(material);
                        HopooShaderToMaterial.Standard.Gloss(material, 0.2f, 5f);
                        HopooShaderToMaterial.Standard.Emission(material, 0f);
                        HopooShaderToMaterial.Standard.Dither(material);
                        material.SetTexture("_EmTex", material.GetTexture("_MainTex"));
                    }
                }
            }

            return model;
        }

        public GameObject PrepareItemDisplayModel(GameObject itemDisplayModel)
        {
            // Add ItemDisplay component for dither, flash and other HG effect support
            ItemDisplay itemDisplay = itemDisplayModel.AddComponent<ItemDisplay>();
            List<CharacterModel.RendererInfo> rendererInfos = new List<CharacterModel.RendererInfo>();
            foreach (Renderer renderer in itemDisplayModel.GetComponentsInChildren<Renderer>())
            {
                CharacterModel.RendererInfo rendererInfo = new CharacterModel.RendererInfo
                {
                    renderer = renderer,
                    defaultMaterial = renderer.material
                };
                rendererInfos.Add(rendererInfo);
            }
            itemDisplay.rendererInfos = rendererInfos.ToArray();

            return itemDisplayModel;
        }

        public static Dictionary<string, Dictionary<object, List<ItemDisplayRule>>> perBodyDisplayRules = new Dictionary<string, Dictionary<object, List<ItemDisplayRule>>>();
        
        public virtual void AddDisplayRule(string bodyName, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            AddDisplayRule(bodyName, itemDisplayPrefab, childName, localPos, localAngles, localScale);
        }

        public virtual void AddDisplayRule(string bodyName, string childName, GameObject itemDisplayPrefab, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            AddDisplayRule(bodyName, itemDisplayPrefab, childName, localPos, localAngles, localScale);
        }

        public virtual void AddDisplayRule(string bodyName, GameObject itemDisplayPrefab, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            AddDisplayRule(bodyName, asset, itemDisplayPrefab, childName, localPos, localAngles, localScale);
        }

        public static void AddDisplayRule(string bodyName, object keyAsset, GameObject itemDisplayPrefab, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            Dictionary<object, List<ItemDisplayRule>> perItemDisplayRules;
            if (!perBodyDisplayRules.TryGetValue(bodyName, out perItemDisplayRules))
            {
                perItemDisplayRules = new Dictionary<object, List<ItemDisplayRule>>();
                perBodyDisplayRules[bodyName] = perItemDisplayRules;
            }
            List<ItemDisplayRule> displayRulesForThisItem;
            if (!perItemDisplayRules.TryGetValue(keyAsset, out displayRulesForThisItem))
            {
                displayRulesForThisItem = new List<ItemDisplayRule>();
                perItemDisplayRules[keyAsset] = displayRulesForThisItem;
            }
            displayRulesForThisItem.Add(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = itemDisplayPrefab,
                childName = childName,
                localPos = localPos,
                localAngles = localAngles,
                localScale = localScale
            });
        }

        public abstract UnlockableDef GetUnlockableDef();

        internal static void PostGameLoad()
        {
            if (onSetupIDRS != null) onSetupIDRS();
            if (BaseCharacterBody.onSetupIDRS != null) BaseCharacterBody.onSetupIDRS();
            var changedIDRS = new List<ItemDisplayRuleSet>();
            foreach (var kvp in perBodyDisplayRules)
            {
                var bodyName = kvp.Key;
                var bodyIndex = BodyCatalog.FindBodyIndex(bodyName);
                if (bodyIndex != BodyIndex.None)
                {
                    var bodyPrefab = BodyCatalog.GetBodyPrefab(bodyIndex);
                    var characterModel = bodyPrefab.GetComponentInChildren<CharacterModel>();
                    var idrs = characterModel.itemDisplayRuleSet;
                    if (idrs)
                    {
                        foreach (var displayRules in kvp.Value)
                        {
                            idrs.SetDisplayRuleGroup((Object)displayRules.Key, new DisplayRuleGroup { rules = displayRules.Value.ToArray() });
                        }
                        if (!changedIDRS.Contains(idrs))
                            changedIDRS.Add(idrs);
                    }
                }
            }
            foreach (var idrs in changedIDRS)
                idrs.GenerateRuntimeValues();
        }

        public abstract PickupIndex GetPickupIndex();

        public static StringBuilder globalStringBuilder = new StringBuilder();

        public class Reskinner : MonoBehaviour
        {
            public string defaultBodyName;

            public void Start()
            {
                MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
                if (meshRenderer)
                {
                    string bodyName = defaultBodyName;
                    int skinIndex = 0;
                    CharacterModel characterModel = GetComponentInParent<CharacterModel>();
                    if (characterModel)
                    {
                        CharacterBody body = characterModel.body;
                        if (body)
                        {
                            bodyName = body.name;
                            skinIndex = (int)body.skinIndex;
                        }
                    }
                    GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
                    if (bodyPrefab)
                    {
                        CharacterBody body = bodyPrefab.GetComponent<CharacterBody>();
                        if (body)
                        {
                            ModelSkinController modelSkinController = body.GetComponentInChildren<ModelSkinController>();
                            if (modelSkinController)
                            {
                                Material mat = GetBestMaterial(modelSkinController.skins[skinIndex].rendererInfos.ToList());
                                meshRenderer.material.shader = mat.shader;
                                meshRenderer.material.CopyPropertiesFromMaterial(mat);

                                ItemDisplay itemDisplay = GetComponent<ItemDisplay>();
                                if (itemDisplay)
                                {
                                    itemDisplay.rendererInfos[0].defaultMaterial = meshRenderer.material;
                                }
                            }
                        }
                    }
                }
            }

            public static Material GetBestMaterial(List<CharacterModel.RendererInfo> rendererInfos)
            {
                rendererInfos.Sort((x, y) => {
                    Shader shaderX = x.defaultMaterial.shader;
                    Shader shaderY = y.defaultMaterial.shader;
                    if (shaderX == HopooShaderToMaterial.Standard.shader && shaderY != HopooShaderToMaterial.Standard.shader) return -1;
                    if (shaderX.name.StartsWith("Hopoo Games/FX/") && !shaderY.name.StartsWith("Hopoo Games/FX/")) return 1;
                    return 0;
                });
                return rendererInfos.First().defaultMaterial;
            }
        }

        public class MysticsRisky2UtilsItemFollowerVisualScaling : MonoBehaviour
        {
            public List<GameObject> effectObjects = new List<GameObject>();

            public void Start()
            {
                foreach (GameObject effectObject in effectObjects)
                {
                    if (effectObject)
                    {
                        float scale = (effectObject.transform.lossyScale.x + effectObject.transform.lossyScale.y + effectObject.transform.lossyScale.z) / 3f;
                        float localScale = (effectObject.transform.localScale.x + effectObject.transform.localScale.y + effectObject.transform.localScale.z) / 3f;

                        foreach (ParticleSystem particleSystem in effectObject.GetComponents<ParticleSystem>())
                        {
                            ParticleSystem.MainModule main = particleSystem.main;
                            ParticleSystem.MinMaxCurve gravityModifier = main.gravityModifier;
                            gravityModifier.constant *= scale;
                            main.gravityModifier = gravityModifier;
                        }

                        foreach (Light light in effectObject.GetComponents<Light>())
                        {
                            light.range *= scale / localScale;
                        }
                    }
                }
            }
        }

        public void SetScalableChildEffect(GameObject model, string childName)
        {
            GameObject child = model.transform.Find(childName).gameObject;
            if (child)
            {
                SetScalableChildEffect(model, child);
            }
            else
            {
                MysticsRisky2UtilsPlugin.logger.LogError("Couldn't find child effect " + childName);
            }
        }

        public void SetScalableChildEffect(GameObject model, GameObject child)
        {
            List<GameObject> effectObjects = model.GetComponent<MysticsRisky2UtilsItemFollowerVisualScaling>().effectObjects;
            if (!effectObjects.Contains(child)) effectObjects.Add(child);
        }
    }
}

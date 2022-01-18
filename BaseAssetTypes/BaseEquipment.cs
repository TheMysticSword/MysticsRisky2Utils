using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseEquipment : BaseItemLike
    {
        public EquipmentDef equipmentDef;
        public static List<BaseEquipment> equipmentThatUsesTargetFinder = new List<BaseEquipment>();
        public TargetFinderType targetFinderType = TargetFinderType.None;
        public GameObject targetFinderVisualizerPrefab;
        /// <summary>
        /// Dictionary of all loaded equipment as instances of BaseEquipment. Keys are equal to BaseEquipment.equipmentDef.name field values.
        /// </summary>
        public static Dictionary<string, BaseEquipment> loadedDictionary = new Dictionary<string, BaseEquipment>();

        public enum TargetFinderType
        {
            None,
            Enemies,
            Friendlies,
            Custom
        }

        public override void Load()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            OnLoad();
            equipmentDef.AutoPopulateTokens();
            loadedDictionary.Add(equipmentDef.name, this);
            asset = equipmentDef;
        }

        public override PickupIndex GetPickupIndex()
        {
            return PickupCatalog.FindPickupIndex(equipmentDef.equipmentIndex);
        }

        public virtual bool OnUse(EquipmentSlot equipmentSlot) { return false; }

        public virtual void OnUseClient(EquipmentSlot equipmentSlot) { }

        public void UseTargetFinder(TargetFinderType type, GameObject visualizerPrefab = null)
        {
            targetFinderType = type;
            targetFinderVisualizerPrefab = visualizerPrefab;
            equipmentThatUsesTargetFinder.Add(this);
        }

        internal static void Init()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipmentDef2) =>
            {
                if (NetworkServer.active)
                {
                    BaseEquipment equipment = loadedDictionary.Values.FirstOrDefault(x => x.equipmentDef == equipmentDef2);
                    if (equipment != null)
                    {
                        return equipment.OnUse(self);
                    }
                }
                return orig(self, equipmentDef2);
            };

            On.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += (orig, self) =>
            {
                orig(self);
                EquipmentIndex equipmentIndex2 = self.equipmentIndex;
                BaseEquipment equipment = loadedDictionary.Values.FirstOrDefault(x => x.equipmentDef.equipmentIndex == equipmentIndex2);
                if (equipment != null)
                {
                    equipment.OnUseClient(self);
                }
            };

            On.RoR2.EquipmentSlot.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<MysticsRisky2UtilsEquipmentTarget>();
            };

            On.RoR2.EquipmentSlot.Update += (orig, self) =>
            {
                orig(self);

                MysticsRisky2UtilsEquipmentTarget targetInfo = self.GetComponent<MysticsRisky2UtilsEquipmentTarget>();
                if (targetInfo)
                {
                    BaseEquipment equipment = equipmentThatUsesTargetFinder.FirstOrDefault(x => x.equipmentDef.equipmentIndex == self.equipmentIndex);
                    if (equipment != null)
                    {
                        if (equipment.equipmentDef.equipmentIndex == self.equipmentIndex)
                        {
                            if (equipment.targetFinderType != TargetFinderType.Custom)
                            {
                                if (self.stock > 0)
                                {
                                    switch (equipment.targetFinderType)
                                    {
                                        case TargetFinderType.Enemies:
                                            targetInfo.ConfigureTargetFinderForEnemies(self);
                                            break;
                                        case TargetFinderType.Friendlies:
                                            targetInfo.ConfigureTargetFinderForFriendlies(self);
                                            break;
                                    }
                                    HurtBox hurtBox = targetInfo.targetFinder.GetResults().FirstOrDefault();
                                    if (hurtBox)
                                    {
                                        targetInfo.obj = hurtBox.healthComponent.gameObject;
                                        targetInfo.indicator.visualizerPrefab = equipment.targetFinderVisualizerPrefab;
                                        targetInfo.indicator.targetTransform = hurtBox.transform;
                                    }
                                    else
                                    {
                                        targetInfo.Invalidate();
                                    }
                                    targetInfo.indicator.active = hurtBox;
                                }
                                else
                                {
                                    targetInfo.Invalidate();
                                    targetInfo.indicator.active = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        targetInfo.Invalidate();
                        targetInfo.indicator.active = false;
                    }
                }
            };
        }

        public override UnlockableDef GetUnlockableDef()
        {
            if (!equipmentDef.unlockableDef)
            {
                equipmentDef.unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
                equipmentDef.unlockableDef.cachedName = "Equipment." + equipmentDef.name;
                equipmentDef.unlockableDef.nameToken = ("EQUIPMENT_" + equipmentDef.name + "_NAME").ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            }
            return equipmentDef.unlockableDef;
        }

        public class MysticsRisky2UtilsEquipmentTarget : MonoBehaviour
        {
            public GameObject obj;
            public Indicator indicator;
            public BullseyeSearch targetFinder;
            public object customTargetFinder;

            public void Awake()
            {
                indicator = new Indicator(gameObject, null);
            }

            public void Invalidate()
            {
                obj = null;
                indicator.targetTransform = null;
            }

            public void ConfigureTargetFinderBase(EquipmentSlot self)
            {
                if (targetFinder == null) targetFinder = new BullseyeSearch();
                targetFinder.teamMaskFilter = TeamMask.allButNeutral;
                targetFinder.teamMaskFilter.RemoveTeam(self.characterBody.teamComponent.teamIndex);
                targetFinder.sortMode = BullseyeSearch.SortMode.Angle;
                targetFinder.filterByLoS = true;
                float num;
                Ray ray = CameraRigController.ModifyAimRayIfApplicable(self.GetAimRay(), self.gameObject, out num);
                targetFinder.searchOrigin = ray.origin;
                targetFinder.searchDirection = ray.direction;
                targetFinder.maxAngleFilter = 10f;
                targetFinder.viewer = self.characterBody;
            }

            public void ConfigureTargetFinderForEnemies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.characterBody.teamComponent.teamIndex);
                targetFinder.RefreshCandidates();
                targetFinder.FilterOutGameObject(self.gameObject);
            }

            public void ConfigureTargetFinderForFriendlies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                targetFinder.teamMaskFilter = TeamMask.none;
                targetFinder.teamMaskFilter.AddTeam(self.characterBody.teamComponent.teamIndex);
                targetFinder.RefreshCandidates();
                targetFinder.FilterOutGameObject(self.gameObject);
            }

            public T GetCustomTargetFinder<T>() where T : class, new()
            {
                if (customTargetFinder != null && customTargetFinder as T != null)
                {
                    return (T)customTargetFinder;
                }
                return new T();
            }
        }
    }
}

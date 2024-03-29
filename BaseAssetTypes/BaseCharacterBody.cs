using RoR2;
using RoR2.Navigation;
using RoR2.Networking;
using EntityStates;
using R2API;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MysticsRisky2Utils.ContentManagement;
using UnityEngine.AddressableAssets;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseCharacterBody : BaseLoadableAsset
    {
        public GameObject prefab;
        public ModelLocator modelLocator;
        public Transform modelBaseTransform;
        public Transform modelTransform;
        public GameObject meshObject;
        public string bodyName;

        public override void Load()
        {
            OnLoad();
            asset = prefab;
        }

        public void Prepare()
        {
            SkillLocator skillLocator = prefab.AddComponent<SkillLocator>();
            TeamComponent teamComponent = prefab.AddComponent<TeamComponent>();
            CharacterBody characterBody = prefab.AddComponent<CharacterBody>();
            characterBody.baseNameToken = bodyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture) + "_BODY_NAME";
            characterBody.subtitleNameToken = bodyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture) + "_BODY_SUBTITLE";
            HealthComponent healthComponent = prefab.AddComponent<HealthComponent>();
            InputBankTest inputBank = prefab.AddComponent<InputBankTest>();
            InteractionDriver interactionDriver = prefab.AddComponent<InteractionDriver>();
            CharacterDeathBehavior characterDeathBehavior = prefab.AddComponent<CharacterDeathBehavior>();
            EquipmentSlot equipmentSlot = prefab.AddComponent<EquipmentSlot>();

            CharacterNetworkTransform characterNetworkTransform = prefab.AddComponent<CharacterNetworkTransform>();
            characterNetworkTransform.positionTransmitInterval = 0.1f;
            characterNetworkTransform.interpolationFactor = 2f;

            CameraTargetParams cameraTargetParams = prefab.AddComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = ccpStandard;

            Interactor interactor = prefab.AddComponent<Interactor>();
            interactor.maxInteractionDistance = 3f;

            modelLocator = prefab.AddComponent<ModelLocator>();
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;
            modelLocator.modelBaseTransform = modelBaseTransform;
            modelLocator.modelTransform = modelTransform;

            AimAnimator aimAnimator = modelTransform.gameObject.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBank;

            CharacterModel characterModel = modelTransform.gameObject.AddComponent<CharacterModel>();
            characterModel.body = characterBody;
            ItemDisplayRuleSet idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            idrs.name = "idrs" + bodyName;
            characterModel.itemDisplayRuleSet = idrs;

            // Apply HG shader
            foreach (Renderer renderer in meshObject.GetComponentsInChildren<Renderer>())
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null && material.shader.name == "Standard" && material.shader != HopooShaderToMaterial.Standard.shader)
                    {
                        HopooShaderToMaterial.Standard.Apply(material);
                        HopooShaderToMaterial.Standard.Dither(material);
                    }
                }
            }

            NetworkStateMachine networkStateMachine = prefab.AddComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = new EntityStateMachine[] { };
        }

        public void Dither(Collider bounds, Renderer[] renderers)
        {
            DitherModel ditherModel = prefab.AddComponent<DitherModel>();
            ditherModel.bounds = bounds;
            ditherModel.renderers = renderers;
        }

        public void Dither()
        {
            Dither(meshObject.GetComponent<Collider>(), new Renderer[] { meshObject.GetComponent<Renderer>() });
        }

        // todo: SetUpKinematicMotor

        public void SetUpRigidbodyMotor()
        {
            RigidbodyDirection rigidbodyDirection = prefab.AddComponent<RigidbodyDirection>();
            rigidbodyDirection.rigid = prefab.GetComponent<Rigidbody>();

            QuaternionPID angularVelocityPID = prefab.AddComponent<QuaternionPID>();
            angularVelocityPID.customName = "Angular Velocity PID";
            angularVelocityPID.PID = new Vector3(5.0f, 0.1f, 0.0f);
            angularVelocityPID.gain = 2f;

            VectorPID torquePID = prefab.AddComponent<VectorPID>();
            torquePID.customName = "Torque PID";
            torquePID.PID = new Vector3(5.0f, 0.1f, 0.0f);
            torquePID.isAngle = true;
            torquePID.gain = 2f;

            rigidbodyDirection.angularVelocityPID = angularVelocityPID;
            rigidbodyDirection.torquePID = torquePID;

            VectorPID forcePID = prefab.AddComponent<VectorPID>();
            forcePID.customName = "Force PID";
            forcePID.PID = new Vector3(3.0f, 0.0f, 0.0f);
            forcePID.isAngle = false;
            forcePID.gain = 1f;

            RigidbodyMotor rigidbodyMotor = prefab.AddComponent<RigidbodyMotor>();
            rigidbodyMotor.rigid = prefab.GetComponent<Rigidbody>();
            rigidbodyMotor.forcePID = forcePID;
            rigidbodyMotor.centerOfMassOffset = Vector3.zero;
            rigidbodyMotor.enableOverrideMoveVectorInLocalSpace = false;
            rigidbodyMotor.canTakeImpactDamage = true;
        }

        public EntityStateMachine SetUpEntityStateMachine(string customName, System.Type initialStateType, System.Type mainStateType)
        {
            EntityStateMachine newStateMachine = prefab.AddComponent<EntityStateMachine>();
            newStateMachine.customName = customName;
            newStateMachine.initialStateType = new SerializableEntityStateType(initialStateType);
            newStateMachine.mainStateType = new SerializableEntityStateType(mainStateType);
            HG.ArrayUtils.ArrayAppend(ref prefab.GetComponent<NetworkStateMachine>().stateMachines, newStateMachine);
            return newStateMachine;
        }

        public class HurtBoxSetUpInfo
        {
            public Transform transform;
            public bool isBullseye;
            public bool isMain;
            public bool isSniperTarget;
        }

        public void SetUpHurtBoxGroup(HurtBoxSetUpInfo[] hurtBoxSetUpInfos)
        {
            HurtBoxGroup hurtBoxGroup = modelTransform.gameObject.AddComponent<HurtBoxGroup>();
            List<HurtBox> hurtBoxes = new List<HurtBox>();

            HealthComponent healthComponent = prefab.GetComponent<HealthComponent>();

            short indexInGroup = 0;
            int bullseyeCount = 0;

            foreach (HurtBoxSetUpInfo hurtBoxSetUpInfo in hurtBoxSetUpInfos)
            {
                HurtBox hurtBox = hurtBoxSetUpInfo.transform.gameObject.AddComponent<HurtBox>();
                hurtBox.healthComponent = healthComponent;
                hurtBox.isBullseye = hurtBoxSetUpInfo.isBullseye;
                hurtBox.isSniperTarget = hurtBoxSetUpInfo.isSniperTarget;
                hurtBox.damageModifier = HurtBox.DamageModifier.Normal;
                hurtBox.hurtBoxGroup = hurtBoxGroup;
                hurtBox.indexInGroup = indexInGroup;
                indexInGroup++;
                if (hurtBoxSetUpInfo.isBullseye) bullseyeCount++;
                if (hurtBoxSetUpInfo.isMain) hurtBoxGroup.mainHurtBox = hurtBox;
                hurtBoxes.Add(hurtBox);
            }
            hurtBoxGroup.hurtBoxes = hurtBoxes.ToArray();
            hurtBoxGroup.bullseyeCount = bullseyeCount;
        }

        public void AfterCharacterBodySetup()
        {
            CharacterBody characterBody = prefab.GetComponent<CharacterBody>();
            characterBody.PerformAutoCalculateLevelStats();
        }

        public void AfterCharacterModelSetUp()
        {
            CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
            for (int i = 0; i < characterModel.baseLightInfos.Length; i++)
            {
                ref CharacterModel.LightInfo ptr = ref characterModel.baseLightInfos[i];
                if (ptr.light)
                {
                    ptr.defaultColor = ptr.light.color;
                }
            }
            if (characterModel.autoPopulateLightInfos)
            {
                CharacterModel.LightInfo[] first = (from light in characterModel.GetComponentsInChildren<Light>()
                                                    select new CharacterModel.LightInfo(light)).ToArray();
                if (!first.SequenceEqual(characterModel.baseLightInfos))
                {
                    characterModel.baseLightInfos = first;
                }
            }
        }

        public void SetUpChildLocator(ChildLocator.NameTransformPair[] transformPairs)
        {
            ChildLocator childLocator = modelTransform.gameObject.AddComponent<ChildLocator>();
            childLocator.transformPairs = transformPairs;
        }

        public void SetUpPingInfo(Sprite pingIconOverride)
        {
            PingInfoProvider pingInfoProvider = prefab.AddComponent<PingInfoProvider>();
            pingInfoProvider.pingIconOverride = pingIconOverride;
        }

        private static CharacterCameraParams _ccpStandard;
        public static CharacterCameraParams ccpStandard
        {
            get
            {
                if (!_ccpStandard)
                {
                    GameObject commandoBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
                    _ccpStandard = commandoBody.GetComponent<CameraTargetParams>().cameraParams;
                }
                return _ccpStandard;
            }
        }

        public static System.Action onSetupIDRS;

        public void AddDisplayRule(object keyAsset, GameObject itemDisplayPrefab, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            BaseItemLike.AddDisplayRule(bodyName + "Body", keyAsset, itemDisplayPrefab, childName, localPos, localAngles, localScale);
        }
    }
}

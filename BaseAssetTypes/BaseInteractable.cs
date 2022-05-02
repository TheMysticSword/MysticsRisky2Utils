using RoR2;
using RoR2.Hologram;
using RoR2.Navigation;
using EntityStates;
using R2API;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MysticsRisky2Utils.ContentManagement;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseInteractable : BaseLoadableAsset
    {
        public GameObject prefab;
        public ModelLocator modelLocator;
        public Transform modelBaseTransform;
        public Transform modelTransform;
        public GameObject meshObject;
        public string genericDisplayNameToken;
        public InteractableSpawnCard spawnCard;

        public override void Load()
        {
            OnLoad();
            keyToInteractable.Add(GetType().Name, this);
            asset = prefab;
        }

        public void Prepare()
        {
            modelLocator = prefab.AddComponent<ModelLocator>();
            modelLocator.dontDetatchFromParent = true;
            modelLocator.modelBaseTransform = modelBaseTransform;
            modelLocator.modelTransform = modelTransform;
            modelLocator.normalizeToFloor = false;

            Highlight highlight = prefab.AddComponent<Highlight>();
            highlight.targetRenderer = meshObject.GetComponent<Renderer>();
            highlight.strength = 1f;
            highlight.highlightColor = Highlight.HighlightColor.interactive;

            prefab.AddComponent<GenericDisplayNameProvider>().displayToken = genericDisplayNameToken;

            EntityLocator entityLocator = meshObject.AddComponent<EntityLocator>();
            entityLocator.entity = prefab;

            Transform hologramPivot = prefab.transform.Find("HologramPivot");
            if (hologramPivot)
            {
                HologramProjector hologramProjector = prefab.AddComponent<HologramProjector>();
                hologramProjector.displayDistance = 15f;
                hologramProjector.hologramPivot = hologramPivot;
                hologramProjector.disableHologramRotation = false;
            }

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

            spawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            spawnCard.name = "isc" + GetType().Name;
            spawnCard.prefab = prefab;
            spawnCard.sendOverNetwork = true;
            spawnCard.hullSize = HullClassification.Human;
            spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
            spawnCard.requiredFlags = NodeFlags.None;
            spawnCard.forbiddenFlags = NodeFlags.None;
            spawnCard.directorCreditCost = 30;
            spawnCard.occupyPosition = true;
            spawnCard.orientToFloor = true;
            spawnCard.slightlyRandomizeOrientation = true;
            spawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;
            interactableSpawnCards.Add(spawnCard);
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

        public class MysticsRisky2UtilsPurchasableInteractable : MonoBehaviour
        {
            public string interactableName;
            public PurchaseInteraction purchaseInteraction;
            public EntityStateMachine entityStateMachine;

            public void Awake()
            {
                purchaseInteraction = GetComponent<PurchaseInteraction>();
                entityStateMachine = GetComponent<EntityStateMachine>();

                if (purchaseInteraction)
                {
                    if (!string.IsNullOrEmpty(interactableName) && keyToInteractable.ContainsKey(interactableName))
                    {
                        BaseInteractable baseInteractable = keyToInteractable[interactableName];
                        if (baseInteractable.onPurchase != null)
                        {
                            purchaseInteraction.onPurchase.AddListener((interactor) => baseInteractable.onPurchase(gameObject, interactor, this));
                        }
                    }
                }
            }
        }

        public void SetPurchasable()
        {
            MysticsRisky2UtilsPurchasableInteractable component = prefab.AddComponent<MysticsRisky2UtilsPurchasableInteractable>();
            component.interactableName = GetType().Name;
        }

        public event System.Action<GameObject, Interactor, MysticsRisky2UtilsPurchasableInteractable> onPurchase;

        public static void AddDirectorCardTo(string sceneName, string categoryName, DirectorCard directorCard)
        {
            Dictionary<string, List<DirectorCard>> categoryCards;
            if (sceneCategoryCards.ContainsKey(sceneName)) categoryCards = sceneCategoryCards[sceneName];
            else
            {
                categoryCards = new Dictionary<string, List<DirectorCard>>();
                sceneCategoryCards.Add(sceneName, categoryCards);
            }

            List<DirectorCard> cards;
            if (categoryCards.ContainsKey(categoryName)) cards = categoryCards[categoryName];
            else
            {
                cards = new List<DirectorCard>();
                categoryCards.Add(categoryName, cards);
            }

            cards.Add(directorCard);
        }

        public void SetUpEntityStateMachine()
        {
            EntityStateMachine entityStateMachine = prefab.AddComponent<EntityStateMachine>();
            entityStateMachine.initialStateType = entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(Idle));
            NetworkStateMachine networkStateMachine = prefab.AddComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = new EntityStateMachine[] {
                entityStateMachine
            };
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

        public static StringBuilder globalStringBuilder = new StringBuilder();
        public static Dictionary<string, BaseInteractable> keyToInteractable = new Dictionary<string, BaseInteractable>();
        public static Dictionary<string, Dictionary<string, List<DirectorCard>>> sceneCategoryCards = new Dictionary<string, Dictionary<string, List<DirectorCard>>>();
        public static List<InteractableSpawnCard> interactableSpawnCards = new List<InteractableSpawnCard>();

        internal static void Init()
        {
            SceneDirector.onGenerateInteractableCardSelection += (sceneDirector, dccs) =>
            {
                SceneInfo sceneInfo = SceneInfo.instance;
                if (sceneInfo)
                {
                    SceneDef sceneDef = sceneInfo.sceneDef;
                    if (sceneDef && sceneCategoryCards.ContainsKey(sceneDef.baseSceneName))
                    {
                        Dictionary<string, List<DirectorCard>> categoryCards = sceneCategoryCards[sceneDef.baseSceneName];
                        DirectorCardCategorySelection.Category[] categories = dccs.categories;
                        if (categories != null)
                        {
                            for (int i = 0; i < dccs.categories.Length; i++)
                            {
                                DirectorCardCategorySelection.Category category = dccs.categories[i];
                                if (categoryCards.ContainsKey(category.name))
                                {
                                    foreach (DirectorCard directorCard in categoryCards[category.name]) {
                                        dccs.AddCard(i, directorCard);
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}

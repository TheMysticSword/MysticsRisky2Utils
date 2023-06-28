using MysticsRisky2Utils.ContentManagement;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using UnityEngine;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseCharacterMaster : BaseLoadableAsset
    {
        public GameObject prefab;
        public CharacterSpawnCard spawnCard;
        public string masterName;

        public override void Load()
        {
            OnLoad();
            asset = prefab;
        }

        public void Prepare()
        {
            Inventory inventory = prefab.AddComponent<Inventory>();
            MinionOwnership minionOwnership = prefab.AddComponent<MinionOwnership>();

            CharacterMaster characterMaster = prefab.AddComponent<CharacterMaster>();
            characterMaster.spawnOnStart = false;
            characterMaster.destroyOnBodyDeath = true;
            characterMaster.isBoss = false;
            characterMaster.preventGameOver = true;

            spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
            spawnCard.name = "csc" + masterName;
            spawnCard.prefab = prefab;
            spawnCard.sendOverNetwork = true;
            spawnCard.hullSize = HullClassification.Human;
            spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
            spawnCard.requiredFlags = NodeFlags.None;
            spawnCard.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            spawnCard.directorCreditCost = 0;
            spawnCard.occupyPosition = true;
            spawnCard.eliteRules = SpawnCard.EliteRules.Default;
            spawnCard.noElites = false;
            spawnCard.forbiddenAsBoss = false;
            characterSpawnCards[masterName] = spawnCard;
        }

        public void AddDirectorCardTo(string sceneName, string categoryName, DirectorCard directorCard)
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

        public static Dictionary<string, Dictionary<string, List<DirectorCard>>> sceneCategoryCards = new Dictionary<string, Dictionary<string, List<DirectorCard>>>();
        public static Dictionary<string, CharacterSpawnCard> characterSpawnCards = new Dictionary<string, CharacterSpawnCard>();

        internal static void Init()
        {
            On.RoR2.ClassicStageInfo.Awake += ClassicStageInfo_Awake;
        }

        private static void ClassicStageInfo_Awake(On.RoR2.ClassicStageInfo.orig_Awake orig, ClassicStageInfo self)
        {
            orig(self);
            if (self.monsterCategories)
            {
                SceneInfo sceneInfo = self.GetComponent<SceneInfo>();
                SceneDef sceneDef = sceneInfo.sceneDef;
                if (sceneDef)
                {
                    if (sceneCategoryCards.ContainsKey(sceneDef.baseSceneName))
                    {
                        Dictionary<string, List<DirectorCard>> categoryCards = sceneCategoryCards[sceneDef.baseSceneName];
                        DirectorCardCategorySelection dccs = self.monsterCategories;
                        if (dccs != null)
                        {
                            for (int i = 0; i < dccs.categories.Length; i++)
                            {
                                DirectorCardCategorySelection.Category category = dccs.categories[i];
                                if (categoryCards.ContainsKey(category.name))
                                {
                                    foreach (DirectorCard directorCard in categoryCards[category.name])
                                    {
                                        dccs.AddCard(i, directorCard);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

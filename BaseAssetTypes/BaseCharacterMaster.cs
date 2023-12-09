using Mono.Cecil.Cil;
using MonoMod.Cil;
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
            IL.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;
        }

        private static void ClassicStageInfo_RebuildCards(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchStfld<ClassicStageInfo>("modifiableMonsterCategories")
            ))
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<System.Action<ClassicStageInfo>>((classicStageInfo) =>
                {
                    var dccs = classicStageInfo.modifiableMonsterCategories;
                    if (!(dccs is FamilyDirectorCardCategorySelection))
                    {
                        var sceneInfo = classicStageInfo.GetComponent<SceneInfo>();
                        var sceneDef = sceneInfo.sceneDef;
                        if (sceneDef)
                        {
                            if (sceneCategoryCards.ContainsKey(sceneDef.baseSceneName))
                            {
                                var categoryCards = sceneCategoryCards[sceneDef.baseSceneName];
                                for (var i = 0; i < dccs.categories.Length; i++)
                                {
                                    var category = dccs.categories[i];
                                    if (categoryCards.ContainsKey(category.name))
                                    {
                                        foreach (var directorCard in categoryCards[category.name])
                                        {
                                            dccs.AddCard(i, directorCard);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
            else
            {
                MysticsRisky2UtilsPlugin.logger.LogError("Failed to hook ClassicStageInfo.RebuildCards. Custom enemies will not spawn naturally!");
            }
        }
    }
}

using RoR2;
using R2API;
using R2API.Utils;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RoR2.UI;
using System.Reflection;

namespace MysticsRisky2Utils
{
    public static class DebugTools
    {
        internal static System.Type declaringType;

        public static bool enabled = false;
        public static void Enable()
        {
            if (!enabled)
            {
                enabled = true;
                MysticsRisky2UtilsPlugin.logger.LogWarning("Debug tools enabled by " + Assembly.GetCallingAssembly().FullName);
                declaringType = MethodBase.GetCurrentMethod().DeclaringType;

                ConCommandHelper.Load(declaringType.GetMethod("CCSpectator", MysticsRisky2UtilsPlugin.bindingFlagAll));
                ConCommandHelper.Load(declaringType.GetMethod("CCGiveMonstersItem", MysticsRisky2UtilsPlugin.bindingFlagAll));
                ConCommandHelper.Load(declaringType.GetMethod("CCClearMonsterItems", MysticsRisky2UtilsPlugin.bindingFlagAll));
                ConCommandHelper.Load(declaringType.GetMethod("CCNotification", MysticsRisky2UtilsPlugin.bindingFlagAll));
                ConCommandHelper.Load(declaringType.GetMethod("CCSpawnInteractable", MysticsRisky2UtilsPlugin.bindingFlagAll));
                ConCommandHelper.Load(declaringType.GetMethod("CCSpawnAI", MysticsRisky2UtilsPlugin.bindingFlagAll));

                On.RoR2.CharacterBody.Awake += (orig, self) =>
                {
                    orig(self);
                    self.gameObject.AddComponent<MysticsRisky2UtilsDebugToolsBodyFields>();
                };

                On.RoR2.GenericPickupController.AttemptGrant += (orig, self, body) =>
                {
                    MysticsRisky2UtilsDebugToolsBodyFields component = body.GetComponent<MysticsRisky2UtilsDebugToolsBodyFields>();
                    if (component && component.spectating) return;
                    orig(self, body);
                };
                On.RoR2.GenericPickupController.GetInteractability += (orig, self, interactor) =>
                {
                    CharacterBody body = interactor.GetComponent<CharacterBody>();
                    if (body)
                    {
                        MysticsRisky2UtilsDebugToolsBodyFields component = body.GetComponent<MysticsRisky2UtilsDebugToolsBodyFields>();
                        if (component && component.spectating) return Interactability.Disabled;
                    }
                    return orig(self, interactor);
                };

                CharacterMaster.onStartGlobal += (master) =>
                {
                    if (master.teamIndex == TeamIndex.Monster)
                        foreach (KeyValuePair<ItemIndex, int> keyValuePair in monsterItems)
                        {
                            master.inventory.GiveItem(keyValuePair.Key, keyValuePair.Value);
                        }
                };

                LanguageAPI.Add("MYSTICSRISKY2UTILS_EMPTY_FORMAT", "{0}");
            }
        }

        public static ItemDef GetItem(string name)
        {
            name = name.ToLowerInvariant();
            ItemDef finalItemDef = null;
            foreach (ItemIndex itemIndex in ItemCatalog.allItems)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                if (Language.GetString(itemDef.nameToken).ToLowerInvariant().StartsWith(name) || itemDef.name.ToLowerInvariant().StartsWith(name))
                {
                    finalItemDef = itemDef;
                    break;
                }
            }
            return finalItemDef;
        }

        public const string ConCommandPrefix = "mr2u_";
        public static bool OnlineCheck()
        {
            return PlayerCharacterMasterController.instances.Count > 1;
        }

        public class MysticsRisky2UtilsDebugToolsBodyFields : MonoBehaviour
        {
            public bool spectating = false;
        }

        [ConCommand(commandName = ConCommandPrefix + "spectator", flags = ConVarFlags.ExecuteOnServer, helpText = "Become invisible and remove ability to pick items up")]
        public static void CCSpectator(ConCommandArgs args)
        {
            if (OnlineCheck()) return;
            MysticsRisky2UtilsDebugToolsBodyFields component = args.senderBody.GetComponent<MysticsRisky2UtilsDebugToolsBodyFields>();
            component.spectating = !component.spectating;
            args.senderBody.modelLocator.modelTransform.GetComponentInChildren<CharacterModel>().invisibilityCount += 999 * (component.spectating ? 1 : -1);
        }

        public static Dictionary<ItemIndex, int> monsterItems = new Dictionary<ItemIndex, int>();

        [ConCommand(commandName = ConCommandPrefix + "givemonsters", flags = ConVarFlags.ExecuteOnServer, helpText = "Give monsters an item")]
        public static void CCGiveMonstersItem(ConCommandArgs args)
        {
            int count = args.Count >= 2 ? int.Parse(args[1]) : 1;
            ItemDef itemDef = GetItem(args[0]);
            if (itemDef)
            {
                Debug.Log("Giving enemies " + count + " " + Language.GetString(itemDef.nameToken));
                if (monsterItems.ContainsKey(itemDef.itemIndex)) monsterItems[itemDef.itemIndex] += count;
                else monsterItems.Add(itemDef.itemIndex, count);
            }
        }

        [ConCommand(commandName = ConCommandPrefix + "resetmonsteritems", flags = ConVarFlags.ExecuteOnServer, helpText = "Reset monster items")]
        public static void CCClearMonsterItems(ConCommandArgs args)
        {
            monsterItems.Clear();
        }

        [ConCommand(commandName = ConCommandPrefix + "interactable", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn an interactable")]
        public static void CCSpawnInteractable(ConCommandArgs args)
        {
            InteractableSpawnCard interactableSpawnCard = Resources.LoadAll<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard").ToList().FirstOrDefault(x => x.name == args[0]);
            if (!interactableSpawnCard) interactableSpawnCard = BaseAssetTypes.BaseInteractable.interactableSpawnCards.ToList().FirstOrDefault(x => x.name == args[0]);
            if (interactableSpawnCard)
            {
                Debug.Log("Spawning " + interactableSpawnCard.name);
                if (args.senderBody)
                {
                    RaycastHit hitInfo;
                    if (args.senderBody.inputBank.GetAimRaycast(1000f, out hitInfo)) {
                        interactableSpawnCard.DoSpawn(hitInfo.point, Quaternion.identity, new DirectorSpawnRequest(
                            interactableSpawnCard,
                            new DirectorPlacementRule
                            {
                                placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                                maxDistance = 100f,
                                minDistance = 0f,
                                position = hitInfo.point,
                                preventOverhead = true
                            },
                            RoR2Application.rng
                        ));
                    }
                }
            }
        }

        [ConCommand(commandName = ConCommandPrefix + "ai", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn an AI controlled body")]
        public static void CCSpawnAI(ConCommandArgs args)
        {
            CharacterSpawnCard characterSpawnCard = Resources.LoadAll<CharacterSpawnCard>("SpawnCards/CharacterSpawnCard").ToList().FirstOrDefault(x => x.name == args[0]);
            if (!characterSpawnCard) characterSpawnCard = BaseAssetTypes.BaseCharacterMaster.characterSpawnCards.ToList().FirstOrDefault(x => x.name == args[0]);
            if (characterSpawnCard)
            {
                Debug.Log("Spawning " + characterSpawnCard.name);
                if (args.senderBody)
                {
                    RaycastHit hitInfo;
                    if (args.senderBody.inputBank.GetAimRaycast(1000f, out hitInfo))
                    {
                        DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(
                            characterSpawnCard,
                            new DirectorPlacementRule
                            {
                                placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                                maxDistance = 100f,
                                minDistance = 0f,
                                position = hitInfo.point,
                                preventOverhead = true
                            },
                            RoR2Application.rng
                        );
                        directorSpawnRequest.teamIndexOverride = args.Count >= 2 ? (TeamIndex)int.Parse(args[1]) : TeamIndex.Monster;
                        characterSpawnCard.DoSpawn(hitInfo.point, Quaternion.identity, directorSpawnRequest);
                    }
                }
            }
        }
    }
}
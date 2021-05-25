using RoR2;
using System.Collections.Generic;
using System.Globalization;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using MysticsRisky2Utils.ContentManagement;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseAchievement : BaseLoadableAsset
    {
        public string name;
        public string unlockableName;
        public string prerequisiteName;
        public System.Type trackerType;
        public System.Type serverTrackerType;
        public AchievementDef achievementDef;
        public abstract AssetPathModification iconPath { get; }

        public override void Load()
        {
            OnLoad();
            string nameNoToken = name;
            name = TokenPrefix + name;
            achievementDef = new AchievementDef
            {
                identifier = name,
                unlockableRewardIdentifier = unlockableName,
                prerequisiteAchievementIdentifier = prerequisiteName,
                nameToken = "ACHIEVEMENT_" + name.ToUpper(CultureInfo.InvariantCulture) + "_NAME",
                descriptionToken = "ACHIEVEMENT_" + name.ToUpper(CultureInfo.InvariantCulture) + "_DESCRIPTION",
                achievedIcon = AssetBundle.LoadAsset<Sprite>(iconPath(nameNoToken)),
                type = trackerType,
                serverTrackerType = serverTrackerType
            };
            registeredAchievements.Add(this);
            asset = achievementDef;
        }

        public static List<BaseAchievement> registeredAchievements = new List<BaseAchievement>();

        public static void Init()
        {
            IL.RoR2.AchievementManager.CollectAchievementDefs += (il) =>
            {
                ILCursor c = new ILCursor(il);

                if (c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchEndfinally(),
                    x => x.MatchLdloc(1)
                ))
                {
                    c.Emit(OpCodes.Ldsfld, typeof(AchievementManager).GetField("achievementIdentifiers", MysticsRisky2UtilsPlugin.bindingFlagAll));
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<System.Action<List<AchievementDef>, List<string>, Dictionary<string, AchievementDef>>>((list, achievementIdentifiers, map) =>
                    {
                        foreach (BaseAchievement achievement in registeredAchievements)
                        {
                            AchievementDef achievementDef = achievement.achievementDef;
                            achievementIdentifiers.Add(achievement.name);
                            map.Add(achievement.name, achievementDef);
                            list.Add(achievementDef);
                            UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(achievementDef.unlockableRewardIdentifier);
                            if (unlockableDef != null)
                            {
                                unlockableDef.getHowToUnlockString = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                                {
                                    Language.GetString(achievementDef.nameToken),
                                    Language.GetString(achievementDef.descriptionToken)
                                });
                                unlockableDef.getUnlockedString = () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                                {
                                    Language.GetString(achievementDef.nameToken),
                                    Language.GetString(achievementDef.descriptionToken)
                                });
                            }
                        }
                    });
                    c.Emit(OpCodes.Ldloc_1);
                }
            };
        }
    }
}

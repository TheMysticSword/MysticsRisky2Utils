using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MysticsRisky2Utils.ContentManagement;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseElite : BaseLoadableAsset
    {
        public EliteDef eliteDef;
        public Texture recolorRamp;
        public int tier = 0;

        public static List<BaseElite> elites = new List<BaseElite>();

        public abstract Texture LoadRecolorRamp(string assetName);

        public override void Load()
        {
            eliteDef = ScriptableObject.CreateInstance<EliteDef>();
            OnLoad();
            eliteDef.modifierToken = "ELITE_MODIFIER_" + TokenPrefix.ToUpper() + eliteDef.name.ToUpper();
            eliteDef.shaderEliteRampIndex = 0;
            recolorRamp = LoadRecolorRamp(eliteDef.name);
            asset = eliteDef;
            elites.Add(this);
        }

        internal static void Init()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
            IL.RoR2.CharacterModel.UpdateMaterials += CharacterModel_UpdateMaterials;

            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            MethodInfo scriptedCombatEncounterSpawnHandler = typeof(ScriptedCombatEncounter).GetMethod("<Spawn>g__HandleSpawn|18_0", MysticsRisky2UtilsPlugin.bindingFlagAll);
            if (scriptedCombatEncounterSpawnHandler != null)
            {
                ILHook h = new ILHook(
                    scriptedCombatEncounterSpawnHandler,
                    il =>
                    {
                        ILCursor c = new ILCursor(il);

                        int arrayLocalVarPos = 0;
                        if (c.TryGotoNext(
                            MoveType.After,
                            x => x.MatchNewarr<EliteDef>()
                        ) && c.TryGotoNext(
                            MoveType.After,
                            x => x.MatchLdsfld(typeof(RoR2Content.Elites), "Fire")
                        ) && c.TryGotoNext(
                            MoveType.After,
                            x => x.MatchLdsfld(typeof(RoR2Content.Elites), "Lightning")
                        ) && c.TryGotoNext(
                            MoveType.After,
                            x => x.MatchLdsfld(typeof(RoR2Content.Elites), "Ice")
                        ) && c.TryGotoNext(
                            MoveType.After,
                            x => x.MatchStloc(out arrayLocalVarPos)
                        ))
                        {
                            c.Emit(OpCodes.Ldloc, arrayLocalVarPos);
                            c.EmitDelegate<System.Func<EliteDef[], EliteDef[]>>((array) =>
                            {
                                foreach (BaseElite customElite in elites.FindAll(x => x.tier == 1))
                                {
                                    HG.ArrayUtils.ArrayAppend(ref array, customElite.eliteDef);
                                }
                                return array;
                            });
                            c.Emit(OpCodes.Stloc, arrayLocalVarPos);
                        }
                        else
                        {
                            MysticsRisky2UtilsPlugin.logger.LogWarning("Failed to add custom elites to ScriptedCombatEncounter's Artifact of Honor elite list. Alloy Worship Unit, Aurelionite and other scripted combat encounter enemies cannot become custom elites.");
                        }
                    }
                );
            } else
            {
                MysticsRisky2UtilsPlugin.logger.LogWarning("Failed to add custom elites to ScriptedCombatEncounter's Artifact of Honor elite list. Alloy Worship Unit, Aurelionite and other scripted combat encounter enemies cannot become custom elites.");
                MysticsRisky2UtilsPlugin.logger.LogWarning("(for TheMysticSword: ScriptedCombatEncounter HandleSpawn method no longer exists)");
            }
        }

        private static void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
        {
            orig(self);
            self.gameObject.AddComponent<MysticsRisky2UtilsCustomEliteFields>();
        }

        private static readonly int EliteRampPropertyID = Shader.PropertyToID("_EliteRamp");
        private static void CharacterModel_UpdateMaterials(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterModel>("propertyStorage"),
                x => x.MatchLdsfld(typeof(CommonShaderProperties), "_EliteIndex")
            );
            c.GotoNext(
                MoveType.After,
                x => x.MatchCallOrCallvirt<MaterialPropertyBlock>("SetFloat")
            );
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<System.Action<CharacterModel>>((characterModel) =>
            {
                MysticsRisky2UtilsCustomEliteFields component = characterModel.gameObject.GetComponent<MysticsRisky2UtilsCustomEliteFields>();
                if (component)
                {
                    BaseElite customElite = elites.FirstOrDefault(x => x.eliteDef.eliteIndex == characterModel.myEliteIndex);
                    if (customElite != null)
                    {
                        if (!component.eliteRampReplaced) component.eliteRampReplaced = true;
                        characterModel.propertyStorage.SetTexture(EliteRampPropertyID, customElite.recolorRamp);
                    }
                    else if (component.eliteRampReplaced)
                    {
                        component.eliteRampReplaced = false;
                        characterModel.propertyStorage.SetTexture(EliteRampPropertyID, Shader.GetGlobalTexture(EliteRampPropertyID));
                    }
                }
            });
        }

        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            foreach (BaseElite customElite in elites)
            {
                switch (customElite.tier)
                {
                    case 1:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[1].eliteTypes, customElite.eliteDef);
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[2].eliteTypes, customElite.eliteDef);
                        break;
                    case 2:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[3].eliteTypes, customElite.eliteDef);
                        break;
                }
            }
        }

        private class MysticsRisky2UtilsCustomEliteFields : MonoBehaviour
        {
            public bool eliteRampReplaced = false;
        }
    }
}

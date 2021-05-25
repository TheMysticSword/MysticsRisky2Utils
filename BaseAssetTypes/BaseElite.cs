using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MysticsRisky2Utils.ContentManagement;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseElite : BaseLoadableAsset
    {
        public EliteDef eliteDef;
        public Texture recolorRamp;
        public int tier = 0;

        public abstract AssetPathModification recolorRampPath { get; }

        public static List<BaseElite> elites = new List<BaseElite>();

        public override void Load()
        {
            eliteDef = ScriptableObject.CreateInstance<EliteDef>();
            OnLoad();
            eliteDef.modifierToken = "ELITE_MODIFIER_" + TokenPrefix.ToUpper() + eliteDef.name.ToUpper();
            eliteDef.shaderEliteRampIndex = 0;
            recolorRamp = AssetBundle.LoadAsset<Texture>(recolorRampPath(eliteDef.name));
            asset = eliteDef;
            elites.Add(this);
        }

        public static void Init()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
            IL.RoR2.CharacterModel.UpdateMaterials += CharacterModel_UpdateMaterials;

            On.RoR2.CombatDirector.Init += CombatDirector_Init;
        }

        public static void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
        {
            orig(self);
            self.gameObject.AddComponent<MysticsRisky2UtilsCustomEliteFields>();
        }

        private static readonly int EliteRampPropertyID = Shader.PropertyToID("_EliteRamp");
        public static void CharacterModel_UpdateMaterials(ILContext il)
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
                if (characterModel.body)
                {
                    MysticsRisky2UtilsCustomEliteFields component = characterModel.GetComponent<MysticsRisky2UtilsCustomEliteFields>();
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
                }
            });
        }

        public static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
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

        public class MysticsRisky2UtilsCustomEliteFields : MonoBehaviour
        {
            public bool eliteRampReplaced = false;
        }
    }
}

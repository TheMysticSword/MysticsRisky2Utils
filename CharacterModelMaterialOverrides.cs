using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MysticsRisky2Utils
{
    public static class CharacterModelMaterialOverrides
    {
        internal static void Init()
        {
            IL.RoR2.CharacterModel.UpdateRendererMaterials += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(
                    MoveType.AfterLabel,
                    x => x.MatchLdarg(3),
                    x => x.MatchBrtrue(out _),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CharacterModel>("activeOverlayCount")
                ))
                {
                    c.Emit(OpCodes.Ldarg, 0);
                    c.Emit(OpCodes.Ldloc, 0);
                    c.Emit(OpCodes.Ldarg, 3);
                    bool ignoreOverlays2 = false;
                    c.EmitDelegate<System.Func<CharacterModel, Material, bool, Material>>((characterModel, material, ignoreOverlays) =>
                    {
                        foreach (var materialOverride in materialOverrides)
                        {
                            materialOverride(characterModel, ref material, ref ignoreOverlays);
                        }
                        ignoreOverlays2 = ignoreOverlays;
                        return material;
                    });
                    c.Emit(OpCodes.Stloc, 0);
                    c.EmitDelegate<System.Func<bool>>(() =>
                    {
                        return ignoreOverlays2;
                    });
                    c.Emit(OpCodes.Starg, 3);
                }
            };
        }

        public static void AddOverride(MaterialOverrideHandler handler)
        {
            materialOverrides.Add(handler);
        }

        public delegate void MaterialOverrideHandler(CharacterModel characterModel, ref Material material, ref bool ignoreOverlays);

        public static List<MaterialOverrideHandler> materialOverrides = new List<MaterialOverrideHandler>();
    }
}
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MysticsRisky2Utils
{
    public static class CharacterModelMaterialOverrides
    {
        internal static void Init()
        {
            On.RoR2.CharacterModel.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<MysticsRisky2UtilsCharacterModelMaterialOverridesComponent>();
            };

            IL.RoR2.CharacterModel.UpdateRendererMaterials += (il) =>
            {
                ILCursor c = new ILCursor(il);
                int ignoreOverlaysPos = 0;
                if (c.TryGotoNext(
                    MoveType.AfterLabel,
                    x => x.MatchLdarg(out ignoreOverlaysPos),
                    x => x.MatchBrtrue(out _),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CharacterModel>("activeOverlayCount")
                ))
                {
                    c.Emit(OpCodes.Ldarg, 0);
                    c.Emit(OpCodes.Ldloc, 0);
                    c.Emit(OpCodes.Ldarg, ignoreOverlaysPos);
                    bool ignoreOverlays2 = false;
                    c.EmitDelegate<System.Func<CharacterModel, Material, bool, Material>>((characterModel, material, ignoreOverlays) =>
                    {
                        var component = characterModel.GetComponent<MysticsRisky2UtilsCharacterModelMaterialOverridesComponent>();
                        if (component)
                        {
                            var activeOverride = materialOverrides.FirstOrDefault(x => component.activeOverrides.Contains(x.key));
                            if (!activeOverride.Equals(default(MaterialOverrideInfo)))
                            {
                                activeOverride.handler(characterModel, ref material, ref ignoreOverlays);
                            }
                        }
                        ignoreOverlays2 = ignoreOverlays;
                        return material;
                    });
                    c.Emit(OpCodes.Stloc, 0);
                    c.EmitDelegate<System.Func<bool>>(() =>
                    {
                        return ignoreOverlays2;
                    });
                    c.Emit(OpCodes.Starg, ignoreOverlaysPos);
                }
            };

            IL.RoR2.CharacterModel.UpdateMaterials += (il) =>
            {
                ILCursor c = new ILCursor(il);
                int itemDisplayPos = 0;
                if (c.TryGotoNext(
                    MoveType.AfterLabel,
                    x => x.MatchCallOrCallvirt<CharacterModel.ParentedPrefabDisplay>("get_itemDisplay"),
                    x => x.MatchStloc(out itemDisplayPos)
                ) && c.TryGotoNext(
                    MoveType.AfterLabel,
                    x => x.MatchCallOrCallvirt<ItemDisplay>("SetVisibilityLevel")
                ))
                {
                    c.Emit(OpCodes.Ldarg, 0);
                    c.Emit(OpCodes.Ldloc, itemDisplayPos);
                    c.EmitDelegate<System.Action<CharacterModel, ItemDisplay>>((characterModel, itemDisplay) =>
                    {
                        var component = characterModel.GetComponent<MysticsRisky2UtilsCharacterModelMaterialOverridesComponent>();
                        if (component)
                        {
                            var activeOverride = materialOverrides.FirstOrDefault(x => component.activeOverrides.Contains(x.key));
                            if (!activeOverride.Equals(default(MaterialOverrideInfo)))
                            {
                                Material material = null;
                                bool ignoreOverlays = false;
                                activeOverride.handler(characterModel, ref material, ref ignoreOverlays);
                                foreach (var rendererInfo in itemDisplay.rendererInfos)
                                {
                                    rendererInfo.renderer.material = material;
                                }
                            }
                        }
                    });
                }
            };
        }

        private class MysticsRisky2UtilsCharacterModelMaterialOverridesComponent : MonoBehaviour
        {
            public List<string> activeOverrides = new List<string>();
        };

        public static void SetOverrideActive(CharacterModel model, string key, bool active)
        {
            var component = model.GetComponent<MysticsRisky2UtilsCharacterModelMaterialOverridesComponent>();
            if (component)
            {
                var isActiveNow = component.activeOverrides.Contains(key);
                if (isActiveNow != active)
                {
                    if (isActiveNow) component.activeOverrides.Remove(key);
                    else component.activeOverrides.Add(key);

                    model.materialsDirty = true;
                    foreach (var parentedPrefabDisplay in model.parentedPrefabDisplays)
                    {
                        var itemDisplay = parentedPrefabDisplay.itemDisplay;
                        var visibilityLevel = itemDisplay.visibilityLevel;
                        itemDisplay.visibilityLevel = (VisibilityLevel)(-1);
                        itemDisplay.SetVisibilityLevel(visibilityLevel);
                    }
                }
            }
        }

        public static void AddOverride(string key, MaterialOverrideHandler handler)
        {
            materialOverrides.Add(new MaterialOverrideInfo
            {
                key = key,
                handler = handler
            });
        }

        public delegate void MaterialOverrideHandler(CharacterModel characterModel, ref Material material, ref bool ignoreOverlays);

        public struct MaterialOverrideInfo
        {
            public string key;
            public MaterialOverrideHandler handler;
        }

        public static List<MaterialOverrideInfo> materialOverrides = new List<MaterialOverrideInfo>();
    }
}
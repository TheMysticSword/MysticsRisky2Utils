using RoR2;
using R2API.Utils;
using UnityEngine;
using System.Collections.Generic;

namespace MysticsRisky2Utils
{
    public static class Overlays
    {
        public struct OverlayInfo
        {
            public Material material;
            public System.Func<CharacterModel, bool> condition;

            public OverlayInfo(Material material, System.Func<CharacterModel, bool> condition)
            {
                this.material = material;
                this.condition = condition;
            }
        }

        public static List<OverlayInfo> overlays = new List<OverlayInfo>();

        internal static void Init()
        {
            On.RoR2.CharacterModel.UpdateOverlays += (orig, self) =>
            {
                orig(self);
                if (self.body)
                {
                    foreach (OverlayInfo overlayInfo in overlays)
                    {
                        if (self.activeOverlayCount >= CharacterModel.maxOverlays) return;
                        if (overlayInfo.condition(self))
                        {
                            Material[] array = self.currentOverlays;
                            int num = self.activeOverlayCount;
                            self.activeOverlayCount = num + 1;
                            array[num] = overlayInfo.material;
                        }
                    }
                }
            };
        }

        public static void CreateOverlay(Material material, System.Func<CharacterModel, bool> condition)
        {
            overlays.Add(new OverlayInfo(material, condition));
        }
    }
}

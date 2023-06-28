using RoR2;
using UnityEngine;
using System.Collections.Generic;
using MysticsRisky2Utils.ContentManagement;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseBuff : BaseLoadableAsset
    {
        public BuffDef buffDef;
        private bool _refreshable = false;
        public bool refreshable
        {
            get
            {
                return _refreshable;
            }
            set
            {
                if (_refreshable == value) return;
                _refreshable = value;
                if (value)
                    refreshableBuffs.Add(buffDef);
                else
                    refreshableBuffs.Remove(buffDef);
            }
        }
        private static List<BuffDef> refreshableBuffs = new List<BuffDef>();

        public override void Load()
        {
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            OnLoad();
            asset = buffDef;
        }

        internal static void Init()
        {
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
        }

        private static void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            orig(self, buffDef, duration);
            if (refreshableBuffs.Contains(buffDef))
            {
                var buffIndex = buffDef.buffIndex;
                for (var i = 0; i < self.timedBuffs.Count; i++)
                {
                    var timedBuff = self.timedBuffs[i];
                    if (timedBuff.buffIndex == buffIndex)
                    {
                        if (timedBuff.timer < duration)
                        {
                            timedBuff.timer = duration;
                        }
                    }
                }
            }
        }
    }
}

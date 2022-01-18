using RoR2;
using UnityEngine;
using System.Collections.Generic;
using MysticsRisky2Utils.ContentManagement;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseBuff : BaseLoadableAsset
    {
        public BuffDef buffDef;
        
        public override void Load()
        {
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            OnLoad();
            asset = buffDef;
        }
    }
}

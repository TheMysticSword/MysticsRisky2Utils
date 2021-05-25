using RoR2;
using UnityEngine;
using System.Collections.Generic;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseItem : BaseItemLike
    {
        public ItemDef itemDef;
        public static List<BaseItem> loadedItems = new List<BaseItem>();
        
        public override void Load()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            PreLoad();
            bool disabled = false;
            if (itemDef.inDroppableTier) disabled = IsDisabledByConfig();
            string name = itemDef.name;
            itemDef.name = TokenPrefix + itemDef.name;
            itemDef.AutoPopulateTokens();
            itemDef.name = name;
            AfterTokensPopulated();
            if (!disabled) {
                OnLoad();
                loadedItems.Add(this);
            } else
            {
                itemDef.tier = ItemTier.NoTier;
            }
            asset = itemDef;
        }

        public override void SetAssets(string assetName)
        {
            model = AssetBundle.LoadAsset<GameObject>(modelPath(assetName));
            model.name = "mdl" + itemDef.name;

            bool followerModelSeparate = AssetBundle.Contains(followerModelPath(assetName));
            if (followerModelSeparate)
            {
                followerModel = AssetBundle.LoadAsset<GameObject>(followerModelPath(assetName));
                followerModel.name = "mdl" + itemDef.name + "Follower";
            }

            PrepareModel(model);
            if (followerModelSeparate) PrepareModel(followerModel);

            // Separate the follower model from the pickup model for adding different visual effects to followers
            if (!followerModelSeparate) CopyModelToFollower();

            itemDef.pickupModelPrefab = model;
            SetIcon(assetName);
        }

        public override void SetIcon(string assetName)
        {
            itemDef.pickupIconSprite = AssetBundle.LoadAsset<Sprite>(iconPath(assetName));
        }

        public override UnlockableDef GetUnlockableDef()
        {
            if (!itemDef.unlockableDef)
            {
                itemDef.unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
                itemDef.unlockableDef.cachedName = TokenPrefix + "Items." + itemDef.name;
                itemDef.unlockableDef.nameToken = ("ITEM_" + TokenPrefix + itemDef.name + "_NAME").ToUpper();
            }
            return itemDef.unlockableDef;
        }

        public override PickupIndex GetPickupIndex()
        {
            return PickupCatalog.FindPickupIndex(itemDef.itemIndex);
        }

        public float ModifierTimesFunction(MysticsRisky2UtilsPlugin.GenericCharacterInfo genericCharacterInfo, bool stacks = true)
        {
            if (genericCharacterInfo.inventory)
            {
                int itemCount = genericCharacterInfo.inventory.GetItemCount(itemDef);
                if (stacks) return itemCount;
                else return itemCount > 0 ? 1f : 0f;
            }
            return 0f;
        }
    }
}

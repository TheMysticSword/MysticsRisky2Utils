using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseItem : BaseItemLike
    {
        public ItemDef itemDef;
        /// <summary>
        /// Dictionary of all loaded items as instances of BaseItem. Keys are equal to BaseItem.itemDef.name field values.
        /// </summary>
        public static Dictionary<string, BaseItem> loadedDictionary = new Dictionary<string, BaseItem>();

        public override void Load()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            OnLoad();
            itemDef.AutoPopulateTokens();
            loadedDictionary.Add(itemDef.name, this);
            asset = itemDef;
        }

        public override UnlockableDef GetUnlockableDef()
        {
            if (!itemDef.unlockableDef)
            {
                itemDef.unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
                itemDef.unlockableDef.cachedName = "Items." + itemDef.name;
                itemDef.unlockableDef.nameToken = ("ITEM_" + itemDef.name + "_NAME").ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                
                itemDef.unlockableDef.achievementIcon = MysticsRisky2Utils.Utils.CreateItemIconWithBackgroundFromItem(itemDef);
            }
            return itemDef.unlockableDef;
        }

        public override PickupIndex GetPickupIndex()
        {
            return PickupCatalog.FindPickupIndex(itemDef.itemIndex);
        }

        public void SetItemTierWhenAvailable(ItemTier tier)
        {
            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef)
                {
                    itemDef.tier = tier;
                    try
                    {
                        itemDef.deprecatedTier = tier;
                    }
                    catch (System.Exception e)
                    {
                        MysticsRisky2UtilsPlugin.logger.LogWarning("Error setting deprecatedTier: " + e);
                    }
                }
            });
        }
    }
}

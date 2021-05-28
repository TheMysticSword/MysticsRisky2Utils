using RoR2;
using System.Collections.Generic;
using System.Linq;
using HG;
using UnityEngine;

namespace MysticsRisky2Utils
{
    public static class CostTypeCreation
    {
        internal static void Init()
        {
            CostTypeCatalog.modHelper.getAdditionalEntries += (list) =>
            {
                foreach (CustomCostTypeInfo customCostType in customCostTypes) list.Add(customCostType.costTypeDef);
            };
            On.RoR2.CostTypeCatalog.Register += (orig, costType, costTypeDef) =>
            {
                orig(costType, costTypeDef);
                CustomCostTypeInfo customCostType = customCostTypes.FirstOrDefault(x => x.costTypeDef == costTypeDef);
                if (!customCostType.Equals(default(CustomCostTypeInfo)))
                {
                    customCostType.index = costType;
                    if (customCostType.onRegister != null) customCostType.onRegister(customCostType.index);
                }
            };
        }

        public struct CustomCostTypeInfo
        {
            public CostTypeDef costTypeDef;
            public System.Action<CostTypeIndex> onRegister;
            public CostTypeIndex index;
        }

        public static List<CustomCostTypeInfo> customCostTypes = new List<CustomCostTypeInfo>();

        public static void CreateCostType(CustomCostTypeInfo customCostType)
        {
            customCostTypes.Add(customCostType);
        }
    }
}

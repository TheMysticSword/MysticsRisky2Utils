using BepInEx.Configuration;
using System;
using UnityEngine;

namespace MysticsRisky2Utils.SoftDependencies
{
    public static class SoftDependencyManager
    {
        internal static void Init()
        {
            var pluginInfos = BepInEx.Bootstrap.Chainloader.PluginInfos;
            if (pluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                RiskOfOptionsDependency.enabled = true;
            }
        }

        public static class RiskOfOptionsDependency
        {
            public static bool enabled = false;

            public static void RegisterModInfo(string modGUID, string modName, string description, Sprite iconSprite = null)
            {
                RiskOfOptionsDependencyInternal.RegisterModInfo(modGUID, modName, description, iconSprite);
            }

            public static void AddOptionInt(string modGUID, string modName, ConfigEntry<int> configEntry, int min = 0, int max = 1000, bool restartRequired = false)
            {
                RiskOfOptionsDependencyInternal.AddOptionInt(modGUID, modName, configEntry, min, max, restartRequired);
            }

            public static void AddOptionFloat(string modGUID, string modName, ConfigEntry<float> configEntry, float min = 0, float max = 1000, bool restartRequired = false)
            {
                RiskOfOptionsDependencyInternal.AddOptionFloat(modGUID, modName, configEntry, min, max, restartRequired);
            }

            public static void AddOptionBool(string modGUID, string modName, ConfigEntry<bool> configEntry, bool restartRequired = false)
            {
                RiskOfOptionsDependencyInternal.AddOptionBool(modGUID, modName, configEntry, restartRequired);
            }
        }
    }
}

using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MysticsRisky2Utils
{
    public static class ConfigOptions
    {
        internal static void Init()
        {
            On.RoR2.UI.LogBook.LogBookController.Awake += LogBookController_Awake;
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private static bool reloadLogbook = false;
        private static void LogBookController_Awake(On.RoR2.UI.LogBook.LogBookController.orig_Awake orig, RoR2.UI.LogBook.LogBookController self)
        {
            orig(self);
            if (reloadLogbook)
            {
                reloadLogbook = false;
                RoR2.UI.LogBook.LogBookController.BuildStaticData();
            }
        }

        private static string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, RoR2.Language self, string token)
        {
            var result = orig(self, token);
            foreach (var configurableValue in ConfigurableValue.instancesList.FindAll(x => x.stringsToAffect.Contains(token)))
            {
                result = result.Replace("{" + configurableValue.key + "}", configurableValue.ToString());
            }
            return result;
        }

        public abstract class ConfigurableValue
        {
            public static List<ConfigurableValue> instancesList = new List<ConfigurableValue>();

            public List<string> stringsToAffect = new List<string>();
            public string key = "";
            public string id = "";

            public static ConfigurableValue<T> Create<T>(ConfigFile configFile, string section, string key, T defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useDefaultValueConfigEntry = null, bool restartRequired = false, Action<T> onChanged = null)
            {
                return new ConfigurableValue<T>(configFile, section, key, defaultValue, description, stringsToAffect, useDefaultValueConfigEntry, restartRequired, onChanged);
            }

            public static ConfigurableValue<int> CreateInt(string modGUID, string modName, ConfigFile configFile, string section, string key, int defaultValue, int min = 0, int max = 1000, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useDefaultValueConfigEntry = null, bool restartRequired = false, Action<int> onChanged = null)
            {
                var configurableValue = Create<int>(configFile, section, key, defaultValue, description, stringsToAffect, useDefaultValueConfigEntry, restartRequired, onChanged);
                if (SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.enabled)
                {
                    SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.AddOptionInt(modGUID, modName, configurableValue.bepinexConfigEntry, min: min, max: max, restartRequired: restartRequired);
                }
                return configurableValue;
            }

            public static ConfigurableValue<float> CreateFloat(string modGUID, string modName, ConfigFile configFile, string section, string key, float defaultValue, float min = 0, float max = 1000, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useDefaultValueConfigEntry = null, bool restartRequired = false, Action<float> onChanged = null)
            {
                var configurableValue = Create<float>(configFile, section, key, defaultValue, description, stringsToAffect, useDefaultValueConfigEntry, restartRequired, onChanged);
                if (SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.enabled)
                {
                    SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.AddOptionFloat(modGUID, modName, configurableValue.bepinexConfigEntry, min: min, max: max, restartRequired: restartRequired);
                }
                return configurableValue;
            }

            public static ConfigurableValue<bool> CreateBool(string modGUID, string modName, ConfigFile configFile, string section, string key, bool defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useDefaultValueConfigEntry = null, bool restartRequired = false, Action<bool> onChanged = null)
            {
                var configurableValue = Create<bool>(configFile, section, key, defaultValue, description, stringsToAffect, useDefaultValueConfigEntry, restartRequired, onChanged);
                if (SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.enabled)
                {
                    SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.AddOptionBool(modGUID, modName, configurableValue.bepinexConfigEntry, restartRequired: restartRequired);
                }
                return configurableValue;
            }

            public static ConfigurableValue<string> CreateString(string modGUID, string modName, ConfigFile configFile, string section, string key, string defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useDefaultValueConfigEntry = null, bool restartRequired = false, Action<string> onChanged = null)
            {
                var configurableValue = Create<string>(configFile, section, key, defaultValue, description, stringsToAffect, useDefaultValueConfigEntry, restartRequired, onChanged);
                if (SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.enabled)
                {
                    SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.AddOptionString(modGUID, modName, configurableValue.bepinexConfigEntry, restartRequired: restartRequired);
                }
                return configurableValue;
            }
        }

        public class ConfigurableValue<T> : ConfigurableValue
        {
            public ConfigEntry<T> bepinexConfigEntry;
            private ConfigEntry<bool> useDefaultValueConfigEntry;
            private T defaultValue;

            public ConfigurableValue(ConfigFile configFile, string section, string key, T defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useDefaultValueConfigEntry = null, bool restartRequired = false, Action<T> onChanged = null)
            {
                id = System.IO.Path.GetFileNameWithoutExtension(configFile.ConfigFilePath) + "." + section + "." + key;
                var existing = instancesList.FirstOrDefault(x => x.id == id);
                if (existing != null)
                {
                    var existingCast = existing as ConfigurableValue<T>;
                    bepinexConfigEntry = existingCast.bepinexConfigEntry;
                    this.useDefaultValueConfigEntry = useDefaultValueConfigEntry;
                }
                else
                {
                    bepinexConfigEntry = configFile.Bind<T>(section, key, defaultValue, description);
                    instancesList.Add(this);
                }

                this.useDefaultValueConfigEntry = useDefaultValueConfigEntry;
                this.key = key;
                this.defaultValue = defaultValue;
                if (stringsToAffect != null) this.stringsToAffect = stringsToAffect;
                else this.stringsToAffect = new List<string>();

                if (onChanged != null)
                {
                    bepinexConfigEntry.SettingChanged += (x, y) =>
                    {
                        onChanged(Value);
                        reloadLogbook = true;
                    };
                    onChanged(Value);
                    reloadLogbook = true;
                }
            }

            public T Value
            {
                get
                {
                    if (useDefaultValueConfigEntry != null && useDefaultValueConfigEntry.Value) return defaultValue;
                    return bepinexConfigEntry.Value;
                }
            }

            public override string ToString()
            {
                return System.Convert.ToString(Value, System.Globalization.CultureInfo.InvariantCulture);
            }

            public static implicit operator T(ConfigurableValue<T> configurableValue)
            {
                return configurableValue.Value;
            }
        }
    }
}
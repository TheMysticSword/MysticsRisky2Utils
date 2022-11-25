using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using UnityEngine;
using BepInEx.Configuration;

namespace MysticsRisky2Utils.SoftDependencies
{
    internal static class RiskOfOptionsDependencyInternal
    {
        public static void RegisterModInfo(string modGUID, string modName, string description, Sprite iconSprite)
        {
            ModSettingsManager.SetModDescription(description, modGUID, modName);
            if (iconSprite != null) ModSettingsManager.SetModIcon(iconSprite, modGUID, modName);
        }

        public static void AddOptionInt(string modGUID, string modName, ConfigEntry<int> configEntry, int min, int max, bool restartRequired)
        {
            ModSettingsManager.AddOption(new IntSliderOption(configEntry, new IntSliderConfig
            {
                min = min,
                max = max,
                restartRequired = restartRequired
            }), modGUID, modName);
        }

        public static void AddOptionFloat(string modGUID, string modName, ConfigEntry<float> configEntry, float min, float max, bool restartRequired)
        {
            ModSettingsManager.AddOption(new StepSliderOption(configEntry, new StepSliderConfig
            {
                min = min,
                max = max,
                increment = 0.01f,
                restartRequired = restartRequired
            }), modGUID, modName);
        }

        public static void AddOptionBool(string modGUID, string modName, ConfigEntry<bool> configEntry, bool restartRequired)
        {
            ModSettingsManager.AddOption(new CheckBoxOption(configEntry, restartRequired), modGUID, modName);
        }

        public static void AddOptionString(string modGUID, string modName, ConfigEntry<string> configEntry, bool restartRequired)
        {
            ModSettingsManager.AddOption(new StringInputFieldOption(configEntry, new InputFieldConfig
            {
                submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit,
                restartRequired = restartRequired
            }), modGUID, modName);
        }
    }
}

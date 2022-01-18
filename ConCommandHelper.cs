using System.Collections.Generic;
using System.Reflection;
using RoR2;

namespace MysticsRisky2Utils
{
    public static class ConCommandHelper
    {
        public static Dictionary<string, Console.ConCommand> loadedConCommands = new Dictionary<string, Console.ConCommand>();

        public static void Load(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes<ConCommandAttribute>();
            foreach (var attribute in attributes)
            {
                var conCommand = new Console.ConCommand
                {
                    flags = attribute.flags,
                    helpText = attribute.helpText,
                    action = (Console.ConCommandDelegate)System.Delegate.CreateDelegate(
                        typeof(Console.ConCommandDelegate), methodInfo)
                };
                loadedConCommands.Add(attribute.commandName.ToLower(System.Globalization.CultureInfo.InvariantCulture), conCommand);
            }
        }

        internal static void Init()
        {
            On.RoR2.Console.InitConVars += (orig, self) =>
            {
                orig(self);

                var catalog = self.concommandCatalog;
                foreach (KeyValuePair<string, Console.ConCommand> keyValuePair in loadedConCommands)
                {
                    catalog[keyValuePair.Key] = keyValuePair.Value;
                }
            };
        }
    }
}
using EntityStates;

namespace MysticsRisky2Utils
{
    internal static class StateSerializerFix
    {
        public static void Init()
        {
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Add(typeof(SerializableEntityStateType).GetMethod("set_stateType", MysticsRisky2UtilsPlugin.bindingFlagAll), (SetStateTypeDelegate)SetStateType);
        }

        public static void SetStateType(ref SerializableEntityStateType self, System.Type value)
        {
            self._typeName = value.AssemblyQualifiedName;
        }

        public delegate void SetStateTypeDelegate(ref SerializableEntityStateType self, System.Type value);
    }
}
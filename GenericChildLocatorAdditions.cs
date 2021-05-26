namespace MysticsRisky2Utils
{
    internal static class GenericChildLocatorAdditions
    {
        public static void Init()
        {
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlRoboBallMini",
                transformLocation = "RoboBallMiniArmature/ROOT",
                childName = "ROOT"
            });
        }
    }
}

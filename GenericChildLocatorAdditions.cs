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
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlTreebot",
                transformLocation = "TreebotArmature/ROOT/Base/PlatformBase/ArmPlatformBase/Antennae.1/Antennae.005/Antennae.003/Antennae.007/Antennae.002/Antennae.006/Antennae.004",
                childName = "MR2UAntennae4"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlClayBruiser",
                transformLocation = "ClayBruiserArmature/ROOT/base/stomach/chest",
                childName = "Chest"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlRoboBallBoss",
                transformLocation = "RoboBallBossArmature/ROOT/Shell",
                childName = "Shell"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlGravekeeper",
                transformLocation = "GravekeeperArmature/ROOT/JarBase",
                childName = "JarBase"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlGrandparent",
                transformLocation = "GrandparentArmature/ROOT/base/stomach/chest",
                childName = "Chest"
            });
        }
    }
}

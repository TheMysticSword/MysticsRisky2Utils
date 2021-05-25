using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MysticsRisky2Utils
{
    public static class ChildLocatorAdditions
    {
        public static void Init()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
        }

        public struct Addition
        {
            public string modelName;
            public string transformLocation;
            public string childName;
        }

        public static List<Addition> list = new List<Addition>();

        public static void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
        {
            orig(self);
            if (self.childLocator)
            {
                foreach (Addition addition in list.FindAll(x => x.modelName == Utils.TrimCloneFromString(self.gameObject.name)))
                {
                    if (!self.childLocator.transformPairs.Any(x => x.name == addition.childName))
                    {
                        Transform transform = self.transform.Find(addition.transformLocation);
                        if (transform)
                        {
                            HG.ArrayUtils.ArrayAppend(ref self.childLocator.transformPairs, new ChildLocator.NameTransformPair
                            {
                                name = addition.childName,
                                transform = transform
                            });
                        }
                    }
                }
            }
        }
    }
}
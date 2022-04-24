using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MysticsRisky2Utils
{
    public static class BrotherInfection
    {
        private static GameObject _white;
        public static GameObject white
        {
            get
            {
                if (_white == null)
                    _white = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ItemInfection, White.prefab").WaitForCompletion();
                return _white;
            }
        }

        private static GameObject _green;
        public static GameObject green
        {
            get
            {
                if (_green == null)
                    _green = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ItemInfection, Green.prefab").WaitForCompletion();
                return _green;
            }
        }

        private static GameObject _red;
        public static GameObject red
        {
            get
            {
                if (_red == null)
                    _red = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ItemInfection, Red.prefab").WaitForCompletion();
                return _red;
            }
        }

        private static GameObject _blue;
        public static GameObject blue
        {
            get
            {
                if (_blue == null)
                    _blue = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ItemInfection, Blue.prefab").WaitForCompletion();
                return _blue;
            }
        }
    }
}
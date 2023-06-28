using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MysticsRisky2Utils
{
    public static class EliteDisplays
    {
        private static GameObject _fireHorn;
        public static GameObject fireHorn
        {
            get
            {
                if (_fireHorn == null)
                    _fireHorn = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/DisplayEliteHorn.prefab").WaitForCompletion();
                return _fireHorn;
            }
        }

        private static GameObject _lightningHorn;
        public static GameObject lightningHorn
        {
            get
            {
                if (_lightningHorn == null)
                    _lightningHorn = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/DisplayEliteRhinoHorn.prefab").WaitForCompletion();
                return _lightningHorn;
            }
        }

        private static GameObject _iceCrown;
        public static GameObject iceCrown
        {
            get
            {
                if (_iceCrown == null)
                    _iceCrown = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteIce/DisplayEliteIceCrown.prefab").WaitForCompletion();
                return _iceCrown;
            }
        }

        private static GameObject _poisonCrown;
        public static GameObject poisonCrown
        {
            get
            {
                if (_poisonCrown == null)
                    _poisonCrown = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElitePoison/DisplayEliteUrchinCrown.prefab").WaitForCompletion();
                return _poisonCrown;
            }
        }

        private static GameObject _hauntedCrown;
        public static GameObject hauntedCrown
        {
            get
            {
                if (_hauntedCrown == null)
                    _hauntedCrown = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/DisplayEliteStealthCrown.prefab").WaitForCompletion();
                return _hauntedCrown;
            }
        }

        private static GameObject _lunarEye;
        public static GameObject lunarEye
        {
            get
            {
                if (_lunarEye == null)
                    _lunarEye = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLunar/DisplayEliteLunar,Eye.prefab").WaitForCompletion();
                return _lunarEye;
            }
        }
    }
}
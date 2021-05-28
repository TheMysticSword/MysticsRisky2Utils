using TMPro;
using UnityEngine;
using RoR2.UI;

namespace MysticsRisky2Utils
{
    public class MysticsRisky2UtilsTextMeshUseLanguageDefaultFont : MonoBehaviour
    {
        public void Awake()
        {
            TextMeshProUGUI component = GetComponent<TextMeshProUGUI>();
            if (component)
            {
                component.font = HGTextMeshProUGUI.defaultLanguageFont;
                component.UpdateFontAsset();
            }
        }
    }
}

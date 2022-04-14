using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MysticsRisky2Utils
{
    public static class HopooShaderToMaterial
    {
        public class Properties
        {
            public Dictionary<string, float> floats = new Dictionary<string, float>();
            public Dictionary<string, Color> colors = new Dictionary<string, Color>();
            public Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
            public Dictionary<string, Vector2> textureOffsets = new Dictionary<string, Vector2>();
            public Dictionary<string, Vector2> textureScales = new Dictionary<string, Vector2>();
        }

        public static void Apply(Material mat, Shader shader, Properties properties = null)
        {
            mat.shader = shader;
            if (properties == null) properties = new Properties();
            foreach (KeyValuePair<string, float> keyValuePair in properties.floats) mat.SetFloat(keyValuePair.Key, keyValuePair.Value);
            foreach (KeyValuePair<string, Color> keyValuePair in properties.colors) mat.SetColor(keyValuePair.Key, keyValuePair.Value);
            foreach (KeyValuePair<string, Texture> keyValuePair in properties.textures) mat.SetTexture(keyValuePair.Key, keyValuePair.Value);
            foreach (KeyValuePair<string, Vector2> keyValuePair in properties.textureOffsets) mat.SetTextureOffset(keyValuePair.Key, keyValuePair.Value);
            foreach (KeyValuePair<string, Vector2> keyValuePair in properties.textureScales) mat.SetTextureScale(keyValuePair.Key, keyValuePair.Value);
        }

        public class Standard
        {
            public static Shader shader = LegacyShaderAPI.Find("Hopoo Games/Deferred/Standard");

            public static void Apply(Material mat, Properties properties = null)
            {
                if (properties == null) properties = new Properties();
                if (mat.HasProperty("_BumpScale")) properties.floats.Add("_NormalStrength", mat.GetFloat("_BumpScale"));
                if (mat.HasProperty("_BumpMap"))
                {
                    properties.textures.Add("_NormalTex", mat.GetTexture("_BumpMap"));
                    properties.textureOffsets.Add("_NormalTex", mat.GetTextureOffset("_BumpMap"));
                    properties.textureScales.Add("_NormalTex", mat.GetTextureScale("_BumpMap"));
                }
                if (mat.HasProperty("_EmTex"))
                {
                    properties.textures.Add("_EmTex", mat.GetTexture("_EmissionMap"));
                    properties.textureOffsets.Add("_EmTex", mat.GetTextureOffset("_EmissionMap"));
                    properties.textureScales.Add("_EmTex", mat.GetTextureScale("_EmissionMap"));
                }
                HopooShaderToMaterial.Apply(mat, shader, properties);
            }
            
            public static void DisableEverything(Material mat)
            {
                mat.DisableKeyword("DITHER");
                mat.SetFloat("_DitherOn", 0f);
                mat.DisableKeyword("FORCE_SPEC");
                mat.SetFloat("_SpecularHighlights", 0f);
                mat.SetFloat("_SpecularStrength", 0f);
                mat.DisableKeyword("_EMISSION");
                mat.SetFloat("_EmPower", 0f);
                mat.SetColor("_EmColor", new Color(0f, 0f, 0f, 1f));
                mat.DisableKeyword("FRESNEL_EMISSION");
                mat.SetFloat("_FresnelBoost", 0f);
            }

            public static void Dither(Material mat)
            {
                mat.EnableKeyword("DITHER");
                mat.SetFloat("_DitherOn", 1f);
            }

            public static void Gloss(Material mat, float glossiness = 1f, float specularExponent = 10f, Color? color = null)
            {
                mat.EnableKeyword("FORCE_SPEC");
                mat.SetFloat("_SpecularHighlights", 1f);
                mat.SetFloat("_SpecularExponent", specularExponent);
                mat.SetFloat("_SpecularStrength", glossiness);
                mat.SetColor("_SpecularTint", color ?? Color.white);
            }

            public static void Emission(Material mat, float power = 1f, Color? color = null)
            {
                mat.EnableKeyword("_EMISSION");
                mat.EnableKeyword("FRESNEL_EMISSION");
                mat.SetFloat("_EmPower", power);
                mat.SetColor("_EmColor", color ?? Color.white);
                if (power == 0)
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.DisableKeyword("FRESNEL_EMISSION");
                }
            }
        }

        public class CloudRemap
        {
            public static Shader shader = LegacyShaderAPI.Find("Hopoo Games/FX/Cloud Remap");

            public static void Apply(Material mat, Properties properties = default(Properties))
            {
                if (properties == null) properties = new Properties();
                HopooShaderToMaterial.Apply(mat, shader, properties);
                mat.SetFloat("_AlphaBias", 0f);
                mat.SetFloat("_AlphaBoost", 1f);
                mat.SetFloat("_Cull", 0f);
                mat.SetFloat("_DepthOffset", 0f);
                mat.SetFloat("_Fade", 1f);
                mat.SetFloat("_FadeCloseDistance", 0.5f);
                mat.SetFloat("_FadeCloseOn", 0f);
                mat.SetFloat("_InvFade", 2f);
                mat.SetFloat("_ZTest", 4f);
                mat.SetFloat("_ZWrite", 1f);
            }

            public static void Apply(Material mat, Texture remapTexture = null, Texture cloud1Texture = null, Texture cloud2Texture = null, Properties properties = default(Properties))
            {
                Apply(mat, properties);
                mat.SetTexture("_Cloud1Tex", cloud1Texture);
                mat.SetTexture("_Cloud2Tex", cloud2Texture);
                mat.SetTexture("_RemapTex", remapTexture);
            }

            public static void Boost(Material mat, float power = 1f)
            {
                mat.SetFloat("_Boost", power);
            }
        }
    }
}
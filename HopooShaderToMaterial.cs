using System.Collections.Generic;
using UnityEngine;

namespace MysticsRisky2Utils
{
    public static class HopooShaderToMaterial
    {
        public struct Properties
        {
            public Dictionary<string, float> floats;
            public Dictionary<string, Color> colors;
            public Dictionary<string, Texture> textures;
        }

        public static void Apply(Material mat, Shader shader, Properties properties = default(Properties))
        {
            mat.shader = shader;
            if (properties.floats != null) foreach (KeyValuePair<string, float> keyValuePair in properties.floats) mat.SetFloat(keyValuePair.Key, keyValuePair.Value);
            if (properties.colors != null) foreach (KeyValuePair<string, Color> keyValuePair in properties.colors) mat.SetColor(keyValuePair.Key, keyValuePair.Value);
            if (properties.textures != null) foreach (KeyValuePair<string, Texture> keyValuePair in properties.textures) mat.SetTexture(keyValuePair.Key, keyValuePair.Value);
        }

        public class Standard
        {
            public static Shader shader = Resources.Load<Shader>("shaders/deferred/hgstandard");

            public static void Apply(Material mat, Properties properties = default(Properties))
            {
                HopooShaderToMaterial.Apply(mat, shader, properties);
                mat.SetTexture("_NormalTex", mat.GetTexture("_BumpMap"));
                mat.SetTexture("_EmTex", mat.GetTexture("_EmissionMap"));
            }
            public static void Apply(params Material[] mats)
            {
                foreach (Material mat in mats) Apply(mat);
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
            }
        }

        public class CloudRemap
        {
            public static Shader shader = Resources.Load<Shader>("shaders/fx/hgcloudremap");

            public static void Apply(Material mat, Properties properties = default(Properties))
            {
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
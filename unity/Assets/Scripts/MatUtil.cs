using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CriminalStory
{
    /// <summary>
    /// Material helper. The project targets HDRP (Unity 6.3,
    /// Packages/manifest.json), but a freshly opened project only uses
    /// HDRP once an HDRP Pipeline Asset is created and assigned
    /// (see unity/README.md). To render correctly in every state,
    /// materials are created with "HDRP/Lit" when a scriptable pipeline
    /// is active, and with "Standard" otherwise.
    /// </summary>
    public static class MatUtil
    {
        private static readonly Dictionary<string, Material> Cache =
            new Dictionary<string, Material>();

        public static Material Get(Color color)
        {
            return Get(color, false, 0.9f);
        }

        public static Material Get(Color color, bool emissive, float roughness)
        {
            string key = color.ToString() + "|" + emissive + "|" + roughness;
            Material mat;
            if (Cache.TryGetValue(key, out mat))
            {
                return mat;
            }

            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                shader = Shader.Find("HDRP/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Lit");
                }
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
            if (mat.HasProperty("_Roughness"))
            {
                mat.SetFloat("_Roughness", roughness);
            }
            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 1.0f - roughness);
            }
            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0.0f);
            }
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", color);
                }
            }
            Cache[key] = mat;
            return mat;
        }

        private static readonly Dictionary<string, Material> TexturedCache =
            new Dictionary<string, Material>();

        /// <summary>
        /// A PBR material from a downloaded ambientCG set in Resources
        /// ("Textures/&lt;Set&gt;/&lt;Set&gt;_Color" etc., CC0 — see ASSETS.md).
        /// Applies colour + NormalGL + roughness maps with tiling, so large
        /// surfaces stop looking like flat "clay" (checklist C2).
        /// Returns a plain colour material if the set is missing.
        /// </summary>
        public static Material GetTextured(string setName, Vector2 tiling, Color tint)
        {
            string key = setName + "|" + tiling + "|" + tint;
            Material mat;
            if (TexturedCache.TryGetValue(key, out mat))
            {
                return mat;
            }

            Texture2D colorTex = Resources.Load<Texture2D>("Textures/" + setName + "/" + setName + "_Color");
            if (colorTex == null)
            {
                mat = Get(tint);
                TexturedCache[key] = mat;
                return mat;
            }
            Texture2D normalTex = Resources.Load<Texture2D>("Textures/" + setName + "/" + setName + "_NormalGL");
            Texture2D roughTex = Resources.Load<Texture2D>("Textures/" + setName + "/" + setName + "_Roughness");

            colorTex.wrapMode = TextureWrapMode.Repeat;
            if (normalTex != null) normalTex.wrapMode = TextureWrapMode.Repeat;
            if (roughTex != null) roughTex.wrapMode = TextureWrapMode.Repeat;

            // Start from the colour-material path to reuse shader selection,
            // then clone so map assignment does not pollute the colour cache.
            Material baseMat = Get(Color.white);
            mat = new Material(baseMat);

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", tint);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", tint);
            }
            // Colour map: HDRP/URP use _BaseColorMap, built-in uses _MainTex.
            if (mat.HasProperty("_BaseColorMap"))
            {
                mat.SetTexture("_BaseColorMap", colorTex);
                mat.SetTextureScale("_BaseColorMap", tiling);
                mat.SetTextureOffset("_BaseColorMap", Vector2.zero);
            }
            if (mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", colorTex);
                mat.SetTextureScale("_MainTex", tiling);
            }
            if (normalTex != null)
            {
                string normalProp = mat.HasProperty("_NormalMap") ? "_NormalMap" : "_BumpMap";
                if (mat.HasProperty(normalProp))
                {
                    mat.SetTexture(normalProp, normalTex);
                    mat.SetTextureScale(normalProp, tiling);
                    mat.EnableKeyword("_NORMALMAP");
                    if (mat.HasProperty("_NormalScale"))
                    {
                        mat.SetFloat("_NormalScale", 1.0f);
                    }
                    if (mat.HasProperty("_BumpScale"))
                    {
                        mat.SetFloat("_BumpScale", 1.0f);
                    }
                }
            }
            if (roughTex != null)
            {
                // Built-in Standard reads smoothness from _MetallicGlossMap alpha;
                // HDRP Lit reads roughness from _MaskMap green channel. A plain
                // roughness map is not in either packed format, so approximate:
                // keep scalar smoothness mid-low and rely on colour+normal for
                // the visible relief. (Packed mask maps are a later refinement.)
                if (mat.HasProperty("_Smoothness"))
                {
                    mat.SetFloat("_Smoothness", 0.25f);
                }
                if (mat.HasProperty("_Glossiness"))
                {
                    mat.SetFloat("_Glossiness", 0.25f);
                }
            }
            TexturedCache[key] = mat;
            return mat;
        }

        /// <summary>A fresh (uncached) transparent material, e.g. for the objective marker.</summary>
        public static Material GetTransparent(Color color, float alpha)
        {
            Color c = color;
            c.a = alpha;
            Material mat = Get(c, true, 0.5f);
            // Get() caches by colour incl. alpha, so this instance is unique
            // to this colour+alpha combination and safe to make transparent.
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1.0f); // URP: transparent
            }
            if (mat.HasProperty("_SurfaceType"))
            {
                mat.SetFloat("_SurfaceType", 1.0f); // HDRP: transparent
            }
            if (mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 3.0f); // Built-in Standard: transparent
            }
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }
    }

    /// <summary>Small helpers to build primitive-based geometry in code.</summary>
    public static class BuildUtil
    {
        public static GameObject Box(string name, Transform parent, Vector3 pos,
            Vector3 scale, Color color, bool emissive = false)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = MatUtil.Get(color, emissive);
            renderer.castShadow = true;
            renderer.receiveShadow = true;
            return go;
        }

        /// <summary>A box with a PBR texture set instead of a flat colour.</summary>
        public static GameObject TexturedBox(string name, Transform parent, Vector3 pos,
            Vector3 scale, string textureSet, Vector2 tiling, Color tint)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = MatUtil.GetTextured(textureSet, tiling, tint);
            renderer.castShadow = true;
            renderer.receiveShadow = true;
            return go;
        }

        public static GameObject BoxNoCollider(string name, Transform parent, Vector3 pos,
            Vector3 scale, Color color, bool emissive = false)
        {
            GameObject go = Box(name, parent, pos, scale, color, emissive);
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Cylinder(string name, Transform parent, Vector3 pos,
            Vector3 scale, Color color, bool emissive = false)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = MatUtil.Get(color, emissive);
            renderer.castShadow = true;
            renderer.receiveShadow = true;
            return go;
        }

        public static GameObject Sphere(string name, Transform parent, Vector3 pos,
            Vector3 scale, Color color, bool emissive = false)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = MatUtil.Get(color, emissive);
            renderer.castShadow = true;
            renderer.receiveShadow = true;
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>Default UI font (built-in). Japanese glyphs are supplied
        /// by the OS font fallback on Windows; see unity/README.md.</summary>
        public static Font DefaultFont()
        {
            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (System.Exception)
            {
                font = null;
            }
            if (font == null)
            {
                try
                {
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch (System.Exception)
                {
                    font = null;
                }
            }
            return font;
        }
    }
}

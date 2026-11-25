using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CriminalStory
{
    /// <summary>
    /// Post-processing, built as a global HDRP Volume in code
    /// (Bloom / Tonemapping(ACES) / ColorAdjustments / Vignette /
    /// DepthOfField / Fog incl. volumetric fog / HDRI Sky when the
    /// HDRI is imported as a Cubemap).
    ///
    /// IMPORTANT: Volume overrides only take effect while HDRP is the
    /// active pipeline (HDRP Pipeline Asset created + assigned — the
    /// exact clicks are in unity/README.md). Without HDRP assigned the
    /// game still runs with the built-in pipeline and the Chapter 0
    /// "haze" overlay (SubtitleUI) carries the newborn-blur effect alone.
    ///
    /// Chapter 0 design: the newborn's blurry first sight is expressed
    /// by DepthOfField (near/far blur closing in on the mother's face)
    /// driven by SetBirthBlur(), plus the haze overlay as a fallback.
    /// </summary>
    public class PostProcessSetup : MonoBehaviour
    {
        private DepthOfField _depthOfField;

        public void Build()
        {
            GameObject volumeGo = new GameObject("GlobalVolume");
            volumeGo.transform.SetParent(transform, false);
            Volume volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;

            Tonemapping tonemapping = Add<Tonemapping>(profile);
            tonemapping.mode.value = TonemappingMode.ACES;

            Bloom bloom = Add<Bloom>(profile);
            bloom.threshold.value = 0.85f;
            bloom.intensity.value = 0.35f;
            bloom.scatter.value = 0.6f;

            ColorAdjustments color = Add<ColorAdjustments>(profile);
            color.postExposure.value = 0.15f;
            color.contrast.value = 12f;
            color.saturation.value = -6f;

            Vignette vignette = Add<Vignette>(profile);
            vignette.intensity.value = 0.28f;
            vignette.smoothness.value = 0.45f;

            _depthOfField = Add<DepthOfField>(profile);
            _depthOfField.mode.value = DepthOfFieldMode.Manual;
            SetBirthBlur(1f);

            // Fog + volumetric fog (the volumetric part also needs
            // "Volumetrics" enabled in the HDRP Pipeline Asset).
            Fog fog = Add<Fog>(profile);
            fog.meanFreePath.value = 160f;
            fog.albedo.value = new Color(0.80f, 0.85f, 0.90f);
            fog.enableVolumetricFog.value = true;

            // HDRI sky: only when the Poly Haven HDRI has been imported
            // as a Cubemap (Texture Shape = Cube; see README). Otherwise
            // HDRP falls back to its default sky settings.
            Cubemap skyCube = Resources.Load<Cubemap>("Environment/kloppenheim_06_puresky_1k");
            if (skyCube != null)
            {
                HDRISky sky = Add<HDRISky>(profile);
                sky.hdriSky.value = skyCube;
            }
        }

        /// <summary>
        /// t = 1: fully blurred newborn vision. t = 0: clear.
        /// Called every frame during Chapter 0's first seconds.
        /// </summary>
        public void SetBirthBlur(float t)
        {
            if (_depthOfField == null)
            {
                return;
            }
            t = Mathf.Clamp01(t);
            _depthOfField.active = t > 0.02f;
            _depthOfField.focusDistance.value = 2.0f;
            _depthOfField.nearFocusStart.value = Mathf.Lerp(0.6f, 0.05f, t);
            _depthOfField.nearFocusEnd.value = Mathf.Lerp(1.1f, 1.6f, t);
            _depthOfField.farFocusStart.value = Mathf.Lerp(30f, 2.4f, t);
            _depthOfField.farFocusEnd.value = Mathf.Lerp(80f, 5.0f, t);
        }

        /// <summary>From Chapter 1 on, the world is in focus.</summary>
        public void SetTownMode()
        {
            if (_depthOfField != null)
            {
                _depthOfField.active = false;
            }
        }

        private static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T component = ScriptableObject.CreateInstance<T>();
            component.active = true;
            profile.components.Add(component);
            return component;
        }
    }
}

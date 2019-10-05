using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using SerializableAttribute = System.SerializableAttribute;

namespace Kino.PostProcessing
{
    [Serializable, VolumeComponentMenu("Post-processing/Kino/Recolor")]
    public sealed class Recolor : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        #region Local enum and wrapper class

        public enum EdgeSource { Color, Depth, Normal }
        [Serializable] public sealed class EdgeSourceParameter : VolumeParameter<EdgeSource> {}

        #endregion

        #region Effect parameters

        public ColorParameter edgeColor = new ColorParameter(new Color(0, 0, 0, 0), false, true, true);
        public EdgeSourceParameter edgeSource = new EdgeSourceParameter { value = EdgeSource.Depth };
        public ClampedFloatParameter edgeThreshold = new ClampedFloatParameter(0.5f, 0, 1);
        public ClampedFloatParameter edgeContrast = new ClampedFloatParameter(0.5f, 0, 1);
        public GradientParameter fillGradient = new GradientParameter();
        public ClampedFloatParameter fillOpacity = new ClampedFloatParameter(0, 0, 1);

        #endregion

        #region Private members

        static class ShaderIDs
        {
            internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
            internal static readonly int EdgeThresholds = Shader.PropertyToID("_EdgeThresholds");
            internal static readonly int FillOpacity = Shader.PropertyToID("_FillOpacity");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        Material _material;
        GradientColorKey[] _gradientCache;

        #endregion

        #region IPostProcessComponent implementation

        public bool IsActive() =>
            _material != null && (edgeColor.value.a > 0 || fillOpacity.value > 0);

        #endregion

        #region CustomPostProcessVolumeComponent implementation

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Recolor");

        #if !UNITY_EDITOR
            // At runtime, copy gradient color keys only once on initialization.
            _gradientCache = fillGradient.value.colorKeys;
        #endif
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
        #if UNITY_EDITOR
            // In editor, copy gradient color keys every frame.
            _gradientCache = fillGradient.value.colorKeys;
        #endif

            Vector2 edgeThresh;

            if (edgeSource == EdgeSource.Depth)
            {
                var thresh = 1 / Mathf.Lerp(1000, 1, edgeThreshold.value);
                var scaler = 1 + 2 / (1.01f - edgeContrast.value);
                edgeThresh = new Vector2(thresh, thresh * scaler);
            }
            else // Depth & Color
            {
                var t1 = edgeThreshold.value;
                var t2 = t1 + 1.01f - edgeContrast.value;
                edgeThresh = new Vector2(t1, t2);
            }

            _material.SetColor(ShaderIDs.EdgeColor, edgeColor.value);
            _material.SetVector(ShaderIDs.EdgeThresholds, edgeThresh);
            _material.SetFloat(ShaderIDs.FillOpacity, fillOpacity.value);
            GradientUtility.SetColorKeys(_material, _gradientCache);

            var pass = (int)edgeSource.value;
            if (fillOpacity.value > 0 && _gradientCache.Length > 3) pass += 3;
            if (fillGradient.value.mode == GradientMode.Blend) pass += 6;

            // Blit to destRT with the overlay shader.
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);
            HDUtils.DrawFullScreen(cmd, _material, destRT, null, pass);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        #endregion
    }
}

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Effect settings

    [System.Serializable]
    [PostProcess(typeof(RecolorRenderer), PostProcessEvent.BeforeStack, "Kino/Recolor")]
    public sealed class Recolor : PostProcessEffectSettings
    {
        public enum EdgeSource { Color, Depth, Normal }

        [System.Serializable]
        public sealed class EdgeSourceParameter : ParameterOverride<EdgeSource> {}

        public ColorParameter edgeColor = new ColorParameter { value = new Color(0, 0, 0, 0) };

        public EdgeSourceParameter edgeSource = new EdgeSourceParameter { value = EdgeSource.Depth };

        [Range(0, 1)] public FloatParameter edgeThreshold = new FloatParameter { value = 0.5f };

        [Range(0, 1)] public FloatParameter edgeContrast = new FloatParameter { value = 0.5f };

        public GradientParameter fillGradient = new GradientParameter();

        [Range(0, 1)] public FloatParameter fillOpacity = new FloatParameter { value = 0 };
    }

    #endregion

    #region Effect renderer

    sealed class RecolorRenderer : PostProcessEffectRenderer<Recolor>
    {
        static class ShaderIDs
        {
            internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
            internal static readonly int EdgeThresholds = Shader.PropertyToID("_EdgeThresholds");
            internal static readonly int FillOpacity = Shader.PropertyToID("_FillOpacity");
        }

        GradientColorKey[] _gradientCache;

        Vector2 EdgeThresholdVector {
            get {
                if (settings.edgeSource == Recolor.EdgeSource.Depth)
                {
                    var thresh = 1 / Mathf.Lerp(1000, 1, settings.edgeThreshold);
                    var scaler = 1 + 2 / (1.01f - settings.edgeContrast);
                    return new Vector2(thresh, thresh * scaler);
                }
                else // Depth & Color
                {
                    var thresh = settings.edgeThreshold;
                    return new Vector2(thresh, thresh + 1.01f - settings.edgeContrast);
                }
            }
        }

        public override void Init()
        {
        #if !UNITY_EDITOR
            // At runtime, copy gradient color keys only once on initialization.
            _gradientCache = settings.fillGradient.value.colorKeys;
        #endif
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return settings.edgeSource == Recolor.EdgeSource.Depth ?
                DepthTextureMode.Depth : DepthTextureMode.None;
        }

        public override void Render(PostProcessRenderContext context)
        {
        #if UNITY_EDITOR
            // In editor, copy gradient color keys every frame.
            _gradientCache = settings.fillGradient.value.colorKeys;
        #endif

            var cmd = context.command;
            cmd.BeginSample("Recolor");

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Recolor"));
            sheet.properties.SetColor(ShaderIDs.EdgeColor, settings.edgeColor);
            sheet.properties.SetVector(ShaderIDs.EdgeThresholds, EdgeThresholdVector);
            sheet.properties.SetFloat(ShaderIDs.FillOpacity, settings.fillOpacity);
            GradientUtility.SetColorKeys(sheet, _gradientCache);

            var pass = (int)settings.edgeSource.value;
            if (settings.fillOpacity > 0 && _gradientCache.Length > 3) pass += 3;
            if (settings.fillGradient.value.mode == GradientMode.Blend) pass += 6;
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, pass);

            cmd.EndSample("Recolor");
        }
    }

    #endregion
}

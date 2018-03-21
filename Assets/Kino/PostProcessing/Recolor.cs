using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Local types

    public enum EdgeSource { Color, Depth, Normal }

    [System.Serializable]
    public sealed class EdgeSourceParameter : ParameterOverride<EdgeSource> {}

    [System.Serializable]
    public sealed class GradientParameter : ParameterOverride<Gradient> {}

    #endregion

    #region Effect settings

    [System.Serializable]
    [PostProcess(typeof(RecolorRenderer), PostProcessEvent.BeforeStack, "Kino/Recolor")]
    public sealed class Recolor : PostProcessEffectSettings
    {
        public ColorParameter edgeColor =
            new ColorParameter { value = new Color(0, 0, 0, 0) };

        public EdgeSourceParameter edgeSource =
            new EdgeSourceParameter { value = EdgeSource.Depth };

        [Range(0, 1)] public FloatParameter edgeThreshold =
            new FloatParameter { value = 0.5f };

        [Range(0, 1)] public FloatParameter edgeContrast =
            new FloatParameter { value = 0.5f };

        public GradientParameter fillGradient =
            new GradientParameter{ value = new Gradient() };

        [Range(0, 1)] public FloatParameter fillOpacity =
            new FloatParameter { value = 0 };
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

        int[] _fillKeyIDs;

        Vector4 KeyToVector(GradientColorKey key)
        {
            var c = key.color.linear;
            return new Vector4(c.r, c.g, c.b, key.time);
        }

        Vector2 EdgeThresholdVector {
            get {
                if (settings.edgeSource == EdgeSource.Depth)
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
            _fillKeyIDs = new int[8];
            for (var i = 0; i < 8; i++)
                _fillKeyIDs[i] = Shader.PropertyToID("_FillKey" + i);
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Recolor");

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Recolor"));
            sheet.properties.SetColor(ShaderIDs.EdgeColor, settings.edgeColor);
            sheet.properties.SetVector(ShaderIDs.EdgeThresholds, EdgeThresholdVector);
            sheet.properties.SetFloat(ShaderIDs.FillOpacity, settings.fillOpacity);

            var colorKeys = settings.fillGradient.value.colorKeys;
            for (var i = 0; i < 8; i++)
                sheet.properties.SetVector(
                    _fillKeyIDs[i],
                    KeyToVector(colorKeys[Mathf.Min(i, colorKeys.Length - 1)])
                );

            var pass = (int)settings.edgeSource.value;
            if (settings.fillOpacity > 0 && colorKeys.Length > 3) pass += 3;
            if (settings.fillGradient.value.mode == GradientMode.Blend) pass += 6;
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, pass);

            cmd.EndSample("Recolor");
        }
    }

    #endregion
}

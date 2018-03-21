using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    public enum ContourMode { Color, Depth, Normal }

    [System.Serializable]
    public sealed class ContourModeParameter : ParameterOverride<ContourMode> {}

    [System.Serializable]
    [PostProcess(typeof(ContourRenderer), PostProcessEvent.BeforeStack, "Kino/Contour")]
    public sealed class Contour : PostProcessEffectSettings
    {
        public ContourModeParameter mode = new ContourModeParameter { value = ContourMode.Depth };
        public ColorParameter lineColor = new ColorParameter { value = new Color(0, 0, 0, 0) };
        public ColorParameter backgroundColor = new ColorParameter { value = new Color(1, 1, 1, 0) };
        [Range(0, 1)] public FloatParameter threshold = new FloatParameter { value = 0.5f };
        [Range(0, 1)] public FloatParameter contrast = new FloatParameter { value = 0.5f };
    }

    sealed class ContourRenderer : PostProcessEffectRenderer<Contour>
    {
        static class ShaderIDs
        {
            internal static readonly int LineColor = Shader.PropertyToID("_LineColor");
            internal static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");
            internal static readonly int Thresholds = Shader.PropertyToID("_Thresholds");
        }

        Vector2 ThresholdVector {
            get {
                if (settings.mode == ContourMode.Depth)
                {
                    var thresh = 1 / Mathf.Lerp(1000, 1, settings.threshold);
                    var scaler = 1 + 2 / (1.01f - settings.contrast);
                    return new Vector2(thresh, thresh * scaler);
                }
                else // Depth & Color
                {
                    var thresh = settings.threshold;
                    return new Vector2(thresh, thresh + 1.01f - settings.contrast);
                }
            }
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Contour");

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Contour"));
            sheet.properties.SetColor(ShaderIDs.LineColor, settings.lineColor);
            sheet.properties.SetColor(ShaderIDs.BackgroundColor, settings.backgroundColor);
            sheet.properties.SetVector(ShaderIDs.Thresholds, ThresholdVector);

            var pass = (int)settings.mode.value;
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, pass);

            cmd.EndSample("Contour");
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    [System.Serializable]
    [PostProcess(typeof(ContourRenderer), PostProcessEvent.BeforeStack, "Kino/Contour")]
    public sealed class Contour : PostProcessEffectSettings
    {
        public ColorParameter lineColor = new ColorParameter { value = new Color(0, 0, 0, 0) };
        public ColorParameter backgroundColor = new ColorParameter { value = new Color(1, 1, 1, 0) };
        [Range(0, 1)] public FloatParameter threshold = new FloatParameter { value = 0.5f };
        [Range(0, 1)] public FloatParameter contrast = new FloatParameter { value = 0.5f };
    }

    public sealed class ContourRenderer : PostProcessEffectRenderer<Contour>
    {
        static class ShaderIDs
        {
            internal static readonly int LineColor = Shader.PropertyToID("_LineColor");
            internal static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");
            internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
            internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Contour");

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Contour"));

            sheet.properties.SetColor(ShaderIDs.LineColor, settings.lineColor);
            sheet.properties.SetColor(ShaderIDs.BackgroundColor, settings.backgroundColor);
            sheet.properties.SetFloat(ShaderIDs.Threshold, settings.threshold);
            sheet.properties.SetFloat(ShaderIDs.Contrast, settings.contrast);

            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            cmd.EndSample("Contour");
        }
    }
}

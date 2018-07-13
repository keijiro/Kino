using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Effect settings

    [System.Serializable]
    [PostProcess(typeof(IsolineRenderer), PostProcessEvent.BeforeStack, "Kino/Isoline")]
    public sealed class Isoline : PostProcessEffectSettings
    {
        #region Nested type definitions

        public enum ModulationMode { None, Frac, Sin }

        [System.Serializable]
        public sealed class ModulationModeParameter :
            ParameterOverride<ModulationMode> {}

        #endregion

        #region Contour detector settings

        public Vector3Parameter baseAxis =
            new Vector3Parameter { value = Vector3.up };

        public FloatParameter lineInterval =
            new FloatParameter { value = 1 };

        public FloatParameter lineOffset =
            new FloatParameter { value = 0 };

        public FloatParameter lineScroll =
            new FloatParameter { value = 0 };

        #endregion

        #region Line appearance settings

        [ColorUsage(true, true)] public ColorParameter lineColor =
            new ColorParameter { value = new Color(1.5f, 0.2f, 0.2f, 1) };

        public FloatParameter lineWidth =
            new FloatParameter { value = 1.5f };

        [Range(0, 1)] public FloatParameter sourceContribution =
            new FloatParameter { value = 0 };

        public ColorParameter backgroundColor =
            new ColorParameter { value = new Color(0, 0, 0, 0) };

        #endregion

        #region Color modulation settings

        [Range(0, 1)] public FloatParameter modulationStrength =
            new FloatParameter { value = 0 };

        [Range(0, 1)] public FloatParameter modulationFrequency =
            new FloatParameter { value = 0.1f };

        [Range(0, 1)] public FloatParameter modulationWidth =
            new FloatParameter { value = 0.1f };

        public FloatParameter modulationOffset =
            new FloatParameter { value = 0 };

        public FloatParameter modulationScroll =
            new FloatParameter { value = 1 };

        #endregion
    }

    #endregion

    #region Effect renderer

    sealed class IsolineRenderer : PostProcessEffectRenderer<Isoline>
    {
        static class ShaderIDs
        {
            internal static readonly int InverseView = Shader.PropertyToID("_InverseView");
            internal static readonly int LineColor = Shader.PropertyToID("_LineColor");
            internal static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");
            internal static readonly int ContourAxis = Shader.PropertyToID("_ContourAxis");
            internal static readonly int ContourParams = Shader.PropertyToID("_ContourParams");
            internal static readonly int ModParams = Shader.PropertyToID("_ModParams");
        }

        Vector4 MakeVector(Vector3 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, w);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Isoline");

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Isoline"));

            sheet.properties.SetMatrix(ShaderIDs.InverseView, context.camera.cameraToWorldMatrix);

            sheet.properties.SetColor(ShaderIDs.LineColor, settings.lineColor);
            sheet.properties.SetColor(ShaderIDs.BackgroundColor, settings.backgroundColor);

            var offs = Time.time * settings.lineScroll + settings.lineOffset;
            sheet.properties.SetVector(ShaderIDs.ContourAxis, MakeVector(
                settings.baseAxis.value.normalized, -offs
            ));

            sheet.properties.SetVector(ShaderIDs.ContourParams, new Vector4(
                settings.lineInterval, settings.lineScroll,
                settings.lineWidth, settings.sourceContribution
            ));

            offs = Time.time * settings.modulationScroll + settings.modulationOffset;
            sheet.properties.SetVector(ShaderIDs.ModParams, new Vector4(
                settings.modulationStrength, settings.modulationFrequency,
                settings.modulationWidth, -offs
            ));

            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            cmd.EndSample("Isoline");
        }
    }

    #endregion
}

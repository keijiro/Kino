using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Effect settings

    [System.Serializable]
    [PostProcess(typeof(InvertRenderer), PostProcessEvent.AfterStack, "Kino/Invert")]
    public sealed class Invert : PostProcessEffectSettings
    {
        [Range(0, 1)] public FloatParameter strength = new FloatParameter();
    }

    #endregion

    #region Effect renderer

    sealed class InvertRenderer : PostProcessEffectRenderer<Invert>
    {
        static class ShaderIDs
        {
            internal static readonly int Strength = Shader.PropertyToID("_Strength");
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Invert"));
            sheet.properties.SetFloat(ShaderIDs.Strength, settings.strength);

            var cmd = context.command;
            cmd.BeginSample("Invert");
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            cmd.EndSample("Invert");
        }
    }

    #endregion
}

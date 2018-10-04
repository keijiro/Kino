using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Effect settings

    [System.Serializable]
    [PostProcess(typeof(SharpenRenderer), PostProcessEvent.AfterStack, "Kino/Sharpen")]
    public sealed class Sharpen : PostProcessEffectSettings
    {
        [Range(0, 1)] public FloatParameter strength = new FloatParameter();
    }

    #endregion

    #region Effect renderer

    sealed class SharpenRenderer : PostProcessEffectRenderer<Sharpen>
    {
        static class ShaderIDs
        {
            internal static readonly int Strength = Shader.PropertyToID("_Strength");
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Sharpen"));
            sheet.properties.SetFloat(ShaderIDs.Strength, settings.strength);

            var cmd = context.command;
            cmd.BeginSample("Sharpen");
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            cmd.EndSample("Sharpen");
        }
    }

    #endregion
}

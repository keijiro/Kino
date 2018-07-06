using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Effect settings

    [System.Serializable]
    [PostProcess(typeof(OverlayRenderer), PostProcessEvent.AfterStack, "Kino/Overlay")]
    public sealed class Overlay : PostProcessEffectSettings
    {
        public GradientParameter gradient = new GradientParameter();

        [Range(0, 1)] public FloatParameter opacity = new FloatParameter { value = 0 };

        [Range(-180, 180)] public FloatParameter angle = new FloatParameter { value = 0 };
    }

    #endregion

    #region Effect renderer

    sealed class OverlayRenderer : PostProcessEffectRenderer<Overlay>
    {
        static class ShaderIDs
        {
            internal static readonly int Opacity = Shader.PropertyToID("_Opacity");
            internal static readonly int Direction = Shader.PropertyToID("_Direction");
        }

        GradientColorKey[] _gradientCache;

        Vector2 DirectionVector {
            get {
                var rad = Mathf.Deg2Rad * settings.angle;
                return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            }
        }

        public override void Init()
        {
        #if !UNITY_EDITOR
            // At runtime, copy gradient color keys only once on initialization.
            _gradientCache = settings.gradient.value.colorKeys;
        #endif
        }

        public override void Render(PostProcessRenderContext context)
        {
        #if UNITY_EDITOR
            // In editor, copy gradient color keys every frame.
            _gradientCache = settings.gradient.value.colorKeys;
        #endif

            var cmd = context.command;
            cmd.BeginSample("Overlay");

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Overlay"));
            sheet.properties.SetFloat(ShaderIDs.Opacity, settings.opacity);
            sheet.properties.SetVector(ShaderIDs.Direction, DirectionVector);
            GradientUtility.SetColorKeys(sheet, _gradientCache);

            var pass = _gradientCache.Length > 3 ? 1 : 0;
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, pass);

            cmd.EndSample("Overlay");
        }
    }

    #endregion
}

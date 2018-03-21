using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Effect settings

    [System.Serializable]
    [PostProcess(typeof(OverlayRenderer), PostProcessEvent.AfterStack, "Kino/Overlay")]
    public sealed class Overlay : PostProcessEffectSettings
    {
        public GradientParameter gradient = new GradientParameter{ value = new Gradient() };
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

        int[] _gradientKeyIDs;

        Vector2 DirectionVector {
            get {
                var rad = Mathf.Deg2Rad * settings.angle;
                return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            }
        }

        Vector4 KeyToVector(GradientColorKey key)
        {
            var c = key.color.linear;
            return new Vector4(c.r, c.g, c.b, key.time);
        }

        public override void Init()
        {
            _gradientKeyIDs = new int[8];
            for (var i = 0; i < 8; i++)
                _gradientKeyIDs[i] = Shader.PropertyToID("_GradientKey" + i);
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Overlay");

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Overlay"));
            sheet.properties.SetFloat(ShaderIDs.Opacity, settings.opacity);
            sheet.properties.SetVector(ShaderIDs.Direction, DirectionVector);

            var colorKeys = settings.gradient.value.colorKeys;
            for (var i = 0; i < 8; i++)
                sheet.properties.SetVector(
                    _gradientKeyIDs[i],
                    KeyToVector(colorKeys[Mathf.Min(i, colorKeys.Length - 1)])
                );

            var pass = colorKeys.Length > 3 ? 1 : 0;
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, pass);

            cmd.EndSample("Overlay");
        }
    }

    #endregion
}

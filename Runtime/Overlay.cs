using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    #region Effect settings

    // Base implementation (shared with PreStackOverlay)
    public class OverlayBase : PostProcessEffectSettings
    {
        #region Nested types

        public enum Source { Color, Gradient, Texture }
        public enum BlendMode { Normal, Screen, Overlay, Multiply, SoftLight, HardLight }

        [System.Serializable] public sealed class SourceParameter : ParameterOverride<Source> {}
        [System.Serializable] public sealed class BlendModeParameter : ParameterOverride<BlendMode> {}

        #endregion

        #region Common parameters

        public SourceParameter source = new SourceParameter { value = Source.Gradient };
        public BlendModeParameter blendMode = new BlendModeParameter { value = BlendMode.Overlay };
        [Range(0, 1)] public FloatParameter opacity = new FloatParameter { value = 0 };

        #endregion

        #region Single color mode parameters

        [ColorUsage(false)] public ColorParameter color = new ColorParameter() { value = Color.red };

        #endregion

        #region Gradient mode parameters

        public GradientParameter gradient = new GradientParameter();
        [Range(-180, 180)] public FloatParameter angle = new FloatParameter { value = 0 };

        #endregion

        #region Texture mode parameters

        public TextureParameter texture = new TextureParameter();
        public BoolParameter sourceAlpha = new BoolParameter { value = true };

        #endregion
    }

    // Specialization for the post-stack overlay effect
    [System.Serializable]
    [PostProcess(typeof(OverlayRenderer), PostProcessEvent.AfterStack, "Kino/Overlay")]
    public sealed class Overlay : OverlayBase {}

    #endregion

    #region Effect renderer

    // Base implementation (shared with PreStackOverlayRenderer)
    public class OverlayRendererBase<T> : PostProcessEffectRenderer<T> where T : OverlayBase
    {
        static class ShaderIDs
        {
            internal static readonly int Color = Shader.PropertyToID("_Color");
            internal static readonly int Direction = Shader.PropertyToID("_Direction");
            internal static readonly int Opacity = Shader.PropertyToID("_Opacity");
            internal static readonly int SourceTex = Shader.PropertyToID("_SourceTex");
            internal static readonly int UseSourceAlpha = Shader.PropertyToID("_UseSourceAlpha");
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
            // Skip it when opacity is zero.
            if (settings.opacity == 0) return;

            // Common parameters
            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/PostProcessing/Overlay"));
            sheet.properties.SetFloat(ShaderIDs.Opacity, settings.opacity);

            var pass = (int)settings.blendMode.value * 3;

            if (settings.source == Overlay.Source.Color)
            {
                // Single color mode parameters
                sheet.properties.SetColor(ShaderIDs.Color, settings.color);
                sheet.properties.SetTexture(ShaderIDs.SourceTex, RuntimeUtilities.whiteTexture);
                sheet.properties.SetFloat(ShaderIDs.UseSourceAlpha, 0);
            }
            else if (settings.source == Overlay.Source.Gradient)
            {
            #if UNITY_EDITOR
                // In editor, copy gradient color keys every frame.
                _gradientCache = settings.gradient.value.colorKeys;
            #endif

                // Gradient mode parameters
                sheet.properties.SetVector(ShaderIDs.Direction, DirectionVector);
                GradientUtility.SetColorKeys(sheet, _gradientCache);
                pass += _gradientCache.Length > 3 ? 2 : 1;
            }
            else // Overlay.Source.Texture
            {
                // Skip it when no texture is given.
                if (settings.texture.value == null) return;

                // Texture mode parameters
                sheet.properties.SetColor(ShaderIDs.Color, Color.white);
                sheet.properties.SetTexture(ShaderIDs.SourceTex, settings.texture);
                sheet.properties.SetFloat(ShaderIDs.UseSourceAlpha, settings.sourceAlpha ? 1 : 0);
            }

            // Blit with the shader
            var cmd = context.command;
            cmd.BeginSample("Overlay");
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, pass);
            cmd.EndSample("Overlay");
        }
    }

    // Specialization for the post-stack overlay effect
    public sealed class OverlayRenderer : OverlayRendererBase<Overlay> {}

    #endregion
}

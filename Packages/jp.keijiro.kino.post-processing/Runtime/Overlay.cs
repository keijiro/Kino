using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using SerializableAttribute = System.SerializableAttribute;

namespace Kino.PostProcessing
{
    [Serializable, VolumeComponentMenu("Post-processing/Kino/Overlay")]
    public sealed class Overlay : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        #region Local enums and wrapper classes

        public enum SourceType { Color, Gradient, Texture }
        public enum BlendMode { Normal, Screen, Overlay, Multiply, SoftLight, HardLight }

        [Serializable] public sealed class SourceTypeParameter : VolumeParameter<SourceType> {}
        [Serializable] public sealed class BlendModeParameter : VolumeParameter<BlendMode> {}

        #endregion

        #region Common parameters

        public SourceTypeParameter sourceType = new SourceTypeParameter { value = SourceType.Gradient };
        public BlendModeParameter blendMode = new BlendModeParameter { value = BlendMode.Overlay };
        public ClampedFloatParameter opacity = new ClampedFloatParameter(0, 0, 1);

        #endregion

        #region Single color mode paremter

        public ColorParameter color = new ColorParameter(Color.red, false, false, true);

        #endregion

        #region Gradient mode parameters

        public GradientParameter gradient = new GradientParameter();
        public ClampedFloatParameter angle = new ClampedFloatParameter(0, -180, 180);

        #endregion

        #region Texture mode parameters

        public TextureParameter texture = new TextureParameter(null);
        public BoolParameter sourceAlpha = new BoolParameter(true);

        #endregion

        #region Private members

        static class ShaderIDs
        {
            internal static readonly int Color = Shader.PropertyToID("_Color");
            internal static readonly int Direction = Shader.PropertyToID("_Direction");
            internal static readonly int Opacity = Shader.PropertyToID("_Opacity");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
            internal static readonly int OverlayTexture = Shader.PropertyToID("_OverlayTexture");
            internal static readonly int UseTextureAlpha = Shader.PropertyToID("_UseTextureAlpha");
        }

        Material _material;
        GradientColorKey[] _gradientCache;

        #endregion

        #region IPostProcessComponent implementation

        public bool IsActive() => _material != null && opacity.value > 0;

        #endregion

        #region CustomPostProcessVolumeComponent implementation

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Overlay");

        #if !UNITY_EDITOR
            // At runtime, copy gradient color keys only once on initialization.
            _gradientCache = gradient.value.colorKeys;
        #endif
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            _material.SetFloat(ShaderIDs.Opacity, opacity.value);

            var pass = (int)blendMode.value * 3;

            if (sourceType == Overlay.SourceType.Color)
            {
                // Single color mode parameters
                _material.SetColor(ShaderIDs.Color, color.value);
                _material.SetTexture(ShaderIDs.OverlayTexture, Texture2D.whiteTexture);
                _material.SetFloat(ShaderIDs.UseTextureAlpha, 0);
            }
            else if (sourceType == Overlay.SourceType.Gradient)
            {
            #if UNITY_EDITOR
                // In editor, copy gradient color keys every frame.
                _gradientCache = gradient.value.colorKeys;
            #endif

                // Gradient direction vector
                var rad = Mathf.Deg2Rad * angle.value;
                var dir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));

                // Gradient mode parameters
                _material.SetVector(ShaderIDs.Direction, dir);
                GradientUtility.SetColorKeys(_material, _gradientCache);
                pass += _gradientCache.Length > 3 ? 2 : 1;
            }
            else // Overlay.Source.Texture
            {
                // Skip when no texture is given.
                if (texture.value == null) return;

                // Texture mode parameters
                _material.SetColor(ShaderIDs.Color, Color.white);
                _material.SetTexture(ShaderIDs.OverlayTexture, texture.value);
                _material.SetFloat(ShaderIDs.UseTextureAlpha, sourceAlpha.value ? 1 : 0);
            }

            // Blit to destRT with the overlay shader.
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);
            HDUtils.DrawFullScreen(cmd, _material, destRT, null, pass);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        #endregion
    }
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using SerializableAttribute = System.SerializableAttribute;

namespace Kino.PostProcessing
{
    [Serializable, VolumeComponentMenu("Post-processing/Kino/Recolor")]
    public sealed class Recolor : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        #region Local enum and wrapper class

        public enum EdgeSource { Color, Depth, Normal }
        [Serializable] public sealed class EdgeSourceParameter : VolumeParameter<EdgeSource> {}

        public enum DitherType { Bayer2x2, Bayer3x3, Bayer4x4, Bayer8x8 }
        [Serializable] public sealed class DitherTypeParameter : VolumeParameter<DitherType> {}

        #endregion

        #region Effect parameters

        public ColorParameter edgeColor = new ColorParameter(new Color(0, 0, 0, 0), false, true, true);
        public EdgeSourceParameter edgeSource = new EdgeSourceParameter { value = EdgeSource.Depth };
        public ClampedFloatParameter edgeThreshold = new ClampedFloatParameter(0.5f, 0, 1);
        public ClampedFloatParameter edgeContrast = new ClampedFloatParameter(0.5f, 0, 1);
        public GradientParameter fillGradient = new GradientParameter();
        public ClampedFloatParameter fillOpacity = new ClampedFloatParameter(0, 0, 1);
        public DitherTypeParameter ditherType = new DitherTypeParameter { value = DitherType.Bayer4x4 };
        public ClampedFloatParameter ditherStrength = new ClampedFloatParameter(0, 0, 1);

        #endregion

        #region Private members

        static class ShaderIDs
        {
            internal static readonly int DitherStrength = Shader.PropertyToID("_DitherStrength");
            internal static readonly int DitherTexture = Shader.PropertyToID("_DitherTexture");
            internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
            internal static readonly int EdgeThresholds = Shader.PropertyToID("_EdgeThresholds");
            internal static readonly int FillOpacity = Shader.PropertyToID("_FillOpacity");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        Material _material;

        Gradient _cachedGradient;
        GradientColorKey [] _cachedColorKeys;

        DitherType _ditherType;
        Texture2D _ditherTexture;

        #endregion

        #region IPostProcessComponent implementation

        public bool IsActive() =>
            _material != null && (edgeColor.value.a > 0 || fillOpacity.value > 0);

        #endregion

        #region CustomPostProcessVolumeComponent implementation

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Recolor");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_ditherType != ditherType.value || _ditherTexture == null)
            {
                CoreUtils.Destroy(_ditherTexture);
                _ditherType = ditherType.value;
                _ditherTexture = GenerateDitherTexture(_ditherType);
            }

#if UNITY_EDITOR
            // In Editor, the gradient will be modified without any hint,
            // so we have to copy the color keys every frame.
            if (true)
#else
            // In Player, we assume no one can modify gradients in profiles,
            // so we update the cache only when the reference was updated.
            if (_cachedGradient != fillGradient.value)
#endif
            {
                _cachedGradient = fillGradient.value;
                _cachedColorKeys = _cachedGradient.colorKeys;
            }

            Vector2 edgeThresh;

            if (edgeSource.value == EdgeSource.Depth)
            {
                var thresh = 1 / Mathf.Lerp(1000, 1, edgeThreshold.value);
                var scaler = 1 + 2 / (1.01f - edgeContrast.value);
                edgeThresh = new Vector2(thresh, thresh * scaler);
            }
            else // Depth & Color
            {
                var t1 = edgeThreshold.value;
                var t2 = t1 + 1.01f - edgeContrast.value;
                edgeThresh = new Vector2(t1, t2);
            }

            _material.SetColor(ShaderIDs.EdgeColor, edgeColor.value);
            _material.SetVector(ShaderIDs.EdgeThresholds, edgeThresh);
            _material.SetFloat(ShaderIDs.FillOpacity, fillOpacity.value);
            GradientUtility.SetColorKeys(_material, _cachedColorKeys);

            _material.SetTexture(ShaderIDs.DitherTexture, _ditherTexture);
            _material.SetFloat(ShaderIDs.DitherStrength, ditherStrength.value);

            var pass = (int)edgeSource.value;
            if (fillOpacity.value > 0 && _cachedColorKeys.Length > 4) pass += 3;
            if (fillGradient.value.mode == GradientMode.Blend) pass += 6;

            // Blit to destRT with the overlay shader.
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);
            HDUtils.DrawFullScreen(cmd, _material, destRT, null, pass);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
            CoreUtils.Destroy(_ditherTexture);
        }

        #endregion

        #region Dither texture generator

        static Texture2D GenerateDitherTexture(DitherType type)
        {
            if (type == DitherType.Bayer2x2)
            {
                var tex = new Texture2D(2, 2, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {0, 170, 255, 85});
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer3x3)
            {
                var tex = new Texture2D(3, 3, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {
                    0, 223, 95, 191, 159, 63, 127, 31, 255
                });
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer4x4)
            {
                var tex = new Texture2D(4, 4, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {
                    0, 136, 34, 170, 204, 68, 238, 102,
                    51, 187, 17, 153, 255, 119, 221, 85
                });
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer8x8)
            {
                var tex = new Texture2D(8, 8, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {
                    0, 194, 48, 242, 12, 206, 60, 255,
                    129, 64, 178, 113, 141, 76, 190, 125,
                    32, 226, 16, 210, 44, 238, 28, 222,
                    161, 97, 145, 80, 174, 109, 157, 93,
                    8, 202, 56, 250, 4, 198, 52, 246,
                    137, 72, 186, 121, 133, 68, 182, 117,
                    40, 234, 24, 218, 36, 230, 20, 214,
                    170, 105, 153, 89, 165, 101, 149, 85
                });
                tex.Apply();
                return tex;
            }

            return null;
        }

        #endregion
    }
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kino.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Kino/Utility")]
    public sealed class Utility : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter saturation = new ClampedFloatParameter(1, 0, 2);
        public ClampedFloatParameter hueShift = new ClampedFloatParameter(0, -1, 1);
        public ClampedFloatParameter invert = new ClampedFloatParameter(0, 0, 1);
        public ColorParameter fade = new ColorParameter(new Color(0, 0, 0, 0), false, true, true);

        Material _material;

        static class ShaderIDs
        {
            internal static readonly int FadeColor = Shader.PropertyToID("_FadeColor");
            internal static readonly int HueShift = Shader.PropertyToID("_HueShift");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
            internal static readonly int Invert = Shader.PropertyToID("_Invert");
            internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
        }

        public bool IsActive() => _material != null &&
            (saturation.value != 1 || hueShift.value != 0 || invert.value > 0 || fade.value.a > 0);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Utility");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            _material.SetColor(ShaderIDs.FadeColor, fade.value);
            _material.SetFloat(ShaderIDs.HueShift, hueShift.value);
            _material.SetFloat(ShaderIDs.Invert, invert.value);
            _material.SetFloat(ShaderIDs.Saturation, saturation.value);
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);

            HDUtils.DrawFullScreen(cmd, _material, destRT);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }
    }
}

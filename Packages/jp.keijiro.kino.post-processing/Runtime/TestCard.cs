using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kino.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Kino/Test Card")]
    public sealed class TestCard : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        #region Effect parameters

        public ClampedFloatParameter opacity = new ClampedFloatParameter(0, 0, 1);

        #endregion

        #region Private members

        static class ShaderIDs
        {
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
            internal static readonly int Opacity = Shader.PropertyToID("_Opacity");
        }

        Material _material;

        #endregion

        #region IPostProcessComponent implementation

        public bool IsActive() => _material != null && opacity.value > 0;

        #endregion

        #region CustomPostProcessVolumeComponent implementation

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/TestCard");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            _material.SetFloat(ShaderIDs.Opacity, opacity.value);
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);
            HDUtils.DrawFullScreen(cmd, _material, destRT, null, 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        #endregion
    }
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kino.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Kino/Slice")]
    public sealed class Slice : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        #region Effect parameters

        public FloatParameter rowCount = new FloatParameter(30);
        public ClampedFloatParameter angle = new ClampedFloatParameter(0, -90, 90);
        public ClampedFloatParameter displacement = new ClampedFloatParameter(0, -1, 1);
        public IntParameter randomSeed = new IntParameter(0);

        #endregion

        #region Private members

        static class ShaderIDs
        {
            internal static readonly int Direction = Shader.PropertyToID("_Direction");
            internal static readonly int Displacement = Shader.PropertyToID("_Displacement");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
            internal static readonly int Rows = Shader.PropertyToID("_Rows");
            internal static readonly int Seed = Shader.PropertyToID("_Seed");
        }

        Material _material;

        #endregion

        #region IPostProcessComponent implementation

        public bool IsActive() => _material != null && displacement.value != 0;

        #endregion

        #region CustomPostProcessVolumeComponent implementation

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Slice");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            var rad = angle.value * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            var seed = (uint)randomSeed.value;
            seed = (seed << 16) | (seed >> 16);

            _material.SetVector(ShaderIDs.Direction, dir);
            _material.SetFloat(ShaderIDs.Displacement, displacement.value);
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);
            _material.SetFloat(ShaderIDs.Rows, rowCount.value);
            _material.SetInt(ShaderIDs.Seed, (int)seed);

            HDUtils.DrawFullScreen(cmd, _material, destRT, null, 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        #endregion
    }
}

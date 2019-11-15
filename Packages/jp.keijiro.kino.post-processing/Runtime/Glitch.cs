using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kino.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Kino/Glitch")]
    public sealed class Glitch : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter drift = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter jitter = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter jump = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter shake = new ClampedFloatParameter(0, 0, 1);

        Material _material;

        float _prevTime;
        float _jumpTime;

        static class ShaderIDs
        {
            internal static readonly int Drift = Shader.PropertyToID("_Drift");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
            internal static readonly int Jitter = Shader.PropertyToID("_Jitter");
            internal static readonly int Jump = Shader.PropertyToID("_Jump");
            internal static readonly int Seed = Shader.PropertyToID("_Seed");
            internal static readonly int Shake = Shader.PropertyToID("_Shake");
        }

        public bool IsActive() => _material != null &&
            (drift.value > 0 || jitter.value > 0 || jump.value > 0 || shake.value > 0);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Glitch");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            // Update the time parameters.
            var time = Time.time;
            _jumpTime += (time - _prevTime) * jump.value * 11.3f;
            _prevTime = time;

            // Drift parameters (time, displacement)
            var vdrift = new Vector2(
                time * 606.11f % (Mathf.PI * 2),
                drift.value * 0.04f
            );

            // Jitter parameters (threshold, displacement)
            var jv = jitter.value;
            var vjitter = new Vector3(
                Mathf.Max(0, 1.001f - jv * 1.2f),
                0.002f + jv * jv * jv * 0.05f
            );

            // Jump parameters (scroll, displacement)
            var vjump = new Vector2(_jumpTime, jump.value);

            // Invoke the shader.
            _material.SetInt(ShaderIDs.Seed, (int)(time * 10000));
            _material.SetVector(ShaderIDs.Drift, vdrift);
            _material.SetVector(ShaderIDs.Jitter, vjitter);
            _material.SetVector(ShaderIDs.Jump, vjump);
            _material.SetFloat(ShaderIDs.Shake, shake.value * 0.2f);
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);
            HDUtils.DrawFullScreen(cmd, _material, destRT);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }
    }
}

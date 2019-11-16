using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kino.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Kino/Glitch")]
    public sealed class Glitch : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter block = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter drift = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter jitter = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter jump = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter shake = new ClampedFloatParameter(0, 0, 1);

        Material _material;

        float _prevTime;
        float _jumpTime;

        float _blockTime;
        int _blockSeed1 = 71;
        int _blockSeed2 = 113;
        int _blockStride = 1;

        static class ShaderIDs
        {
            internal static readonly int BlockSeed1 = Shader.PropertyToID("_BlockSeed1");
            internal static readonly int BlockSeed2 = Shader.PropertyToID("_BlockSeed2");
            internal static readonly int BlockStrength = Shader.PropertyToID("_BlockStrength");
            internal static readonly int BlockStride = Shader.PropertyToID("_BlockStride");
            internal static readonly int Drift = Shader.PropertyToID("_Drift");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
            internal static readonly int Jitter = Shader.PropertyToID("_Jitter");
            internal static readonly int Jump = Shader.PropertyToID("_Jump");
            internal static readonly int Seed = Shader.PropertyToID("_Seed");
            internal static readonly int Shake = Shader.PropertyToID("_Shake");
        }

        public bool IsActive() => _material != null &&
            (block.value > 0 || drift.value > 0 || jitter.value > 0 || jump.value > 0 || shake.value > 0);

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
            var delta = time - _prevTime;
            _jumpTime += delta * jump.value * 11.3f;
            _prevTime = time;

            // Block parameters
            var block3 = block.value * block.value * block.value;

            // Shuffle block parameters every 1/30 seconds.
            _blockTime += delta * 60;
            if (_blockTime > 1)
            {
                if (Random.value < 0.09f) _blockSeed1 += 251;
                if (Random.value < 0.29f) _blockSeed2 += 373;
                if (Random.value < 0.25f) _blockStride = Random.Range(1, 32);
                _blockTime = 0;
            }

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
            _material.SetFloat(ShaderIDs.BlockStrength, block3);
            _material.SetInt(ShaderIDs.BlockStride, _blockStride);
            _material.SetInt(ShaderIDs.BlockSeed1, _blockSeed1);
            _material.SetInt(ShaderIDs.BlockSeed2, _blockSeed2);
            _material.SetVector(ShaderIDs.Drift, vdrift);
            _material.SetVector(ShaderIDs.Jitter, vjitter);
            _material.SetVector(ShaderIDs.Jump, vjump);
            _material.SetFloat(ShaderIDs.Shake, shake.value * 0.2f);
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);

            // Shader pass number
            var pass = 0;
            if (drift.value > 0 || jitter.value > 0 || jump.value > 0 || shake.value > 0) pass += 1;
            if (block.value > 0) pass += 2;

            // Blit
            HDUtils.DrawFullScreen(cmd, _material, destRT, null, pass);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }
    }
}

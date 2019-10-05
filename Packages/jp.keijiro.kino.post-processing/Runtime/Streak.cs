using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;
using SerializableAttribute = System.SerializableAttribute;
using System.Collections.Generic;

namespace Kino.PostProcessing
{
    [Serializable, VolumeComponentMenu("Post-processing/Kino/Streak")]
    public sealed class Streak : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        #region Effect parameters

        public ClampedFloatParameter threshold = new ClampedFloatParameter(1, 0, 5);
        public ClampedFloatParameter stretch = new ClampedFloatParameter(0.75f, 0, 1);
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0, 0, 1);
        public ColorParameter tint = new ColorParameter(new Color(0.55f, 0.55f, 1), false, false, true);

        #endregion

        #region Private members

        static class ShaderIDs
        {
            internal static readonly int Color = Shader.PropertyToID("_Color");
            internal static readonly int HighTexture = Shader.PropertyToID("_HighTexture");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
            internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
            internal static readonly int SourceTexture = Shader.PropertyToID("_SourceTexture");
            internal static readonly int Stretch = Shader.PropertyToID("_Stretch");
            internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
        }

        Material _material;
        MaterialPropertyBlock _prop;

        // Image pyramid storage
        // We have to use different pyramids for each camera, so we use a
        // dictionary and camera GUIDs as a key to store each pyramid.
        Dictionary<int, StreakPyramid> _pyramids;

        StreakPyramid GetPyramid(HDCamera camera)
        {
            StreakPyramid candid;
            var cameraID = camera.camera.GetInstanceID();

            if (_pyramids.TryGetValue(cameraID, out candid))
            {
                // Reallocate the RTs when the screen size was changed.
                if (!candid.CheckSize(camera)) candid.Reallocate(camera);
            }
            else
            {
                // No one found: Allocate a new pyramid.
                _pyramids[cameraID] = candid = new StreakPyramid(camera);
            }

            return candid;
        }

        #endregion

        #region IPostProcessComponent implementation

        public bool IsActive() => _material != null && intensity.value > 0;

        #endregion

        #region CustomPostProcessVolumeComponent implementation

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.BeforePostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Streak");
            _prop = new MaterialPropertyBlock();
            _pyramids = new Dictionary<int, StreakPyramid>();
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            var pyramid = GetPyramid(camera);

            // Common parameters
            _material.SetFloat("_Threshold", threshold.value);
            _material.SetFloat("_Stretch", stretch.value);
            _material.SetFloat("_Intensity", intensity.value);
            _material.SetColor("_Color", tint.value);
            _material.SetTexture("_SourceTexture", srcRT);

            // Source -> Prefilter -> MIP 0
            HDUtils.DrawFullScreen(cmd, _material, pyramid[0].down, _prop, 0);

            // Downsample
            var level = 1;
            for (; level < StreakPyramid.MaxMipLevel && pyramid[level].down != null; level++)
            {
                _prop.SetTexture(ShaderIDs.InputTexture, pyramid[level - 1].down);
                HDUtils.DrawFullScreen(cmd, _material, pyramid[level].down, _prop, 1);
            }

            // Upsample & combine
            var lastRT = pyramid[--level].down;
            for (level--; level >= 1; level--)
            {
                var mip = pyramid[level];
                _prop.SetTexture(ShaderIDs.InputTexture, lastRT);
                _prop.SetTexture(ShaderIDs.HighTexture, mip.down);
                HDUtils.DrawFullScreen(cmd, _material, mip.up, _prop, 2);
                lastRT = mip.up;
            }

            // Final composition
            _prop.SetTexture(ShaderIDs.InputTexture, lastRT);
            HDUtils.DrawFullScreen(cmd, _material, destRT, _prop, 3);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
            foreach (var pyramid in _pyramids.Values) pyramid.Release();
        }

        #endregion
    }

    #region Image pyramid class used in Streak effect

    sealed class StreakPyramid
    {
        public const int MaxMipLevel = 16;

        int _baseWidth, _baseHeight;
        readonly (RTHandle down, RTHandle up) [] _mips = new (RTHandle, RTHandle) [MaxMipLevel];

        public (RTHandle down, RTHandle up) this [int index]
        {
            get { return _mips[index]; }
        }

        public StreakPyramid(HDCamera camera)
        {
            Allocate(camera);
        }

        public bool CheckSize(HDCamera camera)
        {
            return _baseWidth == camera.actualWidth && _baseHeight == camera.actualHeight;
        }

        public void Reallocate(HDCamera camera)
        {
            Release();
            Allocate(camera);
        }

        public void Release()
        {
            foreach (var mip in _mips)
            {
                if (mip.down != null) RTHandles.Release(mip.down);
                if (mip.up   != null) RTHandles.Release(mip.up);
            }
        }

        void Allocate(HDCamera camera)
        {
            _baseWidth = camera.actualWidth;
            _baseHeight = camera.actualHeight;

            var width = _baseWidth;
            var height = _baseHeight / 2;

            const GraphicsFormat RTFormat = GraphicsFormat.R16G16B16A16_SFloat;

            _mips[0] = (RTHandles.Alloc(width, height, colorFormat: RTFormat), null);

            for (var i = 1; i < MaxMipLevel; i++)
            {
                width /= 2;
                _mips[i] = width < 4 ?  (null, null) :
                    (RTHandles.Alloc(width, height, colorFormat: RTFormat),
                     RTHandles.Alloc(width, height, colorFormat: RTFormat));
            }
        }
    }

    #endregion
}

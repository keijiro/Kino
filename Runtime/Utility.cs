using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    [System.Serializable]
    public sealed class GradientParameter : ParameterOverride<Gradient>
    {
        protected override void OnEnable()
        {
            if (value == null) value = GradientUtility.DefaultGradient;
        }
    }

    public static class GradientUtility
    {
        static readonly GradientColorKey[] _defaultColorKeys = new []
        {
            new GradientColorKey(Color.blue, 0),
            new GradientColorKey(Color.red, 1)
        };

        static readonly GradientAlphaKey[] _defaultAlphaKeys = new []
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };

        static readonly int[] _colorKeyPropertyIDs = new []
        {
            Shader.PropertyToID("_ColorKey0"),
            Shader.PropertyToID("_ColorKey1"),
            Shader.PropertyToID("_ColorKey2"),
            Shader.PropertyToID("_ColorKey3"),
            Shader.PropertyToID("_ColorKey4"),
            Shader.PropertyToID("_ColorKey5"),
            Shader.PropertyToID("_ColorKey6"),
            Shader.PropertyToID("_ColorKey7")
        };

        public static Gradient DefaultGradient {
            get {
                var g = new Gradient();
                g.SetKeys(_defaultColorKeys, _defaultAlphaKeys);
                return g;
            }
        }

        public static int GetColorKeyPropertyID(int index)
        {
            return _colorKeyPropertyIDs[index];
        }

        public static void SetColorKeys(PropertySheet sheet, GradientColorKey[] colorKeys)
        {
            for (var i = 0; i < 8; i++)
                sheet.properties.SetVector(
                    GetColorKeyPropertyID(i),
                    colorKeys[Mathf.Min(i, colorKeys.Length - 1)].ToVector()
                );
        }

    }

    public static class GradientColorKeyExtension
    {
        public static Vector4 ToVector(this GradientColorKey key)
        {
            var c = key.color.linear;
            return new Vector4(c.r, c.g, c.b, key.time);
        }
    }
}

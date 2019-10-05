using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kino.PostProcessing
{
    [VolumeComponentEditor(typeof(Overlay))]
    sealed class OverlayEditor : VolumeComponentEditor
    {
        SerializedDataParameter _sourceType;
        SerializedDataParameter _blendMode;
        SerializedDataParameter _opacity;
        SerializedDataParameter _color;
        SerializedDataParameter _gradient;
        SerializedDataParameter _angle;
        SerializedDataParameter _texture;
        SerializedDataParameter _sourceAlpha;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Overlay>(serializedObject);

            _sourceType  = Unpack(o.Find(x => x.sourceType));
            _blendMode   = Unpack(o.Find(x => x.blendMode));
            _opacity     = Unpack(o.Find(x => x.opacity));
            _color       = Unpack(o.Find(x => x.color));
            _gradient    = Unpack(o.Find(x => x.gradient));
            _angle       = Unpack(o.Find(x => x.angle));
            _texture     = Unpack(o.Find(x => x.texture));
            _sourceAlpha = Unpack(o.Find(x => x.sourceAlpha));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(_sourceType);

            var sourceType = (Overlay.SourceType)_sourceType.value.enumValueIndex;

            if (sourceType == Overlay.SourceType.Color)
            {
                PropertyField(_color);
            }
            else if (sourceType == Overlay.SourceType.Gradient)
            {
                PropertyField(_gradient);
                PropertyField(_angle);
            }
            else // Overlay.SourceType.Texture
            {
                PropertyField(_texture);
                PropertyField(_sourceAlpha);
            }

            PropertyField(_blendMode);
            PropertyField(_opacity);
        }
    }
}

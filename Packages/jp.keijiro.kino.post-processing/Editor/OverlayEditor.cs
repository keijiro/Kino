using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    class OverlayEditorBase<T> : PostProcessEffectEditor<T> where T : OverlayBase
    {
        SerializedParameterOverride _source;
        SerializedParameterOverride _blendMode;
        SerializedParameterOverride _opacity;

        SerializedParameterOverride _color;

        SerializedParameterOverride _gradient;
        SerializedParameterOverride _angle;

        SerializedParameterOverride _texture;
        SerializedParameterOverride _sourceAlpha;

        public override void OnEnable()
        {
            _source    = FindParameterOverride(x => x.source);
            _blendMode = FindParameterOverride(x => x.blendMode);
            _opacity   = FindParameterOverride(x => x.opacity);

            _color = FindParameterOverride(x => x.color);

            _gradient = FindParameterOverride(x => x.gradient);
            _angle    = FindParameterOverride(x => x.angle);

            _texture     = FindParameterOverride(x => x.texture);
            _sourceAlpha = FindParameterOverride(x => x.sourceAlpha);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(_source);

            var source = (Overlay.Source)_source.value.enumValueIndex;

            if (source == Overlay.Source.Color)
            {
                PropertyField(_color);
            }
            else if (source == Overlay.Source.Gradient)
            {
                PropertyField(_gradient);
                PropertyField(_angle);
            }
            else // Overlay.Source.Texture
            {
                PropertyField(_texture);
                PropertyField(_sourceAlpha);
            }

            PropertyField(_blendMode);
            PropertyField(_opacity);
        }
    }

    [PostProcessEditor(typeof(Overlay))]
    sealed class OverlayEditor : OverlayEditorBase<Overlay> {}

    [PostProcessEditor(typeof(PreStackOverlay))]
    sealed class PreStackOverlayEditor : OverlayEditorBase<PreStackOverlay> {}
}

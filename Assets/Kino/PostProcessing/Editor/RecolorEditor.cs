using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    [PostProcessEditor(typeof(Recolor))]
    public sealed class RecolorEditor : PostProcessEffectEditor<Recolor>
    {
        static class Labels
        {
            internal static readonly GUIContent Source    = new GUIContent("Source");
            internal static readonly GUIContent Threshold = new GUIContent("Threshold");
            internal static readonly GUIContent Contrast  = new GUIContent("Contrast");
            internal static readonly GUIContent Color     = new GUIContent("Color");
            internal static readonly GUIContent Gradient  = new GUIContent("Gradient");
            internal static readonly GUIContent Opacity   = new GUIContent("Opacity");
        }

        SerializedParameterOverride _edgeSource;
        SerializedParameterOverride _edgeThreshold;
        SerializedParameterOverride _edgeContrast;
        SerializedParameterOverride _edgeColor;
        SerializedParameterOverride _fillGradient;
        SerializedParameterOverride _fillOpacity;

        public override void OnEnable()
        {
            _edgeColor     = FindParameterOverride(x => x.edgeColor);
            _edgeSource    = FindParameterOverride(x => x.edgeSource);
            _edgeThreshold = FindParameterOverride(x => x.edgeThreshold);
            _edgeContrast  = FindParameterOverride(x => x.edgeContrast);
            _fillGradient  = FindParameterOverride(x => x.fillGradient);
            _fillOpacity   = FindParameterOverride(x => x.fillOpacity);
        }

        public override void OnInspectorGUI()
        {
            EditorUtilities.DrawHeaderLabel("Edge");

            PropertyField(_edgeColor, Labels.Color);
            PropertyField(_edgeSource, Labels.Source);
            PropertyField(_edgeThreshold, Labels.Threshold);
            PropertyField(_edgeContrast, Labels.Contrast);

            EditorGUILayout.Space();
            EditorUtilities.DrawHeaderLabel("Fill");

            PropertyField(_fillGradient, Labels.Gradient);
            PropertyField(_fillOpacity, Labels.Opacity);
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kino.PostProcessing
{
    [VolumeComponentEditor(typeof(Recolor))]
    public sealed class RecolorEditor : VolumeComponentEditor
    {
        static class Labels
        {
            internal static readonly GUIContent Source    = new GUIContent("Source");
            internal static readonly GUIContent Threshold = new GUIContent("Threshold");
            internal static readonly GUIContent Contrast  = new GUIContent("Contrast");
            internal static readonly GUIContent Color     = new GUIContent("Color");
            internal static readonly GUIContent Gradient  = new GUIContent("Gradient");
            internal static readonly GUIContent Opacity   = new GUIContent("Opacity");
            internal static readonly GUIContent Type      = new GUIContent("Type");
            internal static readonly GUIContent Strength  = new GUIContent("Strength");
        }

        SerializedDataParameter _edgeSource;
        SerializedDataParameter _edgeThreshold;
        SerializedDataParameter _edgeContrast;
        SerializedDataParameter _edgeColor;
        SerializedDataParameter _fillGradient;
        SerializedDataParameter _fillOpacity;
        SerializedDataParameter _ditherType;
        SerializedDataParameter _ditherStrength;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Recolor>(serializedObject);

            _edgeColor      = Unpack(o.Find(x => x.edgeColor));
            _edgeSource     = Unpack(o.Find(x => x.edgeSource));
            _edgeThreshold  = Unpack(o.Find(x => x.edgeThreshold));
            _edgeContrast   = Unpack(o.Find(x => x.edgeContrast));
            _fillGradient   = Unpack(o.Find(x => x.fillGradient));
            _fillOpacity    = Unpack(o.Find(x => x.fillOpacity));
            _ditherType     = Unpack(o.Find(x => x.ditherType));
            _ditherStrength = Unpack(o.Find(x => x.ditherStrength));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Edge", EditorStyles.miniLabel);

            PropertyField(_edgeColor, Labels.Color);
            PropertyField(_edgeSource, Labels.Source);
            PropertyField(_edgeThreshold, Labels.Threshold);
            PropertyField(_edgeContrast, Labels.Contrast);

            EditorGUILayout.LabelField("Fill", EditorStyles.miniLabel);

            PropertyField(_fillGradient, Labels.Gradient);
            PropertyField(_fillOpacity, Labels.Opacity);

            EditorGUILayout.LabelField("Dithering", EditorStyles.miniLabel);

            PropertyField(_ditherType, Labels.Type);
            PropertyField(_ditherStrength, Labels.Strength);
        }
    }
}

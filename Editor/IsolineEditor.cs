using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    [PostProcessEditor(typeof(Isoline))]
    public sealed class IsolineEditor : PostProcessEffectEditor<Isoline>
    {
        static class Labels
        {
            internal static readonly GUIContent Interval    = new GUIContent("Interval");
            internal static readonly GUIContent Offset      = new GUIContent("Offset");
            internal static readonly GUIContent Scroll      = new GUIContent("Scroll");
            internal static readonly GUIContent SourceBlend = new GUIContent("Source Blend");
            internal static readonly GUIContent Background  = new GUIContent("Background");
            internal static readonly GUIContent Strength    = new GUIContent("Strength");
            internal static readonly GUIContent Frequency   = new GUIContent("Frequency");
            internal static readonly GUIContent Width       = new GUIContent("Width");
        }

        SerializedParameterOverride _baseAxis;
        SerializedParameterOverride _lineInterval;
        SerializedParameterOverride _lineOffset;
        SerializedParameterOverride _lineScroll;

        SerializedParameterOverride _lineColor;
        SerializedParameterOverride _lineWidth;
        SerializedParameterOverride _sourceContribution;
        SerializedParameterOverride _backgroundColor;

        SerializedParameterOverride _modulationStrength;
        SerializedParameterOverride _modulationFrequency;
        SerializedParameterOverride _modulationWidth;
        SerializedParameterOverride _modulationOffset;
        SerializedParameterOverride _modulationScroll;

        public override void OnEnable()
        {
            _baseAxis            = FindParameterOverride(x => x.baseAxis);
            _lineInterval        = FindParameterOverride(x => x.lineInterval);
            _lineOffset          = FindParameterOverride(x => x.lineOffset);
            _lineScroll          = FindParameterOverride(x => x.lineScroll);

            _lineColor           = FindParameterOverride(x => x.lineColor);
            _lineWidth           = FindParameterOverride(x => x.lineWidth);
            _sourceContribution  = FindParameterOverride(x => x.sourceContribution);
            _backgroundColor     = FindParameterOverride(x => x.backgroundColor);

            _modulationStrength  = FindParameterOverride(x => x.modulationStrength);
            _modulationFrequency = FindParameterOverride(x => x.modulationFrequency);
            _modulationWidth     = FindParameterOverride(x => x.modulationWidth);
            _modulationOffset    = FindParameterOverride(x => x.modulationOffset);
            _modulationScroll    = FindParameterOverride(x => x.modulationScroll);
        }

        public override void OnInspectorGUI()
        {
            EditorUtilities.DrawHeaderLabel("Contour Detector");

            PropertyField(_baseAxis);
            PropertyField(_lineInterval, Labels.Interval);
            PropertyField(_lineOffset, Labels.Offset);
            PropertyField(_lineScroll, Labels.Scroll);

            EditorGUILayout.Space();
            EditorUtilities.DrawHeaderLabel("Line Appearance");

            PropertyField(_lineColor);
            PropertyField(_lineWidth);
            PropertyField(_sourceContribution, Labels.SourceBlend);
            PropertyField(_backgroundColor, Labels.Background);

            EditorGUILayout.Space();
            EditorUtilities.DrawHeaderLabel("Color Modulation");

            PropertyField(_modulationStrength, Labels.Strength);
            PropertyField(_modulationFrequency, Labels.Frequency);
            PropertyField(_modulationWidth, Labels.Width);
            PropertyField(_modulationOffset, Labels.Offset);
            PropertyField(_modulationScroll, Labels.Scroll);
        }
    }
}

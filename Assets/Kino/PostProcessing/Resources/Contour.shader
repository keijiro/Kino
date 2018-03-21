Shader "Hidden/Kino/PostProcessing/Contour"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            #define CONTOUR_COLOR
            #include "Contour.hlsl"
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            #define CONTOUR_DEPTH
            #include "Contour.hlsl"
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            #define CONTOUR_NORMAL
            #include "Contour.hlsl"
            ENDHLSL
        }
    }
}

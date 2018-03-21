Shader "Hidden/Kino/PostProcessing/Overlay"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 3 keys gradient
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // 8 keys gradient
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            #define OVERLAY_GRADIENT_EXT
            #include "Overlay.hlsl"
            ENDHLSL
        }
    }
}

Shader "Hidden/Kino/PostProcessing/Overlay"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Normal mode (alpha blending)

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragTexture
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_NORMAL
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_NORMAL
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_NORMAL
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Screen mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragTexture
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_SCREEN
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_SCREEN
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_SCREEN
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Overlay mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragTexture
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_OVERLAY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_OVERLAY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_OVERLAY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Multiply mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragTexture
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_MULTIPLY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_MULTIPLY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_MULTIPLY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Soft light mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragTexture
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_SOFTLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_SOFTLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_SOFTLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Hard light mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragTexture
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_HARDLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_BLEND_HARDLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragGradient
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_HARDLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }
    }
}

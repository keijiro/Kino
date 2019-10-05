Shader "Hidden/Kino/PostProcess/Overlay"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Normal mode (alpha blending)

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentTexture
            #define OVERLAY_BLEND_NORMAL
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_BLEND_NORMAL
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_NORMAL
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Screen mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentTexture
            #define OVERLAY_BLEND_SCREEN
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_BLEND_SCREEN
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_SCREEN
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Overlay mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentTexture
            #define OVERLAY_BLEND_OVERLAY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_BLEND_OVERLAY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_OVERLAY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Multiply mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentTexture
            #define OVERLAY_BLEND_MULTIPLY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_BLEND_MULTIPLY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_MULTIPLY
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Soft light mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentTexture
            #define OVERLAY_BLEND_SOFTLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_BLEND_SOFTLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_SOFTLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        // Hard light mode

        Pass // Texture
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentTexture
            #define OVERLAY_BLEND_HARDLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 3 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_BLEND_HARDLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }

        Pass // 8 keys gradient
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGradient
            #define OVERLAY_GRADIENT_EXT
            #define OVERLAY_BLEND_HARDLIGHT
            #include "Overlay.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}

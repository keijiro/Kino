Shader "Hidden/Kino/PostProcessing/Contour"
{
    HLSLINCLUDE

    #include "PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float4 _MainTex_TexelSize;

    half4 _LineColor;
    half4 _BackgroundColor;
    half _Threshold;
    half _Contrast;

    half4 Frag(VaryingsDefault i) : SV_Target
    {
        float2 uv = i.texcoord;

        // Source color
        half4 c0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

        // Four sample points of the roberts cross operator
        float2 uv0 = uv;                                   // TL
        float2 uv1 = uv + _MainTex_TexelSize.xy;           // BR
        float2 uv2 = uv + float2(_MainTex_TexelSize.x, 0); // TR
        float2 uv3 = uv + float2(0, _MainTex_TexelSize.y); // BL

        half edge = 0;

        // Color samples
        float3 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1).rgb;
        float3 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2).rgb;
        float3 c3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv3).rgb;

        // Roberts cross operator
        float3 cg1 = c1 - c0.rgb;
        float3 cg2 = c3 - c2;
        float cg = sqrt(dot(cg1, cg1) + dot(cg2, cg2));

        edge = smoothstep(_Threshold, _Threshold + 1.01 - _Contrast, cg * 10);

        half3 cb = lerp(c0.rgb, _BackgroundColor.rgb, _BackgroundColor.a);
        half3 co = lerp(cb, _LineColor.rgb, edge * _LineColor.a);
        return half4(co, c0.a);
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            ENDHLSL
        }
    }
}

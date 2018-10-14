Shader "Hidden/Kino/PostProcessing/Sharpen"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float4 _MainTex_TexelSize;
    half _Strength;

    half4 Frag(VaryingsDefault i) : SV_Target
    {
        float4 duv = float4(1, 1, -1, 0) * _MainTex_TexelSize.xyxy;

        half4 c0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - duv.xy);
        half4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - duv.wy);
        half4 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - duv.zy);

        half4 c3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + duv.zw);
        half4 c4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + duv.ww);
        half4 c5 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + duv.xw);

        half4 c6 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + duv.zy);
        half4 c7 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + duv.wy);
        half4 c8 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + duv.xy);

        return c4 - (c0 + c1 + c2 + c3 - 8 * c4 + c5 + c6 + c7 + c8) * _Strength;
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

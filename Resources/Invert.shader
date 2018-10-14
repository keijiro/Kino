Shader "Hidden/Kino/PostProcessing/Invert"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    half _Strength;

    half4 Frag(VaryingsDefault i) : SV_Target
    {
        half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

    #ifndef UNITY_COLORSPACE_GAMMA
        c.rgb = LinearToSRGB(c.rgb);
    #endif

        c.rgb = lerp(c.rgb, 1 - c.rgb, _Strength);

    #ifndef UNITY_COLORSPACE_GAMMA
        c.rgb = SRGBToLinear(c.rgb);
    #endif

        return c;
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
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            ENDHLSL
        }
    }
}

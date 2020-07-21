Shader "Hidden/Kino/PostProcess/Utility"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vertex(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    float4 _FadeColor;
    float _HueShift;
    float _Invert;
    float _Saturation;
    TEXTURE2D_X(_InputTexture);

    float4 Fragment(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float4 c = LOAD_TEXTURE2D_X(_InputTexture, positionSS);
        float3 rgb = c.rgb;

        // Saturation
        rgb = max(0, lerp(Luminance(rgb), rgb, _Saturation));

        // Linear -> sRGB
        rgb = LinearToSRGB(rgb);

        // Hue shift
        float3 hsv = RgbToHsv(rgb);
        hsv.x = frac(hsv.x + _HueShift);
        rgb = HsvToRgb(hsv);

        // Invert
        rgb = lerp(rgb, 1 - rgb, _Invert);

        // Fade
        rgb = lerp(rgb, _FadeColor.rgb, _FadeColor.a);

        // sRGB -> Linear
        c.rgb = SRGBToLinear(rgb);

        return c;
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
    Fallback Off
}

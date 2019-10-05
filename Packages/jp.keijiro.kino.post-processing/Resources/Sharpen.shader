Shader "Hidden/Kino/PostProcess/Sharpen"
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

    TEXTURE2D_X(_InputTexture);
    float _Intensity;

    float4 SampleInput(int2 coord)
    {
        coord = min(max(0, coord), _ScreenSize.xy - 1);
        return LOAD_TEXTURE2D_X(_InputTexture, coord);
    }

    float4 Fragment(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        int2 positionSS = input.texcoord * _ScreenSize.xy;

        float4 c0 = SampleInput(positionSS + int2(-1, -1));
        float4 c1 = SampleInput(positionSS + int2( 0, -1));
        float4 c2 = SampleInput(positionSS + int2(+1, -1));

        float4 c3 = SampleInput(positionSS + int2(-1, 0));
        float4 c4 = SampleInput(positionSS + int2( 0, 0));
        float4 c5 = SampleInput(positionSS + int2(+1, 0));

        float4 c6 = SampleInput(positionSS + int2(-1, +1));
        float4 c7 = SampleInput(positionSS + int2( 0, +1));
        float4 c8 = SampleInput(positionSS + int2(+1, +1));

        return c4 - (c0 + c1 + c2 + c3 - 8 * c4 + c5 + c6 + c7 + c8) * _Intensity;
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

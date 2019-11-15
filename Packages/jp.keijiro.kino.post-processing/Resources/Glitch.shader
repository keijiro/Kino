Shader "Hidden/Kino/PostProcess/Glitch"
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

    uint _Seed;
    float2 _Drift;
    float2 _Jitter;
    float2 _Jump;
    float _Shake;

    TEXTURE2D_X(_InputTexture);

    float4 Fragment(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Texture space position
        float tx = input.texcoord.x;
        float ty = input.texcoord.y;

        // Jump
        ty = lerp(ty, frac(ty + _Jump.x), _Jump.y);

        // Screen space Y coordinate
        uint sy = ty * _ScreenSize.y;

        // Jitter
        float jitter = Hash(sy + _Seed) * 2 - 1;
        tx += jitter * (_Jitter.x < abs(jitter)) * _Jitter.y;

        // Shake
        tx = frac(tx + (Hash(_Seed) - 0.5) * _Shake);

        // Drift
        float drift = sin(ty * 2 + _Drift.x) * _Drift.y;

        // Input sample
        uint sx1 = (tx        ) * _ScreenSize.x;
        uint sx2 = (tx + drift) * _ScreenSize.x;
        float4 c1 = LOAD_TEXTURE2D_X(_InputTexture, uint2(sx1, sy));
        float4 c2 = LOAD_TEXTURE2D_X(_InputTexture, uint2(sx2, sy));

        return float4(c1.r, c2.g, c1.b, c1.a);
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

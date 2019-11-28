Shader "Hidden/Kino/PostProcess/Slice"
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

    float2 _Direction;
    float _Displacement;
    float _Rows;
    uint _Seed;

    float4 Fragment(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        const float aspect = (float)_ScreenSize.x / _ScreenSize.y;
        const float inv_aspect = (float)_ScreenSize.y / _ScreenSize.x;

        const float2 axis1 = _Direction;
        const float2 axis2 = float2(-axis1.y, axis1.x);

        float2 uv = input.texcoord;
        float param = dot(uv - 0.5, axis2 * float2(aspect, 1));
        uint seed = _Seed + (uint)((param + 10) * _Rows + 0.5);
        float delta = Hash(seed) - 0.5;

        uv += axis1 * delta * _Displacement * float2(inv_aspect, 1);

        uv = ClampAndScaleUVForBilinear(uv);
        return SAMPLE_TEXTURE2D_X(_InputTexture, s_linear_clamp_sampler, uv);
    }

    ENDHLSL
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
    Fallback Off
}

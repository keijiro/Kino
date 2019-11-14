#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

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

float4 _EdgeColor;
float2 _EdgeThresholds;
float2 _FillOpacity;

float4 _ColorKey0;
float4 _ColorKey1;
float4 _ColorKey2;
float4 _ColorKey3;
float4 _ColorKey4;
float4 _ColorKey5;
float4 _ColorKey6;
float4 _ColorKey7;

float3 LoadWorldNormal(uint2 positionSS)
{
    NormalData data;
    DecodeFromNormalBuffer(positionSS, data);
    return data.normalWS;
}

float4 Fragment(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    uint2 positionSS = input.texcoord * _ScreenSize.xy;

    // Source color
    float4 c0 = LOAD_TEXTURE2D_X(_InputTexture, positionSS);

    // Four sample points of the roberts cross operator
    // TL / BR / TR / BL
    uint2 uv0 = positionSS;
    uint2 uv1 = min(positionSS + uint2(1, 1), _ScreenSize.xy - 1);
    uint2 uv2 = uint2(uv1.x, uv0.y);
    uint2 uv3 = uint2(uv0.x, uv1.y);

#ifdef RECOLOR_EDGE_COLOR

    // Color samples
    float3 c1 = LOAD_TEXTURE2D_X(_InputTexture, uv1).rgb;
    float3 c2 = LOAD_TEXTURE2D_X(_InputTexture, uv2).rgb;
    float3 c3 = LOAD_TEXTURE2D_X(_InputTexture, uv3).rgb;

    // Roberts cross operator
    float3 g1 = c1 - c0.rgb;
    float3 g2 = c3 - c2;
    float g = sqrt(dot(g1, g1) + dot(g2, g2)) * 10;

#endif

#ifdef RECOLOR_EDGE_DEPTH

    // Depth samples
    float d0 = LoadCameraDepth(uv0);
    float d1 = LoadCameraDepth(uv1);
    float d2 = LoadCameraDepth(uv2);
    float d3 = LoadCameraDepth(uv3);

    // Roberts cross operator
    float g = length(float2(d1 - d0, d3 - d2)) * 100;

#endif

#ifdef RECOLOR_EDGE_NORMAL

    // Normal samples
    float3 n0 = LoadWorldNormal(uv0);
    float3 n1 = LoadWorldNormal(uv1);
    float3 n2 = LoadWorldNormal(uv2);
    float3 n3 = LoadWorldNormal(uv3);

    // Roberts cross operator
    float3 g1 = n1 - n0;
    float3 g2 = n3 - n2;
    float g = sqrt(dot(g1, g1) + dot(g2, g2));

#endif

    // Apply fill gradient.
    float3 fill = _ColorKey0.rgb;
    float lum = Luminance(LinearToSRGB(c0.rgb));

    static float dmatrix [] =
        //{0.0, 0.5, 0.75, 0.25};
        //{0.0, 0.7777777777777778, 0.3333333333333333, 0.6666666666666666, 0.5555555555555556, 0.2222222222222222, 0.4444444444444444, 0.1111111111111111, 0.8888888888888888};
        //{0.0, 0.5, 0.125, 0.625, 0.75, 0.25, 0.875, 0.375, 0.1875, 0.6875, 0.0625, 0.5625, 0.9375, 0.4375, 0.8125, 0.3125};
        {0.0, 0.75, 0.1875, 0.9375, 0.046875, 0.796875, 0.234375, 0.984375, 0.5, 0.25, 0.6875, 0.4375, 0.546875, 0.296875, 0.734375, 0.484375, 0.125, 0.875, 0.0625, 0.8125, 0.171875, 0.921875, 0.109375, 0.859375, 0.625, 0.375, 0.5625, 0.3125, 0.671875, 0.421875, 0.609375, 0.359375, 0.03125, 0.78125, 0.21875, 0.96875, 0.015625, 0.765625, 0.203125, 0.953125, 0.53125, 0.28125, 0.71875, 0.46875, 0.515625, 0.265625, 0.703125, 0.453125, 0.15625, 0.90625, 0.09375, 0.84375, 0.140625, 0.890625, 0.078125, 0.828125, 0.65625, 0.40625, 0.59375, 0.34375, 0.640625, 0.390625, 0.578125, 0.328125};

    //float dither = dmatrix[(positionSS.x & 1) + (positionSS.y & 1) * 2];
    //float dither = dmatrix[(positionSS.x % 3) + (positionSS.y % 3) * 3];
    //float dither = dmatrix[(positionSS.x & 3) + (positionSS.y & 3) * 4];
    float dither = dmatrix[(positionSS.x & 7) + (positionSS.y & 7) * 8];

    //float dither = GenerateHashedRandomFloat(positionSS);
    lum += (dither - 0.5) * _FillOpacity.y;

#ifdef RECOLOR_GRADIENT_LERP
    fill = lerp(fill, _ColorKey1.rgb, saturate((lum - _ColorKey0.w) / (_ColorKey1.w - _ColorKey0.w)));
    fill = lerp(fill, _ColorKey2.rgb, saturate((lum - _ColorKey1.w) / (_ColorKey2.w - _ColorKey1.w)));
    fill = lerp(fill, _ColorKey3.rgb, saturate((lum - _ColorKey2.w) / (_ColorKey3.w - _ColorKey2.w)));
    #ifdef RECOLOR_GRADIENT_EXT
    fill = lerp(fill, _ColorKey4.rgb, saturate((lum - _ColorKey3.w) / (_ColorKey4.w - _ColorKey3.w)));
    fill = lerp(fill, _ColorKey5.rgb, saturate((lum - _ColorKey4.w) / (_ColorKey5.w - _ColorKey4.w)));
    fill = lerp(fill, _ColorKey6.rgb, saturate((lum - _ColorKey5.w) / (_ColorKey6.w - _ColorKey5.w)));
    fill = lerp(fill, _ColorKey7.rgb, saturate((lum - _ColorKey6.w) / (_ColorKey7.w - _ColorKey6.w)));
    #endif
#else
    fill = lum > _ColorKey0.w ? _ColorKey1.rgb : fill;
    fill = lum > _ColorKey1.w ? _ColorKey2.rgb : fill;
    fill = lum > _ColorKey2.w ? _ColorKey3.rgb : fill;
    #ifdef RECOLOR_GRADIENT_EXT
    fill = lum > _ColorKey3.w ? _ColorKey4.rgb : fill;
    fill = lum > _ColorKey4.w ? _ColorKey5.rgb : fill;
    fill = lum > _ColorKey5.w ? _ColorKey6.rgb : fill;
    fill = lum > _ColorKey6.w ? _ColorKey7.rgb : fill;
    #endif
#endif

    float edge = smoothstep(_EdgeThresholds.x, _EdgeThresholds.y, g);
    float3 cb = lerp(c0.rgb, fill, _FillOpacity.x);
    float3 co = lerp(cb, _EdgeColor.rgb, edge * _EdgeColor.a);
    return float4(co, c0.a);
}

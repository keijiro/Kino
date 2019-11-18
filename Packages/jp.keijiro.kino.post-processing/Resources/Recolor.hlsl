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
float _FillOpacity;

float4 _ColorKey0;
float4 _ColorKey1;
float4 _ColorKey2;
float4 _ColorKey3;
float4 _ColorKey4;
float4 _ColorKey5;
float4 _ColorKey6;
float4 _ColorKey7;

TEXTURE2D(_DitherTexture);
float _DitherStrength;

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

#if defined(RECOLOR_EDGE_DEPTH) || defined(RECOLOR_EDGE_NORMAL)

    // Depth samples
    float d0 = LoadCameraDepth(uv0);
    float d1 = LoadCameraDepth(uv1);
    float d2 = LoadCameraDepth(uv2);
    float d3 = LoadCameraDepth(uv3);

#endif

#ifdef RECOLOR_EDGE_DEPTH

    // Roberts cross operator
    float g = length(float2(d1 - d0, d3 - d2)) * 100;

#endif

#ifdef RECOLOR_EDGE_NORMAL

    // Normal samples
    float3 n0 = LoadWorldNormal(uv0);
    float3 n1 = LoadWorldNormal(uv1);
    float3 n2 = LoadWorldNormal(uv2);
    float3 n3 = LoadWorldNormal(uv3);

    // Background removal
#if UNITY_REVERSED_Z
    n0 *= d0 > 0; n1 *= d1 > 0; n2 *= d2 > 0; n3 *= d3 > 0;
#else
    n0 *= d0 < 0; n1 *= d1 < 1; n2 *= d2 < 1; n3 *= d3 < 1;
#endif

    // Roberts cross operator
    float3 g1 = n1 - n0;
    float3 g2 = n3 - n2;
    float g = sqrt(dot(g1, g1) + dot(g2, g2));

#endif

    // Dithering
    uint tw, th;
    _DitherTexture.GetDimensions(tw, th);
    float dither = LOAD_TEXTURE2D(_DitherTexture, positionSS % uint2(tw, th)).x;
    dither = (dither - 0.5) * _DitherStrength;

    // Apply fill gradient.
    float3 fill = _ColorKey0.rgb;
    float lum = Luminance(c0.rgb) + dither;

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
    float3 cb = lerp(c0.rgb, fill, _FillOpacity);
    float3 co = lerp(cb, _EdgeColor.rgb, edge * _EdgeColor.a);
    return float4(co, c0.a);
}

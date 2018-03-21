#include "PostProcessing/Shaders/StdLib.hlsl"
#include "PostProcessing/Shaders/Colors.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
TEXTURE2D_SAMPLER2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2);
TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

float4 _MainTex_TexelSize;

half4 _EdgeColor;
half2 _EdgeThresholds;
half _FillOpacity;

half4 _FillKey0;
half4 _FillKey1;
half4 _FillKey2;
half4 _FillKey3;
half4 _FillKey4;
half4 _FillKey5;
half4 _FillKey6;
half4 _FillKey7;

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

#ifdef RECOLOR_EDGE_COLOR

    // Color samples
    half3 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1).rgb;
    half3 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2).rgb;
    half3 c3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv3).rgb;

    // Roberts cross operator
    half3 g1 = c1 - c0.rgb;
    half3 g2 = c3 - c2;
    half g = sqrt(dot(g1, g1) + dot(g2, g2)) * 10;

#endif

#ifdef RECOLOR_EDGE_DEPTH

    // Depth samples
    float d0 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv0, 0);
    float d1 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv1, 0);
    float d2 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv2, 0);
    float d3 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv3, 0);

    // Roberts cross operator
    half g = length(float2(d1 - d0, d3 - d2)) * 100;

#endif

#ifdef RECOLOR_EDGE_NORMAL

    // Normal samples
    half3 n0 = SAMPLE_TEXTURE2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2, uv0).rgb;
    half3 n1 = SAMPLE_TEXTURE2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2, uv1).rgb;
    half3 n2 = SAMPLE_TEXTURE2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2, uv2).rgb;
    half3 n3 = SAMPLE_TEXTURE2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2, uv3).rgb;

    // Roberts cross operator
    float3 g1 = n1 - n0;
    float3 g2 = n3 - n2;
    float g = sqrt(dot(g1, g1) + dot(g2, g2));

#endif

    // Apply fill gradient.
    half3 fill = _FillKey0.rgb;
    half lum = Luminance(LinearToSRGB(c0.rgb));
#ifdef RECOLOR_GRADIENT_LERP
    fill = lerp(fill, _FillKey1.rgb, saturate((lum - _FillKey0.w) / (_FillKey1.w - _FillKey0.w)));
    fill = lerp(fill, _FillKey2.rgb, saturate((lum - _FillKey1.w) / (_FillKey2.w - _FillKey1.w)));
    fill = lerp(fill, _FillKey3.rgb, saturate((lum - _FillKey2.w) / (_FillKey3.w - _FillKey2.w)));
    #ifdef RECOLOR_GRADIENT_EXT
    fill = lerp(fill, _FillKey4.rgb, saturate((lum - _FillKey3.w) / (_FillKey4.w - _FillKey3.w)));
    fill = lerp(fill, _FillKey5.rgb, saturate((lum - _FillKey4.w) / (_FillKey5.w - _FillKey4.w)));
    fill = lerp(fill, _FillKey6.rgb, saturate((lum - _FillKey5.w) / (_FillKey6.w - _FillKey5.w)));
    fill = lerp(fill, _FillKey7.rgb, saturate((lum - _FillKey6.w) / (_FillKey7.w - _FillKey6.w)));
    #endif
#else
    fill = lum > _FillKey0.w ? _FillKey1.rgb : fill;
    fill = lum > _FillKey1.w ? _FillKey2.rgb : fill;
    fill = lum > _FillKey2.w ? _FillKey3.rgb : fill;
    #ifdef RECOLOR_GRADIENT_EXT
    fill = lum > _FillKey3.w ? _FillKey4.rgb : fill;
    fill = lum > _FillKey4.w ? _FillKey5.rgb : fill;
    fill = lum > _FillKey5.w ? _FillKey6.rgb : fill;
    fill = lum > _FillKey6.w ? _FillKey7.rgb : fill;
    #endif
#endif

    half edge = smoothstep(_EdgeThresholds.x, _EdgeThresholds.y, g);
    half3 cb = lerp(c0.rgb, fill, _FillOpacity);
    half3 co = lerp(cb, _EdgeColor.rgb, edge * _EdgeColor.a);
    return half4(co, c0.a);
}

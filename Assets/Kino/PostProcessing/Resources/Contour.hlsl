#include "PostProcessing/Shaders/StdLib.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
TEXTURE2D_SAMPLER2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2);
TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

float4 _MainTex_TexelSize;

half4 _LineColor;
half4 _BackgroundColor;
half2 _Thresholds;

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

#ifdef CONTOUR_COLOR

    // Color samples
    half3 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1).rgb;
    half3 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2).rgb;
    half3 c3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv3).rgb;

    // Roberts cross operator
    half3 g1 = c1 - c0.rgb;
    half3 g2 = c3 - c2;
    half g = sqrt(dot(g1, g1) + dot(g2, g2)) * 10;

#endif

#ifdef CONTOUR_DEPTH

    // Depth samples
    float d0 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv0, 0);
    float d1 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv1, 0);
    float d2 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv2, 0);
    float d3 = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv3, 0);

    // Roberts cross operator
    half g = length(float2(d1 - d0, d3 - d2)) * 100;

#endif

#ifdef CONTOUR_NORMAL

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

    half edge = smoothstep(_Thresholds.x, _Thresholds.y, g);
    half3 cb = lerp(c0.rgb, _BackgroundColor.rgb, _BackgroundColor.a);
    half3 co = lerp(cb, _LineColor.rgb, edge * _LineColor.a);
    return half4(co, c0.a);
}

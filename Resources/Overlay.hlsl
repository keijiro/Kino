#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
half3 _Color;
half _Opacity;

//
// Blend function
//

half4 BlendFunction(half4 c1, half4 c2)
{
    half4 c;

#if defined(OVERLAY_BLEND_NORMAL)

    c = c2;

#elif defined(OVERLAY_BLEND_MULTIPLY)

    c = c1 * c2;

#elif defined(OVERLAY_BLEND_SCREEN)

    c = 1 - (1 - c1) * (1 - c2);

#elif defined(OVERLAY_BLEND_SOFTLIGHT)

    half4 a = c1 * c2 * 2 + (1 - c2 * 2) * c1 * c1;
    half4 b = (1 - c2) * c1 * 2 + (c2 * 2 - 1) * sqrt(c1);
    c = lerp(a, b, c2 > 0.5);

#else

    half4 a = c1 * c2 * 2;
    half4 b = 1 - (1 - c1) * (1 - c2) * 2;

    #if defined(OVERLAY_BLEND_OVERLAY)

    c = lerp(a, b, c1 > 0.5);

    #else // OVERLAY_BLEND_HARDLIGHT

    c = lerp(a, b, c2 > 0.5);

    #endif

#endif

    return lerp(c1, c, c2.a);
}

//
// Texture mode
//

TEXTURE2D_SAMPLER2D(_SourceTex, sampler_SourceTex);
half _UseSourceAlpha;

half4 FragTexture(VaryingsDefault i) : SV_Target
{
    half4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
    half4 c2 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.texcoord);

#if !defined(UNITY_COLORSPACE_GAMMA)
    c1.rgb = LinearToSRGB(c1.rgb);
    c2.rgb = LinearToSRGB(c2.rgb);
#endif

    c2.rgb *= _Color;
    c2.a = _Opacity * lerp(1, c2.a, _UseSourceAlpha);

    half4 c = BlendFunction(c1, c2);

#if !defined(UNITY_COLORSPACE_GAMMA)
    c.rgb = SRGBToLinear(c.rgb);
#endif

    return c;
}

//
// Gradient mode
//

half2 _Direction;
half4 _ColorKey0;
half4 _ColorKey1;
half4 _ColorKey2;
half4 _ColorKey3;
half4 _ColorKey4;
half4 _ColorKey5;
half4 _ColorKey6;
half4 _ColorKey7;

half4 FragGradient(VaryingsDefault i) : SV_Target
{
    half4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

#if !defined(UNITY_COLORSPACE_GAMMA)
    c1.rgb = LinearToSRGB(c1.rgb);
#endif

    half p = dot(i.texcoord - 0.5f, _Direction) + 0.5f;
    half3 c2 = _ColorKey0.rgb;
    c2 = lerp(c2, _ColorKey1.rgb, saturate((p - _ColorKey0.w) / (_ColorKey1.w - _ColorKey0.w)));
    c2 = lerp(c2, _ColorKey2.rgb, saturate((p - _ColorKey1.w) / (_ColorKey2.w - _ColorKey1.w)));

#ifdef OVERLAY_GRADIENT_EXT
    c2 = lerp(c2, _ColorKey3.rgb, saturate((p - _ColorKey2.w) / (_ColorKey3.w - _ColorKey2.w)));
    c2 = lerp(c2, _ColorKey4.rgb, saturate((p - _ColorKey3.w) / (_ColorKey4.w - _ColorKey3.w)));
    c2 = lerp(c2, _ColorKey5.rgb, saturate((p - _ColorKey4.w) / (_ColorKey5.w - _ColorKey4.w)));
    c2 = lerp(c2, _ColorKey6.rgb, saturate((p - _ColorKey5.w) / (_ColorKey6.w - _ColorKey5.w)));
    c2 = lerp(c2, _ColorKey7.rgb, saturate((p - _ColorKey6.w) / (_ColorKey7.w - _ColorKey6.w)));
#endif

    half4 c = BlendFunction(c1, half4(c2, _Opacity));

#if !defined(UNITY_COLORSPACE_GAMMA)
    c.rgb = SRGBToLinear(c.rgb);
#endif

    return c;
}

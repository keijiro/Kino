#include "PostProcessing/Shaders/StdLib.hlsl"
#include "PostProcessing/Shaders/Colors.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

half _Opacity;
half2 _Direction;

half4 _GradientKey0;
half4 _GradientKey1;
half4 _GradientKey2;
half4 _GradientKey3;
half4 _GradientKey4;
half4 _GradientKey5;
half4 _GradientKey6;
half4 _GradientKey7;

half4 Frag(VaryingsDefault i) : SV_Target
{
    half4 src = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
    half3 c1 = LinearToSRGB(src.rgb);

    half p = dot(i.texcoord - 0.5f, _Direction) + 0.5f;
    half3 c2 = _GradientKey0.rgb;
    c2 = lerp(c2, _GradientKey1.rgb, saturate((p - _GradientKey0.w) / (_GradientKey1.w - _GradientKey0.w)));
    c2 = lerp(c2, _GradientKey2.rgb, saturate((p - _GradientKey1.w) / (_GradientKey2.w - _GradientKey1.w)));
    #ifdef OVERLAY_GRADIENT_EXT
    c2 = lerp(c2, _GradientKey3.rgb, saturate((p - _GradientKey2.w) / (_GradientKey3.w - _GradientKey2.w)));
    c2 = lerp(c2, _GradientKey4.rgb, saturate((p - _GradientKey3.w) / (_GradientKey4.w - _GradientKey3.w)));
    c2 = lerp(c2, _GradientKey5.rgb, saturate((p - _GradientKey4.w) / (_GradientKey5.w - _GradientKey4.w)));
    c2 = lerp(c2, _GradientKey6.rgb, saturate((p - _GradientKey5.w) / (_GradientKey6.w - _GradientKey5.w)));
    c2 = lerp(c2, _GradientKey7.rgb, saturate((p - _GradientKey6.w) / (_GradientKey7.w - _GradientKey6.w)));
    #endif

    half3 c = c1 > 0.5 ? 2 * c1 * c2 : 1 - 2 * (1 - c1) * (1 - c2);
    c = lerp(c1, c, _Opacity);

    return half4(SRGBToLinear(c), src.a);
}

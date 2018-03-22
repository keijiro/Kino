#include "PostProcessing/Shaders/StdLib.hlsl"
#include "PostProcessing/Shaders/Colors.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

half _Opacity;
half2 _Direction;

half4 _ColorKey0;
half4 _ColorKey1;
half4 _ColorKey2;
half4 _ColorKey3;
half4 _ColorKey4;
half4 _ColorKey5;
half4 _ColorKey6;
half4 _ColorKey7;

half4 Frag(VaryingsDefault i) : SV_Target
{
    half4 src = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
    half3 c1 = LinearToSRGB(src.rgb);

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

    half3 c = c1 > 0.5 ? 2 * c1 * c2 : 1 - 2 * (1 - c1) * (1 - c2);
    c = lerp(c1, c, _Opacity);

    return half4(SRGBToLinear(c), src.a);
}

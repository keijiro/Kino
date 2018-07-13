Shader "Hidden/Kino/PostProcessing/Isoline"
{
    HLSLINCLUDE

    #include "PostProcessing/Shaders/StdLib.hlsl"
    #include "PostProcessing/Shaders/Colors.hlsl"
    #include "DepthUtils.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

    float4x4 _InverseView;

    half4 _LineColor;
    half4 _BackgroundColor;
    half4 _ContourAxis;
    half4 _ContourParams; // interval, scroll, width, source contribution
    half4 _ModParams; // strength, frequency, midpoint, offset

    float2 Contour(VaryingsDepthSupport input)
    {
        const float Width = _ContourParams.z;

        // Depth to world space conversion
        float3 vpos = ComputeViewSpacePosition(input);
        float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;

        // Potential value and derivatives
        float pot = (dot(_ContourAxis.xyz, wpos) + _ContourAxis.w) / _ContourParams.x;

        // Contour detection
        float fw = fwidth(pot);
        float fww = fw * Width;
        float ct = saturate((abs(1 - frac(pot) * 2) - 1 + fww) / fww);

        // Frequency filter
        ct = lerp(ct, 0, smoothstep(0.25, 0.5, fw));

        return float2(pot, ct);
    }

    half Modulation(float pot)
    {
        const float Strength = _ModParams.x;
        const float Frequency = _ModParams.y;
        const float Thresh = 1 - _ModParams.z;
        const float Midpoint = lerp(Thresh, 1, 0.95);
        const float Offset = _ModParams.w;

        float x = frac(pot * Frequency + Offset);
        half cv_in = smoothstep(Thresh, Midpoint, x);
        half cv_out = smoothstep(0, 1 - Midpoint, 1 - x);

        return lerp(1, cv_in * cv_out, Strength);
    }

    half3 BlendContour(half3 source, half contour)
    {
        const float SourceContrib = _ContourParams.w;

        half3 bg = lerp(source, _BackgroundColor.rgb, _BackgroundColor.a);
        half3 ln = _LineColor.rgb * lerp(1, Luminance(source), SourceContrib);

        return bg + ln * contour;
    }

    half4 FragmentIsoline(VaryingsDepthSupport input) : SV_Target
    {
        float2 contour = Contour(input);
        half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
        color.rgb = BlendContour(color.rgb, contour.y * Modulation(contour.x));
        return color;
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertexDepthSupport
            #pragma fragment FragmentIsoline
            ENDHLSL
        }
    }
}

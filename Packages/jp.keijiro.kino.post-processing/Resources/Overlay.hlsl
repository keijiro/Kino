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
float3 _Color;
float _Opacity;

//
// Blend function
//

float4 BlendFunction(float4 c1, float4 c2)
{
    float4 c;

#if defined(OVERLAY_BLEND_NORMAL)

    c = c2;

#elif defined(OVERLAY_BLEND_MULTIPLY)

    c = c1 * c2;

#elif defined(OVERLAY_BLEND_SCREEN)

    c = 1 - (1 - c1) * (1 - c2);

#elif defined(OVERLAY_BLEND_SOFTLIGHT)

    float4 a = c1 * c2 * 2 + (1 - c2 * 2) * c1 * c1;
    float4 b = (1 - c2) * c1 * 2 + (c2 * 2 - 1) * sqrt(c1);
    c = lerp(a, b, c2 > 0.5);

#else

    float4 a = c1 * c2 * 2;
    float4 b = 1 - (1 - c1) * (1 - c2) * 2;

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

TEXTURE2D(_OverlayTexture);
SAMPLER(sampler_OverlayTexture);
float _UseTextureAlpha;

float4 FragmentTexture(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    uint2 positionSS = input.texcoord * _ScreenSize.xy;
    float4 c1 = LOAD_TEXTURE2D_X(_InputTexture, positionSS);
    float4 c2 = SAMPLE_TEXTURE2D(_OverlayTexture, sampler_OverlayTexture, input.texcoord);

    c1.rgb = LinearToSRGB(c1.rgb);
    c2.rgb = LinearToSRGB(c2.rgb);

    c2.rgb *= _Color;
    c2.a = _Opacity * lerp(1, c2.a, _UseTextureAlpha);

    float4 c = BlendFunction(c1, c2);

    c.rgb = SRGBToLinear(c.rgb);

    return c;
}

//
// Gradient mode
//

float2 _Direction;
float4 _ColorKey0;
float4 _ColorKey1;
float4 _ColorKey2;
float4 _ColorKey3;
float4 _ColorKey4;
float4 _ColorKey5;
float4 _ColorKey6;
float4 _ColorKey7;

float4 FragmentGradient(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    uint2 positionSS = input.texcoord * _ScreenSize.xy;
    float4 c1 = LOAD_TEXTURE2D_X(_InputTexture, positionSS);

    c1.rgb = LinearToSRGB(c1.rgb);

    float p = dot(input.texcoord - 0.5f, _Direction) + 0.5f;
    float3 c2 = _ColorKey0.rgb;
    c2 = lerp(c2, _ColorKey1.rgb, saturate((p - _ColorKey0.w) / (_ColorKey1.w - _ColorKey0.w)));
    c2 = lerp(c2, _ColorKey2.rgb, saturate((p - _ColorKey1.w) / (_ColorKey2.w - _ColorKey1.w)));

#ifdef OVERLAY_GRADIENT_EXT
    c2 = lerp(c2, _ColorKey3.rgb, saturate((p - _ColorKey2.w) / (_ColorKey3.w - _ColorKey2.w)));
    c2 = lerp(c2, _ColorKey4.rgb, saturate((p - _ColorKey3.w) / (_ColorKey4.w - _ColorKey3.w)));
    c2 = lerp(c2, _ColorKey5.rgb, saturate((p - _ColorKey4.w) / (_ColorKey5.w - _ColorKey4.w)));
    c2 = lerp(c2, _ColorKey6.rgb, saturate((p - _ColorKey5.w) / (_ColorKey6.w - _ColorKey5.w)));
    c2 = lerp(c2, _ColorKey7.rgb, saturate((p - _ColorKey6.w) / (_ColorKey7.w - _ColorKey6.w)));
#endif

    float4 c = BlendFunction(c1, float4(c2, _Opacity));

    c.rgb = SRGBToLinear(c.rgb);

    return c;
}

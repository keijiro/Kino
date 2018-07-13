#ifndef _KINO_DEPTH_UTILS_
#define _KINO_DEPTH_UTILS_

#define EXCLUDE_FAR_PLANE

float4x4 unity_CameraInvProjection;

TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

struct VaryingsDepthSupport
{
    float4 position : SV_Position;
    float2 texcoord : TEXCOORD0;
    float3 ray : TEXCOORD1;
};

// Vertex shader that procedurally outputs a full screen triangle
VaryingsDepthSupport VertexDepthSupport(uint vertexID : SV_VertexID)
{
    // Render settings
    float far = _ProjectionParams.z;
    float2 orthoSize = unity_OrthoParams.xy;
    float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

    // Vertex ID -> clip space vertex position
    float x = (vertexID != 1) ? -1 : 3;
    float y = (vertexID == 2) ? -3 : 1;
    float3 vpos = float3(x, y, 1.0);

    // Perspective: view space vertex position of the far plane
    float3 rayPers = mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;

    // Orthographic: view space vertex position
    float3 rayOrtho = float3(orthoSize * vpos.xy, 0);

    VaryingsDepthSupport o;
    o.position = float4(vpos.x, -vpos.y, 1, 1);
    o.texcoord = (vpos.xy + 1) / 2;
    o.ray = lerp(rayPers, rayOrtho, isOrtho);
    return o;
}

float3 ComputeViewSpacePosition(VaryingsDepthSupport input)
{
    // Render settings
    float near = _ProjectionParams.y;
    float far = _ProjectionParams.z;
    float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

    // Z buffer sample
    float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);

    // Far plane exclusion
    #if !defined(EXCLUDE_FAR_PLANE)
    float mask = 1;
    #elif defined(UNITY_REVERSED_Z)
    float mask = z > 0;
    #else
    float mask = z < 1;
    #endif

    // Perspective: view space position = ray * depth
    float3 vposPers = input.ray * Linear01Depth(z);

    // Orthographic: linear depth (with reverse-Z support)
    #if defined(UNITY_REVERSED_Z)
    float depthOrtho = -lerp(far, near, z);
    #else
    float depthOrtho = -lerp(near, far, z);
    #endif

    // Orthographic: view space position
    float3 vposOrtho = float3(input.ray.xy, depthOrtho);

    // Result: view space position
    return lerp(vposPers, vposOrtho, isOrtho) * mask;
}

#endif

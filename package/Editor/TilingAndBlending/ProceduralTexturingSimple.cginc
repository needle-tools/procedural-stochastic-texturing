// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

// Code from mgear / https://gist.github.com/unitycoder
// https://gist.github.com/unitycoder/6c09f186da7c626e534f16f8c4063bef
// https://www.reddit.com/r/Unity3D/comments/dhr5g2/i_made_a_stochastic_texture_sampling_shader/
// based on the paper from Thomas Deliot and Eric Heitz, https://drive.google.com/file/d/1QecekuuyWgw68HU9tg6ENfrCTCVIjm6l/view

#ifdef UNITY_COMMON_INCLUDED
// for SRPs
#define DECLARE_TEX(tex) TEXTURE2D(tex)
#define GRADIENT_SAMPLE(tex, uv, dx, dy) SAMPLE_TEXTURE2D_GRAD(tex, sampler_BaseMap, uv, dx, dy)
#else
// for built-in
#define DECLARE_TEX(tex) sampler2D tex
#define GRADIENT_SAMPLE(tex, uv, dx, dy) tex2D(tex, uv, dx, dy) 
#endif

//hash for randomness
float2 hash2D2D (float2 s)
{
    //magic numbers
    return frac(sin(fmod(float2(dot(s, float2(127.1,311.7)), dot(s, float2(269.5,183.3))), 3.14159))*43758.5453);
}

//stochastic sampling
float4 tex2DStochastic(DECLARE_TEX(tex), /*SAMPLER(_sampler),*/ float Blend, float2 UV)
{
    //triangle vertices and blend weights
    //BW_vx[0...2].xyz = triangle verts
    //BW_vx[3].xy = blend weights (z is unused)
    float4x3 BW_vx;

    //uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
    float2 skewUV = mul(float2x2 (1.0 , 0.0 , -0.57735027 , 1.15470054), UV * 3.464);

    //vertex IDs and barycentric coords
    float2 vxID = float2 (floor(skewUV));
    float3 barry = float3 (frac(skewUV), 0);
    barry.z = 1.0-barry.x-barry.y;

    BW_vx = ((barry.z>0) ? 
        float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
        float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0-barry.y, 1.0-barry.x)));

    BW_vx[3] = pow(BW_vx[3], Blend * 15);
    float sum = dot(BW_vx[3], float3(1,1,1));
    BW_vx[3] /= sum;
    
    //calculate derivatives to avoid triangular grid artifacts
    float2 dx = ddx(UV);
    float2 dy = ddy(UV); 

    //blend samples with calculated weights
    return mul(GRADIENT_SAMPLE(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +  
            mul(GRADIENT_SAMPLE(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) + 
            mul(GRADIENT_SAMPLE(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z); 
}

// ShaderGraph custom functions
#ifdef UNITY_COMMON_INCLUDED

void StochasticSample_float(DECLARE_TEX(Texture), SAMPLER(Sampler), float Blend, float2 UV, out float4 color) {
    color = tex2DStochastic(Texture, /*Sampler,*/ Blend, UV);  
}

void StochasticSample_half(DECLARE_TEX(Texture), SAMPLER(Sampler), half Blend, half2 UV, out half4 color) {
    color = tex2DStochastic(Texture, /*Sampler,*/ Blend, UV);  
}

#endif
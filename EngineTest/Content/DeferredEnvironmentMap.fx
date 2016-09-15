
float3 cameraPosition;
float3 cameraDirection;
//this is used to compute the world-position
float4x4 InvertViewProjection;

#include "helper.fx"

static int SamplesCount = 8;

Texture2D colorMap;
// normals, and specularPower in the alpha channel
Texture2D normalMap;
//depth
texture depthMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

TextureCube ReflectionCubeMap;
sampler ReflectionCubeMapSampler = sampler_state
{
    texture = <ReflectionCubeMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    Mipfilter = LINEAR;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    return output;

}

float GetNormalVariance(float2 texCoord, float3 baseNormal, float offset)
{
    float variance = 0;

    float3 normalTest;
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        normalTest = normalMap.Sample(normalSampler, texCoord.xy + offset*
                     SampleOffsets[i] * InverseResolution);
        normalTest = decode(normalTest.xyz);

        variance += 1-dot(baseNormal, normalTest);
    }

    return variance/SAMPLE_COUNT;
}

PixelShaderOutput PixelShaderFunctionClassic(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    float4 position;
    position.x = input.TexCoord.x * 2.0f - 1.0f;
    position.y = -(input.TexCoord.y * 2.0f - 1.0f);

    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
    //get metalness
    float roughness = normalData.a;
    //get specular intensity from the colorMap
    float4 color = tex2D(colorSampler, texCoord);

    float metalness = decodeMetalness(color.a);
    
    float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

    float materialType = decodeMattype(color.a);

    float matMultiplier = 1;

    if (abs(materialType - 1) < 0.1f)
    {
        matMultiplier = 1;
    }

    if (abs(materialType - 3) < 0.1f)
    {
        matMultiplier = 2;
    }

    float depthVal = 1 - tex2D(depthSampler, texCoord).r;
    ////compute screen-space position

    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;
    //surface-to-light vector

    
    float3 incident = -cameraPosition + position.xyz;
    float3 reflectionVector = reflect(incident, normal);

    float VdotH = saturate(dot(normal, incident));
    float fresnel = pow(1.0 - VdotH, 5.0);
    fresnel *= (1.0 - f0);
    fresnel += f0;

    //float NdotC = saturate(dot(-incident, normal));

    reflectionVector.z = -reflectionVector.z;

    //float3 ReflectColor = ReflectionCubeMap.Sample(ReflectionCubeMapSampler, reflectionVector) * (1 - roughness) * (1 + matMultiplier); //* NdotC * NdotC * NdotC;

    //roughness from 0.05 to 0.5
    float mip = roughness / 0.04f;


    float3 ReflectColor = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, mip)* (1 - roughness) * (1 + matMultiplier) * fresnel; //* NdotC * NdotC * NdotC;

    float3 DiffuseReflectColor = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, 9) * fresnel; //* NdotC * NdotC * NdotC;

    
    

    PixelShaderOutput output;

    output.Diffuse = float4(DiffuseReflectColor, 0) * 0.01;
    output.Specular = float4(ReflectColor, 0) * 0.02;

    return output;

}

technique Classic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunctionClassic();
    }
}

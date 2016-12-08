
float3 cameraPosition;
//this is used to compute the world-position
float4x4 InvertViewProjection;
float3x3 InvertView;

float3 FrustumCorners[4]; //In Viewspace!

#include "helper.fx"

Texture2D AlbedoMap;
// normals, and specularPower in the alpha channel
Texture2D NormalMap;

Texture2D ReflectionMap;
//depth
Texture2D DepthMap;
sampler colorSampler = sampler_state
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler reflectionSampler = sampler_state
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    Mipfilter = LINEAR;
};

sampler depthSampler = sampler_state
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
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
    float3 viewDir : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

float3 GetFrustumRay(float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    output.viewDir = GetFrustumRay(input.TexCoord);
    return output;

}
//
//float GetNormalVariance(float2 texCoord, float3 baseNormal, float offset)
//{
//    float variance = 0;
//
//    float3 normalTest;
//    for (int i = 0; i < SAMPLE_COUNT; i++)
//    {
//        normalTest = NormalMap.Sample(normalSampler, texCoord.xy + offset*
//                     SampleOffsets[i] * InverseResolution).rgb;
//        normalTest = decode(normalTest.xyz);
//
//        variance += 1-dot(baseNormal, normalTest);
//    }
//
//    return variance/SAMPLE_COUNT;
//}


PixelShaderOutput PixelShaderFunctionClassic(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = tex2D(normalSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    
    //If x and y
    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
            output.Diffuse = float4(0, 0, 0, 0);
            output.Specular = float4(0.6706f, 0.8078f, 0.9216f,0)*0.05f; //float4(0, 0.4431f, 0.78, 0) * 0.05f;
            return output;
    }
    //get metalness
    float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
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

    float3 incident = normalize(input.viewDir);

    float3 reflectionVector = reflect(incident, normal);

	//float4 reflectionVectortrafo = mul(float4(reflectionVector,1), InvertView);

	//reflectionVector = reflectionVectortrafo.xyz / reflectionVectortrafo.w;

	reflectionVector = mul(reflectionVector, InvertView);

    float VdotH = saturate(dot(normal, incident));
    float fresnel = pow(1.0 - VdotH, 5.0);
    fresnel *= (1.0 - f0);
    fresnel += f0;

    //float NdotC = saturate(dot(-incident, normal));

    reflectionVector.z = -reflectionVector.z;

    //float3 ReflectColor = ReflectionCubeMap.Sample(ReflectionCubeMapSampler, reflectionVector) * (1 - roughness) * (1 + matMultiplier); //* NdotC * NdotC * NdotC;

    //roughness from 0.05 to 0.5
    float mip = roughness / 0.04f;


    float4 ReflectColor = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, mip) * (1 - roughness) * (1 + matMultiplier) * fresnel; //* NdotC * NdotC * NdotC;

    float4 DiffuseReflectColor = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, 9) * fresnel; //* NdotC * NdotC * NdotC;


    float4 ssreflectionMap = ReflectionMap.Sample(normalSampler, input.TexCoord);
	if (ssreflectionMap.a > 0) ReflectColor.rgb = ssreflectionMap.rgb;

    output.Diffuse = float4(DiffuseReflectColor.xyz, 0) * 0.1;
    output.Specular = float4(ReflectColor.xyz, 0) * 0.4;

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



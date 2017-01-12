//Environment cube maps, TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "helper.fx"

//We want to get from VS to WS. Usually this would mean an inverted VS. To get to 3x3 it's useful to use TI on this one.
//So it's T I I = T
float3x3 TransposeView;
float3 FrustumCorners[4];

Texture2D AlbedoMap;
Texture2D NormalMap;
Texture2D ReflectionMap;

sampler PointSampler
{
    Texture = <AlbedoMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

//It won't compile without this? Need to investigate why I can't use PointSampler1
sampler PointSampler2
{
    Texture = <NormalMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

TextureCube ReflectionCubeMap;
SamplerState ReflectionCubeMapSampler
{
    texture = <ReflectionCubeMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 ViewDir : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

float3 GetFrustumRay(float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;
    output.ViewDir = GetFrustumRay(input.TexCoord);
    return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  BASE FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

PixelShaderOutput PixelShaderFunctionBasic(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = tex2D(PointSampler2, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

	//We use this to fake a sky color in the specular component
    if (normalData.x + normalData.y <= 0.001f)
    {
            output.Diffuse = float4(0, 0, 0, 0);
            output.Specular = float4(0.6706f, 0.8078f, 0.9216f,0)*0.05f; //float4(0, 0.4431f, 0.78, 0) * 0.05f;
            return output;
    }

    //get metalness
    float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
    float4 color = tex2D(PointSampler, texCoord);

    float metalness = decodeMetalness(color.a);
    
    float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

    float materialType = decodeMattype(color.a);

    float matMultiplier = 1;

    if (abs(materialType - 1) < 0.1f)
    {
        matMultiplier = 1;
    }

	//Probably obsolete now, only for testing purposes
    if (abs(materialType - 3) < 0.1f)
    {
        matMultiplier = 2;
    }

	//The incoming vector from the camera
    float3 incident = normalize(input.ViewDir);

	//The reflected vector which points to our cube map
    float3 reflectionVector = reflect(incident, normal);

	//Transform the reflectionVector from VS to WS
	reflectionVector = mul(reflectionVector, TransposeView);

	//Fresnel
    float VdotH = saturate(dot(normal, incident));
    float fresnel = pow(1.0 - VdotH, 5.0);
    fresnel *= (1.0 - f0);
    fresnel += f0;

    reflectionVector.z = -reflectionVector.z;

    //roughness from 0.05 to 0.5, coarsest of approximations
    float mip = roughness / 0.04f;

	float4 ReflectColor = pow(abs(ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, mip)), 0.45454545f);

    ReflectColor *= (1 - roughness) * (1 + matMultiplier) * fresnel; //* NdotC * NdotC * NdotC;

	float4 DiffuseReflectColor = pow(abs(ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, 9)), 0.45454545f);

    DiffuseReflectColor *= fresnel; //* NdotC * NdotC * NdotC;

	//Sample our screen space reflection map and use the environment map only as fallback
    float4 ssreflectionMap = ReflectionMap.Sample(PointSampler, input.TexCoord);
	if (ssreflectionMap.a > 0) ReflectColor.rgb = ssreflectionMap.rgb;
	else ReflectColor.rgb = pow(abs(ReflectColor.rgb), 2.2f);
	DiffuseReflectColor.rgb = pow(abs(DiffuseReflectColor.rgb), 2.2f);

    output.Diffuse = float4(DiffuseReflectColor.xyz, 0) * 0.1;
    output.Specular = float4(ReflectColor.xyz, 0) * 0.1;

    return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique Basic
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunctionBasic();
    }
}



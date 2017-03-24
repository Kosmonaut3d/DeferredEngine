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

float2 Resolution = { 1280, 800 };

bool FireflyReduction;
float FireflyThreshold = 0.1f;

float EnvironmentMapSpecularStrength = 1.0f;
float EnvironmentMapSpecularStrengthRcp = 1.0f;
float EnvironmentMapDiffuseStrength = 0.2f;

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

float GetLuma(float3 rgb)
{
	return (0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b)*10;
}

float4 GetSSR(float2 TexCoord)
{
	
	//Just a bit shorter
	int3 texCoord = int3(TexCoord * Resolution, 0);

	//Get our current Position in viewspace
	float4 similarSampleAcc = ReflectionMap.Load(texCoord);

	float alpha = similarSampleAcc.a;

	if(similarSampleAcc.a <= 0.01) return float4(0,0,0,0);

	if (!FireflyReduction) 
		return similarSampleAcc;

	float similarLuma = GetLuma(similarSampleAcc.rgb);

	//Ignore rest for dark values
	//if (similarLuma < 0.1f) return similarSampleAcc;

	float4 neighbor;
	float neighborLuma;

	float similarSamples = 1;

	float4 differentSampleAcc;
	float differentSamples = 0;

	[loop]
	for (int x = -2; x <= 2; x++)
	{
		for (int y = -2; y <= 2; y++)
		{
			//Don't sample mid again
			if (y == 0 && x == 0) continue;

			neighbor = ReflectionMap.Load(int3(texCoord.x + x, texCoord.y + y, 0));
			neighborLuma = GetLuma(neighbor.rgb);

			float weight = 1;

			if (abs(x) > 1 || abs(y) > 1) weight = 0.5f;

			//is similar?
			if (abs(neighborLuma - similarLuma) < FireflyThreshold)
			{
				//Join to similar samples
				similarSamples += weight;
				similarSampleAcc += neighbor * weight;
			}
			else
			{
				differentSamples += weight;
				differentSampleAcc += neighbor * weight;
			}
		}
	}

	similarSampleAcc /= similarSamples;
	differentSampleAcc /= differentSamples;

	return float4(similarSamples > differentSamples ? similarSampleAcc.rgb : differentSampleAcc.rgb, alpha);
}

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

	float4 ReflectColor = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, mip);

    ReflectColor *= (1 - roughness) * (1 + matMultiplier) * fresnel; //* NdotC * NdotC * NdotC;

	float4 DiffuseReflectColor = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, 9);

    DiffuseReflectColor *= fresnel; //* NdotC * NdotC * NdotC;

	//Sample our screen space reflection map and use the environment map only as fallback
	float4 ssreflectionMap = GetSSR(input.TexCoord);
	
	if (ssreflectionMap.a > 0) ReflectColor.rgb = ssreflectionMap.rgb * EnvironmentMapSpecularStrengthRcp;
	
    output.Diffuse = float4(DiffuseReflectColor.xyz, 0) * EnvironmentMapDiffuseStrength;
    output.Specular = float4(ReflectColor.xyz, 0) *EnvironmentMapSpecularStrength;

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



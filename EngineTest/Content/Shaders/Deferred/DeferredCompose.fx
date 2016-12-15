
Texture2D colorMap;
// normals, and specularPower in the alpha channel
Texture2D diffuseLightMap;
Texture2D specularLightMap;
Texture2D volumeLightMap;
Texture2D SSRMap;

static float2 Resolution = float2(1280, 800);

float average_hologram_depth = 10;
bool useGauss = true;

#include "helper.fx"

float exposure = 2;

Texture2D SSAOMap;

bool useSSAO = true;

sampler pointSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

Texture2D HologramMap;
sampler linearSampler = sampler_state
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
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




////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;
    return output;
}

float pixelsize_intended = 3;
 

float4 GaussianSampler(float2 TexCoord, float offset)
{
    float4 finalColor = float4(0, 0, 0, 0);
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        finalColor += HologramMap.SampleLevel(linearSampler, TexCoord.xy +
                    offset * SampleOffsets[i] * InverseResolution, 0) * SampleWeights[i];
    }
    return finalColor;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 diffuseColor = colorMap.Sample(pointSampler, input.TexCoord);

	float albedoColorProp = diffuseColor.a;

	float materialType = decodeMattype(albedoColorProp);

	float metalness = decodeMetalness(albedoColorProp);

	float3 diffuseContrib = float3(0, 0, 0);

	//
	[branch]
	if (useGauss)
	{
		[branch]
		if (abs(materialType - 2) < 0.1f)
		{
			float4 hologramColor = GaussianSampler(input.TexCoord, 3);
			//    float2 pixel = trunc(input.TexCoord * Resolution);

			//    float pixelsize2 = 2 * pixelsize;
			//    if (pixel.x % pixelsize2 <= pixelsize && pixel.y % pixelsize2 <= pixelsize)
			diffuseContrib = float3(0, hologramColor.x * 0.49, hologramColor.x * 0.95f) * 0.06f;
		}
	}
	else
	{
		float pixelsize = pixelsize_intended;

		float2 hologramTexCoord = trunc(input.TexCoord * Resolution / pixelsize / 2) / Resolution * pixelsize * 2;

		float hologramColor = HologramMap.Sample(linearSampler, hologramTexCoord).r;
		if (abs(materialType - 2) < 0.1f)
		{
			float2 pixel = trunc(input.TexCoord * Resolution);

			float pixelsize2 = 2 * pixelsize;
			if (pixel.x % pixelsize2 <= pixelsize && pixel.y % pixelsize2 <= pixelsize)
				diffuseContrib = float3(0, hologramColor * 0.49, hologramColor * 0.95f) * 0.06f;

		}
	}

	if (abs(materialType - 3) < 0.1f)
	{
		return diffuseColor;
		}

	//SSAO
	float ssaoContribution = 1;
	if (useSSAO)
	{
		ssaoContribution = SSAOMap.Sample(linearSampler, input.TexCoord).r;
	}

	float f0 = lerp(0.04f, diffuseColor.g * 0.25 + 0.75, metalness);

	float3 diffuseLight = diffuseLightMap.Sample(pointSampler, input.TexCoord).rgb;
	float3 specularLight = specularLightMap.Sample(pointSampler, input.TexCoord).rgb;

	float3 volumeLight = volumeLightMap.Sample(pointSampler, input.TexCoord).rgb;

	//float4 ssreflectionMap = SSRMap.Sample(linearSampler, input.TexCoord);
	//specularLight += ssreflectionMap.rgb / exposure / 2;
	////lerp(specularLight, ssreflectionMap.rgb / exposure, ssreflectionMap.a);

	float3 plasticFinal = diffuseColor.rgb * (diffuseLight)+specularLight;

	float3 metalFinal = specularLight * diffuseColor.rgb;

	float3 finalValue = lerp(plasticFinal, metalFinal, metalness) + diffuseContrib;

	return float4(finalValue * ssaoContribution + volumeLight,  1) * exposure;
}

float4 PixelShaderSSRFunction(VertexShaderOutput input) : COLOR0
{
	float4 diffuseColor = colorMap.Sample(pointSampler, input.TexCoord);

	//linear?
	diffuseColor.rgb = pow(abs(diffuseColor.rgb), 2.2f);

	float albedoColorProp = diffuseColor.a;

	float materialType = decodeMattype(albedoColorProp);

	float metalness = decodeMetalness(albedoColorProp);

	float3 diffuseContrib = float3(0, 0, 0);

	//
	[branch]
	if (useGauss)
	{
		[branch]
		if (abs(materialType - 2) < 0.1f)
		{
			float4 hologramColor = GaussianSampler(input.TexCoord, 3);
			//    float2 pixel = trunc(input.TexCoord * Resolution);

			//    float pixelsize2 = 2 * pixelsize;
			//    if (pixel.x % pixelsize2 <= pixelsize && pixel.y % pixelsize2 <= pixelsize)
			diffuseContrib = float3(0, hologramColor.x * 0.49, hologramColor.x * 0.95f) * 0.06f;
		}
	}
	else
	{
		float pixelsize = pixelsize_intended;

		float2 hologramTexCoord = trunc(input.TexCoord * Resolution / pixelsize / 2) / Resolution * pixelsize * 2;

		float hologramColor = HologramMap.Sample(linearSampler, hologramTexCoord).r;
		if (abs(materialType - 2) < 0.1f)
		{
			float2 pixel = trunc(input.TexCoord * Resolution);

			float pixelsize2 = 2 * pixelsize;
			if (pixel.x % pixelsize2 <= pixelsize && pixel.y % pixelsize2 <= pixelsize)
				diffuseContrib = float3(0, hologramColor * 0.49, hologramColor * 0.95f) * 0.06f;

		}
	}

	if (abs(materialType - 3) < 0.1f)
	{
		return diffuseColor;
	}

	//SSAO
	float ssaoContribution = 1;
	if (useSSAO)
	{
		ssaoContribution = SSAOMap.Sample(linearSampler, input.TexCoord).r;
	}

	float f0 = lerp(0.04f, diffuseColor.g * 0.25 + 0.75, metalness);

	float3 diffuseLight = diffuseLightMap.Sample(pointSampler, input.TexCoord).rgb;
	float3 specularLight = specularLightMap.Sample(pointSampler, input.TexCoord).rgb;

	float3 volumeLight = volumeLightMap.Sample(pointSampler, input.TexCoord).rgb;

	diffuseLight = pow(abs(diffuseLight.rgb), 2.2f);
	specularLight = pow(abs(specularLight.rgb), 2.2f);
	volumeLight = pow(abs(volumeLight.rgb), 2.2f);
	//float4 ssreflectionMap = SSRMap.Sample(linearSampler, input.TexCoord);
	//specularLight += ssreflectionMap.rgb / exposure / 2;
	////lerp(specularLight, ssreflectionMap.rgb / exposure, ssreflectionMap.a);

	float3 plasticFinal = diffuseColor.rgb * (diffuseLight)+specularLight;

	float3 metalFinal = specularLight * diffuseColor.rgb;

	float3 finalValue = lerp(plasticFinal, metalFinal, metalness) + diffuseContrib;

	float3 output = (finalValue * ssaoContribution + volumeLight) ;

	return float4(pow(abs(output), 1 / 2.2f) * exposure, 1);
}


technique Technique1                                         
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique TechniqueSSR
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderSSRFunction();
    }
}

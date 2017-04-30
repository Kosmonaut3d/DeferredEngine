////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Deferred Compose
//  Composes the light buffers and the GBuffer to our final HDR Output.
//  Converts albedo from Gamma 2.2 to 1.0 and outputs an HDR file.

#include "../Common/helper.fx"

Texture2D colorMap;
Texture2D normalMap;
Texture2D diffuseLightMap;
Texture2D specularLightMap;
Texture2D volumeLightMap;

static float2 Resolution = float2(1280, 800);

Texture2D SSAOMap;

bool useSSAO = true;

//Texture2D HologramMap;
//float average_hologram_depth = 10;
//bool useGauss = true;

sampler pointSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
    float2 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uint id:SV_VERTEXID)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 0, 1);
	output.TexCoord.x = (float)(id / 2) * 2.0;
	output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;

	return output;
}

// For smooth holograms
//float4 GaussianSampler(float2 TexCoord, float offset)
//{
//    float4 finalColor = float4(0, 0, 0, 0);
//    for (int i = 0; i < SAMPLE_COUNT; i++)
//    {
//        finalColor += HologramMap.SampleLevel(linearSampler, TexCoord.xy +
//                    offset * SampleOffsets[i] * InverseResolution, 0) * SampleWeights[i];
//    }
//    return finalColor;
//}



float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	int3 texCoordInt = int3(input.Position.xy, 0);

	float4 diffuseColor = colorMap.Load(texCoordInt);
	float4 normalInfo = normalMap.Load(texCoordInt);
	//Convert gamma for linear pipeline
	diffuseColor.rgb = pow(abs(diffuseColor.rgb), 2.2f);

	// See Resources/MaterialEffect for different mat types!
	// materialType 3 = emissive
	// materialType 2 = hologram
	// materialType 1 = default
	float materialType = decodeMattype(normalInfo.b);

	float metalness = decodeMetalness(normalInfo.b);

	//Our "volumetric" light data. This is a seperate buffer that is renders on top of all other stuff.
	float3 volumetrics = volumeLightMap.Load(texCoordInt).rgb;

	//Emissive Material
	//If the material is emissive (matType == 3) we store the factor inside metalness. We simply output the emissive material and do not compose with lighting
	if (abs(materialType - 3) < 0.1f)
	{
		// Optional: 2 << metalness*8, pow(2, m*8) etc.
		// Note: metalness is used as the power value in this case
		return float4(diffuseColor.rgb * metalness * 8 + volumetrics, 1);
	}

	float3 diffuseContrib = float3(0, 0, 0);

	// Hologram effect -> see https://kosmonautblog.wordpress.com/2016/07/25/deferred-engine-progress-part2/
	/*[branch]
	if (useGauss)
	{
		float pixelsize_intended = 3;
 
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
	} */

	//SSAO
	float ssaoContribution = 1;

	[branch]
	if (useSSAO)
	{
		ssaoContribution = SSAOMap.SampleLevel(pointSampler, input.TexCoord, 0).r;
	}

	//float f0 = lerp(0.04f, diffuseColor.g * 0.25 + 0.75, metalness);

	float3 diffuseLight = diffuseLightMap.Load(texCoordInt).rgb;
	float3 specularLight = specularLightMap.Load(texCoordInt).rgb;

	float3 plasticFinal = diffuseColor.rgb * (diffuseLight)+specularLight;

	float3 metalFinal = diffuseColor.rgb * specularLight;

	float3 finalValue = lerp(plasticFinal, metalFinal, metalness) + diffuseContrib;

	float3 output = (finalValue * ssaoContribution + volumetrics);

	return float4(output, 1);
}

technique TechniqueLinear
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

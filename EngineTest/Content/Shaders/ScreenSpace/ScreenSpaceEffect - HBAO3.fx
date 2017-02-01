
float3 CameraPosition;
//this is used to compute the world-position
float4x4 InverseViewProjection;
float4x4 Projection;
float4x4 ViewProjection;

float2 Resolution = float2(1280, 800);

float3 FrustumCorners[4]; //In Viewspace!

#include "helper.fx"

Texture2D NormalMap;
Texture2D DepthMap;

Texture2D SSAOMap;

SamplerState texSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

SamplerState blurSamplerPoint
{
	Texture = <SSAOMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

SamplerState blurSamplerLinear
{
	Texture = <SSAOMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = POINT;
};

float FalloffMin = 0.000001f;
float FalloffMax = 0.002f;

int Samples = 8;

float Strength = 4;

float SampleRadius = 0.05f;

////////////////////////////////////////////////////////////////////////////
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
	float3 ViewRay : TEXCOORD1;
};

struct VertexShaderOutputBlur
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

float3 GetFrustumRay(float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

float3 GetFrustumRay2(float2 texCoord)
{
	float3 x1 = lerp(FrustumCorners[0], FrustumCorners[1], texCoord.x);
	float3 x2 = lerp(FrustumCorners[2], FrustumCorners[3], texCoord.x);
	float3 outV = lerp(x1, x2, texCoord.y);
	return outV;
}


 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
	output.ViewRay = GetFrustumRay(input.TexCoord);
    return output;
}

VertexShaderOutputBlur VertexShaderBlurFunction(VertexShaderInput input)
{
    VertexShaderOutputBlur output;
    output.Position = float4(input.Position, 1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    return output;
}


float3 randomNormal(float2 tex)
{
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f));
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f));
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f));
    return normalize(float3(noiseX, noiseY, noiseZ));
}

float3 getPosition(float2 texCoord)
{
	float linearDepth = DepthMap.SampleLevel(texSampler, texCoord, 0).r;
	return GetFrustumRay2(texCoord) * linearDepth;
}

float weightFunction(float3 vec3, float radius)
{
	// NVIDIA's weighting function
	return 1.0 - /*length(vec3) / radius;*/pow(length(vec3) / radius, 2.0);
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	const float PI = 3.141;

	const float3 kernel[] =
	{
	float3(0.2024537f, 0.841204f, -0.9060141f),
	float3(-0.2200423f, 0.6282339f, -0.8275437f),
	float3(-0.7578573f, -0.5583301f, 0.2347527f),
	float3(-0.4540417f, -0.252365f, 0.0694318f),
	float3(0.3677659f, 0.1086345f, -0.4466777f),
	float3(0.8775856f, 0.4617546f, -0.6427765f),
	float3(-0.8433938f, 0.1451271f, 0.2202872f),
	float3(-0.4037157f, -0.8263387f, 0.4698132f),
	float3(0.7867433f, -0.141479f, -0.1567597f),
	float3(0.4839356f, -0.8253108f, -0.1563844f),
	float3(0.4401554f, -0.4228428f, -0.3300118f),
	float3(0.0019193f, -0.8048455f, 0.0726584f),
	float3(-0.0483353f, -0.2527294f, 0.5924745f),
	float3(-0.4192392f, 0.2084218f, -0.3672943f),
	float3(-0.6657394f, 0.6298575f, 0.6342437f),
	float3(-0.0001783f, 0.2834622f, 0.8343929f),
	};

	float2 texCoord = float2(input.TexCoord);

	//get normal data from the NormalMap
	float4 normalData = NormalMap.Sample(texSampler, texCoord);
	//tranform normal back into [-1,1] range
	float3 currentNormal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

	float linearDepth = DepthMap.Sample(texSampler, texCoord).r;

	if (linearDepth > 0.99999f)
	{
		return float4(1, 1, 1, 1);
	}
	float3 currentPos = input.ViewRay * linearDepth;

	//alternative 
	//currentPos = getPosition(texCoord);

	float currentDistance = -input.ViewRay.z;

	float2 aspectRatio = float2(min(1, Resolution.y / Resolution.x), min(1.0f, Resolution.x / Resolution.y));

	float amount = 0.0;

	float3 noise = randomNormal(texCoord);

	//HBAO 2 dir
	for (int i = 0; i < Samples/2; i++)
	{
		float3 kernelVec = reflect(kernel[i], noise);
		/*kernelVec.xy *= aspectRatio;
*/
		float radius = SampleRadius;

		float2 kernelVecSS = (kernelVec.xy / currentDistance) * radius * aspectRatio;

		//Clamp to 0,1
		kernelVecSS = saturate(texCoord + kernelVecSS) - texCoord;

		float biggestAnglePos = 0.05f;

		float biggestAngleNeg = 0.05f;

		float wAO = 0.0;

		float3 viewDir = float3(0, 0, -1);

		float3 kernelNormalized = normalize(float3(kernelVec.xy, 0));
		float3 projectedNormal = dot(currentNormal, viewDir) * viewDir + dot(currentNormal, kernelNormalized) * kernelNormalized;
		projectedNormal = currentNormal;

		for (int b = 1; b <= 4; b++)
		{
			float3 sampleVec = getPosition(texCoord + kernelVecSS * b / 4.0f) - currentPos;
			float sampleAngle;

			if ((texCoord + kernelVecSS * b / 4.0f).y > 1) return float4(0, 0, 0, 0);

			if (length(sampleVec) < radius*0.4f)
			{

				sampleAngle = dot(normalize(sampleVec), projectedNormal);

				if (sampleAngle > biggestAnglePos)
				{

					biggestAnglePos = sampleAngle;
				}
			}

			sampleVec = getPosition(texCoord - kernelVecSS * b / 4.0f) - currentPos;

			if (length(sampleVec) < radius*0.4f)
			{

				sampleAngle = dot(normalize(sampleVec), projectedNormal);
				if (sampleAngle > biggestAngleNeg)
				{

					biggestAngleNeg = sampleAngle;
				}
			}
		}

		//Plane is created by
		// 0 0 -1 and kernelVec 0
		/*float3 kernelNormalized = normalize(float3(kernelVec.xy, 0));
		float3 projectedNormal = dot(currentNormal, viewDir) * viewDir + dot(currentNormal, kernelNormalized) * kernelNormalized;
*/

		////Angle normal
		float n = acos(dot(currentNormal, viewDir));

		float h1 = -acos(biggestAngleNeg);

		float h2 = acos(biggestAnglePos);
/*
		h1 = n + max(h1 - n, -PI / 2);
		h2 = n + min(h2 - n, PI / 2);*/

		float integral = (1 - cos(h1)) + (1 - cos(h2)) /*+ 0.1f*/;
		/*integral /= PI;*/
		
		amount += integral;
		//amount += (0.25 * (-cos(2 * h1 - n) + cos(n) + 2 * h1 * sin(n)) + 0.25*(-cos(2 * h2 - n) + cos(n) + 2 * h2*sin(n))) * length(projectedNormal);
		/*amount *= length(projectedNormal);*/
		/*biggestAngle = max(0, biggestAngle);
		
		amount -= biggestAngle * Strength + currentNormal.z * 0.0000001f;*/
	}

	amount /= Samples;

	amount = 1 - ((1 - amount)*Strength);

	return float4(amount, amount, amount, currentNormal.z);

}



float4 BilateralBlurVertical(VertexShaderOutputBlur input) : SV_TARGET
{
    const uint numSamples = 9;
    const float texelsize = InverseResolution.x; 
    const float samplerOffsets[numSamples] =
      { -4.0f, -3.0f, -2.0f, -1.0f, 0.0f, 1.0f, 2.0f, 3.0f, 4.0f };
    const float gaussianWeights[numSamples] =
    {
        0.055119, 0.081029, 0.106701, 0.125858, 0.13298, 0.125858, 0.106701, 0.081029, 0.055119
    };
    
    const uint numSamples2 = 4;
    const float samplerOffsets2[numSamples2] =
      { -7.5f, -5.5f, 5.5f, 7.5f };
    const float gaussianWeights2[numSamples2] =
    {
        0.012886, 0.051916, 0.051916, 0.012886,
    };

    float compareDepth = DepthMap.Sample(texSampler, input.TexCoord).r;

    float4 result = 0;
    float weightSum = 0.0f;
    [unroll]
    for (uint i = 0; i < numSamples; ++i)
    {
        float2 sampleOffset = float2( texelsize * samplerOffsets[i],0);
        float2 samplePos = input.TexCoord + sampleOffset;
         
        float sampleDepth = DepthMap.Sample(texSampler, samplePos).r;
         
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights[i];
        
        result += SSAOMap.Sample(blurSamplerPoint, samplePos) * weight;
        
        weightSum += weight;
    }

   [unroll]
    for (uint j = 0; j < numSamples2; ++j)
    {
        float2 sampleOffset = float2(texelsize * samplerOffsets2[j],0);
        float2 samplePos = input.TexCoord + sampleOffset;
         
        float sampleDepth = DepthMap.Sample(texSampler, samplePos).r;
        
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights2[j];
          
        result += SSAOMap.Sample(blurSamplerLinear, samplePos, 0) * weight;
          
        weightSum += weight;
         
    }

    result /= weightSum;
    
    return result;
}

float4 BilateralBlurHorizontal(VertexShaderOutputBlur input) : SV_TARGET
{
    const uint numSamples = 9;
    const float texelsize = InverseResolution.y;
    const float samplerOffsets[numSamples] =
      { -4.0f, -3.0f, -2.0f, -1.0f, 0.0f, 1.0f, 2.0f, 3.0f, 4.0f };
    const float gaussianWeights[numSamples] =
    {
    0.055119, 0.081029,0.106701, 0.125858, 0.13298, 0.125858,0.106701,0.081029, 0.055119
    };
    
    const uint numSamples2 = 4;
    const float samplerOffsets2[numSamples2] =
      { -7.5f, -5.5f, 5.5f, 7.5f };
    const float gaussianWeights2[numSamples2] =
    {
        0.012886, 0.051916, 0.051916, 0.012886,
    };

    float compareDepth = DepthMap.Sample(texSampler, input.TexCoord).r;
    
    float4 result = 0;
    float weightSum = 0.0f;
    [unroll]
    for (uint i = 0; i < numSamples; ++i)
    {
        float2 sampleOffset = float2(0.0f, texelsize * samplerOffsets[i]);
        float2 samplePos = input.TexCoord + sampleOffset;
        
        float sampleDepth = DepthMap.Sample(texSampler, samplePos).r;
         
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights[i];
        
        result += SSAOMap.Sample(blurSamplerPoint, samplePos) * weight;
         
        weightSum += weight;
          
    }

    [unroll]
    for (uint j = 0; j < numSamples2; ++j)
    {
        float2 sampleOffset = float2(0.0f, texelsize * samplerOffsets2[j]);
        float2 samplePos = input.TexCoord + sampleOffset;
         
        float sampleDepth = DepthMap.SampleLevel(texSampler, samplePos, 0).r;
          
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights2[j];
        
        result += SSAOMap.Sample(blurSamplerLinear, samplePos) * weight;
          
        weightSum += weight;
         
    }

    result /= weightSum;
    
    return result;
}


technique SSAO
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique BilateralVertical
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderBlurFunction();
        PixelShader = compile ps_4_0 BilateralBlurVertical();
    }
}

technique BilateralHorizontal
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderBlurFunction();
        PixelShader = compile ps_4_0 BilateralBlurHorizontal();
    }
}

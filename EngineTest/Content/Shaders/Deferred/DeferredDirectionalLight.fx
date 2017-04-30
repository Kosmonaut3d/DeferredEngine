//Lightshader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"

float4x4 ViewProjection;

//color of the light 
float3 lightColor;
//position of the camera, for specular light
float3 cameraPosition = float3(0,0,0);
//this is used to compute the world-position
float4x4 InvertViewProjection;
float4x4 LightViewProjection;
float4x4 LightView;
float LightFarClip;

float3 LightVector;
//control the brightness of the light
float lightIntensity = 1.0f;

// diffuse color, and specularIntensity in the alpha channel
Texture2D AlbedoMap;
// normals, and specularPower in the alpha channel
Texture2D NormalMap;
      
//depth
Texture2D DepthMap;

Texture2D ShadowMap;
Texture2D SSShadowMap;

int ShadowFiltering = 0; //PCF, PCF(3), PCF(7), Poisson, VSM

float ShadowMapSize = 2048;
float DepthBias = 0.02;

       

SamplerState pointSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP; 
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
  
SamplerState linearSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

SamplerState ShadowSampler
{
	Texture = (ShadowMap);
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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uint id:SV_VERTEXID)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 0, 1);
	output.TexCoord.x = (float)(id / 2) * 2.0;
	output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;

	output.ViewDir = GetFrustumRay(id);
	return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

float GetVariableBias(float nDotL)
{
	//return /*(1 - abs(nDotL)) * DepthBias;*/clamp(0.001 * tan(acos(nDotL)), 0, DepthBias);
	return clamp(0.001 * sqrt(1 - nDotL * nDotL) / nDotL, 0, DepthBias);
}

// Calculates the shadow term using PCF with edge tap smoothing
float CalcShadowTermSoftPCF(float fLightDepth, float ndotl, float2 vTexCoord, int iSqrtSamples)
{
	float fShadowTerm = 0.0f;

	//float variableBias = GetVariableBias(ndotl);
/*
	float variableBias = (-cos(ndotl) + 1)*0.02;*/
	//variableBias = DepthBias;

	float shadowMapSize = ShadowMapSize;

	float2 fractionals = frac(ShadowMapSize * vTexCoord);
	float2 complFractionals = float2(1, 1) - fractionals;

	float fRadius = iSqrtSamples - 1;

	[unroll]
	for (float y = -fRadius; y <= fRadius; y++)
	{
		[unroll]
		for (float x = -fRadius; x <= fRadius; x++)
		{
			float2 vOffset = 0;
			vOffset = float2(x, y);

			int3 vSamplePoint = int3(vTexCoord * ShadowMapSize + vOffset, 0);
			float fDepth = ShadowMap.Load(vSamplePoint).x;
			float fSample = (fLightDepth <= fDepth /*+ variableBias*(x+y)*/);

			// Edge tap smoothing
			float xWeight = 1;
			float yWeight = 1;

			if (x == -fRadius)
				xWeight = complFractionals.x;
			else if (x == fRadius)
				xWeight = fractionals.x;

			if (y == -fRadius)
				yWeight = complFractionals.y;
			else if (y == fRadius)
				yWeight = fractionals.y;

			fShadowTerm += fSample * xWeight * yWeight;
		}
	}

	fShadowTerm /= (fRadius * fRadius * 4);

	return fShadowTerm;
}

float CalcShadowTermPCF(float linearDepthLV, float ndotl, float2 shadowTexCoord)
{
	float lightTerm = 0;

	float2 fractionals = frac(ShadowMapSize * shadowTexCoord);

	//safe to assume it's a square
	float size = 1.0f / ShadowMapSize;

	float variableBias = GetVariableBias(ndotl);

	float testDepth = linearDepthLV - variableBias;
	//Center
	lightTerm = (linearDepthLV < ShadowMap.SampleLevel(ShadowSampler, shadowTexCoord, 0).r);

	//Right
	lightTerm += (testDepth < ShadowMap.SampleLevel(ShadowSampler, shadowTexCoord + float2(size, 0), 0).r) * fractionals.x;

	//Left
	lightTerm += (testDepth < ShadowMap.SampleLevel(ShadowSampler, shadowTexCoord + float2(-size, 0), 0).r) * (1- fractionals.x);

	//Top
	lightTerm += (testDepth < ShadowMap.SampleLevel(ShadowSampler, shadowTexCoord + float2(0, size), 0).r) * fractionals.y;

	//Bot
	lightTerm += (testDepth < ShadowMap.SampleLevel(ShadowSampler, shadowTexCoord + float2(0, -size), 0).r) * (1 - fractionals.y);

	//samples[1] = (light_space_depth - variableBias < ShadowMap.SampleLevel(pointSampler, shadow_coord + float2(size, 0), 0).r) * fractionals;
	
	lightTerm /= 3;

	return lightTerm;
}

float random(float4 seed4)
{
	float dot_product = dot(seed4, float4(12.9898, 78.233, 45.164, 94.673));
	return frac(sin(dot_product) * 43758.5453);
}

float CalcShadowPoisson(float light_space_depth, float ndotl, float2 shadow_coord, float2 texCoord)
{
	float shadow_term = 0;

	const float2 poissonDisk[] =
	{
		float2(0.1908291f, 0.1823764f),
		float2(0.4236465f, 0.76107f),
		float2(-0.3056469f, 0.5557697f),
		float2(-0.4979181f, 0.1770361f),
		float2(0.4962559f, -0.2154941f),
		float2(0.6897131f, 0.4324413f),
		float2(-0.3782056f, -0.3405231f),
		float2(0.04382932f, -0.2403435f),
		float2(0.886423f, -0.05176726f),
		float2(0.4599024f, -0.6679791f),
		float2(-0.8389286f, -0.4176486f),
		float2(-0.9797052f, -0.0152119f),
		float2(-0.2747172f, -0.7914276f),
		float2(-0.7316247f, 0.6114004f),
		float2(-0.220655f, 0.9378002f),
		float2(0.1389218f, -0.8920172f) };

	//float2 v_lerps = frac(ShadowMapSize * shadow_coord);

	float variableBias = GetVariableBias(ndotl);

	//safe to assume it's a square
	float size = 1 / ShadowMapSize;

	const uint j = 16;
	[unroll]
	for (uint i = 0; i < 4; i++)
	{
		int index = int(16.0 * random(float4(texCoord.xyy, i))) % j;

		float2 texCoords = shadow_coord + poissonDisk[index] * size;

		shadow_term += (light_space_depth - variableBias < ShadowMap.SampleLevel(pointSampler, texCoords, 0).r);
	}

	shadow_term /= 4.0f;

	return shadow_term;
}

//ChebyshevUpperBound
float CalcShadowVSM(float distance, float2 texCoord)
{
	// We retrive the two moments previously stored (depth and depth*depth)
	float2 moments = 1 - ShadowMap.SampleLevel(linearSampler, texCoord, 0).rg;

	// Surface is fully lit. as the current fragment is before the light occluder
	//if (distance <= moments.x)
	//    return 1.0;

	// The fragment is either in shadow or penumbra. We now use chebyshev's upperBound to check
	// How likely this pixel is to be lit (p_max)
	float variance = moments.y - (moments.x * moments.x);
	variance = max(variance, 0.0002);

	float d = distance - moments.x;
	float p_max = variance / (variance + d * d);

	return p_max;
}


		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  MAIN FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////


PixelShaderOutput PixelShaderUnshadowedFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    [branch]
    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        output.Diffuse = float4(0, 0, 0, 0);
        output.Specular = float4(0, 0, 0, 0);
        return output;
    }
    else
    {
		//get metalness
        float roughness = normalData.a;
		//get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Sample(pointSampler, texCoord);

        float metalness = decodeMetalness(normalData.b);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        float3 cameraDirection = -normalize(input.ViewDir);

        float NdL = saturate(dot(normal, -LightVector));

        float3 diffuse = DiffuseOrenNayar(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
        float3 specular = SpecularCookTorrance(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);

        output.Diffuse = float4(diffuse, 0) * (1 - f0) * 0.1f;
        output.Specular = float4(specular, 0) * 0.1f;

        return output;
    }
}



float4 PixelShaderScreenSpaceShadowFunction(VertexShaderOutput input) : SV_Target
{
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        return float4(0, 1, 0, 0);
    }
    else
    {
		float NdL = saturate(dot(normal, -LightVector));

		//Get our current Position in viewspace
		float linearDepth = DepthMap.Sample(pointSampler, texCoord).r;
		float3 positionVS = input.ViewDir * linearDepth;

		float4 positionInLS = mul(float4(positionVS, 1), LightViewProjection);
		float depthInLS = (positionInLS.z / positionInLS.w);

		float2 ShadowTexCoord = mad(positionInLS.xy / positionInLS.w, 0.5f, float2(0.5f, 0.5f));
		ShadowTexCoord.y = 1 - ShadowTexCoord.y;
		//float depthInSM = 1-ShadowMap.Sample(pointSampler, ShadowTexCoord);
		float shadowContribution = 1;

		[branch]
		if (NdL > 0)
		{
			[branch]
			if (ShadowFiltering == 0)
			{
				shadowContribution = CalcShadowTermPCF(depthInLS, NdL, ShadowTexCoord);
			}
			else if (ShadowFiltering == 1)
			{
				shadowContribution = CalcShadowTermSoftPCF(depthInLS, NdL, ShadowTexCoord, 3);
			}
			else if (ShadowFiltering == 2)
			{
				shadowContribution = CalcShadowTermSoftPCF(depthInLS, NdL, ShadowTexCoord, 5);
			}
			else if (ShadowFiltering == 3)
			{
				shadowContribution = CalcShadowPoisson(depthInLS, NdL, ShadowTexCoord, texCoord);
			}
			else
			{
				float3 lightVector = LightVector;
				lightVector.z = -LightVector.z;
				shadowContribution = CalcShadowVSM(depthInLS, ShadowTexCoord);
			}
		}
		else
		{
			shadowContribution = 0;
		}

        return float4(0, shadowContribution, 0, 0);
    }
}

//No screen space shadows - we need to calculate them together with the lighting
PixelShaderOutput PixelShaderShadowedFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        output.Diffuse = float4(0, 0, 0, 0);
        output.Specular = float4(0, 0, 0, 0);
        return output;
    }
    else
    {
        float NdL = saturate(dot(normal, -LightVector));

		//Get our current Position in viewspace
		float linearDepth = DepthMap.Sample(pointSampler, texCoord).r;
		float3 positionVS = input.ViewDir * linearDepth; 

        float4 positionInLVP = mul(float4(positionVS,1), LightViewProjection);
		float4 positionInLV = mul(float4(positionVS, 1), LightView);
		float4 depthInLV = positionInLV.z / positionInLV.w / -LightFarClip;

        float2 ShadowTexCoord = mad(positionInLVP.xy / positionInLVP.w, 0.5f, float2(0.5f, 0.5f));
        ShadowTexCoord.y = 1 - ShadowTexCoord.y;

		float shadowContribution = 1;


        [branch]
        if (NdL > 0)
        {
            [branch]
            if (ShadowFiltering == 0)
            {
                shadowContribution = CalcShadowTermPCF(depthInLV, NdL, ShadowTexCoord);
            }
            else if (ShadowFiltering == 1)
            {
                shadowContribution = CalcShadowTermSoftPCF(depthInLV, NdL, ShadowTexCoord, 3);
            }
            else if (ShadowFiltering == 2)
            {
                shadowContribution = CalcShadowTermSoftPCF(depthInLV, NdL, ShadowTexCoord, 5);
            }
            else if (ShadowFiltering == 3)
            {
                shadowContribution = CalcShadowPoisson(depthInLV, NdL, ShadowTexCoord, texCoord);
            }
            else
            {
                float3 lightVector = LightVector;
                lightVector.z = -LightVector.z;
                shadowContribution = CalcShadowVSM(depthInLV, ShadowTexCoord);
            }
        }
        else
        {
            shadowContribution = 0;
        }

    //get metalness
        float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Sample(pointSampler, texCoord);

		float metalness = decodeMetalness(normalData.b);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        float3 cameraDirection = -normalize(input.ViewDir);

        float3 diffuse = float3(0,0,0);
        float3 specular = float3(0, 0, 0);

        [branch]
        if(shadowContribution > 0)
        {
            diffuse = DiffuseOrenNayar(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
            specular = SpecularCookTorrance(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
        }

        output.Diffuse = float4(diffuse, 0) * (1 - f0) * 0.1f * shadowContribution;
        output.Specular = float4(specular, 0) * 0.1f * shadowContribution;

        return output;
    }
}
            
//This one is used when we have screen space shadows already
PixelShaderOutput PixelShaderSSShadowedFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        output.Diffuse = float4(0, 0, 0, 0);
        output.Specular = float4(0, 0, 0, 0);
        return output;
    }
    else
    {     
        float NdL = saturate(dot(normal, -LightVector));

        float shadowContribution = SSShadowMap.Sample(ShadowSampler, texCoord).g;

        //get metalness
        float roughness = normalData.a;
        //get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Sample(pointSampler, texCoord);

		float metalness = decodeMetalness(normalData.b);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        float3 cameraDirection = -normalize(input.ViewDir);


        float3 diffuse = float3(0, 0, 0);
        float3 specular = float3(0, 0, 0);

        [branch]
        if (shadowContribution > 0)
        {
            diffuse = DiffuseOrenNayar(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
            specular = SpecularCookTorrance(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
        }

        output.Diffuse = float4(diffuse, 0) * (1 - f0) * 0.1f * shadowContribution;
        output.Specular = float4(specular, 0) * 0.1f * shadowContribution;

        return output;
    }
}



technique ShadowOnly
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderScreenSpaceShadowFunction();
    }
}

technique Unshadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderUnshadowedFunction();
    }
}

technique Shadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderShadowedFunction();
    }
}

technique SSShadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderSSShadowedFunction();
    }
}
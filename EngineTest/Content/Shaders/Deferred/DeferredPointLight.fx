
//Lightshader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"
#include "../Common/sdf.fx"

float4x4 WorldView;
float4x4 WorldViewProj;
float4x4 InverseView;

//Is the camera inside the light sphere?
int inside = -1;

//Temporal displacement
//Inclueded in Helper
//float Time = 1;

//color of the light 
float3 lightColor;
//position of the camera, for specular light
float3 cameraPosition = float3(0,0,0);
//this is used to compute the world-position
float4x4 InvertViewProjection;
//this is the position of the light
float3 lightPosition = float3(0,0,0);
//how far does this light reach
float lightRadius = 0;
//control the brightness of the light
float lightIntensity = 1.0f;

//Density of our light volume if we render it that way
float lightVolumeDensity = 1;

float ShadowMapSize = 512;
float ShadowMapRadius = 3;
float DepthBias = 0.02;

//Needed for manual texture sampling
float2 Resolution = float2(1280, 800);

//Multiply output with this to get results that can be evaluated alone
const float OUTPUTCONST = 0.1f;

// diffuse color, and specularIntensity in the alpha channel
Texture2D AlbedoMap;
// normals, and specularPower in the alpha channel
Texture2D NormalMap;
//depth
Texture2D DepthMap;

Texture2D NoiseMap;

SamplerState PointSampler
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

Texture2D ShadowMap;

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
    float4 Position : POSITION0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
	float4 PositionVS : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
	float4 Volume : COLOR2;
};

struct PixelShaderInput
{
    float3 PositionVS : POSITION0;
    float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	//processing geometry coordinates
	output.PositionVS = mul(input.Position, WorldView);
	output.Position = mul(input.Position, WorldViewProj);
	output.ScreenPosition = output.Position;
	return output;
}

//For stencil mask only
float4 VertexShaderBasic(VertexShaderInput input) : POSITION0
{
	return mul(input.Position, WorldViewProj);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER DECLARATIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

float CalcShadowTermPCF(float linearDepthLV, float ndotl, float3 vec3);
void LightingCalculation(in int3 texCoordInt, in float distanceToLight, in float3 lightVector, in float3 cameraDirection, inout float3 diffuseOutput, inout float3 specularOutput);
void LightingShadowedCalculation(in int3 texCoordInt, in float distanceToLight, in float3 lightVector, in float3 cameraDirection, inout float3 diffuseOutput, inout float3 specularOutput);
float4 LoadShadowMap(float3 vec3);
float GetVariableBias(float nDotL);
float ShadowCheck(float distance, float3 texCoord);
float integrateVolume(float d2, float d1, float radius);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

	//Check helper.fx for more helper functions

	void LightingCalculation(in int3 texCoordInt, in float distanceToLight, in float3 lightVector, in float3 cameraDirection, inout float3 diffuseOutput, inout float3 specularOutput)
	{
		//get normal data from the NormalMap
		float4 normalData = NormalMap.Load(texCoordInt);
		//tranform normal back into [-1,1] range
		float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
												//get metalness
		float roughness = normalData.a;
		//get specular intensity from the AlbedoMap
		float4 color = AlbedoMap.Load(texCoordInt);

		float metalness = decodeMetalness(normalData.b);

		float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

		//compute attenuation based on distance - linear attenuation
		//float attenuation = saturate(1.0f - distanceLtoR / lightRadius);
		float relativeDistance = distanceToLight / lightRadius; //normalized
		float denominator = (4 * relativeDistance + 1);

		float attenuation = saturate(1 / (denominator*denominator) - 0.04*relativeDistance);

		//normalize light vector
		lightVector /= distanceToLight;

		float NdL = saturate(dot(normal, lightVector));
		float3 diffuseLight = 0;
		[branch]
		if (metalness < 0.99)
		{
			diffuseLight = DiffuseOrenNayar(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
		}
		float3 specular = SpecularCookTorrance(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
		diffuseOutput = (attenuation * diffuseLight * (1 - f0)) * OUTPUTCONST;
		specularOutput = specular * attenuation * OUTPUTCONST;
	}

	void LightingShadowedCalculation(in int3 texCoordInt, in float distanceToLight, in float3 lightVector, in float3 cameraDirection, inout float3 diffuseOutput, inout float3 specularOutput)
	{
		float4 normalData = NormalMap.Load(texCoordInt);
		//tranform normal back into [-1,1] range
		float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
												//get metalness
		float roughness = normalData.a;
		//get specular intensity from the AlbedoMap
		float4 color = AlbedoMap.Load(texCoordInt);

		float metalness = decodeMetalness(normalData.b);

		float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

		//compute attenuation based on distance - linear attenuation
		//float attenuation = saturate(1.0f - distanceLtoR / lightRadius);
		float relativeDistance = distanceToLight / lightRadius; //normalized
		float denominator = (4 * relativeDistance + 1);

		float attenuation = saturate(1 / (denominator*denominator) - 0.04*relativeDistance);

		//normalize light vector
		lightVector /= distanceToLight;
		//compute diffuse light
		float NdL = saturate(dot(normal, lightVector));
		float3 lightVectorWS = -mul(float4(lightVector, 0), InverseView).xyz;

		float shadowFactor = CalcShadowTermPCF(relativeDistance, NdL, lightVectorWS);

		float3 diffuseLight = float3(0, 0, 0);
		float3 specular = float3(0, 0, 0);

		[branch]
		if (shadowFactor > 0.01f && distanceToLight < lightRadius)
		{
			[branch]
			if (metalness < 0.99)
			{
				diffuseLight = DiffuseOrenNayar(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
			}
			specular = SpecularCookTorrance(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
		}
		diffuseOutput = (attenuation * diffuseLight * (1 - f0)) *shadowFactor * OUTPUTCONST;
		specularOutput = specular * attenuation * shadowFactor* OUTPUTCONST;
	}

	void LightingSDFCalculation(in float3 posVS, in int3 texCoordInt, in float distanceToLight, in float3 lightVector, in float3 cameraDirection, inout float3 diffuseOutput, inout float3 specularOutput)
	{
		float4 normalData = NormalMap.Load(texCoordInt);
		//tranform normal back into [-1,1] range
		float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
												//get metalness
		float roughness = normalData.a;
		//get specular intensity from the AlbedoMap
		float4 color = AlbedoMap.Load(texCoordInt);

		float metalness = decodeMetalness(normalData.b);

		float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

		//compute attenuation based on distance - linear attenuation
		//float attenuation = saturate(1.0f - distanceLtoR / lightRadius);
		float relativeDistance = distanceToLight / lightRadius; //normalized
		float denominator = (4 * relativeDistance + 1);

		float attenuation = saturate(1 / (denominator*denominator) - 0.04*relativeDistance);

		//normalize light vector
		lightVector /= distanceToLight;
		//compute diffuse light
		float NdL = saturate(dot(normal, lightVector));


		float3 positionWS = mul(float4(posVS, 1), InverseView).xyz;
		float3 lightWS = mul(float4(lightPosition, 1), InverseView).xyz;

		float shadowFactor = 0;//should be
		float4 sss = 0;

		bool hasSubsurfaceScattering = abs(decodeMattype(normalData.b) - 4.0) < 0.01f;

		[branch]
		if (NdL <= 0.25f && hasSubsurfaceScattering)
		{
			float3 exitpoint = 0;
			sss.rgb = Subsurface(positionWS, lightWS, distance(positionWS, lightWS), exitpoint);
			sss.rgb = saturate(1 - sss.rgb / ShadowMapRadius) * 0.05f;

			sss.a = max(max(sss.r, sss.g), sss.b);

			positionWS = exitpoint;
		}

		[branch]
		if (NdL > 0 || sss.a > 0)
		{
			/*float3 start = sss.a > 0 ? lightWS : positionWS;
			float3 end = sss.a > 0 ? positionWS : lightWS;*/

			float3 start = positionWS;
			float3 end = lightWS;
			
			shadowFactor = RaymarchSoft(start, end, distance(positionWS, lightWS), sss.a > 0 ? (1-sss.a)*8.0f :32.0f);

		}
		

		float3 diffuseLight = float3(0, 0, 0);
		float3 specular = float3(0, 0, 0);

		[branch]
		if (shadowFactor > 0.01f && distanceToLight < lightRadius)
		{
			[branch]
			if (metalness < 0.99)
			{
				diffuseLight = DiffuseOrenNayar(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
			}
			specular = SpecularCookTorrance(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
		}
		diffuseOutput = (attenuation * diffuseLight * (1 - f0)) *shadowFactor * OUTPUTCONST + color.rgb * shadowFactor * sss.rgb * attenuation * lightIntensity * lightColor * OUTPUTCONST;
		specularOutput = specular * attenuation * shadowFactor* OUTPUTCONST;
	}

	//Loads a texel based off of input vector
	float4 LoadShadowMap(float3 vec3)
	{
		return ShadowMap.Load(int3(GetSampleCoordinate(vec3) * float2(ShadowMapSize, 6 * ShadowMapSize), 0)).r;
	}

	//Loads a texel based on float2 texCoord. The texCoord can be obtained by calling GetSampleCoordinate(vec3).
	//One can additionally add an offset, like this GetSampleOffset(sampleCoord, float2(x, y)*texelSize)
	float4 LoadShadowMapUV(float2 coord)
	{
		return ShadowMap.Load(int3(coord * float2(ShadowMapSize, 6 * ShadowMapSize), 0)).r;
	}

//For Virtual Shadow Maps
//float chebyshevUpperBound(float distance, float3 texCoord)
//{
//	texCoord.z = -texCoord.z;
//	// We retrive the two moments previously stored (depth and depth*depth)
//	float2 moments = 1 - shadowCubeMap.SampleLevel(shadowCubeMapSampler, texCoord,0).rg;
//
//	// Surface is fully lit. as the current fragment is before the light occluder
//	// The fragment is either in shadow or penumbra. We now use chebyshev's upperBound to check
//	// How likely this pixel is to be lit (p_max)
//	float variance = moments.y - (moments.x * moments.x);
//	variance = max(variance, 0.000002);
//
//	float d = distance - moments.x;
//	float p_max = variance / (variance + d * d);
//
//	return p_max;
//}

float GetVariableBias(float nDotL)
{
	//return clamp(0.001 * tan(acos(nDotL)), 0, DepthBias);
	//tan = sqrt(1-cos(x)^2)/cos(x)
	return clamp(0.005 * sqrt(1 - nDotL * nDotL) / nDotL, 0, DepthBias);
}

//Shadow filtering with variable kernel, depending on ShadowMapRadius
float CalcShadowTermPCF(float linearDepthLV, float ndotl, float3 vec3)
{
	float fShadowTerm = 0.0;

	float Size = ShadowMapSize;

	//Could be offloaded to a static function
	float texelSize = 1.0f / Size;

	//Variable bias, depending on tan
	float variableBias = GetVariableBias(ndotl);
	
	//Apply bias
	float testDepth = linearDepthLV - variableBias;

	//Get float2 texture coordinate for our shadow map
	float2 sampleCoord = GetSampleCoordinate(vec3);

	//Calculate fractionals for edge tap smoothing
	float2 fractionals = frac(float2(Size * sampleCoord.x, Size * sampleCoord.y * 6));
	float2 complFractionals = float2(1, 1) - fractionals;

	//get the depth of our center sample
	float centerDepth = 1 - LoadShadowMapUV(sampleCoord).r;

	float fRadius = ShadowMapRadius - 1;

	//No PCF -> we are done
	if(fRadius < 1) 
		return testDepth < centerDepth;

	float closestDepth;

	//[unroll] is better, but can't be used when dynamically changing size
	[loop]
	for (float y = -fRadius; y <= fRadius; y++)
	{
		[loop]
		for (float x = -fRadius; x <= fRadius; x++)
		{
			if (x != 0 || y != 0)
			{
				closestDepth = 1 - LoadShadowMapUV(GetSampleOffset(sampleCoord, float2(x, y)*texelSize)).r;
			}
			else
			{
				closestDepth = centerDepth;
			}

			float fSample = testDepth < closestDepth;

			// Edge tap smoothing
			float xWeight = 1;
			float yWeight = 1;

			//Our edge texels get smoothed, they are weighted by area covered
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

	//Edge texels are worth half by default, because the one side is weighted as frac, while the other is 1-frac, so a+b = 1;
	/*
		Example: 
		[ , ] [ / ] [ , ]
		[ / ] [ X ] [ / ]
		[ , ] [ / ] [ , ]

		-> Radius = 1. Center sample is considered 100%, other 4 samples are 0.5. 4 edges are 0.25
		-> 1 + 2 + 1 = 4
		Radius * Radius * 4 = 4;

		Example2:
		-> Radius = 2 -> 9 full samples, 12 half samples, 4 edges are 0.25 
		9 + 6 + 1 = 16
		Radius * Radius * 4 = 16
	*/
	
	fShadowTerm /= (fRadius * fRadius * 4);

	return fShadowTerm;
}

//Plain Shadow Depth difference, no bias
float ShadowCheck(float distance, float3 texCoord)
{
	float2 sampleCoord = GetSampleCoordinate(texCoord);

	float centerDepth = 1 - LoadShadowMapUV(sampleCoord).r;

	return distance < centerDepth;
}

//Integrate our path through some volume. Depends only on distance from light.
float integrateVolume(float d2, float d1, float radius)
{
	// 1 - x/r -> x - x^2 / r*2
	return max(d2 - d2*d2 / (radius * 2) - d1 + d1*d1 / (radius * 2), 0);

	// 1 - x^2 / r^2 -> x - x^3 / r^2*3
	//return max(d2 - d2*d2*d2 / (radius*radius * 3) - d1 + d1*d1*d1 / (radius*radius * 3), 0);
}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  SHADING AND VOLUME TRAVERSAL
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Used for both shadow casting and unshadowed, shared function
PixelShaderInput BaseCalculations(VertexShaderOutput input)
{
	PixelShaderInput output;
     //obtain screen position
    input.ScreenPosition.xyz /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);

    /////////////////////////////////////////

    //read depth, use point sample or load
	output.Depth = DepthMap.Load(int3(input.Position.xy, 0));//DepthMap.SampleLevel(PointSampler, texCoord, 0).r;

	//Basically extend the depth of this ray to the end of the far plane, this gives us the position of the sphere only
	float3 cameraDirVS = input.PositionVS.xyz * (FarClip / -input.PositionVS.z);

    //compute ViewSpace position
	float3 positionVS = output.Depth * cameraDirVS;

    //////////////////////////////////////
    output.PositionVS = positionVS;
    output.TexCoord = texCoord;

    return output;
}

//Unshadowed light without volumetric fog around
PixelShaderOutput BasePixelShaderFunction(PixelShaderInput input)
{
    PixelShaderOutput output;

	output.Diffuse = float4(0, 0, 0, 0);
	output.Specular = float4(0, 0, 0, 0);
	output.Volume = float4(0, 0, 0, 0);

    //surface-to-light vector, in VS
    float3 lightVector = lightPosition - input.PositionVS.xyz;
    float lengthLight = length(lightVector);

	//If the pixel is outside of our light, clip!
    [branch]
    if (lengthLight > lightRadius)
    {
        clip(-1);
        return output;
    }
    else
    {
		int3 texCoordInt = int3(input.TexCoord * Resolution, 0);
		float3 cameraDirection = -normalize(input.PositionVS.xyz);
		LightingCalculation(texCoordInt, lengthLight, lightVector, cameraDirection, output.Diffuse.rgb, output.Specular.rgb);
        return output;
    }
}


//Unshadowed light without volumetric fog around
PixelShaderOutput BasePixelShaderFunctionSDF(PixelShaderInput input)
{
    PixelShaderOutput output;

    output.Diffuse = float4(0, 0, 0, 0);
    output.Specular = float4(0, 0, 0, 0);
    output.Volume = float4(0, 0, 0, 0);

    //surface-to-light vector, in VS
    float3 lightVector = lightPosition - input.PositionVS.xyz;
    float lengthLight = length(lightVector);

	//If the pixel is outside of our light, clip!
    [branch]
    if (lengthLight > lightRadius)
    {
        clip(-1);
        return output;
    }
    else
    {
        int3 texCoordInt = int3(input.TexCoord * Resolution, 0);
        float3 cameraDirection = -normalize(input.PositionVS.xyz);
		LightingSDFCalculation(input.PositionVS, texCoordInt, lengthLight, lightVector, cameraDirection, output.Diffuse.rgb, output.Specular.rgb);
        return output;
    }
}

//Stencil masking
float4 PixelShaderBasic(float4 position : POSITION) : COLOR
{
	return float4(0,0,0,0);
}

//Base function
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderInput p_input = BaseCalculations(input);

	PixelShaderOutput Output;

	float lightDepth = input.PositionVS.z / -FarClip;

	[branch]
	if (lightDepth * inside < p_input.Depth * inside)
	{
		clip(-1);
		return Output;
	}
	else
	{
		return BasePixelShaderFunction(p_input);
	}
}


//Base function
PixelShaderOutput PixelShaderFunctionSDF(VertexShaderOutput input)
{
    PixelShaderInput p_input = BaseCalculations(input);

    PixelShaderOutput Output;

    float lightDepth = input.PositionVS.z / -FarClip;

	[branch]
    if (lightDepth * inside < p_input.Depth * inside)
    {
        clip(-1);
        return Output;
    }
    else
    {
        return BasePixelShaderFunctionSDF(p_input);
    }
}

//Unshadowed light with fog around it
PixelShaderOutput VolumetricPixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	input.ScreenPosition.xyz /= input.ScreenPosition.w;
	float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
	int3 texCoordInt = int3(texCoord * Resolution, 0);

	//read linear depth
	float linDepth = DepthMap.Load(texCoordInt).r;

	//Basically extend the depth of this ray to the end of the far plane, this gives us the position of the sphere only
	//todo: needed?
	float3 cameraDirVS = input.PositionVS.xyz * (FarClip / -input.PositionVS.z);

	//compute ViewSpace position
	float3 positionFromDepthVS = linDepth * cameraDirVS;

	//The way our camera is looking
	float3 cameraDirection = normalize(input.PositionVS.xyz);

	//If inside = 0 we look from the outside, so we enter into the field
	float3 toLightCenter = normalize(lightPosition - input.PositionVS.xyz);

	//Is just the light position since we work from ViewSpace
	float3 cameraToLight = lightPosition;
	float distanceLtoC = length(cameraToLight);

	//normalize
	cameraToLight /= distanceLtoC;

	float alpha = dot(cameraDirection, cameraToLight);

	//P is the vertex position (interpolated)
	//B is the halfway in the sphere
	//R is the reconstructed position from the depthmap

	float distanceCtoP = distance(cameraPosition, input.PositionVS.xyz);
	float distanceCtoB = alpha * distanceLtoC;
	float distanceCtoR = distance(cameraPosition, positionFromDepthVS);

	//Position of B
	float3 b_vector = distanceCtoB * cameraDirection + cameraPosition;

	float totalVolumePassed = 0;

	float distanceLtoP = distance(lightPosition, input.PositionVS.xyz);
	float distanceLtoB = distance(lightPosition, b_vector);
	float distanceLtoR = distance(lightPosition, positionFromDepthVS);

	[branch]
	if (inside <= 0)
	{
		//Not inside
		[branch]
		if (distanceCtoR > distanceCtoP + (distanceCtoB - distanceCtoP) * 2)
		{
			totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoB, lightRadius) - integrateVolume(distanceLtoP, lightRadius, lightRadius);
		}

		else
		{
			if (distanceCtoR > distanceCtoB)
			{
				totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoB, lightRadius) + integrateVolume(distanceLtoB, distanceLtoR, lightRadius);
			}
			else if (distanceCtoR > distanceCtoP)
			{
				totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoR, lightRadius);
			}
		}
	}
	else
	{
		//object behind our volume
		if (distanceCtoR > distanceCtoP)
		{
			//is camera behind mid of volume?
			if (alpha < 0)
			{
				totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoC, lightRadius) / 2;
			}
			else
			{
				totalVolumePassed = 0.5f *(integrateVolume(distanceLtoP, distanceLtoB, lightRadius) + integrateVolume(distanceLtoC, distanceLtoB, lightRadius));
			}
		}
		else
		{
			if (alpha < 0)
			{
				totalVolumePassed = integrateVolume(distanceLtoR, distanceLtoC, lightRadius) / 2;
			}
			else
			{
				if (distanceCtoR < distanceCtoB)
				{
					totalVolumePassed = 0.5f*integrateVolume(distanceLtoC, distanceLtoR, lightRadius);
				}
				else
				{
					totalVolumePassed = 0.5f*(integrateVolume(distanceLtoR, distanceLtoB, lightRadius) + integrateVolume(distanceLtoC, distanceLtoB, lightRadius));
				}
			}
		}
	}

	float3 lightVector = lightPosition - positionFromDepthVS;

	output.Diffuse = 0;
	output.Specular = 0;
	output.Volume = float4((totalVolumePassed * 0.00004 *lightIntensity * lightVolumeDensity) * lightColor, 0);

	[branch]
	if (distanceLtoR < lightRadius)
	{
		LightingCalculation(texCoordInt, distanceLtoR, lightVector, -cameraDirection, output.Diffuse.rgb, output.Specular.rgb);
	}
	return output;
}

//Shadow mapped light!
PixelShaderOutput BasePixelShaderFunctionShadow(PixelShaderInput input)
{
    PixelShaderOutput output;
    
    //surface-to-light vector
    float3 lightVector = lightPosition - input.PositionVS;

    float lengthLight = length(lightVector);

    [branch]
    if (lengthLight > lightRadius)
    {
        clip(-1);
        return output;
    }
	else
	{
		int3 texCoordInt = int3(input.TexCoord * Resolution, 0);
		float3 cameraDirection = -normalize(input.PositionVS);

		LightingShadowedCalculation(texCoordInt, lengthLight, lightVector, cameraDirection, output.Diffuse.rgb, output.Specular.rgb);

		return output;
	}
}


PixelShaderOutput PixelShaderFunctionShadowed(VertexShaderOutput input)
{
	PixelShaderInput p_input = BaseCalculations(input);

	PixelShaderOutput Output;

	/*float lightDepth = input.PositionVS.z / -FarClip;

	if (lightDepth * inside < p_input.Depth * inside)
	{
		clip(-1);
		return Output;
	}
	else
	{*/
		Output = BasePixelShaderFunctionShadow(p_input);

		return Output;
	//}
}

//

//ShadowMapped light with Fog
PixelShaderOutput VolumetricPixelShaderFunctionShadowed(VertexShaderOutput input)
{
	PixelShaderOutput output;

	output.Diffuse = 0;
	output.Specular = 0;

	input.ScreenPosition.xyz /= input.ScreenPosition.w;
	float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);

	int3 texCoordInt = int3(texCoord * Resolution, 0);

	//read linear depth
	float linDepth = DepthMap.Load(texCoordInt).r;

	//Basically extend the depth of this ray to the end of the far plane, this gives us the position of the sphere only
	float3 cameraDirVS = input.PositionVS.xyz * (FarClip / -input.PositionVS.z);

	//compute ViewSpace position
	float3 positionFromDepthVS = linDepth * cameraDirVS;

	//The way our camera is looking
	float3 cameraDirection = normalize(input.PositionVS.xyz);

	//If inside = 0 we look from the outside, so we enter into the field
	float3 toLightCenter = normalize(lightPosition - input.PositionVS.xyz);

	float3 cameraToLight = lightPosition;
	float distanceLtoC = length(cameraToLight);

	cameraToLight /= distanceLtoC;

	float distanceCtoR = distance(cameraPosition, positionFromDepthVS);

	float totalVolumePassed = 0;

	float distanceLtoR = distance(lightPosition, positionFromDepthVS);

	float3 lightVector = lightPosition - positionFromDepthVS;

	//Raymarch

	float3 start_vector;
	float3 end_vector;

	if (inside<1)
	{
		start_vector =  cameraDirection*(distanceLtoC - lightRadius);
		end_vector =  cameraDirection*(distanceLtoC + lightRadius);
	}
	else
	{
		start_vector = 0;// float3(0, 0, 0);
		end_vector = input.PositionVS.xyz;
	}

	float visibility = 0;

	float previousLightDistance = lightRadius * 2;

	if (distanceCtoR > distanceLtoC - lightRadius)
	{

		int steps = 10;

		float3 ray = end_vector - start_vector;

		float noiseX =  (frac(sin(dot(texCoord* (frac(Time)+1), float2(15.8989f, 76.132f) )) * 46336.23745f));

		//float noiseX = NoiseMap.Sample(PointSampler, frac(((texCoord)* Resolution) / 64)).r; // + frac(input.TexCoord* Projection)).r;

		[loop]
		for (int i = 0; i < steps; i++)
		{
			float4 rayPosition = float4(float3(start_vector + ((i + noiseX) / (steps))*ray), 1);

			float3 lightVectorRay = lightPosition - rayPosition.xyz;
			float lightDistance = length(lightVectorRay);

			//normalize
			lightVectorRay /= lightDistance;

			float3 lightVectorWS = -mul(float4(lightVectorRay, 0), InverseView).xyz;

			//float depthInLS = getDepthInLS(rayPosition, lightVectorWS);

			if (length(rayPosition.xyz) > distanceCtoR || previousLightDistance < lightDistance) break;
			else
			{
				previousLightDistance = lightDistance;
				float dist = saturate( lightDistance / lightRadius * 1.1f) - 1;
				dist = dist*dist;
				visibility += ShadowCheck(lightDistance / lightRadius, lightVectorWS) * saturate(dist*dist);//1 - dist*dist / (lightRadius*lightRadius));
			}
		}
		visibility /= steps;
	}
	//startvector > input.WorldPositionVS
	//EndVector

	output.Volume = float4((0.02 * lightIntensity * lightVolumeDensity * visibility) * lightColor, 0);

	LightingShadowedCalculation(texCoordInt, distanceLtoR, lightVector, -cameraDirection, output.Diffuse.rgb, output.Specular.rgb);

	return output;

}
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique WriteStencilMask
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderBasic();
		PixelShader = compile ps_4_0 PixelShaderBasic();
	}
}

technique Unshadowed
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique ShadowedSDF
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunctionSDF();
    }
}

technique UnshadowedVolume
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 VolumetricPixelShaderFunction();
	}
}

technique Shadowed
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunctionShadowed();
	}
}

technique ShadowedVolume
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 VolumetricPixelShaderFunctionShadowed();
    }
}
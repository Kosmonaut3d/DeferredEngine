
//Screen Space Reflection shader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"

float4x4 Projection;
float4x4 InverseProjection;

float FarClip;

const int Samples = 3;
const int SecondarySamples = 3;

const float MinimumThickness = 70;

const float border = 0.1f;
float2 resolution = float2(1280, 800);

Texture2D DepthMap;
Texture2D TargetMap;
Texture2D NormalMap;
Texture2D NoiseMap;

SamplerState texSampler
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = POINT;
};
 
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
    float2 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 ViewRay : TEXCOORD1;
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

	output.ViewRay = GetFrustumRay(id);
	return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

//http://martindevans.me/game-development/2015/02/22/Random-Gibberish/
float Random_Final(float2 uv, float seed)
{
	float fixedSeed = abs(seed) + 1.0;
	float x = dot(uv, float2(12.9898, 78.233) * fixedSeed);
	return frac(sin(x) * 43758.5453);
}

float3 randomNormal(float2 tex)
{
    tex = frac(tex * Time);
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f)) * 2 - 1;
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f)) * 2 - 1;
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f)) * 2 - 1;
    return normalize(float3(noiseX, noiseY, noiseZ));
}

//float3 randomNormal(float2 tex)
//{
//	float noiseX = Random_Final(tex, Time) * 2 - 1;
//	float noiseY = Random_Final(tex, Time*2) * 2 - 1;
//	float noiseZ = Random_Final(tex, Time*3) * 2 - 1;
//	return normalize(float3(noiseX, noiseY, noiseZ));
//}

float3 GetFrustumRay2(float2 texCoord)
{
	float3 x1 = lerp(FrustumCorners[0], FrustumCorners[1], texCoord.x);
	float3 x2 = lerp(FrustumCorners[2], FrustumCorners[3], texCoord.x);
	float3 outV = lerp(x1, x2, texCoord.y);
	return outV;
}

float TransformDepth(float depth, matrix trafoMatrix)
{
	return (depth*trafoMatrix._33 + trafoMatrix._43) / (depth * trafoMatrix._34 + trafoMatrix._44);
}


		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  Main Raymarching algorithm
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Basic
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	const float border2 = 1 - border;
	const float bordermulti = 1 / border;
	float samples = abs(Samples);

	float4 output = 0;

	//Just a bit shorter
	float2 texCoord = float2(input.TexCoord);

	//Get our current Position in viewspace
	float linearDepth = DepthMap.Sample(texSampler, texCoord).r;
	float3 positionVS = input.ViewRay * linearDepth; //GetFrustumRay2(texCoord) * linearDepth;

	//Sample the normal map
	float4 normalData = NormalMap.Sample(texSampler, texCoord);
	//tranform normal back into [-1,1] range
	float3 normal = decode(normalData.xyz);
	float roughness = normalData.a;

	//Exit if material is not reflective
	[branch]
	if (normalData.x + normalData.y <= 0.001f || roughness > 0.8f) //Out of range
	{
		return float4(0, 0, 0, 0);
	}

	//Where does our ray start, where does it go?
	float3 incident = normalize(positionVS);
	float3 reflectVector = reflect(incident, normal);

	//Transform to WVP to get the uv's for our ray
	float4 reflectVectorVPS = mul(float4(positionVS + reflectVector, 1), Projection);
	reflectVectorVPS.xyz /= reflectVectorVPS.w;

	//transform to UV coordinates
	float2 reflectVectorUV = 0.5f * (float2(reflectVectorVPS.x, -reflectVectorVPS.y) + float2(1, 1));

	// raymarch, transform depth to z/w depth so we march equal distances on screen (no perspective distortion)
	float3 rayOrigin = float3(texCoord, TransformDepth(positionVS.z, Projection));
	float3 rayStep = float3(reflectVectorUV - texCoord, reflectVectorVPS.z - rayOrigin.z);

	//extend the ray so it crosses the whole screen once
	float xMultiplier = (rayStep.x > 0 ? (1 - texCoord.x) : -texCoord.x) / rayStep.x;
	float yMultiplier = (rayStep.y > 0 ? (1 - texCoord.y) : -texCoord.y) / rayStep.y;
	float multiplier = min(xMultiplier, yMultiplier) / samples;
	rayStep *= multiplier;

	/*//uniform raystep distance
	if (Samples < 0)
	{
		float length2 = length(rayStep.xy);
		rayStep /= length2 * samples;
	}*/

	//Some variables we need later when precising the hit point
	float startingDepth = rayOrigin.z;
	float3 hitPosition;
	float2 hitTexCoord = 0;

	float temporalComponent = 0;

	[branch]
	if (Time > 0)
	{
		temporalComponent = Random_Final(texCoord, Time+1); // frac(sin(Time * 3.2157) * 46336.23745f);
	}

	//Add some noise
	float noise = NoiseMap.Sample(texSampler, frac(((texCoord + temporalComponent.xx)* resolution ) / 64)).r; // + frac(input.TexCoord* Projection)).r;
	
	//Raymarching the depth buffer
	[loop]
	for (int i = 1; i <= samples; i++)
	{
		//March a step
		float3 rayPosition = rayOrigin + (i - 0.5f + noise)*rayStep;

		//We don't consider rays coming out of the screeen
		if (rayPosition.z < 0 || rayPosition.z>1) break;

		//Get the depth at our new position
		int3 texCoordInt = int3(rayPosition.xy * resolution, 0);

		float linearDepth = DepthMap.Load(texCoordInt).r * -FarClip;
		float sampleDepth = TransformDepth(linearDepth, Projection);

		float depthDifference = sampleDepth - rayPosition.z;

		//needs normal looking to it!

		//Coming towards us, let's go back to linear depth!
		[branch]
		if (rayStep.z < 0 && depthDifference < 0)
		{
			//Where are we currently in linDepth, note - in VS is + in VPS
			float depthMinusThickness = TransformDepth(linearDepth + MinimumThickness, Projection);
			
			if (depthMinusThickness < rayPosition.z)
				continue;
		}

		//March backwards, idea -> binary searcH?
		[branch]
		if (depthDifference <= 0 && sampleDepth >= startingDepth-rayStep.z*0.5f) //sample < rayPosition.z
		{
			hitPosition = rayPosition;

			//Less samples when already going far ... is this good?
			int samples2 = SecondarySamples;//samples + 1 - i;

			bool hit = false;

			float sampleDepthFirstHit = sampleDepth;
			float3 rayPositionFirstHit = rayPosition;

			//March backwards until we are outside again
			[loop]
			for (int j = 1; j <= samples2; j++)
			{
				rayPosition = hitPosition - rayStep * j / (samples2);

				texCoordInt = int3(rayPosition.xy * resolution, 0);

				sampleDepth = TransformDepth(DepthMap.Load(texCoordInt).r * -FarClip - 50.0f, Projection);

				//Looks like we don't hit anything any more?
				[branch]
				if (sampleDepth >= rayPosition.z)
				{
					//only z is relevant

					float origin = rayPositionFirstHit.z;
					
					//should be smaller
					float r = rayPosition.z - origin;

					//y = r * x + c, c = 0
					//y = (b-a)*x + a
					float a = sampleDepthFirstHit - origin;

					float b = sampleDepth - origin;

					float x = (a) / (r - b + a);

					float sampleDepthLerped = lerp(sampleDepth, sampleDepthFirstHit, x);

					hit = true;

					hitTexCoord = lerp(rayPosition.xy, rayPositionFirstHit.xy, x);

					hitTexCoord = rayPosition.xy;

					////In front
					//if (sampleDepthFirstHit <= rayPositionFirstHit.z - rayStep.z*j/samples2)
					//{
					//	hit = false;
					//}

					break;
				}

				sampleDepthFirstHit = sampleDepth;
				rayPositionFirstHit = rayPosition;
			}

			//We haven't hit anything we can travel further
			if (!hit)
				continue;

			int3 hitCoordInt = int3(hitTexCoord.xy * resolution, 0);

			float4 albedoColor = TargetMap.Load(hitCoordInt);
			output.rgb = albedoColor.rgb;
			output.a = 1;

			//Fade out to the edges
			[branch]
			if (rayPosition.y > border2)
			{
				output.a = lerp(1, 0, (hitTexCoord.y - border2) * bordermulti);
			}
			else if (rayPosition.y < border)
			{
				output.a = lerp(0, 1, hitTexCoord.y * bordermulti);
			}
			[branch]
			if (rayPosition.x > border2)
			{
				output.a *= lerp(1, 0, (hitTexCoord.x - border2) * bordermulti);
			}
			else if (rayPosition.x < border)
			{
				output.a *= lerp(0, 1, hitTexCoord.x * bordermulti);
			}

			//Fade out to the front
			
			float fade = saturate(1 - reflectVector.z);
			output.a *= (1 - roughness) * fade;
			//output.rgb *= output.a;

			break;
		}
		startingDepth = rayPosition.z;
	}

	return output;
}

//float roughnessToConeAngle(float roughness)
//{
//    float specularPower = pow(2, 10 * (1-roughness) + 1);
//    // based on phong reflection model
//    const float xi = 0.244f;
//    float exponent = 1.0f / (specularPower + 1.0f);
//	/*
//	 * may need to try clamping very high exponents to 0.0f, test out on mirror surfaces first to gauge
//	 * return specularPower >= 8192 ? 0.0f : cos(pow(xi, exponent));
//	 */
//    return pow(xi, exponent); //cos(pow(xi, exponent));
//}

//Temporal Jitter based on roughness

float4 PixelShaderFunctionTAA(VertexShaderOutput input) : COLOR0
{
	const float border2 = 1 - border;
	const float bordermulti = 1 / border;
	int samples = Samples;

	float4 output = 0;

	//Just a bit shorter
	float2 texCoord = float2(input.TexCoord);

	//Get our current Position in viewspace
	float linearDepth = DepthMap.Sample(texSampler, texCoord).r;
	float3 positionVS = input.ViewRay * linearDepth; //GetFrustumRay2(texCoord) * linearDepth;

													 //Sample the normal map
	float4 normalData = NormalMap.Sample(texSampler, texCoord);
	//tranform normal back into [-1,1] range
	float3 normal = decode(normalData.xyz);
	float roughness = normalData.a;

	//Exit if material is not reflective
	[branch]
	if (normalData.x + normalData.y <= 0.001f || roughness > 0.8f) //Out of range
	{
		return float4(0, 0, 0, 0);
	}

	//Where does our ray start, where does it go?
	float3 incident = normalize(positionVS);

	float temporalComponent = 0;

	float3 randNor;

	[branch]
	if (Time > 0)
	{
		temporalComponent = (Time+10) * 10 * normal.x / positionVS.y / normal.z; // frac(sin(Time * 3.2157) * 46336.23745f);
	}

		//Add some noise
	//float noise = NoiseMap.Sample(texSampler, frac(((texCoord)* resolution + temporalComponent) / 64)).r; // + frac(input.TexCoord* Projection)).r;

	randNor = randomNormal(input.TexCoord); // randomNormal(frac(mul(input.TexCoord, noise).xy)) * -randomNormal(frac(mul(1 - input.TexCoord, noise).xy)); //
	
		//hemisphere
	if (dot(randNor, normal) < 0)
			randNor *= -1;

	//Jitter the normal based on roughness to simulate microfacets. This should be updated to correctly map to lobes with some BRDF.
	normal = normalize(lerp(normal, randNor, roughness));

	float3 reflectVector = reflect(incident, normal);

	//Transform to WVP to get the uv's for our ray
	float4 reflectVectorVPS = mul(float4(positionVS + reflectVector, 1), Projection);
	reflectVectorVPS.xyz /= reflectVectorVPS.w;

	//transform to UV coordinates
	float2 reflectVectorUV = 0.5f * (float2(reflectVectorVPS.x, -reflectVectorVPS.y) + float2(1, 1));

	// raymarch, transform depth to z/w depth so we march equal distances on screen (no perspective distortion)
	float3 rayOrigin = float3(texCoord, TransformDepth(positionVS.z, Projection));
	float3 rayStep = float3(reflectVectorUV - texCoord, reflectVectorVPS.z - rayOrigin.z);

	//extend the ray so it crosses the whole screen once
	float xMultiplier = (rayStep.x > 0 ? (1 - texCoord.x) : -texCoord.x) / rayStep.x;
	float yMultiplier = (rayStep.y > 0 ? (1 - texCoord.y) : -texCoord.y) / rayStep.y;
	float multiplier = min(xMultiplier, yMultiplier) / samples;
	rayStep *= multiplier;

	//Some variables we need later when precising the hit point
	float startingDepth = rayOrigin.z;
	float3 hitPosition;
	float2 hitTexCoord = 0;

	float offsetTaa = -frac(normal.x * (Time+10)) + 0.5f;

	//Raymarching the depth buffer
	[loop]
	for (int i = 1; i <= samples; i++)
	{
		//March a step
		float3 rayPosition = rayOrigin + (i + offsetTaa)*rayStep;

		//We don't consider rays coming out of the screeen
		if (rayPosition.z < 0 || rayPosition.z>1) break;

		//Get the depth at our new position
		int3 texCoordInt = int3(rayPosition.xy * resolution, 0);

		float linearDepth = DepthMap.Load(texCoordInt).r * -FarClip;
		float sampleDepth = TransformDepth(linearDepth, Projection);

		float depthDifference = sampleDepth - rayPosition.z;

		//needs normal looking to it!

		//Coming towards us, let's go back to linear depth!
		[branch]
		if (rayStep.z < 0 && depthDifference < 0)
		{
			//Where are we currently in linDepth, note - in VS is + in VPS
			float depthMinusThickness = TransformDepth(linearDepth + MinimumThickness, Projection);

			if (depthMinusThickness < rayPosition.z)
				continue;
		}

		//March backwards, idea -> binary searcH?
		[branch]
		if (depthDifference <= 0 && sampleDepth >= startingDepth - rayStep.z*0.5f) //sample < rayPosition.z
		{
			hitPosition = rayPosition;

			//Less samples when already going far ... is this good?
			int samples2 = SecondarySamples;//samples + 1 - i;

			bool hit = false;

			float sampleDepthFirstHit = sampleDepth;
			float3 rayPositionFirstHit = rayPosition;

			//March backwards until we are outside again
			[loop]
			for (int j = 1; j <= samples2; j++)
			{
				rayPosition = hitPosition - rayStep * j / (samples2);

				texCoordInt = int3(rayPosition.xy * resolution, 0);

				sampleDepth = TransformDepth(DepthMap.Load(texCoordInt).r * -FarClip - 50.0f, Projection);

				//Looks like we don't hit anything any more?
				[branch]
				if (sampleDepth >= rayPosition.z)
				{
					//only z is relevant

					float origin = rayPositionFirstHit.z;

					//should be smaller
					float r = rayPosition.z - origin;

					//y = r * x + c, c = 0
					//y = (b-a)*x + a
					float a = sampleDepthFirstHit - origin;

					float b = sampleDepth - origin;

					float x = (a) / (r - b + a);

					float sampleDepthLerped = lerp(sampleDepth, sampleDepthFirstHit, x);

					hit = true;

					hitTexCoord = lerp(rayPosition.xy, rayPositionFirstHit.xy, x);

					hitTexCoord = rayPosition.xy;

					////In front
					//if (sampleDepthFirstHit <= rayPositionFirstHit.z - rayStep.z*j/samples2)
					//{
					//	hit = false;
					//}

					break;
				}

				sampleDepthFirstHit = sampleDepth;
				rayPositionFirstHit = rayPosition;
			}

			//We haven't hit anything we can travel further
			if (!hit)
				continue;

			int3 hitCoordInt = int3(  (hitTexCoord.xy * resolution), 0);

			float4 albedoColor = TargetMap.Load(hitCoordInt);
			output.rgb = albedoColor.rgb;
			output.a = 1;

			//Fade out to the edges
			[branch]
			if (hitTexCoord.y > border2)
			{
				output.a = lerp(1, 0, (hitTexCoord.y - border2) * bordermulti);
			}
			else if (hitTexCoord.y < border)
			{
				output.a = lerp(0, 1, hitTexCoord.y * bordermulti);
			}
			[branch]
			if (hitTexCoord.x > border2)
			{
				output.a *= lerp(1, 0, (hitTexCoord.x - border2) * bordermulti);
			}
			else if (hitTexCoord.x < border)
			{
				output.a *= lerp(0, 1, hitTexCoord.x * bordermulti);
			}

			//Fade out to the front

			float fade = saturate(1 - reflectVector.z);
			output.a *= (1 - roughness) * fade;
			//output.rgb *= output.a;

			break;
		}
		startingDepth = rayPosition.z;
	}

return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////


technique TAA
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunctionTAA();
    }
}

technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}
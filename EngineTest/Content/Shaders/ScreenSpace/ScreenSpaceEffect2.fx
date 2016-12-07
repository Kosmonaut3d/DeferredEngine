
//Screen Space Reflection shader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "helper.fx"

float4x4 Projection;
float4x4 InverseProjection;

float3 FrustumCorners[4]; //In Viewspace!

float FarClip;

float Time = 0;

const int Samples = 3;
const int SecondarySamples = 3;

const float border = 0.2f;
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
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
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

	output.ViewRay = GetFrustumRay(input.TexCoord);
    return output;

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

float3 randomNormal(float2 tex)
{
    tex = frac(tex * Time);
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f));
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f));
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f));
    return normalize(float3(noiseX, noiseY, noiseZ));
}

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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
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
	float multiplier = min(xMultiplier, yMultiplier);
	rayStep *= multiplier;
	rayStep /= samples;

	//Some variables we need later when precising the hit point
	float startingDepth = rayOrigin.z;
	float3 hitPosition;
	float2 hitTexCoord = 0;

	float temporalComponent = 0;

	[branch]
	if (Time > 0)
	{
		temporalComponent = Time * 10000 * rayStep.x / rayOrigin.y / rayStep.z ; // frac(sin(Time * 3.2157) * 46336.23745f);
	}

	//Add some noise
	float noise = NoiseMap.Sample(texSampler, frac(( (texCoord ) * resolution + temporalComponent) / 64)).r; // + frac(input.TexCoord* Projection)).r;

	//Raymarching the depth buffer
	[loop]
	for (int i = 1; i <= samples; i++)
	{
		//We don't consider rays coming out of the screeen
		//if (rayStep.z < 0) break;

		//March a step
		float3 rayPosition = rayOrigin + (i - 0.5f + noise)*rayStep;

		if (rayPosition.z < 0 || rayPosition.z>1) break;

		//Get the depth at our new position
		int3 texCoordInt = int3(rayPosition.xy * resolution, 0);
		float sampleDepth = TransformDepth(DepthMap.Load(texCoordInt).r * -FarClip, Projection);

		float depthDifference = sampleDepth - rayPosition.z;

		//March backwards, idea -> binary searcH?
		[branch]
		if (depthDifference < 0) //sample < rayPosition.z
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

				sampleDepth = TransformDepth(DepthMap.Load(texCoordInt).r * -FarClip, Projection);

				//Looks like we don't hit anything any more?
				[branch]
				if (sampleDepth > rayPosition.z)
				{
					//only z is relevant
					float prevZ = 0; // rayPositionVSPrevious.z;
					float d = rayPositionFirstHit.z - rayPosition.z;

					float /*depthPrev*/ b = sampleDepthFirstHit - rayPosition.z;
					float /*depth*/ a = sampleDepth - rayPosition.z;

					float x = -b / (b - a) * (1 / (-d - 1 / (b - a))) / d;

					float sampleDepthLerped = lerp(sampleDepth, sampleDepthFirstHit, x);

					hit = true;

					hitTexCoord = lerp(rayPosition.xy, rayPositionFirstHit.xy, x);

					if (sampleDepthLerped >= hitPosition.z)
					{
						hit = false;
					}
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
			output = albedoColor;
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

			output.rgb *= output.a * (1 - roughness) * fade;
			break;
		}
		startingDepth = rayPosition.z;
	}
	return output;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////


//technique TAA
//{
//    pass Pass1
//    {
//        VertexShader = compile vs_5_0 VertexShaderFunction();
//        PixelShader = compile ps_5_0 PixelShaderFunctionTAA();
//    }
//}
//
technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
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

//float4 PixelShaderFunctionTAA(VertexShaderOutput input) : COLOR0
//{
//    float4 output = float4(0, 0, 0, 0);
//
//    float4 positionVS;
//    positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
//    positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);
//
//    float2 texCoord = float2(input.TexCoord);
//
//    float depthVal = 1 - DepthMap.Sample(texSampler, texCoord).r;
//
//    float4 normalData = NormalMap.Sample(texSampler, texCoord);
//	//tranform normal back into [-1,1] range
//    float3 normal = decode(normalData.xyz);
//    float roughness = normalData.a;
//
//
//    [branch]
//    if (normalData.x + normalData.y <= 0.001f || roughness > 0.8f) //Out of range
//    {
//        return float4(0, 0, 0, 0);
//    }
//
//	////compute screen-space position
//
//	//linDepth
//	//float linDepth = 1 + (Projection._43 / (depthVal - Projection._33));
//
//	//RealSpace
//    positionVS.w = 1.0f;
//    positionVS.z = depthVal;
//    float4 positionWS = mul(positionVS, InverseViewProjection);
//    positionWS /= positionWS.w;
//
//	// float3 incident = normalize(input.viewDirWS);
//    float3 incident = normalize(positionWS.xyz - CameraPosition);
//
//    float3 randNor = randomNormal(frac(mul(input.TexCoord, ViewProjection).xy)) * -randomNormal(frac(mul(1 - input.TexCoord, ViewProjection).xy)); //
//
//    //hemisphere
//    if (dot(randNor, normal) < 0)
//        randNor *= -1;
//
//    normal = normalize(lerp(normal, randNor, roughness)); //roughnessToConeAngle(roughness)); //normalize(normal + (1-roughnessToConeAngle(roughness)) * randNor);
//
//    float3 reflectVector = reflect(incident, normal);
//	// go
//
//    float4 samplePositionVS = mul(positionWS + float4(reflectVector, 0), ViewProjection);
//    samplePositionVS /= samplePositionVS.w;
//
//    float4 Offset = (samplePositionVS - positionVS);
//
//    float xMultiplier = 0;
//    float yMultiplier = 0;
//            //Lets go to the end of the screen
//    if (Offset.x > 0)
//    {
//        xMultiplier = (1 - positionVS.x) / Offset.x;
//    }
//    else
//    {
//        xMultiplier = (-1 - positionVS.x) / Offset.x;
//    }
//
//    if (Offset.y > 0)
//    {
//        yMultiplier = (1 - positionVS.y) / Offset.y;
//    }
//    else
//    {
//        yMultiplier = (-1 - positionVS.y) / Offset.y;
//    }
//
//    //what multiplier is smaller?
//
//    float multiplier = min(xMultiplier, yMultiplier); //xMultiplier < yMultiplier ? xMultiplier : yMultiplier;
//
//    //int samples = 20;
//
//    //Offset *= multiplier / samples;
//
//    Offset *= multiplier;
//
//    float maxOffset = max(abs(Offset.x), abs(Offset.y));
//           
//    static int samples = 15; //int(maxOffset * 20);
//    
//    static float border = 0.1f;
//
//    static float border2 = 1 - border;
//    static float bordermulti = 1 / border;
//
//    Offset /= samples;
//
//    float startingDepth = samplePositionVS.z;
//
//    float4 hitPosition;
//
//    float offsetTaa = -frac(Time * normal.x * Time) * 1 + 0.5f;
//
//	[branch]
//    for (int i = 0; i < samples; i++)
//    {
//		//if (i >= samples)
//		//	break;
//
//        if (Offset.z < -0.5f)
//            break;
//
//        float4 rayPositionVS = samplePositionVS + Offset * (i+offsetTaa);
//
//        //float2 sampleTexCoord = 0.5f * (float2(samplePositionVS.x, -samplePositionVS.y) + 1);
//
//        float2 sampleTexCoord = 0.5f * (float2(rayPositionVS.x, -rayPositionVS.y) + float2(1, 1));
//
//        float depthValRay = 1 - DepthMap.Sample(texSampler, sampleTexCoord).r;
//          
//        //We have a hit
//        [branch]     
//        if (depthValRay <= rayPositionVS.z) //&& (Offset.z > 0)) //&& depthValRay >= startingDepth)  ) //|| (Offset.z < 0 && depthValRay < startingDepth)))
//        {
//            hitPosition = rayPositionVS;
//
//            bool hit = false;
//            
//            int samples2 = samples + 1 - i; //samples + 1 - i;
//
//            float depthValRayPrevious = depthValRay;
//            float4 rayPositionVSPrevious = rayPositionVS;
//
//            //Let's go backwards now and check when we are no longer behind something
//            [branch]
//            for (int j = 1; j <= samples2; j++)
//            {
//                rayPositionVS = hitPosition - Offset * j / (samples2);
//
//                float2 sampleTexCoordAccurate = 0.5f * (float2(rayPositionVS.x, -rayPositionVS.y) + float2(1, 1));
//
//                int3 texCoordInt2 = int3(sampleTexCoordAccurate * resolution, 0);
//                depthValRay = 1 - DepthMap.Load(texCoordInt2).r;
//
//                //Looks like we don't hit anything any more?
//                [branch]
//                if (depthValRay > rayPositionVS.z) 
//                {
//                    //if (!AccuracyMode)
//                    //{
//                    //    sampleTexCoord = sampleTexCoordAccurate;
//                    //}
//                    //else
//                    //{
//                        //lin interpolate
//
//                        //only z is relevant
//                        float prevZ = 0; // rayPositionVSPrevious.z;
//                        float d = rayPositionVSPrevious.z - rayPositionVS.z;
//
//                        float /*depthPrev*/ b = depthValRayPrevious - rayPositionVS.z;
//                        float /*depth*/ a = depthValRay - rayPositionVS.z;
//
//                        float x = -b / (b - a) * (1 / (-d - 1 / (b - a))) / d;
//
//                        float2 sampleTexCoordPrevious = 0.5f * (float2(rayPositionVSPrevious.x, -rayPositionVSPrevious.y) + float2(1, 1));
//
//                        float depthValRayLerped = lerp(depthValRay, depthValRayPrevious, x);
//
//                        hit = true;
//
//                        sampleTexCoord = lerp(sampleTexCoordAccurate, sampleTexCoordPrevious, x);
//
//                        if (depthValRayLerped >= hitPosition.z)
//                        {
//                            hit = false;
//                            //sampleTexCoord = sampleTexCoordAccurate;
//                        }
//                        //else
//                        //{
//                        //    //depthValRay = depthValRayLerped;
//                        //}
//
//                        //if(depthValRay > rayPositionPrevious.z)
//                        //    hit = false;
//                    //}
//                    break;
//                }
//
//                depthValRayPrevious = depthValRay;
//                rayPositionVSPrevious = rayPositionVS;
//            }
//            
//            if (!hit)
//                continue;
//
//            //if (depthValRay < startingDepth)
//            //    break;
//
//            int3 texCoordInt = int3(sampleTexCoord * resolution, 0);
//            float4 albedoColor = TargetMap.Load(texCoordInt);
//
//            //float4 albedoColor = TargetMap.SampleLevel(texSampler, sampleTexCoord, roughness*5*i);
//
//            output = albedoColor;
//            output.a = 1;
//
//            [branch]
//            if (sampleTexCoord.y > border2)
//            {
//                output.a = lerp(1, 0, (sampleTexCoord.y - border2) * bordermulti);
//            }
//            else if (sampleTexCoord.y < border)
//            {
//                output.a = lerp(0, 1, sampleTexCoord.y * bordermulti);
//            }
//            [branch]
//            if (sampleTexCoord.x > border2)
//            {
//                output.a *= lerp(1, 0, (sampleTexCoord.x - border2) * bordermulti);
//            }
//            else if (sampleTexCoord.x < border)
//            {
//                output.a *= lerp(0, 1, sampleTexCoord.x * bordermulti);
//            }
//            
//			output.rgb *= output.a *(1 - roughness);
//			
//            break;
//        }
//
//        startingDepth = rayPositionVS.z;
//    }
//
//    return output;
//}

//float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
//{
//	float4 output = float4(0, 0, 0, 0);
//
//	/* float4 positionVS;
//	positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
//	positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);
//	*/
//	float2 texCoord = float2(input.TexCoord);
//
//	float linearDepth = DepthMap.Sample(texSampler, texCoord).r;
//
//	float3 positionVS = input.ViewRay * linearDepth;
//
//
//	float4 normalData = NormalMap.Sample(texSampler, texCoord);
//	//tranform normal back into [-1,1] range
//	float3 normal = decode(normalData.xyz);
//	float roughness = normalData.a;
//
//
//	[branch]
//	if (normalData.x + normalData.y <= 0.001f || roughness > 0.8f) //Out of range
//	{
//		return float4(0, 0, 0, 0);
//	}
//
//	//RealSpace
//	positionVS.w = 1.0f;
//	positionVS.z = depthVal;
//	float4 positionWS = mul(positionVS, InverseViewProjection);
//	positionWS /= positionWS.w;
//
//	// float3 incident = normalize(input.viewDirWS);
//	float3 incident = normalize(positionWS.xyz - CameraPosition);
//
//	float3 reflectVector = reflect(incident, normal);
//	// go
//
//	float4 samplePositionVS = mul(positionWS + float4(reflectVector, 0), ViewProjection);
//	samplePositionVS /= samplePositionVS.w;
//
//	float4 Offset = (samplePositionVS - positionVS);
//
//	float xMultiplier = 0;
//	float yMultiplier = 0;
//	//Lets go to the end of the screen
//	if (Offset.x > 0)
//	{
//		xMultiplier = (1 - positionVS.x) / Offset.x;
//	}
//	else
//	{
//		xMultiplier = (-1 - positionVS.x) / Offset.x;
//	}
//
//	if (Offset.y > 0)
//	{
//		yMultiplier = (1 - positionVS.y) / Offset.y;
//	}
//	else
//	{
//		yMultiplier = (-1 - positionVS.y) / Offset.y;
//	}
//
//	//what multiplier is smaller?
//
//	float multiplier = min(xMultiplier, yMultiplier); //xMultiplier < yMultiplier ? xMultiplier : yMultiplier;
//
//													  //int samples = 20;
//
//													  //Offset *= multiplier / samples;
//
//	Offset *= multiplier;
//
//	float maxOffset = max(abs(Offset.x), abs(Offset.y));
//
//	static int samples = 15; //int(maxOffset * 20);
//
//	static float border = 0.1f;
//
//	static float border2 = 1 - border;
//	static float bordermulti = 1 / border;
//
//	Offset /= samples;
//
//	float startingDepth = samplePositionVS.z;
//
//	float4 hitPosition;
//
//	[unroll]
//	for (int i = 0; i < samples; i++)
//	{
//		//if (i >= samples)
//		//	break;
//
//		if (Offset.z < 0)
//			break;
//
//		float4 rayPositionVS = samplePositionVS + Offset * i;
//
//		//float2 sampleTexCoord = 0.5f * (float2(samplePositionVS.x, -samplePositionVS.y) + 1);
//
//		float2 sampleTexCoord = 0.5f * (float2(rayPositionVS.x, -rayPositionVS.y) + float2(1, 1));
//
//		float depthValRay = 1 - DepthMap.Sample(texSampler, sampleTexCoord).r;
//
//		//We have a hit
//		[branch]
//		if (depthValRay <= rayPositionVS.z) //&& (Offset.z > 0)) //&& depthValRay >= startingDepth)  ) //|| (Offset.z < 0 && depthValRay < startingDepth)))
//		{
//			hitPosition = rayPositionVS;
//
//			bool hit = false;
//
//			int samples2 = samples + 1 - i; //samples + 1 - i;
//
//			float depthValRayPrevious = depthValRay;
//			float4 rayPositionVSPrevious = rayPositionVS;
//
//			//Let's go backwards now and check when we are no longer behind something
//			[branch]
//			for (int j = 1; j <= samples2; j++)
//			{
//				rayPositionVS = hitPosition - Offset * j / (samples2);
//
//				float2 sampleTexCoordAccurate = 0.5f * (float2(rayPositionVS.x, -rayPositionVS.y) + float2(1, 1));
//
//				int3 texCoordInt2 = int3(sampleTexCoordAccurate * resolution, 0);
//				depthValRay = 1 - DepthMap.Load(texCoordInt2).r;
//
//				//Looks like we don't hit anything any more?
//				[branch]
//				if (depthValRay > rayPositionVS.z)
//				{
//					//if (!AccuracyMode)
//					//{
//					//    sampleTexCoord = sampleTexCoordAccurate;
//					//}
//					//else
//					//{
//					//lin interpolate
//
//					//only z is relevant
//					float prevZ = 0; // rayPositionVSPrevious.z;
//					float d = rayPositionVSPrevious.z - rayPositionVS.z;
//
//					float /*depthPrev*/ b = depthValRayPrevious - rayPositionVS.z;
//					float /*depth*/ a = depthValRay - rayPositionVS.z;
//
//					float x = -b / (b - a) * (1 / (-d - 1 / (b - a))) / d;
//
//					float2 sampleTexCoordPrevious = 0.5f * (float2(rayPositionVSPrevious.x, -rayPositionVSPrevious.y) + float2(1, 1));
//
//					float depthValRayLerped = lerp(depthValRay, depthValRayPrevious, x);
//
//					hit = true;
//
//					sampleTexCoord = lerp(sampleTexCoordAccurate, sampleTexCoordPrevious, x);
//
//					if (depthValRayLerped >= hitPosition.z)
//					{
//						hit = false;
//						//sampleTexCoord = sampleTexCoordAccurate;
//					}
//					//else
//					//{
//					//    //depthValRay = depthValRayLerped;
//					//}
//
//					//if(depthValRay > rayPositionPrevious.z)
//					//    hit = false;
//					//}
//					break;
//				}
//
//				depthValRayPrevious = depthValRay;
//				rayPositionVSPrevious = rayPositionVS;
//			}
//
//			if (!hit)
//				continue;
//
//			//if (depthValRay < startingDepth)
//			//    break;
//
//			int3 texCoordInt = int3(sampleTexCoord * resolution, 0);
//			float4 albedoColor = TargetMap.Load(texCoordInt);
//
//			//float4 albedoColor = TargetMap.SampleLevel(texSampler, sampleTexCoord, roughness*5*i);
//
//			output = albedoColor;
//			output.a = 1;
//
//			[branch]
//			if (sampleTexCoord.y > border2)
//			{
//				output.a = lerp(1, 0, (sampleTexCoord.y - border2) * bordermulti);
//			}
//			else if (sampleTexCoord.y < border)
//			{
//				output.a = lerp(0, 1, sampleTexCoord.y * bordermulti);
//			}
//			[branch]
//			if (sampleTexCoord.x > border2)
//			{
//				output.a *= lerp(1, 0, (sampleTexCoord.x - border2) * bordermulti);
//			}
//			else if (sampleTexCoord.x < border)
//			{
//				output.a *= lerp(0, 1, sampleTexCoord.x * bordermulti);
//			}
//
//			output.rgb *= output.a * (1 - roughness);
//
//			break;
//		}
//
//		startingDepth = rayPositionVS.z;
//	}
//
//	return output;
//}



/*float4 PixelShaderFunction2(VertexShaderOutput input) : COLOR0
{
float4 output = float4(0, 0, 0, 0);
float4 positionVS;
positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);
float2 texCoord = float2(input.TexCoord);
float depthVal = 1 - DepthMap.Sample(texSampler, texCoord).r;
float4 normalData = NormalMap.Sample(texSampler, texCoord);
float3 normal = decode(normalData.xyz);
float roughness = normalData.a;
[branch]
if (normalData.x + normalData.y <= 0.001f || roughness > 0.8f) //Out of range
{
return float4(0, 0, 0, 0);
}
positionVS.w = 1.0f;
positionVS.z = depthVal;
float4 positionWS = mul(positionVS, InverseViewProjection);
positionWS /= positionWS.w;
float3 incident = normalize(positionWS.xyz - CameraPosition);
float3 reflectVector = reflect(incident, normal);
float4 samplePositionVS = mul(positionWS + float4(reflectVector, 0), ViewProjection);
samplePositionVS /= samplePositionVS.w;
float4 Offset = (samplePositionVS - positionVS);
float xMultiplier = 0;
float yMultiplier = 0;
if (Offset.x > 0)
{
xMultiplier = (1 - positionVS.x) / Offset.x;
}
else
{
xMultiplier = (-1 - positionVS.x) / Offset.x;
}

if (Offset.y > 0)
{
yMultiplier = (1 - positionVS.y) / Offset.y;
}
else
{
yMultiplier = (-1 - positionVS.y) / Offset.y;
}
float multiplier = min(xMultiplier, yMultiplier);
Offset *= multiplier;
float maxOffset = max(abs(Offset.x), abs(Offset.y));
static int samples = 15;
static float border = 0.1f;
static float border2 = 1 - border;
static float bordermulti = 1 / border;
Offset /= samples;
float startingDepth = samplePositionVS.z;
float4 hitPosition;
[branch]
for (int i = 0; i < samples; i++)
{
if (Offset.z < 0) break;
float4 rayPositionVS = samplePositionVS + Offset * i;
float2 sampleTexCoord = 0.5f * (float2(rayPositionVS.x, -rayPositionVS.y) + float2(1, 1));
float depthValRay = 1 - DepthMap.Sample(texSampler, sampleTexCoord).r;
[branch]
if (depthValRay <= rayPositionVS.z && (Offset.z > 0)) //&& depthValRay >= startingDepth)  ) //|| (Offset.z < 0 && depthValRay < startingDepth)))
{
hitPosition = rayPositionVS;

int samples2 = samples + 3 - i;
[branch]
for (int j = samples2; j > 0; j--)
{
rayPositionVS = hitPosition - Offset * j / (samples2 + 1);
float2 sampleTexCoordAccurate = 0.5f * (float2(rayPositionVS.x, -rayPositionVS.y) + float2(1, 1));
int3 texCoordInt2 = int3(sampleTexCoordAccurate * resolution, 0);
depthValRay = 1 - DepthMap.Load(texCoordInt2).r;
if (depthValRay < rayPositionVS.z && depthValRay >= startingDepth)
{
sampleTexCoord = sampleTexCoordAccurate;
break;
}
}
if (depthValRay < startingDepth)
break;
int3 texCoordInt = int3(sampleTexCoord * resolution, 0);
float4 albedoColor = TargetMap.Load(texCoordInt);
output = albedoColor;
output.a = 1;
[branch]
if (sampleTexCoord.y > border2)
{
output.a = lerp(1, 0, (sampleTexCoord.y - border2) * bordermulti);
}
else if (sampleTexCoord.y < border)
{
output.a = lerp(0, 1, sampleTexCoord.y * bordermulti);
}
[branch]
if (sampleTexCoord.x > border2)
{
output.a *= lerp(1, 0, (sampleTexCoord.x - border2) * bordermulti);
}
else if (sampleTexCoord.x < border)
{
output.a *= lerp(0, 1, sampleTexCoord.x * bordermulti);
}
output.rgb *= output.a * (1 - roughness);
break;
}
startingDepth = rayPositionVS.z;
}
return output;
}*/
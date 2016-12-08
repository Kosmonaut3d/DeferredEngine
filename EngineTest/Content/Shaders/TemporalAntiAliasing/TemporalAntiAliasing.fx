
float4x4 CurrentToPrevious;


Texture2D DepthMap;
Texture2D AccumulationMap;
Texture2D UpdateMap;

float2 Resolution = { 1280, 800 };

float3 FrustumCorners[4]; //In Viewspace!

float Threshold = 0;

SamplerState texSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
};

SamplerState linearSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = POINT;
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
	float3 ViewRay : TEXCOORD1;
};

struct PixelShaderOutput
{
	float4 Combine : COLOR0;
	float4 Coherence : COLOR1;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

float3 GetFrustumRay(float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
	output.ViewRay = GetFrustumRay(input.TexCoord);
    output.TexCoord = input.TexCoord;
    return output;
}

float3 ToYUV(float3 rgb)
{
	return rgb;
    /*float y = 0.299f * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;

    return float3(y, (rgb.b - y) * 0.493, (rgb.r - y) * 0.877);*/
}

float overlapFunction(float3 x, float3 y)
{
	return dot(x, y) / (length(x)*length(y));//1 - dot(abs(x-y), float3(1, 1, 1)) / 3;
}

float3 GetFrustumRay2(float2 texCoord)
{
	float3 x1 = lerp(FrustumCorners[0], FrustumCorners[1], texCoord.x);
	float3 x2 = lerp(FrustumCorners[2], FrustumCorners[3], texCoord.x);
	float3 outV = lerp(x1, x2, texCoord.y);
	return outV;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    float linearDepth = DepthMap.Sample(texSampler, texCoord).r;

	float3 positionVS = input.ViewRay * linearDepth;

    float4 previousPositionVS = mul(float4(positionVS,1), CurrentToPrevious);
    previousPositionVS /= previousPositionVS.w;

    float2 sampleTexCoord = 0.5f * (float2(previousPositionVS.x, -previousPositionVS.y) + 1);

    //Check how much they match
	int3 TexCoordInt = int3(texCoord * Resolution, 0);

    float4 updatedColorSample = UpdateMap.Load(TexCoordInt);

    int3 sampleTexCoordInt = int3(sampleTexCoord * Resolution, 0);

    float4 accumulationColorSample = AccumulationMap.Load(sampleTexCoordInt);

    float alpha = accumulationColorSample.a;

	

 //   float3 baseColorYUV = ToYUV(updatedColorSample.rgb);
 //////   //overlap
 //   float overlap = overlapFunction(ToYUV(accumulationColorSample.rgb), baseColorYUV);

 //   float overlapThreshold = Threshold; //+ 0.0000000000005f;

 //   bool foundOverlap = overlap > overlapThreshold;

 //   ////if (dot(updatedColorSample.rgb, float3(1, 1, 1)) > 1.8f)
 //   ////   foundOverlap = true;


	////////////////////////////////////////////////// NEIGHBORHOOD CLAMPING /////////////////
	//float neighborOverlap = 0;

 //   if(!foundOverlap)
 //   [branch]
 //   for (int x = -1; x <= 1; x++)
 //       {
 //           [branch]
 //           for (int y = -1; y <= 1; y++)
 //           {
 //           
 //               if (x == 0 && y == 0)
 //               {
 //                   continue;
 //               }

 //               float4 accumulationColorSampleNeighbour = AccumulationMap.Load(sampleTexCoordInt + int3(x, y, 0));

 //               float neighborOverlapTest = overlapFunction(ToYUV(accumulationColorSampleNeighbour.rgb), baseColorYUV);
 //   
 //               if (neighborOverlapTest > overlapThreshold)
 //               {
	//				neighborOverlap = neighborOverlapTest;
 //                   foundOverlap = true;
 //                   break;
 //               }
 //           }

 //           if (foundOverlap)
 //               break;
 //       }

	//float coherence = neighborOverlap;
	//float3 coh = float3(coherence, coherence, coherence);

	////Red if pixel is good
	//if (overlap > overlapThreshold)
	//	coh = float3(0, 1, 0);
	//else
	//{
	//	if (foundOverlap)
	//		coh = float3(1, 0, 0);
	//	else
	//		coh = float3(0, 0, 1);
	//}

	/////////////////////////////////////////////////////////////////


	//////////////////////// DEPTH CLAMPING
	/*float alpha = 0.9375;

    bool foundOverlap = abs(previousDepth - linearDepth) < Threshold;
	
	if (!foundOverlap)
	{
		[branch]
		for (int x = -1; x <= 1; x++)
		    {
		        [branch]
		        for (int y = -1; y <= 1; y++)
		        {
		           
		            if (x == 0 && y == 0)
		            {
		                continue;
		            }

		            previousDepth = AccumulationMap.Load(sampleTexCoordInt + int3(x, y, 0)).a;

		            if (abs(previousDepth - linearDepth) < Threshold)
		            {
		                foundOverlap = true;
		                break;
		            }
		        }

		        if (foundOverlap)
		            break;
		    }
	}

	if (!foundOverlap) alpha = 0;*/

 //   //alpha = 1 - 0.1f;
	alpha = min(1 - 1 / (1 / (1 - alpha) + 1), 0.9375);
/*
	if (Threshold > 0) alpha = 0.6f;*/
	if (linearDepth >= 0.999999) alpha = 0.5f;

	/*float2 diff = texCoord - sampleTexCoord;
	output.Coherence = float4(alpha, 0, 0, 0);*/

	/*if (!foundOverlap)
		alpha = 0;*/

    /*if (abs(previousPositionVS.z - depthVal) > 0.00001 || depthVal >= 0.999999f)
        alpha = 0;*/

	//Out of bounds, no info
	if (sampleTexCoord.x > 1 || sampleTexCoord.x < 0 || sampleTexCoord.y > 1 || sampleTexCoord.y < 0)
		alpha = 0;

	//output.Combine = float4(lerp(updatedColorSample.rgb, accumulationColorSample.rgb, alpha), linearDepth);
	//float depthOutput = alpha > 0 ? lerp(linearDepth, previousDepth, alpha) : linearDepth;

	float3 rgbout = lerp(updatedColorSample.rgb, accumulationColorSample.rgb, alpha);

	/*float3 colorDiff = abs(accumulationColorSample.rgb - updatedColorSample.rgb);

	if (rgbout.r < Threshold / 255)
	{
		rgbout.r = updatedColorSample.r;
	}
	if (rgbout.g < Threshold / 255)
	{
		rgbout.g = updatedColorSample.g;
	}
	if (rgbout.b < Threshold / 255)
	{
		rgbout.b = updatedColorSample.b;
	}*/

	/*
	if (texCoord.x < texCoord.y)
		output.Combine = float4(updatedColorSample.rgb, alpha);
*/
	output.Coherence = float4(sampleTexCoord, 0, 0);

	output.Combine = float4(rgbout, alpha);

	return output;

    //return float4(lerp(updatedColorSample.rgb, accumulationColorSample.rgb, alpha), alpha);

}

technique TAA
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}

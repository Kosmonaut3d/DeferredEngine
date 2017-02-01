
float4x4 CurrentToPrevious;


Texture2D DepthMap;
Texture2D AccumulationMap;

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

float3 GetFrustumRay2(float2 texCoord)
{
	float3 x1 = lerp(FrustumCorners[0], FrustumCorners[1], texCoord.x);
	float3 x2 = lerp(FrustumCorners[2], FrustumCorners[3], texCoord.x);
	float3 outV = lerp(x1, x2, texCoord.y);
	return outV;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    float2 texCoord = float2(input.TexCoord);
	int3 TexCoordInt = int3(texCoord * Resolution, 0);

    float linearDepth = DepthMap.Sample(texSampler, texCoord).r;

	float3 positionVS = input.ViewRay * linearDepth;

    float4 previousPositionVS = mul(float4(positionVS,1), CurrentToPrevious);
    previousPositionVS /= previousPositionVS.w;

    float2 sampleTexCoord = 0.5f * (float2(previousPositionVS.x, -previousPositionVS.y) + 1);
	int3 sampleTexCoordInt = int3(sampleTexCoord * Resolution, 0);

    float4 accumulationColorSample = AccumulationMap.Load(sampleTexCoordInt);

	accumulationColorSample.g = abs(accumulationColorSample.r - linearDepth);

	return accumulationColorSample;
}

technique TAA
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}

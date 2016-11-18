
float4x4 CurrentToPrevious;

Texture2D DepthMap;
Texture2D AccumulationMap;
Texture2D UpdateMap;

float2 Resolution = { 1280, 800 };

SamplerState texSampler
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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    float4 positionVS;
    positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
    positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);

    float2 texCoord = float2(input.TexCoord);
    
    float depthVal = 1 - DepthMap.Sample(texSampler, texCoord).r;

    //Convert to WS and then back to previous VS
    positionVS.w = 1.0f;
    positionVS.z = depthVal;
    float4 previousPositionVS = mul(positionVS, CurrentToPrevious);
    previousPositionVS /= previousPositionVS.w;

    float2 sampleTexCoord = 0.5f * (float2(previousPositionVS.x, -previousPositionVS.y) + 1);

    //Check how much they match

    float4 updatedColorSample = UpdateMap.Sample(texSampler, texCoord);

    //

    //float4 accumulationColorSample = AccumulationMap.Sample(linearSampler, sampleTexCoord);

    int3 sampleTexCoordInt = int3(sampleTexCoord * Resolution, 0);

    float4 accumulationColorSample = AccumulationMap.Load(sampleTexCoordInt);

    float alpha = accumulationColorSample.a;

    //static float overlapThreshold = 0.05f;

    //overlap
    float overlap = dot(accumulationColorSample.rgb, updatedColorSample.rgb);

    float overlapThreshold = (alpha) * 0.1f + 0.0000000000005f;

    bool foundOverlap = overlap > overlapThreshold;

    if(!foundOverlap)
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

                float4 accumulationColorSampleNeighbour = AccumulationMap.Load(sampleTexCoordInt + int3(x, y, 0));

                float overlap = dot(accumulationColorSampleNeighbour.rgb, updatedColorSample.rgb);
    
                if (overlap > overlapThreshold)
                {
                    foundOverlap = true;
                    break;
                }
            }

            if (foundOverlap)
                break;
        }

    //if(!foundOverlap)
    //    return float4(updatedColorSample.rgb, 0);

    //alpha = 1 - 0.1f;
    alpha = min(alpha+(1 - alpha) / 2, 0.9f);

    if (!foundOverlap)
        alpha = alpha/2;

    if (abs(previousPositionVS.z / depthVal - 1) > 0.000001)
        alpha = 0;
    //if ()
    //    alpha = 0;

    return float4(lerp(updatedColorSample.rgb, accumulationColorSample.rgb, alpha), alpha);

    //return AccumulationMap.Sample(linearSampler, sampleTexCoord);
}

technique TAA
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

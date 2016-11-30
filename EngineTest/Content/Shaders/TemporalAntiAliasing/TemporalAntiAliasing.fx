
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

float3 ToYUV(float3 rgb)
{
    float y = 0.299f * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;

    return float3(y, y, y); //(rgb.b - y) * 0.493, (rgb.r - y) * 0.877);
}

float overlapFunction(float3 x, float3 y)
{
    return dot(x / y, float3(1, 1, 1)) / 3;
}


float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    float4 positionVS;
    positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
    positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);

    float2 texCoord = float2(input.TexCoord);
    
    float depthVal = 1 - DepthMap.Sample(texSampler, texCoord).r;

    positionVS.w = 1.0f;
    positionVS.z = depthVal;
    float4 previousPositionVS = mul(positionVS, CurrentToPrevious);
    previousPositionVS /= previousPositionVS.w;

    //float4 PositionWS = mul(positionVS, CurrentToPrevious);
    //PositionWS /= PositionWS.w;

    //float4 previousPositionVS = mul(PositionWS, PreviousViewProjection);
    //previousPositionVS /= previousPositionVS.w;

    float2 sampleTexCoord = 0.5f * (float2(previousPositionVS.x, -previousPositionVS.y) + 1);

    //Check how much they match

    float4 updatedColorSample = UpdateMap.Sample(texSampler, texCoord);

    //

    //float4 accumulationColorSample = AccumulationMap.Sample(linearSampler, sampleTexCoord);

    int3 sampleTexCoordInt = int3(sampleTexCoord * Resolution, 0);

    float4 accumulationColorSample = AccumulationMap.Load(sampleTexCoordInt);

    float alpha = accumulationColorSample.a;

    ////static float overlapThreshold = 0.05f;

    //float3 baseColorYUV = ToYUV(updatedColorSample.rgb);
    ////overlap
    //float overlap = overlapFunction(ToYUV(accumulationColorSample.rgb), baseColorYUV);

    //float overlapThreshold = (1-alpha) * 0.6f; //+ 0.0000000000005f;

    //bool foundOverlap = overlap > overlapThreshold;

    ////if (dot(updatedColorSample.rgb, float3(1, 1, 1)) > 1.8f)
    ////   foundOverlap = true;

    //if(!foundOverlap)
    //[branch]
    //for (int x = -1; x <= 1; x++)
    //    {
    //        [branch]
    //        for (int y = -1; y <= 1; y++)
    //        {
            
    //            if (x == 0 && y == 0)
    //            {
    //                continue;
    //            }

    //            float4 accumulationColorSampleNeighbour = AccumulationMap.Load(sampleTexCoordInt + int3(x, y, 0));

    //            float overlap = overlapFunction(ToYUV(accumulationColorSampleNeighbour.rgb), baseColorYUV);
    
    //            if (overlap > overlapThreshold)
    //            {
    //                foundOverlap = true;
    //                break;
    //            }
    //        }

    //        if (foundOverlap)
    //            break;
    //    }

    //if(!foundOverlap)
    //    alpha = 0;

    //alpha = 1 - 0.1f;
    alpha = min(1 - 1 / (1 / (1 - alpha) + 1), 0.9375);

    //if (!foundOverlap)
    //    alpha = alpha/2;

    if (abs(previousPositionVS.z - depthVal) > 0.00001 || depthVal >= 0.999999f)
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

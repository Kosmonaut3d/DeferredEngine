
float3 cameraPosition;
//this is used to compute the world-position
float4x4 InvertViewProjection;
matrix Projection;
matrix ViewProjection;

float2 resolution = float2(1280, 800);


#include "helper.fx"

Texture2D depthMap;
Texture2D normalMap;
Texture2D albedoMap;
SamplerState depthSampler
{
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

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;
    return output;

}

float linearizeDepth(float depth)
{
    return  (Projection._43 / (depth - Projection._33));
}

float localDepth(float lindepth)
{
    return (Projection._43 / lindepth)+ Projection._33;
}

float4 PixelShaderFunctionClassic(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    float4 positionVS;
    positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
    positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);

    float2 texCoord = float2(input.TexCoord);
    
    float depthVal = 1-depthMap.Sample(depthSampler, texCoord).r;

    float4 normalData = normalMap.Sample(depthSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz);

    ////compute screen-space position

    //linDepth
    float linDepth = 1+( Projection._43 / (depthVal - Projection._33));

    //RealSpace
    positionVS.w = 1.0f;
    positionVS.z = depthVal;
    float4 positionWS = mul(positionVS, InvertViewProjection);
    positionWS /= positionWS.w;

    float3 incident = -cameraPosition + positionWS.xyz;
    float realDepth = length(incident);

    //normalize
    incident /= realDepth;
    float3 reflectVector = reflect(incident, normal);
    // go

    float4 samplePositionWS = positionWS + float4(reflectVector, 0)* realDepth / 15;
                                               
    float4 samplePositionVS = mul(samplePositionWS, ViewProjection);

    samplePositionVS /= samplePositionVS.w;

    ////////////////////////////////

    float4 viewOffset = samplePositionVS - positionVS;

    float zOffsetLinear = linearizeDepth(samplePositionVS.z) - linearizeDepth(positionVS.z);

    /////////////////////////////////


    float4 output = float4(0,0,0,0);

    int samples = 20;
    [unroll]
    for (uint i = 0; i < samples; i++)
    {
        //If the normal goes in our direction abort
        if(viewOffset.z < 0)
        {
            //output = float4(1, 0, 0, 1);
            break;
        }

        float oldZ = samplePositionVS.z;

        //float lerpDelta = i;
        //float delta = lerp(1, 20, lerpDelta / samples);

        float delta = i+1;

        //march in the given direction
        samplePositionVS = //positionVS + viewOffset * i;
        float4(positionVS.xy + viewOffset.xy * delta, localDepth(linearizeDepth(positionVS.z) + zOffsetLinear * delta), 0);

        float2 sampleTexCoord = 0.5f * (float2(samplePositionVS.x, -samplePositionVS.y) + 1);

        if(sampleTexCoord.x < 0 || sampleTexCoord.y < 0 || sampleTexCoord.x >1 || sampleTexCoord.y > 1)
        {
            break;
        }

        float sampleDepthVal = 1 - depthMap.Sample(depthSampler, sampleTexCoord).r;

        if (sampleDepthVal < oldZ)
        {
            break;
        }

        [branch]
        if (sampleDepthVal < samplePositionVS.z)
        {
            int3 texCoordInt = int3(sampleTexCoord * resolution, 0);
            float4 albedoColor = albedoMap.Load(texCoordInt);

            output = albedoColor;
            output.a = 1;

            float border = 0.1f;

            [branch]
            if(sampleTexCoord.y > 0.9f)
            {
                output.a = lerp(1, 0, (sampleTexCoord.y - 0.9) * 10);
            }
            else if (sampleTexCoord.y < 0.1f)
            {
                output.a = lerp(0, 1, sampleTexCoord.y * 10);
            }
            [branch]
            if (sampleTexCoord.x > 0.9f)
            {
                output.a *= lerp(1, 0, (sampleTexCoord.x - 0.9) * 10);
            }
            else if (sampleTexCoord.x < 0.1f)
            {
                output.a *= lerp(0, 1, sampleTexCoord.x * 10);
            }
            
            output.rgb *= output.a;

            break;
        }

    }
    return output;
}

technique SSAO
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunctionClassic();
    }
}

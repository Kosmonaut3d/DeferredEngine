
float3 CameraPosition;
//this is used to compute the world-position
float4x4 InverseViewProjection;
float4x4 Projection;
float4x4 ViewProjection;

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
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

SamplerState blurSamplerLinear
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
       
float FalloffMin = 0.000001f;
float FalloffMax = 0.002f;
  
int Samples = 8;

float Strength = 4;

float SampleRadius = 0.05f;

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
    float3 viewDirVS : TEXCOORD2;
};

struct VertexShaderOutputBlur
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
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    output.viewDirVS = input.Position.xyz;
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

float linearizeDepth(float depth)
{
    return (Projection._43 / (depth - Projection._33));
}

float localDepth(float lindepth)
{
    return (Projection._43 / lindepth) + Projection._33;
}

float zfar = 500;
float znear = 1;

float linearizeDepth2(float z)
{
    float zfar_2 = zfar / (zfar - znear);

    float z0 = z * zfar_2 - znear * zfar_2;

    float w0 = z;

    float native_z = z0 / w0;

    float linZ = (znear * zfar_2 / (zfar_2 - native_z));

    return linZ;
}

float3 randomNormal(float2 tex)
{
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f));
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f));
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f));
    return normalize(float3(noiseX, noiseY, noiseZ));
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(texSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normalWS = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    //Ignore sky!
    [branch]
    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        return float4(1, 0, 0, 0);
    }
    else
    {
        float depthVal = linearizeDepth(1 - DepthMap.Sample(texSampler, texCoord).r);
    
        float3 normalVS = mul(float4(normalWS, 0), ViewProjection).xyz;
        normalVS = normalize(normalVS);

        float3 randNor = randomNormal(input.TexCoord); //

        const float3 sampleSphere[] =
        {
            float3(0.2024537f, 0.841204f, -0.9060141f),
        float3(-0.2200423f, 0.6282339f, -0.8275437f),
        float3(0.3677659f, 0.1086345f, -0.4466777f),
        float3(0.8775856f, 0.4617546f, -0.6427765f),
        float3(0.7867433f, -0.141479f, -0.1567597f),
        float3(0.4839356f, -0.8253108f, -0.1563844f),
        float3(0.4401554f, -0.4228428f, -0.3300118f),
        float3(0.0019193f, -0.8048455f, 0.0726584f),
        float3(-0.7578573f, -0.5583301f, 0.2347527f),
        float3(-0.4540417f, -0.252365f, 0.0694318f),
        float3(-0.0483353f, -0.2527294f, 0.5924745f),
        float3(-0.4192392f, 0.2084218f, -0.3672943f),
        float3(-0.8433938f, 0.1451271f, 0.2202872f),
        float3(-0.4037157f, -0.8263387f, 0.4698132f),
        float3(-0.6657394f, 0.6298575f, 0.6342437f),
        float3(-0.0001783f, 0.2834622f, 0.8343929f),
        };

        float3 pos = float3(input.TexCoord, depthVal);

        float result = 0;

        float radius = SampleRadius * (1 - depthVal);

    [unroll]
        for (uint i = 0; i < Samples; i++)
        {
            float3 offset = reflect(sampleSphere[i], randNor);
                
        //reverse the sign if the normal is looking backward
            offset = sign(dot(offset, normalVS)) * offset; //
            offset.y = -offset.y;
            float3 ray = pos + offset * radius;

        //outside of view
            if ((saturate(ray.x) != ray.x) || (saturate(ray.y) != ray.y))
                continue;


        //sample

            float depthSample = linearizeDepth(1 - DepthMap.SampleLevel(texSampler, ray.xy, 0).r);

            float depthDiff = (depthVal - depthSample);

            float occlusion = depthDiff * (1 - depthVal);

            float falloff = 1 - saturate(depthDiff * (1 - depthVal) - FalloffMin) / (FalloffMax - FalloffMin);
        
            occlusion *= falloff;

            result += occlusion;
        
        }
        result /= Samples;
        result = saturate(1 - result * Strength * 200);

        return float4(result, 0, 0, 0);
    }
}


float4 BilateralBlurVertical(VertexShaderOutputBlur input) : SV_TARGET
{
    const uint numSamples = 9;
    const float texelsize = InverseResolution.x; //Als erstes ermitteln wir die horizontale Texelsize. 
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

    //Wie bereits erwähnt überspringen wir jeden zweiten Pixel. 
      
    float compareDepth = DepthMap.Sample(texSampler, input.TexCoord).r;
      //Tiefe des momentanen Pixels 
    float4 result = 0;
    float weightSum = 0.0f;
    [unroll]
    for (uint i = 0; i < numSamples; ++i)
    {
        float2 sampleOffset = float2( texelsize * samplerOffsets[i],0);
        float2 samplePos = input.TexCoord + sampleOffset;
           //Ermittle die Sampling-Position für den Gaussian-Blur. 
           
        float sampleDepth = DepthMap.Sample(texSampler, samplePos).r;
           //Hole Tiefe für den aktuellen Sample. 
           
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights[i];
           //Berechne bilaterale Wichtung für den aktuellen Sample, je größer die Entfernung 
           //vom momentanen Sample zum Quellsample, desto kleiner die Wichtung. 
           //0.0001 wird addiert um ein teilen durch 0 zu verhindern. 
           
        result += SSAOMap.Sample(blurSamplerPoint, samplePos) * weight;
           //Sample den Punkt und wichte ihn. 
           
        weightSum += weight;
           //Addiere die Wichtung, damit später der Durchschnitt ermittelt werden kann. 
    }

   [unroll]
    for (uint j = 0; j < numSamples2; ++j)
    {
        float2 sampleOffset = float2(texelsize * samplerOffsets2[j],0);
        float2 samplePos = input.TexCoord + sampleOffset;
           //Ermittle die Sampling-Position für den Gaussian-Blur. 
           
        float sampleDepth = DepthMap.Sample(texSampler, samplePos).r;
           //Hole Tiefe für den aktuellen Sample. 
           
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights2[j];
           //Berechne bilaterale Wichtung für den aktuellen Sample, je größer die Entfernung 
           //vom momentanen Sample zum Quellsample, desto kleiner die Wichtung. 
           //0.0001 wird addiert um ein teilen durch 0 zu verhindern. 
           
        result += SSAOMap.Sample(blurSamplerLinear, samplePos, 0) * weight;
           //Sample den Punkt und wichte ihn. 
           
        weightSum += weight;
           //Addiere die Wichtung, damit später der Durchschnitt ermittelt werden kann. 
    }

    result /= weightSum;
      //Ermittle den Durchschnitt durch das Teilen durch die Wichtungssumme. 
    return result;
}

float4 BilateralBlurHorizontal(VertexShaderOutputBlur input) : SV_TARGET
{
    const uint numSamples = 9;
    const float texelsize = InverseResolution.y; //Als erstes ermitteln wir die horizontale Texelsize. 
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

    //Wie bereits erwähnt überspringen wir jeden zweiten Pixel. 
      
    float compareDepth = DepthMap.Sample(texSampler, input.TexCoord).r;
      //Tiefe des momentanen Pixels 
    float4 result = 0;
    float weightSum = 0.0f;
    [unroll]
    for (uint i = 0; i < numSamples; ++i)
    {
        float2 sampleOffset = float2(0.0f, texelsize * samplerOffsets[i]);
        float2 samplePos = input.TexCoord + sampleOffset;
           //Ermittle die Sampling-Position für den Gaussian-Blur. 
           
        float sampleDepth = DepthMap.Sample(texSampler, samplePos).r;
           //Hole Tiefe für den aktuellen Sample. 
           
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights[i];
           //Berechne bilaterale Wichtung für den aktuellen Sample, je größer die Entfernung 
           //vom momentanen Sample zum Quellsample, desto kleiner die Wichtung. 
           //0.0001 wird addiert um ein teilen durch 0 zu verhindern. 
           
        result += SSAOMap.Sample(blurSamplerPoint, samplePos) * weight;
           //Sample den Punkt und wichte ihn. 
           
        weightSum += weight;
           //Addiere die Wichtung, damit später der Durchschnitt ermittelt werden kann. 
    }

    [unroll]
    for (uint j = 0; j < numSamples2; ++j)
    {
        float2 sampleOffset = float2(0.0f, texelsize * samplerOffsets2[j]);
        float2 samplePos = input.TexCoord + sampleOffset;
           //Ermittle die Sampling-Position für den Gaussian-Blur. 
           
        float sampleDepth = DepthMap.SampleLevel(texSampler, samplePos, 0).r;
           //Hole Tiefe für den aktuellen Sample. 
           
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights2[j];
           //Berechne bilaterale Wichtung für den aktuellen Sample, je größer die Entfernung 
           //vom momentanen Sample zum Quellsample, desto kleiner die Wichtung. 
           //0.0001 wird addiert um ein teilen durch 0 zu verhindern. 
           
        result += SSAOMap.Sample(blurSamplerLinear, samplePos) * weight;
           //Sample den Punkt und wichte ihn. 
           
        weightSum += weight;
           //Addiere die Wichtung, damit später der Durchschnitt ermittelt werden kann. 
    }

    result /= weightSum;
      //Ermittle den Durchschnitt durch das Teilen durch die Wichtungssumme. 
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

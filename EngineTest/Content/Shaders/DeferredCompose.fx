
Texture2D colorMap;
// normals, and specularPower in the alpha channel
Texture2D diffuseLightMap;
Texture2D specularLightMap;

static float2 Resolution = float2(1280, 800);

float average_skull_depth = 10;
bool useGauss = false;

#include "helper.fx"

float exposure = 20;

Texture2D SSAOMap;

bool useSSAO = true;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler diffuseLightSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler specularLightSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

Texture2D skull;
sampler linearSampler = sampler_state
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

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;
    return output;
}

float pixelsize_intended = 3;
 

float4 GaussianSampler(float2 TexCoord, float offset)
{
    float4 finalColor = float4(0, 0, 0, 0);
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        finalColor += skull.Sample(linearSampler, TexCoord.xy +
                    offset * SampleOffsets[i] * InverseResolution) * SampleWeights[i];
    }
   // finalColor = colorMap.Sample(colorSampler, TexCoord.xy);
    return finalColor;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 diffuseColor = colorMap.Sample(colorSampler, input.TexCoord);

    float albedoColorProp = diffuseColor.a;

    float pixelsize = pixelsize_intended;
    
    float2 skullTexCoord = trunc(input.TexCoord * Resolution / pixelsize / 2) / Resolution * pixelsize * 2;
       
    

    float materialType = decodeMattype(albedoColorProp);

    float metalness = decodeMetalness(albedoColorProp);
                
    float3 diffuseContrib = float3(0, 0, 0);

    //
    
    if(useGauss)
    {
        float4 skullColor = GaussianSampler(input.TexCoord, 3);

    //[branch]
        if (abs(materialType - 2) < 0.1f)
        {
    //    float2 pixel = trunc(input.TexCoord * Resolution);

    //    float pixelsize2 = 2 * pixelsize;
    //    if (pixel.x % pixelsize2 <= pixelsize && pixel.y % pixelsize2 <= pixelsize)
            diffuseContrib = float3(0, skullColor.x * 0.49, skullColor.x * 0.95f) * 0.06f ;
        }
    }
    else
    {
        float skullColor = skull.Sample(linearSampler, skullTexCoord).r;
        if (abs(materialType - 2) < 0.1f)
        {
        float2 pixel = trunc(input.TexCoord * Resolution);

        float pixelsize2 = 2 * pixelsize;
        if (pixel.x % pixelsize2 <= pixelsize && pixel.y % pixelsize2 <= pixelsize)
                diffuseContrib = float3(0, skullColor * 0.49, skullColor * 0.95f) * 0.06f + float3(0.5f, 0.2f, 0.2f);

        }
    }

    if (abs(materialType - 3) < 0.1f)
    {
        return diffuseColor;
    }
        
    //SSAO
    float ssaoContribution = 1;
    if(useSSAO)
    {
        ssaoContribution = SSAOMap.Sample(linearSampler, input.TexCoord).r;
    }

    float f0 = lerp(0.04f, diffuseColor.g * 0.25 + 0.75, metalness);
    
    float3 diffuseLight = diffuseLightMap.Sample(diffuseLightSampler, input.TexCoord).rgb;
    float3 specularLight = specularLightMap.Sample(specularLightSampler, input.TexCoord).rgb;

    float3 plasticFinal = diffuseColor.rgb * (diffuseLight) + specularLight;
                  
    float3 metalFinal = specularLight * diffuseColor.rgb;

    float3 finalValue = lerp(plasticFinal, metalFinal, metalness) + diffuseContrib;

    return float4(finalValue * ssaoContribution, 1) * exposure;
}


technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

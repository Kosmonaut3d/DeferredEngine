
Texture2D colorMap;
// normals, and specularPower in the alpha channel
Texture2D diffuseLightMap;
Texture2D specularLightMap;

static float2 Resolution = float2(1280, 800);

#include "helper.fx"

float exposure = 20;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
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
sampler skullSampler = sampler_state
{
    Texture = (skullMap);
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

float pixelsize = 3;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 diffuseColor = colorMap.Sample(colorSampler, input.TexCoord);

    float albedoColorProp = diffuseColor.a;

    
    float2 skullTexCoord = trunc(input.TexCoord * Resolution / pixelsize / 2) / Resolution * pixelsize*2;
       
    float skullColor = skull.Sample(skullSampler, skullTexCoord).r;

    float materialType = decodeMattype(albedoColorProp);

    float3 diffuseContrib = float3(0, 0, 0);

    if (abs(materialType - 1) < 0.1f)
    {
        float2 pixel = trunc(input.TexCoord * Resolution);

        float pixelsize2 = 2 * pixelsize;
        if (pixel.x % pixelsize2 <= pixelsize && pixel.y % pixelsize2 <= pixelsize)
        diffuseContrib = float3(0, skullColor * 0.49, skullColor*0.95f) * 0.06f;
    }


    
    float3 diffuseLight = diffuseLightMap.Sample(diffuseLightSampler, input.TexCoord).rgb;
    float3 specularLight = specularLightMap.Sample(specularLightSampler, input.TexCoord).rgb;
    return float4(((diffuseColor.rgb) * diffuseLight + diffuseContrib + specularLight), 1) * exposure;
}


technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

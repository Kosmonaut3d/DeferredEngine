
Texture2D colorMap;
// normals, and specularPower in the alpha channel
Texture2D diffuseLightMap;
Texture2D specularLightMap;

float exposure = 2;

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


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 diffuseColor = colorMap.Sample(colorSampler, input.TexCoord).rgb;
    float3 diffuseLight = diffuseLightMap.Sample(diffuseLightSampler, input.TexCoord).rgb;
    float3 specularLight = specularLightMap.Sample(specularLightSampler, input.TexCoord).rgb;
    return float4((diffuseColor * diffuseLight + specularLight), 1) * exposure;
}


technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

float4x4 ViewProjection;
//color of the light 
float3 lightColor;
//position of the camera, for specular light
float3 cameraPosition = float3(0,0,0);
//this is used to compute the world-position
float4x4 InvertViewProjection;

float3 LightVector;
//control the brightness of the light
float lightIntensity = 1.0f;

// diffuse color, and specularIntensity in the alpha channel
Texture2D AlbedoMap;
// normals, and specularPower in the alpha channel
Texture2D NormalMap;
      
//depth
Texture2D DepthMap;

#include "helper.fx"
       

sampler colorSampler = sampler_state
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler depthSampler = sampler_state
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
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
    float3 viewDir : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
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
    output.viewDir = normalize(mul(output.Position, InvertViewProjection).xyz);
    return output;

}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(normalSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    [branch]
    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        output.Diffuse = float4(0, 0, 0, 0);
        output.Specular = float4(0, 0, 0, 0);
        return output;
    }
    else
    {
    //get metalness
        float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Sample(colorSampler, texCoord);

        float metalness = decodeMetalness(color.a);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        float3 cameraDirection = -normalize(input.viewDir);

        float NdL = saturate(dot(normal, -LightVector));

        float3 diffuse = DiffuseOrenNayar(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
        float3 specular = SpecularCookTorrance(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);

        output.Diffuse = float4(diffuse, 0) * (1 - f0) * 0.01f;
        output.Specular = float4(specular, 0) * 0.01f;

        return output;
    }
}

   
technique Unshadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}

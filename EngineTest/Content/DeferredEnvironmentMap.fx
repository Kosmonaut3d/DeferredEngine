
float3 cameraPosition;
float3 cameraDirection;
//this is used to compute the world-position
float4x4 InvertViewProjection;

#include "helper.fx"

Texture2D colorMap;
// normals, and specularPower in the alpha channel
texture normalMap;
//depth
texture depthMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

TextureCube ReflectionCubeMap;
sampler ReflectionCubeMapSampler = sampler_state
{
    texture = <ReflectionCubeMap>;
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
    return output;

}

PixelShaderOutput PixelShaderFunctionPBR(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    float4 position;
    position.x = input.TexCoord.x * 2.0f - 1.0f;
    position.y = -(input.TexCoord.y * 2.0f - 1.0f);

    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
    //get specular power
    float f0 = normalData.a;
    //get specular intensity from the colorMap
    float4 color = tex2D(colorSampler, texCoord);

    float roughness = decodeRoughness(color.a);

    float materialType = decodeMattype(color.a);

    float matMultiplier = 0;

    if (abs(materialType - 1) < 0.1f)
    {
        matMultiplier = 2;
    }

    if (abs(materialType - 2) < 0.1f)
    {
        matMultiplier = 4;
    }

    float depthVal = 1-tex2D(depthSampler, texCoord).r;
    ////compute screen-space position

    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;
    //surface-to-light vector

    
    float3 incident = -cameraPosition+position.xyz;
    float3 reflectionVector = reflect(incident, normal);

    //float NdotC = saturate(dot(-incident, normal));

    reflectionVector.z = -reflectionVector.z;

    float3 ReflectColor = ReflectionCubeMap.Sample(ReflectionCubeMapSampler, reflectionVector) * (1 - roughness) * (1 + matMultiplier); //* NdotC * NdotC * NdotC;


    PixelShaderOutput output;

    output.Diffuse = float4(0, 0, 0, 0);
    output.Specular = float4(ReflectColor, 0) * 0.01;

    return output;

}

technique PBR
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunctionPBR();
    }
}

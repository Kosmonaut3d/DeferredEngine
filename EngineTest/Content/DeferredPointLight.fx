float4x4 World;
float4x4 View;
float4x4 Projection;
//color of the light 
float3 lightColor;
//position of the camera, for specular light
float3 cameraPosition;
float3 cameraDirection;
//this is used to compute the world-position
float4x4 InvertViewProjection;
//this is the position of the light
float3 lightPosition;
//how far does this light reach
float lightRadius;
//control the brightness of the light
float lightIntensity = 1.0f;
// diffuse color, and specularIntensity in the alpha channel
Texture2D colorMap;
// normals, and specularPower in the alpha channel
texture normalMap;

bool inside = false;

#include "helper.fx"

//depth
texture depthMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
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


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
    float4 Position : POSITION0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};

struct PixelShaderInput
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
    //processing geometry coordinates
    float4 worldPosition = mul(float4(input.Position.rgb, 1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    //need t
    output.ScreenPosition = output.Position;
    return output;
}

PixelShaderInput BaseCalculations(VertexShaderOutput input)
{
     //obtain screen position
    input.ScreenPosition.xyz /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);

    /////////////////////////////////////////

    //read depth
    float depthVal = 1 - tex2D(depthSampler, texCoord).r;

        //compute screen-space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    //////////////////////////////////////

    PixelShaderInput output;
    output.Position = position;
    output.TexCoord = texCoord;

        /////////////////////////////////////
    //CULL?
    float lightDepth = input.ScreenPosition.z;

    float insideMult = inside;
    if (insideMult == 0)
        insideMult = -1;

    [branch]
   // if(lightDepth*inside < depthVal*inside)
    if (lightDepth * insideMult < depthVal * insideMult)
    {
        clip(-1);
        return output;
    }


    return output;
}

PixelShaderOutput BasePixelShaderFunction(PixelShaderInput input) : COLOR0
{
    
    PixelShaderOutput output;
    
    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, input.TexCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
    //get metalness
    float roughness = normalData.a;
    //get specular intensity from the colorMap
    float4 color = tex2D(colorSampler, input.TexCoord);

    float metalness = decodeMetalness(color.a);
    
    float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

    //read depth
    float depthVal = 1-tex2D(depthSampler, input.TexCoord).r;

    //surface-to-light vector
    float3 lightVector = lightPosition - input.Position.xyz;

    float lengthLight = length(lightVector);

    [branch]
    if(lengthLight>lightRadius)
    {
        clip(-1);
        return output;
    }

    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - lengthLight/ lightRadius);
    //normalize light vector
    lightVector = normalize(lightVector);

    float3 cameraDirection = normalize(cameraPosition - input.Position.xyz);
    //compute diffuse light
    float NdL = saturate(dot(normal, lightVector));

    float3 diffuseLight = float3(0, 0, 0);

    [branch]
    if(metalness<0.99)
    {
        diffuseLight = DiffuseOrenNayar(NdL, normal, -lightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    }
    float3 specular = SpecularCookTorrance(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
    //take into account attenuation and lightIntensity.

    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
    output.Diffuse.rgb = (attenuation * diffuseLight * (1 - f0)) * 0.01f; //* (1 - f0)) * (f0 + 1) * (f0 + 1);
    output.Specular.rgb = specular * attenuation          *0.01f;

    return output;
   //return float4(color.rgb * (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1) *0.5f + specular*attenuation, 1);

}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    PixelShaderInput p_input = BaseCalculations(input);
 
    PixelShaderOutput Output;

    Output = BasePixelShaderFunction(p_input);

    return Output;
}

technique PBR
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}






//PixelShaderOutput PixelShaderFunctionClassic(VertexShaderOutput input) : COLOR0
//{
//    //obtain screen position
//    input.ScreenPosition.xy /= input.ScreenPosition.w;
//    //obtain textureCoordinates corresponding to the current pixel
//    //the screen coordinates are in [-1,1]*[1,-1]
//    //the texture coordinates need to be in [0,1]*[0,1]
//    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    
//    //get normal data from the normalMap
//    float4 normalData = tex2D(normalSampler, texCoord);
//    //tranform normal back into [-1,1] range
//    float3 normal = 2.0f * normalData.xyz - 1.0f; //could do mad
//    //get specular power
//    float f0 = normalData.a;
//    //get specular intensity from the colorMap
//    float4 color = tex2D(colorSampler, texCoord);

//    float roughness = decodeMetalness(color.a);

//    //read depth
//    float depthVal = 1 - tex2D(depthSampler, texCoord).r;
           
//    PixelShaderOutput output;

//    //compute screen-space position
//        float4 position;
//        position.xy = input.ScreenPosition.xy;
//        position.z = depthVal;
//        position.w = 1.0f;
//    //transform to world space
//        position = mul(position, InvertViewProjection);
//        position /= position.w;
//    //surface-to-light vector
//        float3 lightVector = lightPosition - position.xyz;
//    //compute attenuation based on distance - linear attenuation
//        float attenuation = saturate(1.0f - length(lightVector) / lightRadius) * lightIntensity;
//    //normalize light vector
//        lightVector = normalize(lightVector);

//        float3 cameraDirection = normalize(cameraPosition - position.xyz);
//    //compute diffuse light
//        float NdL = saturate(dot(normal, lightVector));

//        float3 diffuseLight = NdL * lightColor.rgb;
    
//        float3 reflectionVector = normalize(reflect(-lightVector, normal));
//    //camera-to-surface vector
//        float3 specular = /*(1-roughness)*/(1 - roughness) * (1 - roughness) * 4 * pow(saturate(dot(reflectionVector, cameraDirection)), (1 - roughness) * (1 - roughness) * 60); //+ (1 - NdL) * (1 - NdL) * f0;

//    //take into account attenuation and lightIntensity.

//    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
//        output.Diffuse.rgb = attenuation * diffuseLight * 0.01f;
//        output.Specular.rgb = specular * lightColor * attenuation * 0.01f;
    
//    return output;
//   //return float4(color.rgb * (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1) *0.5f + specular*attenuation, 1);

//}
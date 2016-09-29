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
float3 lightDirection;

#include "helper.fx"

float4x4 LightViewProjection;
//how far does this light reach
float lightRadius;
//control the brightness of the light
float lightIntensity = 1.0f;
// diffuse color, and specularIntensity in the alpha channel
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

Texture2D shadowMap;
sampler shadowSampler = sampler_state
{
    Texture = (colorMap);
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
    float4 Position : POSITION0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
};

struct PixelShaderInput
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
    //processing geometry coordinates
    float4 worldPosition = mul(float4(input.Position.rgb, 1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.ScreenPosition = output.Position;
    return output;
}


float chebyshevUpperBound(float distance, float2 texCoord)
{
		// We retrive the two moments previously stored (depth and depth*depth)
    float2 moments = 1 - shadowMap.Sample(shadowSampler, texCoord).rg;
		
		// Surface is fully lit. as the current fragment is before the light occluder
    if (distance <= moments.x)
        return 1.0;
	
		// The fragment is either in shadow or penumbra. We now use chebyshev's upperBound to check
		// How likely this pixel is to be lit (p_max)
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, 0.000002);
	
    float d = distance - moments.x;
    float p_max = variance / (variance + d * d);
	
    return p_max;
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
    position.z =depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    //////////////////////////////////////

    PixelShaderInput output;
    output.Position = position;
    output.TexCoord = texCoord;

    return output;
}

PixelShaderOutput BasePixelShaderFunction(PixelShaderInput input)
{
    
    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, input.TexCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
    //get metalness
    float roughness = normalData.a;
    //get specular intensity from the colorMap
    float4 color = tex2D(colorSampler, input.TexCoord);

    float metalness = decodeMetalness(color.a);
    
    float f0 = lerp(0.04f, color.r * 0.25 + 0.75, metalness);

    
    //surface-to-light vector
    float3 lightVector = lightPosition - input.Position.xyz;
    float3 lightVectorNormalized = normalize(lightVector);

    //Shadow

    float spotReach = 0.75f;

    float spotStrength = saturate(dot(lightVectorNormalized, lightDirection) - spotReach) * 1 / (1 - spotReach);

    PixelShaderOutput output;
    output.Diffuse = float4(0, 0, 0, 0);
    output.Specular = float4(0, 0, 0, 0);

    if (spotStrength <= 0)
        return output;
    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - length(lightVector) / lightRadius) * spotStrength;
    //normalize light vector
    

    float3 cameraDirection = normalize(cameraPosition - input.Position.xyz);
    //compute diffuse light
    float NdL = saturate(dot(normal, lightVectorNormalized));

    float3 diffuseLight = float3(0, 0, 0);

    if (metalness < 0.95f)
        diffuseLight = DiffuseOrenNayar(NdL, normal, lightVectorNormalized, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
    float3 specular = SpecularCookTorrance(NdL, normal, lightVectorNormalized, cameraDirection, lightIntensity, lightColor, f0, roughness);
    //take into account attenuation and lightIntensity.

    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
    
    output.Diffuse.rgb = (attenuation * diffuseLight * (1 - f0)) * 0.01f;
    output.Specular.rgb = specular * attenuation * 0.01f;

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

PixelShaderOutput PixelShaderFunctionShadowed(VertexShaderOutput input) : COLOR0
{
    PixelShaderInput p_input = BaseCalculations(input);

    //Shadows
    float4 positionInLS = mul(p_input.Position, LightViewProjection);
    float2 ShadowTexCoord = mad(positionInLS.xy / positionInLS.w, 0.5f, float2(0.5f, 0.5f));
    ShadowTexCoord.y = 1 - ShadowTexCoord.y;
    float depthInLS = (positionInLS.z / positionInLS.w);
    
    float shadowVSM = chebyshevUpperBound(depthInLS, ShadowTexCoord);

    //////////////////////////////////////

    PixelShaderOutput Output;

    Output = BasePixelShaderFunction(p_input);

    Output.Diffuse *= shadowVSM;
    Output.Specular *= max(shadowVSM - 0.1f, 0);

    return Output;
}

technique Unshadowed
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique Shadowed
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunctionShadowed();
    }
}


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
float4x4 LightProjection;
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

float3 SpecularCookTorrance(float NdotL, float3 normal, float3 negativeLightDirection, float3 cameraDirectionP, float diffuseIntensity, float3 diffuseColor, float F0, float Roughness)
{
    float3 specular = float3(0, 0, 0);
    [branch]
    if (NdotL > 0.0f)
    {
        //http://ruh.li/GraphicsCookTorrance.html

        float3 cameraDir = cameraDirectionP; //-mul(float4(CameraDir, 0), World).xyz;
        
        float3 halfVector = normalize(negativeLightDirection + cameraDir);

        float NdotH = saturate(dot(normal, halfVector));
        float NdotV = saturate(dot(normal, cameraDir));
        float VdotH = saturate(dot(normal, halfVector));
        float mSquared = Roughness * Roughness;

        //float NH2 = 2.0 * NdotH;
        //float g1 = (NH2 * NdotV) / VdotH;
        //float g2 = (NH2 * NdotL) / VdotH;
        //float geoAtt = min(1.0, min(g1, g2));
        // ->
        float g_min = min(NdotV, NdotL);
        float geoAtt = saturate(2 * NdotH * g_min / VdotH);

        //roughness
        //float r1 = 0.25/(mSquared * pow(NdotH, 4.0));
        //->
        float NdotHtemp = NdotH * NdotH;

        float r1 = 0.25 / (mSquared * NdotHtemp * NdotHtemp);
        float r2 = (mad(NdotH, NdotH, -1.0)) / (mSquared * NdotH * NdotH);
        float roughness2 = r1 * exp(r2);

        //fresnel        (Schlick)
        float fresnel = pow(1.0 - VdotH, 5.0);
        fresnel *= (1.0 - F0);
        fresnel += F0;

        specular = max(0, (fresnel * geoAtt * roughness2) / (4 * NdotV * NdotL)) * diffuseIntensity * diffuseColor * NdotL; //todo check this!!!!!!!!!!! why 3.14?j only relevant if we have it 
        
        //http://malcolm-mcneely.co.uk/blog/?p=214
    }
    return specular;
}

float3 DiffuseOrenNayar(float NdotL, float3 normal, float3 lightDirection, float3 cameraDirection, float lightIntensity, float3 lightColor, float roughness)
{
    const float PI = 3.14159;
    
    // calculate intermediary values
    float NdotV = dot(normal, cameraDirection);

    float angleVN = acos(NdotV);
    float angleLN = acos(NdotL);
    
    float alpha = max(angleVN, angleLN);
    float beta = min(angleVN, angleLN);
    float gamma = dot(cameraDirection - normal * NdotV, lightDirection - normal * NdotL);
    
    float roughnessSquared = roughness * roughness;
    
    // calculate A and B
    float A = 1.0 - 0.5 * (roughnessSquared / (roughnessSquared + 0.57));

    float B = 0.45 * (roughnessSquared / (roughnessSquared + 0.09));
 
    float C = sin(alpha) * tan(beta);
    
    // put it all together
    float L1 = max(0.0, NdotL) * (A + B * max(0.0, gamma) * C);
    
    // get the final color 
    return L1 * lightColor* lightIntensity / 4;
}

float chebyshevUpperBound(float distance, float2 texCoord)
{
		// We retrive the two moments previously stored (depth and depth*depth)
    float2 moments = 1-shadowMap.Sample(shadowSampler, texCoord).rg;
		
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

PixelShaderOutput PixelShaderFunctionPBR(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    
    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData); //2.0f * normalData.xyz - 1.0f;    //could do mad
    //get specular power
    float f0 = normalData.a;
    //get specular intensity from the colorMap
    float4 color = tex2D(colorSampler, texCoord);

    float roughness = decodeRoughness(color.a);

    //float materialType = decodeMattype(color.a);

    //if (abs(materialType - 1) < 0.1f)
    //    roughness = 1;

    //read depth
    float depthVal = 1-tex2D(depthSampler, texCoord).r;
    //compute screen-space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;
    //surface-to-light vector
    float3 lightVector = lightPosition - position.xyz;
    float3 lightVectorNormalized = normalize(lightVector);

    

    //Shadow

    float4 positionInLS = mul(position, LightViewProjection);
    float2 ShadowTexCoord = mad(positionInLS.xy / positionInLS.w, 0.5f, float2(0.5f, 0.5f));
    ShadowTexCoord.y = 1 - ShadowTexCoord.y;
    float depthInLS = (positionInLS.z / positionInLS.w);
    
    float shadowVSM = chebyshevUpperBound(depthInLS, ShadowTexCoord);


    float spotReach = 0.75f;

    float spotStrength = saturate(dot(lightVectorNormalized, lightDirection) - spotReach) * 1/(1-spotReach);

    PixelShaderOutput output;
    output.Diffuse = float4(0, 0, 0, 0);
    output.Specular = float4(0, 0, 0, 0);

    if (spotStrength <= 0)
        return output;
    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - length(lightVector) / lightRadius) * lightIntensity * spotStrength;
    //normalize light vector
    

    float3 cameraDirection = normalize(cameraPosition - position.xyz);
    //compute diffuse light
    float NdL = saturate(dot(normal, lightVectorNormalized));

    float3 diffuseLight = DiffuseOrenNayar(NdL, normal, lightVectorNormalized, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
    float3 specular = SpecularCookTorrance(NdL, normal, lightVectorNormalized, cameraDirection, lightIntensity, lightColor, f0, roughness);
    //take into account attenuation and lightIntensity.

    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
    
    output.Diffuse.rgb = (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1)  * 0.01f * shadowVSM;
    output.Specular.rgb = specular * attenuation          *0.01f * max(shadowVSM-0.1f, 0);

    return output;
   //return float4(color.rgb * (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1) *0.5f + specular*attenuation, 1);

}

PixelShaderOutput PixelShaderFunctionClassic(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    //obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    
    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f; //could do mad
    //get specular power
    float f0 = normalData.a;
    //get specular intensity from the colorMap
    float4 color = tex2D(colorSampler, texCoord);

    float roughness = decodeRoughness(color.a);
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
    //surface-to-light vector
    float3 lightVector = lightPosition - position.xyz;
    float3 lightVectorNormalized = normalize(lightVector);

    

    //Shadow

    float4 positionInLS = mul(position, LightViewProjection);
    float2 ShadowTexCoord = mad(positionInLS.xy / positionInLS.w, 0.5f, float2(0.5f, 0.5f));
    ShadowTexCoord.y = 1 - ShadowTexCoord.y;
    float depthInLS = (positionInLS.z / positionInLS.w);
    
    float shadowVSM = chebyshevUpperBound(depthInLS, ShadowTexCoord);


    float spotReach = 0.75f;

    float spotStrength = saturate(dot(lightVectorNormalized, lightDirection) - spotReach) * 1 / (1 - spotReach);

    PixelShaderOutput output;
    output.Diffuse = float4(0, 0, 0, 0);
    output.Specular = float4(0, 0, 0, 0);

    if (spotStrength <= 0)
        return output;
    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - length(lightVector) / lightRadius) * lightIntensity * spotStrength;
    //normalize light vector
    

    float3 cameraDirection = normalize(cameraPosition - position.xyz);
    //compute diffuse light
    float NdL = saturate(dot(normal, lightVectorNormalized));

        float3 diffuseLight = NdL * lightColor.rgb;
    
        float3 reflectionVector = normalize(reflect(-lightVector, normal));
    //camera-to-surface vector
        float3 specular = /*(1-roughness)*/(1 - roughness) * (1 - roughness) * 4 * pow(saturate(dot(reflectionVector, cameraDirection)), (1 - roughness) * (1 - roughness) * 60); //+ (1 - NdL) * (1 - NdL) * f0;

    //take into account attenuation and lightIntensity.

    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
    output.Diffuse.rgb = attenuation * diffuseLight * 0.01f * shadowVSM;
    output.Specular.rgb = specular * lightColor * attenuation * 0.01f * max(shadowVSM - 0.1f, 0);;
    
    return output;
   //return float4(color.rgb * (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1) *0.5f + specular*attenuation, 1);

}

technique PBR
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunctionPBR();
    }
}

technique Classic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunctionClassic();
    }
}

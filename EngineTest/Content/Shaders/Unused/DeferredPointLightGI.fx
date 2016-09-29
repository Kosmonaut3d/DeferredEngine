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
    float4 Color : COLOR0;
    float4 LightReflection : COLOR1;
    float4 LightDepth : COLOR2;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    //processing geometry coordinates
    //float4 worldPosition = mul(float4(input.Position, 1), World);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);
    output.Position = input.Position;
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

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : SV_Target
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
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    //get specular power
    float f0 = normalData.a;
    //get specular intensity from the colorMap
    float4 color = tex2D(colorSampler, texCoord);

    float roughness = color.a;
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

    float lightDepth = length(lightVector);
    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - lightDepth / lightRadius) * lightIntensity;
    //normalize light vector
    lightVector = normalize(lightVector);
    //compute diffuse light
    float NdL = saturate(dot(normal, lightVector));

    float3 diffuseLight = NdL * lightColor.rgb;
    
    float3 specular = SpecularCookTorrance(NdL, normal, lightVector, normalize(cameraPosition - position.rgb), lightIntensity, lightColor, f0, roughness);
    //take into account attenuation and lightIntensity.

    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specularLight);
    PixelShaderOutput Out;

    Out.Color = float4(color.rgb * (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1) * 0.5f + specular * attenuation, 1);

    Out.LightReflection.rgb = reflect(lightVector, normal); // * saturate(NdotL + (1-roughness))

    //Out Reflection strength
    Out.LightReflection.a = roughness; //saturate(NdotL + (1-roughness))

    Out.LightDepth = lightDepth/100;

    return Out;

}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

float4x4 World;
float4x4 View;
float4x4 Projection;

int samples = 30;

float2 resolution_inverse = float2(1 / 1280.0f, 1 / 800.0f);

//color of the light 
float3 lightColor;

//this is used to compute the world-position
float4x4 InvertViewProjection;
//this is the position of the light
float3 lightPosition;
//how far does this light reach
float lightRadius;
//control the brightness of the light
float lightIntensity = 1.0f;
// diffuse color, and specularIntensity in the alpha channel

// normals, and specularPower in the alpha channel

//depth
Texture2D<float4> colorMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
    
Texture2D<float4> depthMap;
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU =CLAMP;
    AddressV = CLAMP;
    MagFilter =POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture lightDepthMap;
sampler LightDepthSampler = sampler_state
{
    Texture = (lightDepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture lightReflectionMap;
sampler LightReflectionSampler = sampler_state
{
    Texture = (lightReflectionMap);
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



float zNear = 1;
float zFar = 300;
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

float linearDepth(float depthSample)
{
    depthSample = 2.0 * depthSample - 1.0;
    float zLinear = 2.0 * zNear * zFar / (zFar + zNear - depthSample * (zFar - zNear));
    return zLinear;
}

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


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
    //obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
                              
    float4 color = colorMap.Sample(colorSampler, texCoord);


    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = 1-depthMap.Sample(depthSampler, texCoord).r;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    float3 output = 0;
    float distance = 0;

    for (int x = 0; x < samples; ++x)
    {
        for (int y = 0; y < samples; ++y)
        {
            int x_i = x - samples / 2;
            int y_i = y - samples / 2;
                 
            float2 sampleCoord = texCoord + float2(x_i, y_i)*resolution_inverse*10;

            float4 samplePosition;

            samplePosition.xy = sampleCoord;
            samplePosition.w = 1.0f;

            samplePosition.z = 1-depthMap.Sample(depthSampler, sampleCoord).r;

            samplePosition = mul(samplePosition, InvertViewProjection);
            samplePosition /= samplePosition.w;

            float sampleLightDepth = tex2D(LightDepthSampler, sampleCoord).r * 100;

            float3 sampleColor = colorMap.Sample(colorSampler, sampleCoord).rgb * lightColor;

            float3 DirectionVector = samplePosition.xyz - position.xyz;

            float distance = length(DirectionVector);

            float3 sampleReflectance = tex2D(LightReflectionSampler, sampleCoord);

            float nDotR = saturate(dot(normalize(DirectionVector), sampleReflectance));

            nDotR *= nDotR;

            float totaldistanceToLight = sampleLightDepth + distance;

            float attenuation = saturate(1.0f - totaldistanceToLight / lightRadius)* lightIntensity * nDotR;

            float variance = abs(sampleColor.r - sampleColor.g) + abs(sampleColor.r - sampleColor.b) + abs(sampleColor.b - sampleColor.g) + 1;

            float variance2 = dot(abs(sampleColor - color.rgb), float3(1, 1, 1));

            output += sampleColor * attenuation * variance*variance2;   
            
            //exaggerate if colors are not uniform! Hehehehe
            


            //Get the position at the sampled point

            //float4 sampleColor = colorMap.Sample(colorSampler, sampleCoord);

            //output += sampleColor;


        }
    }
    output =  output / (samples * samples)*2;

    //return float4(output, 1);
    return color + float4(output,1);

    //return float4(distance, distance, distance, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

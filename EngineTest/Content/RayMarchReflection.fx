float4x4 InvertViewProjection;
float4x4 ViewProjection;
float4x4 Projection;
Texture2D colorMap;

float3 cameraPosition;
float3 cameraDir;

float zNear = 1;
float zFar = 300;

#define STEPS 40

Texture2D normalMap;
//depth
Texture2D depthMap;
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
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 viewRay : VIEWRAY;
    float2 TexCoord : TEXCOORD0;
};




////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    output.viewRay = cameraPosition - input.Position;
    output.TexCoord = input.TexCoord;
    return output;
}

float linearDepth(float depthSample)
{
    depthSample = 2.0 * depthSample - 1.0;
    float zLinear = 2.0 * zNear * zFar / (zFar + zNear - depthSample * (zFar - zNear));
    return zLinear;
}

float3 rayTrace(float3 reflectionVector, float startDepth, float2 vert_UV)
{

    float3 color = float3(0, 0, 0);
    float stepSize = 0.0005f;

    float size = length(reflectionVector.xy);

    reflectionVector = normalize(reflectionVector / size);
    reflectionVector = reflectionVector * stepSize;

    float2 sampledPosition = vert_UV;

    float currentDepth = startDepth;

    [unroll]
    for (int i = 0; i < STEPS; i++)
    {
        if
        (sampledPosition.x > 1.0 || sampledPosition.x < 0.0 ||
        sampledPosition.y > 1.0 || sampledPosition.y < 0.0)
            break;
        sampledPosition = sampledPosition + reflectionVector.xy;

        currentDepth = currentDepth - reflectionVector.z;

        float sampledDepth = linearDepth(1-depthMap.Sample(depthSampler, sampledPosition).r);

        if (currentDepth > sampledDepth)
        {
            
            float delta = (currentDepth - sampledDepth);
            if (delta >  0.003f)
            {
                color = colorMap.Sample(colorSampler, sampledPosition).rgb;
                break;
                
            }

        }
    }
    return color;

}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 diffuseColor = colorMap.Sample(colorSampler, input.TexCoord).rgb;
    /*float3 diffuseColor = colorMap.Sample(colorSampler, input.TexCoord).rgb;
    float depth = depthMap.Sample(depthSampler, input.TexCoord).r;
    float3 normal = normalMap.Sample(normalSampler, input.TexCoord).rgb;

      */
    //float3 origin = mul(float4(input.Position), InvertViewProjection).xyz;
    
    ////float3 ray = normalize(reflect(input.viewRay, normal));

    //float cameraToWorld = origin- cameraPosition;

    ////float scaleNormal = max(3.0f, cameraToWorld * 1.5f);

    //float3 cameraToWorldNorm = normalize(cameraToWorld);

    //float3 refl = normalize(reflect(cameraToWorldNorm, normal));

    //if(dot(refl, cameraToWorldNorm) < 0.5f )
    //{
    //    return float4(input.viewRay.xy, depth, 1);
    //}

    ////CameraDirectioN!
    //float3 cameraDirection = normalize(cameraPosition - input.Position.xyz);

    //return float4(diffuseColor,1+depth);  */

    //Clip to the near plane

    float3 reflectedColor = float3(0, 0, 0);

    input.Position.xy /= input.Position.w;

    float3 normal = (normalMap.Sample(normalSampler, input.TexCoord).rgb *2)-1;

    float currDepth = 1 - depthMap.Sample(depthSampler, input.TexCoord).r; // linearDepth(1-depthMap.Sample(depthSampler, input.TexCoord).r);

    //float4 position;
    //position.xy =  input.Position.xy;
    //position.z = currDepth;
    //position.w = 1.0f;
    ////transform to world space
    //position = mul(position, InvertViewProjection);
    //position /= position.w;

    float3 cameraToWorldNorm = normalize(cameraDir);

    //float3 eyePosition = normalize(float3(0, 0, zNear));

    //float3 reflectionVector = mul(float4(reflect(-eyePosition, normal), 0), ViewProjection).xyz;

    float3 reflectionVector = mul(float4(reflect(cameraToWorldNorm, normal), 0), ViewProjection).xyz;
    
    reflectedColor = rayTrace(reflectionVector.xyz, linearDepth(currDepth), input.TexCoord);

    //float Ndot = saturate(dot(cameraToWorldNorm, normal));

    float luma = 0.3 * diffuseColor.r + 0.6 * diffuseColor.g + 0.1 * diffuseColor.b;

    return float4(luma, luma, luma, 0) + float4(reflectedColor, 1);
}


technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

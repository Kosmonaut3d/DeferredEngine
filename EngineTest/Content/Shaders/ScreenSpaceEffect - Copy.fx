
float3 CameraPosition;
//this is used to compute the world-position
float4x4 InverseViewProjection;
float4x4 Projection;
float4x4 ViewProjection;

#include "helper.fx"

Texture2D NormalMap;
Texture2D DepthMap;
Texture2D TargetMap;
SamplerState texSampler
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

       
float FalloffMin = 0.000001f;
float FalloffMax = 0.002f;
  
int Samples = 8;

float Strength = 4;

float SampleRadius = 0.05f;



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
    float3 viewDirWS : TEXCOORD1;
    float3 viewDirVS : TEXCOORD2;
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
    output.viewDirVS = input.Position.xyz;
    output.viewDirWS = normalize(mul(normalize(output.Position), InverseViewProjection).xyz);
    return output;

}

float linearizeDepth(float depth)
{
    return (Projection._43 / (depth - Projection._33));
}

float localDepth(float lindepth)
{
    return (Projection._43 / lindepth) + Projection._33;
}

float3 randomNormal(float2 tex)
{
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f));
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f));
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f));
    return normalize(float3(noiseX, noiseY, noiseZ));
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    

    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(texSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normalWS = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    float depthVal = linearizeDepth( 1-DepthMap.Sample(texSampler, texCoord).r);
    
    
    // //obtain screen position
    //float4 positionVS;
    //positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
    //positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);

    ////compute screen-space position
    //positionVS.z = depthVal;
    //positionVS.w = 1.0f;
    ////transform to world space        s
    //float4 positionWS = mul(positionVS, InverseViewProjection);
     
    //positionWS /= positionWS.w;

    //positionWS += float4(normalWS, 0);


    //float4 positionVS2 = mul(positionWS, ViewProjection);

    //positionVS2 /= positionVS2.w;

    float3 normalVS = mul(float4(normalWS, 0), ViewProjection).xyz;

    //float3 normalsWS = mul(float4(normalsVS), InverseViewProjection).xyz;
      
    //float result = dot(normalize(-input.viewDirWS), normalWS);

    //float result = (-linearizeDepth(depthVal) - 0.5f) * 100;

    //float2 sampleTexCoord = 0.5f * (float2(positionVS2.x, -positionVS2.y) + 1)
    //normalsVS *= input.viewDirVS;

    normalVS = normalize(normalVS);

    float3 randNor = randomNormal(input.TexCoord); //

    const float3 sampleSphere[] =
    {
        float3(0.2024537f, 0.841204f, -0.9060141f),
        float3(-0.2200423f, 0.6282339f, -0.8275437f),
        float3(0.3677659f, 0.1086345f, -0.4466777f),
        float3(0.8775856f, 0.4617546f, -0.6427765f),
        float3(0.7867433f, -0.141479f, -0.1567597f),
        float3(0.4839356f, -0.8253108f, -0.1563844f),
        float3(0.4401554f, -0.4228428f, -0.3300118f),
        float3(0.0019193f, -0.8048455f, 0.0726584f),
        float3(-0.7578573f, -0.5583301f, 0.2347527f),
        float3(-0.4540417f, -0.252365f, 0.0694318f),
        float3(-0.0483353f, -0.2527294f, 0.5924745f),
        float3(-0.4192392f, 0.2084218f, -0.3672943f),
        float3(-0.8433938f, 0.1451271f, 0.2202872f),
        float3(-0.4037157f, -0.8263387f, 0.4698132f),
        float3(-0.6657394f, 0.6298575f, 0.6342437f),
        float3(-0.0001783f, 0.2834622f, 0.8343929f),
    };

    //float3 reflection = reflect(input.viewDirVS, normalVS);

    float3 pos = float3(input.TexCoord, depthVal);

    float result = 0;

    float radius = SampleRadius * (1-depthVal);

    for (uint i = 0; i < Samples; i++)
    {
        float3 offset = reflect(sampleSphere[i], randNor);
                
        //reverse the sign if the normal is looking backward
        offset = sign(dot(offset, normalVS)) * offset; //
        offset.y = -offset.y; 
        float3 ray = pos + offset * radius;

        //outside of view
        if ((saturate(ray.x) != ray.x) || (saturate(ray.y) != ray.y))
            continue;


        //sample

        float depthSample = linearizeDepth(1 - DepthMap.Sample(texSampler, ray.xy).r);

        float depthDiff = (depthVal - depthSample);

        float occlusion = depthDiff * (1-depthVal);

        float falloff = 1 - saturate(depthDiff * (1-depthVal) - FalloffMin) / (FalloffMax-FalloffMin);
        
        occlusion *= falloff;

        result += occlusion;
        
    }

    //float3 offset2 = reflect(sampleSphere[1], randNor);

    result /= Samples;
    result = saturate(1 - result * Strength * 200);

    return float4(result,result,result, 1);
}

technique Classic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

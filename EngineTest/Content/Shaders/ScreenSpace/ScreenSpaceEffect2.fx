
#include "helper.fx"

float4x4 ViewProjection;
float4x4 InverseViewProjection;

Texture2D DepthMap;
Texture2D TargetMap;
Texture2D NormalMap;
SamplerState texSampler
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
 
float zfar = 500;
float znear = 1;

float2 resolution = float2(1280, 800);

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

float linearizeDepth(float z)
{
    float zfar_2 = zfar / (zfar - znear);

    float z0 = z * zfar_2 - znear * zfar_2;

    float w0 = z;

    float native_z = z0 / w0;

    float linZ = (znear * zfar_2 / (zfar_2 - native_z));

    return linZ;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    
    float4 positionVS;
    positionVS.x = input.TexCoord.x * 2.0f - 1.0f;
    positionVS.y = -(input.TexCoord.y * 2.0f - 1.0f);

    float2 texCoord = float2(input.TexCoord);
    
    float depthVal = 1 - DepthMap.Sample(texSampler, texCoord).r;

    float4 normalData = NormalMap.Sample(texSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz);

    ////compute screen-space position

    //linDepth
    //float linDepth = 1 + (Projection._43 / (depthVal - Projection._33));

    //RealSpace
    positionVS.w = 1.0f;
    positionVS.z = depthVal;
    float4 positionWS = mul(positionVS, InverseViewProjection);
    positionWS /= positionWS.w;

    float3 incident = normalize(input.viewDirWS);
    

    float3 reflectVector = reflect(incident, normal);
    // go

    float4 samplePositionWS = positionWS + float4(reflectVector, 0);
                                               
    float4 samplePositionVS = mul(samplePositionWS, ViewProjection);

    samplePositionVS /= samplePositionVS.w;

    ////////////////////////////////

    float4 viewOffset = samplePositionVS - positionVS;

    float zOffsetLinear = linearizeDepth(samplePositionVS.z) - linearizeDepth(positionVS.z);

    /////////////////////////////////

    float4 output = float4(0, 0, 0, 0);
           
    float oldZ = 0;

    uint samples = 20;
    [unroll]
    for (uint i = 0; i < samples; i++)
    {
        //If the normal goes in our direction abort
        if (viewOffset.z < 0)
        {
            //output = float4(1, 0, 0, 1);
            break;
        }


        //float lerpDelta = i;
        //float delta = lerp(1, 20, lerpDelta / samples);

        float delta = i + 1;

        //march in the given direction
        samplePositionVS = //positionVS + viewOffset * i;
        float4(positionVS.xyz + viewOffset.xyz * delta, 0);

        float2 sampleTexCoord = 0.5f * (float2(samplePositionVS.x, -samplePositionVS.y) + 1);

        if (sampleTexCoord.x < 0 || sampleTexCoord.y < 0 || sampleTexCoord.x > 1 || sampleTexCoord.y > 1)
        {
            break;
        }

        float sampleDepthVal = linearizeDepth( 1 - DepthMap.Sample(texSampler, sampleTexCoord).r);

        if (sampleDepthVal < oldZ)
        {
            break;
        }

        [branch]
        if (sampleDepthVal < linearizeDepth(samplePositionVS.z))
        {
            int3 texCoordInt = int3(sampleTexCoord * resolution, 0);
            float4 albedoColor = TargetMap.Load(texCoordInt);

            output = albedoColor;
            output.a = 1;

            float border = 0.1f;

            [branch]
            if (sampleTexCoord.y > 0.9f)
            {
                output.a = lerp(1, 0, (sampleTexCoord.y - 0.9) * 10);
            }
            else if (sampleTexCoord.y < 0.1f)
            {
                output.a = lerp(0, 1, sampleTexCoord.y * 10);
            }
            [branch]
            if (sampleTexCoord.x > 0.9f)
            {
                output.a *= lerp(1, 0, (sampleTexCoord.x - 0.9) * 10);
            }
            else if (sampleTexCoord.x < 0.1f)
            {
                output.a *= lerp(0, 1, sampleTexCoord.x * 10);
            }
            
            output.rgb *= output.a;

            break;
        }

    }
    return output;

    //float2 texCoord = float2(input.TexCoord);
    
    ////get normal data from the NormalMap
    //float4 normalData = NormalMap.Sample(texSampler, texCoord);
    //////tranform normal back into [-1,1] range
    //float3 normalWS = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    //float depth =  linearizeDepth(1-DepthMap.Sample(texSampler, texCoord).r);
               
    //float result = depth;

    //float result2 = 0;

    //float realdepth = depth * 499;

    //for (uint i = 1; i < 10; i++)
    //{
    //   if(result > 2*i/10.0f && result < (2*i+1)/10.0f)
    //    {
    //        result2 = depth;
    //    }
    //}

    ////float3 normalVS = mul(float4(normalWS, 0), ViewProjection).xyz;

    ////float3 dir = reflect(input.viewDirVS, normalVS);

    //float3 reflection = reflect(input.viewDirWS, normalWS);

    //float4 dir = mul(float4(reflection,0), ViewProjection);

    //dir /= dir.w;

    //float3 origin = float3(texCoord, depth);

    //float radius = 0.05f;

    //uint samples = 10;
                 
    //float3 target = float3(0, 0, 0);

    //float oldZ = origin.z;

    //for (uint i = 0; i < samples; i++)
    //{
    //    float3 ray = origin + dir.xyz * radius * (i+1);

    //    //if (ray.z > oldZ)
    //    //    break;

    //    float depthSample = linearizeDepth(1 - DepthMap.Sample(texSampler, ray.xy).r);

    //    if (depthSample > depth)
    //    {
    //        target = TargetMap.Sample(texSampler, ray.xy).xyz;
    //        break;
    //    }
    //    oldZ = ray.z;
    //}

    //target *= 0.001f;
    //target += dir;

    ////float3 normalsWS = mul(float4(normalsVS), InverseViewProjection).xyz;
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


      
    //float result = dot(normalize(-input.viewDirWS), normalWS);

    //float result = (-linearizeDepth(depthVal) - 0.5f) * 100;

    //float2 sampleTexCoord = 0.5f * (float2(positionVS2.x, -positionVS2.y) + 1)
    //normalsVS *= input.viewDirVS;


}

technique Classic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

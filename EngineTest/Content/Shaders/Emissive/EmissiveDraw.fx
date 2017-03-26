////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
//  EMISSIVE DRAW, not used right now. Not converted to View space pipeline yet.

matrix World;
matrix WorldViewProj;
//Different ViewPort maybe?
matrix ViewProjection;
matrix InvertViewProjection;

float2 Resolution = float2(1280, 800);

float3 CameraPosition;

float3 Origin;
float Size;

float EmissiveStrength = 1;

float3 EmissiveColor = float3(1, 1, 1);

float Time = 1;

static float3 sampleSphere[] =
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

Texture2D EmissiveMap;
Texture2D DepthMap;
Texture2D NormalMap;
SamplerState PointSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

SamplerState LinearSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = POINT;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct DrawBasic_VSOut
{
    float4 Position : SV_POSITION0;
    float4 WorldPos : TEXCOORD;
};

//Deferred

struct VertexShaderInput
{
    float4 Position : POSITION0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
    //float3 viewDirection : TEXCOORD1;
};

struct VertexShaderOutputSpecular
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
    float3 viewDir : TEXCOORD1;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS


//Draw to map
DrawBasic_VSOut DrawBuffer_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
    Output.WorldPos = mul(input.Position, World);
    return Output;
}


float4 DrawBuffer_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    //Draw the world position!
    float3 output = input.WorldPos.xyz - Origin;

    //normalize
    output /= Size;

    output += float3(1, 1, 1);

    output /= 2;

    return float4(output, 1);
}

//Draw EmissiveEffect

VertexShaderOutput DrawEffectDiffuse_VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    //processing geometry coordinates
    output.Position = mul(input.Position, WorldViewProj);
    //WTF???? Need to check why I have this in
    output.ScreenPosition = output.Position;

    return output;
}

VertexShaderOutputSpecular DrawEffectSpecular_VertexShader(VertexShaderInput input)
{
    VertexShaderOutputSpecular output;
    //processing geometry coordinates
    output.Position = mul(input.Position, WorldViewProj);
    //WTF???? Need to check why I have this in
    output.ScreenPosition = output.Position;
    
    output.viewDir = normalize(mul(output.Position, InvertViewProjection).xyz);

    return output;
}

float3 randomNormal(float2 tex)
{
    tex *= Time;
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f));
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f));
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f));
    return normalize(float3(noiseX, noiseY, noiseZ));
}

float3 decode(float3 n)
{
    return 2.0f * n.xyz - 1.0f;
}


float lengthSquared(float3 v1)
{
    return dot(v1, v1);
}

float4 DrawEffectSpecular_PixelShader(VertexShaderOutputSpecular input) : SV_Target
{
     
    float4 output = float4(0, 0, 0, 0);

     //obtain screen position
    input.ScreenPosition.xyz /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    int3 texCoordInt = int3(texCoord * Resolution, 0);

    //read depth
    float depthVal = 1 - DepthMap.Sample(PointSampler, texCoord).r;

    //compute world space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    [branch]
    if (lengthSquared(position.xyz - Origin) > Size * Size)
        return output;
    else
    {
        
        float4 positionTransformed = mul(float4(position), ViewProjection);
        positionTransformed /= positionTransformed.w;

        float2 texCoordTransformed = 0.5f * (float2(positionTransformed.x, -positionTransformed.y) + 1);

        //get normal data from the NormalMap
        float4 normalData = NormalMap.Load(texCoordInt);
        //tranform normal back into [-1,1] range
        float3 normal = decode(normalData.xyz);

        float roughness = normalData.a;

        float3 viewDir = normalize(position.xyz - CameraPosition);

    
        float3 randNor = randomNormal(texCoord); //

        uint Samples = 8;

        float emissiveContribution = 0;

        [unroll]
        for (uint i = 0; i < Samples; i++)
        {
            float3 offset = reflect(sampleSphere[i], randNor);

            //Hemisphere
            if (dot(normal, offset) < 0)
                offset = -offset;

            offset = normalize(normal * (1 - roughness) * 10 + offset);

            offset = reflect(viewDir, offset);

            //These vectors are projected from the origin

            float3 endpoint = position.xyz + offset * Size / 3;

            float3 vectorDirection = endpoint - position.xyz;

            float3 endpointadjusted = position.xyz + vectorDirection * 1.2f;

            vectorDirection = normalize(vectorDirection);

            float4 samplePositionVS = mul(float4(endpointadjusted, 1), ViewProjection);

            samplePositionVS /= samplePositionVS.w;

            float2 sampleTexCoord = 0.5f * (float2(samplePositionVS.x, -samplePositionVS.y) + 1);

            [unroll]
            for (uint j = 0; j < 4; j++)
            {
                float2 ray = texCoordTransformed + (sampleTexCoord - texCoordTransformed) * j / 4.0f;

                //Read the sample Position!
                int3 texCoordRay = int3(ray * Resolution, 0);

                float4 posEmissive = EmissiveMap.Load(texCoordRay);

                //translate to real position

                //We haven't found anything
                [branch]
                if (posEmissive.a < 1)
                {
                    continue;
                }
                else
                {
                    float3 realPosition = (((posEmissive.xyz * 2) - float3(1, 1, 1)) * Size + Origin);
                                                 
                    float3 foundVectorDirection = realPosition - position.xyz;

                    float vectorLength = length(foundVectorDirection);

                    float dist = saturate(1 - vectorLength / Size * 1.2f);
                
                    //note that a new vectorDirection should be used here.
                    float normalfactor = saturate(dot(normal, foundVectorDirection / vectorLength));
                    //float normalfactor = saturate(dot(normal, vectorDirection));
                    float normalfactor2 = -(normalfactor - 1) * (normalfactor - 1) + 1;

                    emissiveContribution += dist * dist * normalfactor2;

                    break;
                }
            }
        }
        emissiveContribution /= Samples;

        return float4(EmissiveColor * emissiveContribution * EmissiveStrength * 0.05f, 1);
    }
}


float4 DrawEffectDiffuse_PixelShader(VertexShaderOutput input) : SV_Target
{
    float4 output = float4(0, 0, 0, 0);

     //obtain screen position
    input.ScreenPosition.xyz /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    int3 texCoordInt = int3(texCoord * Resolution, 0);
    /////////////////////////////////////////

    //read depth
    float depthVal = 1 - DepthMap.Sample(PointSampler, texCoord).r;

    //compute world space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    //early depth rejection
    [branch]
    if (lengthSquared(position.xyz - Origin) > Size * Size)
        return output;
    else
    {
        //Project into Emissive View Space
        float4 positionTransformed = mul(float4(position), ViewProjection);
        positionTransformed /= positionTransformed.w;
        float2 texCoordTransformed = 0.5f * (float2(positionTransformed.x, -positionTransformed.y) + 1);
          
        //get normal data from the NormalMap
        float4 normalData = NormalMap.Load(texCoordInt);
        //tranform normal back into [-1,1] range
        float3 normal = decode(normalData.xyz);

        float3 randNor = randomNormal(texCoord + position.yz); //

        float emissiveContribution = 0;

        uint Samples = 16;

        [unroll]
        for (uint i = 0; i < Samples; i++)
        {
            float3 offset = reflect(sampleSphere[i], randNor);

            float3 endpoint = Origin + offset * Size / 4 / EmissiveStrength;

            float3 vectorDirection = endpoint - position.xyz;

            if (dot(normal, vectorDirection) < 0)
                continue;
                //vectorDirection = -vectorDirection;
            
            float3 endpointadjusted = position.xyz + vectorDirection * 1.2f;

            vectorDirection = normalize(vectorDirection);

            float4 samplePositionVS = mul(float4(endpointadjusted, 1), ViewProjection);

            samplePositionVS /= samplePositionVS.w;

            float2 sampleTexCoord = 0.5f * (float2(samplePositionVS.x, -samplePositionVS.y) + 1);

            [unroll]
            for (uint j = 0; j < 8; j++)
            {
                float2 ray = texCoordTransformed + (sampleTexCoord - texCoordTransformed) * j / 8.0f;

                if (ray.x < 0 || ray.x > 1 || ray.y < 0 || ray.y > 1)
                    break;

                //Read the sample Position!
                int3 texCoordRay = int3(ray * Resolution, 0);
                float4 posEmissive = EmissiveMap.Load(texCoordRay);

                //We haven't found anything  - note: We should check depth against the position and reject based on that!
                [branch]
                if (posEmissive.a < 1)
                {
                    continue;
                }
                else
                {
                    float3 realPosition = (((posEmissive.xyz * 2) - float3(1, 1, 1)) * Size + Origin);

                    float dist = saturate(1 - distance(position.xyz, realPosition) / Size * 1.2f);

                    float normalfactor = saturate(dot(normal, vectorDirection));

                    float normalfactor2 = -(normalfactor - 1) * (normalfactor - 1) + 1;

                    emissiveContribution += dist * normalfactor;

                    break;
                }
            }
        }
        emissiveContribution /= Samples;

        return float4(EmissiveColor * emissiveContribution * EmissiveStrength * 0.05f, 1);
    }
}

technique DrawEmissiveBuffer
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBuffer_VertexShader();
        PixelShader = compile ps_5_0 DrawBuffer_PixelShader();
    }
}

technique DrawEmissiveSpecularEffect
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawEffectSpecular_VertexShader();
        PixelShader = compile ps_5_0 DrawEffectSpecular_PixelShader();
    }
}

technique DrawEmissiveDiffuseEffect
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawEffectDiffuse_VertexShader();
        PixelShader = compile ps_5_0 DrawEffectDiffuse_PixelShader();
    }
}
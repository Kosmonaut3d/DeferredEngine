
//Depth Reconstruction from linear depth buffer, TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define MAXLIGHTS 40
#define MAXLIGHTSPERTILE 40

#include "../Common/helper.fx"

float2 Resolution = { 1280.0, 720.0 };

float4x4  World;
float4x4  WorldViewProj;
float3x3  WorldViewIT;

//Light

int LightAmount = 0;

float3 LightPositionWS[MAXLIGHTS];
float LightRadius[MAXLIGHTS];
float LightIntensity[MAXLIGHTS];
float3 LightColor[MAXLIGHTS];

float cols = 20.0f;
float rows = 10.0f;
/*uint*/float TiledListLength[200];

float3 CameraPositionWS;

const float OUTPUTCONST = 0.1f;

SamplerState texSampler
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
 
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 PositionWS : TEXCOORD0;
    float3 Normal : NORMAL;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  VERTEX SHADER
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProj);
    output.PositionWS = mul(input.Position, World).xyz;
    output.Normal = mul(input.Normal, WorldViewIT); //mul(float4(input.Normal,0), World).xyz;

    return output;

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  PIXEL SHADER
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

float GetTileIndex(float2 texCoord)
{
    float row = trunc(rows * texCoord.y);
    float col = trunc(cols * texCoord.x);

    return row * cols + col;
}

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  Main function
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal);

    //Pre-determined look of object
    float3 diffuseColor = float3(0.5f, 0.5f, 0.5f);

    diffuseColor = pow(abs(diffuseColor), 2.2f);

    const float roughness = 0.2f;
     
    const float metalness = 0.1f;

    float f0 = lerp(0.04f, diffuseColor.g * 0.25 + 0.75, metalness);

    float3 cameraDirection = normalize(CameraPositionWS - input.PositionWS);

    //Lighting////////////////////////////

    float3 diffuseLight = float3(0, 0, 0);
    float3 specularLight = float3(0, 0, 0);

    for (int i = 0; i < LightAmount; i++)
    {
        float3 lightVector = LightPositionWS[i] - input.PositionWS;
        float lightDistance = length(lightVector);
        float lightRadius = LightRadius[i];

        if (lightDistance >= lightRadius) continue;

        float lightIntensity = LightIntensity[i];
        float3 lightColor = LightColor[i];

        //compute attenuation based on distance - linear attenuation
        //float attenuation = saturate(1.0f - distanceLtoR / lightRadius);
        float x = lightDistance / lightRadius; //normalized
        float bottom = (4 * x + 1);
        float attenuation = saturate(1 / (bottom*bottom) - 0.04*x);

        //Normalize
        lightVector /= lightDistance;

        float NdL = saturate(dot(normal, lightVector));

        //diffuseLight += NdL * lightIntensity * lightColor * attenuation * OUTPUTCONST;

        if (metalness < 0.99)
        {
            diffuseLight += DiffuseOrenNayar(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, roughness) * (1-f0) * attenuation * OUTPUTCONST;
        }
        specularLight += SpecularCookTorrance(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, f0, roughness) * attenuation * OUTPUTCONST;
    }

    //Compose//////////////////////////////////

    specularLight = clamp(specularLight, 0, 1000);

    float3 plasticFinal = diffuseColor.rgb * (diffuseLight)+specularLight;

    float3 metalFinal = specularLight * diffuseColor.rgb;

    float3 finalValue = lerp(plasticFinal, metalFinal, metalness);

    //Shader effect

    float NdC = 1 - saturate(dot(normal, cameraDirection));

    finalValue = lerp( finalValue, finalValue + float3(0.2f, 0.1f, 0.1f), NdC * NdC * NdC);

    //Show normals
    //finalValue = (normal + float3(1,1,1))*0.5f;

    float2 texCoord = input.Position.xy / Resolution;

    //Tiled
    /*float listLength = TiledListLength[GetTileIndex(texCoord)];

    finalValue.rgb = listLength/4.0f;*/

    return float4(finalValue, 0.6f);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  TECHNIQUES
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}


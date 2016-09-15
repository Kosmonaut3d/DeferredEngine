//Lightshader Bounty Road 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
//      PROJECTION  

matrix  View;
matrix  World;
matrix  Projection;
matrix  WorldViewProj;

float4 FogColor = float4(1, 0.375, 0, 1);

float2 InverseResolution = (1.0f/1280.0f, 1.0f/800.0f); 

#define SAMPLE_COUNT 9
static float2 SampleOffsets[9] = 
{
    float2(-1, -1), float2(0, -1), float2(1, -1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, 1), float2(0, 1), float2(1, 1)
};

static float SampleWeights[9] = 
{
    0.077847f, 
    0.123317f, 
    0.077847f,
    0.123317f, 
    0.195346f,
    0.123317f,
    0.077847f, 
    0.123317f, 
    0.077847f,
};

//      Light

float3 LightPosition;
float3 LightDirection;
float3 LightColor;
float LightIntensity;
float LightRadius;

#define POINTLIGHTAMOUNT 20

int lowerBound = 0;

float3 PointLightPosition[POINTLIGHTAMOUNT];
float3 PointLightColor[POINTLIGHTAMOUNT];
float PointLightIntensity[POINTLIGHTAMOUNT];
float PointLightRadius[POINTLIGHTAMOUNT];

float3 CameraPosition;
float3 CameraDirection;

TextureCube ReflectionCubeMap;
sampler ReflectionCubeMapSampler = sampler_state
{
    texture = <ReflectionCubeMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};



//      MATERIAL
float   Roughness = 0.1f; // 0 : smooth, 1: rough
float   F0 = 0.5f;


float CLIP_VALUE = 0.99;

float4 DiffuseColor = float4(1,1,1, 1);

Texture2D colorMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

Texture2D<float4> Specular;
sampler SpecularTextureSampler
{
    Texture = (Specular);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;

    AddressU = Wrap;
    AddressV = Wrap;
};

Texture2D<float4> Depth;
sampler DepthSampler
{
    Texture = (Mask);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;

    AddressU = Wrap;
    AddressV = Wrap;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
	float4 Position : SV_POSITION0;
	float3 Normal   : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct DrawBasic_VSOut
{
    float4 ScreenPosition : POSITION0;
    float4 WorldPos : TEXCOORD3;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD1;
    float2 Depth : TEXCOORD2;
};

struct Render_IN
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0;
    float2 Depth : DEPTH;
    float f0 : TEXCOORD1;
    float roughness : TEXCOORD2;
    float2 TexCoord : TEXCOORD3;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

struct LightStruct
{
    float3 Diffuse : COLOR0;
    float3 Specular : COLOR1;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
DrawBasic_VSOut DrawBasic_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSOut Output;
 
    Output.Normal = mul(float4(input.Normal, 0), World);

    //input.Position.xyz += refract(CameraDirection, Output.Normal, 0.66) * 0.002;

    Output.WorldPos = mul(float4(input.Position), World);
    float4 viewPosition = mul(Output.WorldPos, View);
    float4 PositionVS = mul(viewPosition, Projection);


    Output.ScreenPosition = PositionVS;
    Output.Depth.x = PositionVS.z;
    Output.Depth.y = PositionVS.w;
    return Output;
}

float3 SpecularCookTorrance(float NdotL, float3 normal, float3 negativeLightDirection, float3 cameraDirectionP, float diffuseIntensity, float3 diffuseColor, float f0, float roughness)
{
    float3 specular = float3(0, 0, 0);

    [branch]
    if (NdotL > 0.0f)
    {
        float3 halfVector = normalize(negativeLightDirection + cameraDirectionP);

        float NdotH = saturate(dot(normal, halfVector));
        float NdotV = saturate(dot(normal, cameraDirectionP));
        float VdotH = saturate(dot(cameraDirectionP, halfVector));
        float mSquared = roughness * roughness;


        //Trowbridge-Reitz
        float D_lowerTerm = (NdotH * NdotH * (mSquared * mSquared - 1) + 1);
        float D = mSquared * mSquared / (3.14 * D_lowerTerm * D_lowerTerm);

        //fresnel        (Schlick)
        float F = pow(1.0 - VdotH, 5.0);
        F *= (1.0 - f0);
        F += f0;

        //Schlick Smith
        float k = (roughness + 1) * (roughness + 1) / 8;
        float g_v = NdotV / (NdotV * (1 - k) + k);
        float g_l = NdotL / (NdotL * (1 - k) + k);

        float G = g_l * g_v;

        specular = max(0, (D * F * G) / (4 * NdotV * NdotL)) * diffuseIntensity * diffuseColor * NdotL; //todo check this!!!!!!!!!!! why 3.14?j only relevant if we have it 
        
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
    return L1 * lightColor * lightIntensity / 4;
}



float4 GaussianSampler(float2 TexCoord, float offset)
{
    float4 finalColor = float4(0, 0, 0, 0);
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        finalColor += colorMap.Sample(colorSampler, TexCoord.xy +
                    offset * SampleOffsets[i] * InverseResolution) * SampleWeights[i];
    }
   // finalColor = colorMap.Sample(colorSampler, TexCoord.xy);
    return finalColor;
}

float lengthSquared(float3 v1)
{
    return v1.x * v1.x + v1.y * v1.y + v1.z * v1.z;
}

LightStruct ComputePointLights(float3 worldPos, float3 normal )
{
    LightStruct Out;

    float3 diffusePoint = float3(0, 0, 0);
    float3 specularPoint = float3(0, 0, 0);

    float3 cameraDir = normalize(CameraDirection);
    [loop]
    for (int i = 0; i < lowerBound; i++)
    {
        float3 DirectionToLight = PointLightPosition[i] - worldPos;
                           
        float Distance = length(DirectionToLight);

        float radius = PointLightRadius[i];
             
        [branch]
        if (Distance < radius)
        {

            DirectionToLight /= Distance;

            float attenuation = saturate(1.0f - Distance / PointLightRadius[i]) * PointLightIntensity[i];

            float NdotLPoint = saturate(dot(DirectionToLight, normal));

            diffusePoint += DiffuseOrenNayar(NdotLPoint, normal, DirectionToLight, cameraDir, PointLightIntensity[i], PointLightColor[i], Roughness) * attenuation;
            
            specularPoint += SpecularCookTorrance(NdotLPoint, normal, DirectionToLight, -cameraDir, PointLightIntensity[i], PointLightColor[i], F0, Roughness) * attenuation;
            
        }
    }
    Out.Diffuse = diffusePoint;
    Out.Specular = specularPoint;
    return Out;
}


PixelShaderOutput Lighting(Render_IN input)
{
    
    PixelShaderOutput Out;
    
        float3 LightDir = normalize
    (LightPosition - input.Position.xyz);

    float spotReach = 0.75;
        float spotStrength = 
    saturate(dot(LightDir, LightDirection) - spotReach) * 1 / (1 - spotReach);

    //compute attenuation based on distance - linear attenuation
        float attenuation = 
        saturate(1.0f - length(LightPosition - input.Position.xyz) / LightRadius) * LightIntensity * spotStrength;

        float NdotL = saturate(dot(input.Normal, LightDir));

        float3 specular = SpecularCookTorrance(NdotL, -input.Normal, -LightDir, normalize(CameraDirection), LightIntensity, LightColor, F0, Roughness) * attenuation;

        float alpha = dot(specular, float3(0.33, 0.33, 0.33));

        float NdotC = saturate(dot(input.Normal, -CameraDirection));

    float3 refractVector = refract(CameraDirection, input.Normal, 0.66 - Roughness) * 0.1;

    float2 refraction = mul(float4(refractVector, 0), View * Projection).xy;

    float2 TexCoordRefract = input.TexCoord + refraction;

    float4 backgroundColorRefract = GaussianSampler(TexCoordRefract, Roughness*10) * NdotC;
    //colorMap.Sample(colorSampler, TexCoordRefract) * NdotC;


    float3 reflectionVector = reflect(CameraDirection, input.Normal);

    //float NdotC = saturate(dot(-incident, normal));

    reflectionVector.z = -reflectionVector.z;

    float4 backgroundColorReflect = ReflectionCubeMap.Sample(ReflectionCubeMapSampler, reflectionVector) * (1 - Roughness) * 0.4f;
    //float3 reflectVector = reflect(CameraDirection, input.Normal) * .5;

    //float2 reflection = mul(float4(reflectVector, 0), View * Projection).xy;

    //float2 TexCoordReflect = input.TexCoord - reflection;

    //float4 backgroundColorReflect = colorMap.Sample(colorSampler, TexCoordReflect) * (1-NdotC) * 0.5;

    float4 glassColor = backgroundColorReflect + backgroundColorRefract;

    glassColor.a = 0.5f;

    float Transparency = min(Roughness,0.05 * (1-NdotC));

    LightStruct pointLightContribution = ComputePointLights(input.Position.xyz, input.Normal);

    Out.Color = float4(input.Color.rgb *
    (DiffuseOrenNayar(NdotL, input.Normal, LightDir, normalize(CameraDirection), LightIntensity, LightColor, Roughness)
    + pointLightContribution.Diffuse)
     * 0.05f * attenuation, Transparency) * Transparency
    //Specular
    + glassColor * (1 - Transparency) + float4(specular, 0.8f) * 0.2f + float4(pointLightContribution.Specular, 0.8f);

   

    return Out;
}

PixelShaderOutput DrawBasic_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 outputColor = DiffuseColor; //* input.Color;

    //input.ScreenPosition.xy /= input.ScreenPosition.w;
    ////obtain textureCoordinates corresponding to the current pixel
    ////the screen coordinates are in [-1,1]*[1,-1]
    ////the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = float2(0.63f*input.ScreenPosition.x, input.ScreenPosition.y)* InverseResolution;
    //0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    //read depth
    float depthVal = Depth.Sample(DepthSampler, texCoord).r;

    float depth = 1 - input.Depth.x / input.Depth.y;

    if(depth < depthVal)
    {
        clip(-1);
        PixelShaderOutput output;
        output.Color = float4(depthVal,0, 0,1);
    }

    renderParams.Position = input.WorldPos;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = Roughness;
    renderParams.TexCoord = texCoord;

    return Lighting(renderParams);
}


technique GlassForward
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawBasic_PixelShader();
    }
}

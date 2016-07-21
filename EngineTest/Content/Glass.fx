//Lightshader Bounty Road 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
//      PROJECTION  

matrix  View;
matrix  World;
matrix  Projection;
matrix  WorldViewProj;

float4 FogColor = float4(1, 0.375, 0, 1);

float2 Resolution;

//      Light

float3 LightPosition;
float3 LightDirection;
float3 LightColor;
float LightIntensity;
float LightRadius;

float3 CameraPosition;
float3 CameraDirection;



//      MATERIAL
float   Roughness = 0.05f; // 0 : smooth, 1: rough
float   F0 = 0.8f;
float Transparency = 0.3f;

float CLIP_VALUE = 0.99;

float4 DiffuseColor = float4(1,1,1, 1);

Texture2D<float4> Texture : register(t0); 
sampler TextureSampler : register(s0)
{
    Texture = (Texture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;

    AddressU = Wrap;
    AddressV = Wrap;
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
    float4 Position : SV_POSITION0;
    float4 WorldPos : TEXCOORD3;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD1;
    float2 Depth : TEXCOORD2;
};

struct Render_IN
{
    float4 Position : SV_POSITION0;
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

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
DrawBasic_VSOut DrawBasic_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
    Output.Normal = mul(float4(input.Normal, 0), World).xyz;
    Output.WorldPos = mul(input.Position, World);
    Output.TexCoord = input.TexCoord;
    Output.Depth.x = Output.Position.z;
    Output.Depth.y = Output.Position.w;
    return Output;
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
    return L1 * lightColor * lightIntensity / 4;
}


PixelShaderOutput Lighting(Render_IN input)
{
    
    //Deferred MRT

    float depth = 1- input.Depth.x / input.Depth.y;

    float sampleDepth = Depth.Sample(DepthSampler, input.TexCoord).r;
    PixelShaderOutput Out;
    if (depth < sampleDepth+.002f)
    {
        Out.Color = float4(1, 1, 0, 1);
    }
    else
    {

        

        float3 LightDir = normalize
    (LightPosition - input.Position.xyz);

        float spotStrength = 1;
        saturate(dot(LightDir, LightDirection) - 0.75);

    //compute attenuation based on distance - linear attenuation
        float attenuation = 1;
        saturate(1.0f - length(LightPosition - input.Position.xyz) / LightRadius) * LightIntensity * spotStrength;

        float NdotL = saturate(dot(input.Normal, LightDir));

        float3 specular = SpecularCookTorrance(NdotL, -input.Normal, -LightDir, normalize(CameraDirection), LightIntensity, LightColor, F0, Roughness) * attenuation;

        float alpha = dot(specular, float3(0.33, 0.33, 0.33));

        float NdotC = saturate(dot(input.Normal, CameraDirection));

    

        Out.Color = float4(input.Color.rgb *
    DiffuseOrenNayar(NdotL, input.Normal, LightDir, normalize(CameraDirection), LightIntensity, LightColor, Roughness) * 0.1f * attenuation, Transparency * 4 * NdotC) + float4(specular, alpha) * 0.1f;

    }

    return Out;
}

PixelShaderOutput DrawBasic_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 outputColor = DiffuseColor; //* input.Color;



         
    renderParams.Position = input.WorldPos;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = Roughness;
    renderParams.TexCoord = input.TexCoord;

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

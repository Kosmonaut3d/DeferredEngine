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

#include "helper.fx"


//      MATERIAL
float   Roughness = 0.3f; // 0 : smooth, 1: rough
float   F0 = 0.05f;

int MaterialType = 0;

float CLIP_VALUE = 0.99;

float4 DiffuseColor = float4(0.8f, 0.8f, 0.8f, 1);

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

Texture2D<float4> Mask;
sampler MaskSampler
{
    Texture = (Mask);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;

    AddressU = Wrap;
    AddressV = Wrap;
};

Texture2D<float4> NormalMap;
sampler NormalMapSampler
{
    Texture = (NormalMap);

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
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD1;
    float2 Depth : TEXCOORD2;
};

struct DrawNormals_VSIn
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;
    float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
    float2 TexCoord : TEXCOORD0;
};

struct DrawNormals_VSOut
{
    float4 Position : SV_POSITION0;
    float3x3 WorldToTangentSpace : TEXCOORD3;
    float2 TexCoord : TEXCOORD1;
    float4 WorldPos : TEXCOORD2;
    float2 Depth : TEXCOORD0;
};

struct Render_IN
{
    float4 Position : SV_POSITION0;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0;
    float2 Depth : DEPTH;
    float f0 : TEXCOORD1;
    float roughness : TEXCOORD2;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
DrawBasic_VSOut DrawBasic_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
    Output.Normal = mul(float4(input.Normal, 0), World).xyz;
    Output.TexCoord = input.TexCoord;
    Output.Depth.x = Output.Position.z;
    Output.Depth.y = Output.Position.w;
    return Output;
}

DrawNormals_VSOut DrawNormals_VertexShader(DrawNormals_VSIn input)
{
    DrawNormals_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
    Output.WorldToTangentSpace[0] = mul(normalize(float4(input.Tangent, 0)), World).xyz;
    Output.WorldToTangentSpace[1] = mul(normalize(float4(input.Binormal, 0)), World).xyz;
    Output.WorldToTangentSpace[2] = mul(normalize(float4(input.Normal, 0)), World).xyz;
    Output.TexCoord = input.TexCoord;
    Output.Depth.x = Output.Position.z;
    Output.Depth.y = Output.Position.w;
    return Output;
}


PixelShaderOutput Lighting(Render_IN input)
{
    //float4 gamma_color = pow(abs(input.Color), 2.2f);

    //float4 finalValue = gamma_color*(float4(0.2,0.2,0.2,1));
                 
    float4 finalValue = input.Color;

    //Deferred MRT

    PixelShaderOutput Out;

    Out.Color = finalValue;

    Out.Color.a = encodeRoughnessMattype(input.roughness, MaterialType);

    //if (MaterialType == 1)
    //    clip(-1);

    Out.Normal.rgb =  encode(input.Normal); //

    Out.Normal.a = input.f0;

    Out.Depth = 1- input.Depth.x / input.Depth.y;

    return Out;
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTexture_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;
 
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = Roughness;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecular_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float4 RoughnessTexture = Specular.Sample(SpecularTextureSampler, input.TexCoord);
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = 1 - (RoughnessTexture.r+RoughnessTexture.b+RoughnessTexture.g) / 3;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecularNormal_PixelShader(DrawNormals_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float4 RoughnessTexture = Specular.Sample(SpecularTextureSampler, input.TexCoord);

    // NORMAL MAP ////
    float3 normalMap = 1 * NormalMap.Sample(NormalMapSampler, (input.TexCoord)).rgb - float3(0.5f, 0.5f, 0.5f);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = 1 - (RoughnessTexture.r+RoughnessTexture.b+RoughnessTexture.g) / 3;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureNormal_PixelShader(DrawNormals_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    // NORMAL MAP ////
    float3 normalMap = 1 * NormalMap.Sample(NormalMapSampler, (input.TexCoord)).rgb - float3(0.5f, 0.5f, 0.5f);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = Roughness;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureMask_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float mask = Mask.Sample(MaskSampler, input.TexCoord).r;
    if (mask < CLIP_VALUE)
        clip(-1);
 
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = Roughness;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecularMask_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float4 RoughnessTexture = Specular.Sample(SpecularTextureSampler, input.TexCoord);

    float mask = Mask.Sample(MaskSampler, input.TexCoord).r;
    if (mask < CLIP_VALUE)
        clip(-1);
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = 1 - (RoughnessTexture.r+RoughnessTexture.b+RoughnessTexture.g) / 3;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecularNormalMask_PixelShader(DrawNormals_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float4 RoughnessTexture = Specular.Sample(SpecularTextureSampler, input.TexCoord);

    float mask = Mask.Sample(MaskSampler, input.TexCoord).r;
    if (mask < CLIP_VALUE)
        clip(-1);

    // NORMAL MAP ////
    float3 normalMap = 1 * NormalMap.Sample(NormalMapSampler, (input.TexCoord)).rgb - float3(0.5f, 0.5f, 0.5f);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = 1 - (RoughnessTexture.r+RoughnessTexture.b+RoughnessTexture.g) / 3;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureNormalMask_PixelShader(DrawNormals_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float mask = Mask.Sample(MaskSampler, input.TexCoord).r;
    if (mask < CLIP_VALUE)
        clip(-1);

    // NORMAL MAP ////
    float3 normalMap = 1 * NormalMap.Sample(NormalMapSampler, (input.TexCoord)).rgb - float3(0.5f, 0.5f, 0.5f);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = Roughness;
    
    return Lighting(renderParams);
}

PixelShaderOutput DrawBasic_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    Render_IN renderParams;

    float4 outputColor = DiffuseColor; //* input.Color;

         
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.f0 = F0;
    renderParams.roughness = Roughness;

    return Lighting(renderParams);
}

technique DrawBasic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawBasic_PixelShader();
    }
}

technique DrawTexture
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTexture_PixelShader();
    }
}

technique DrawTextureSpecular
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecular_PixelShader();
    }
}

technique DrawTextureSpecularNormal
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularNormal_PixelShader();
    }
}

technique DrawTextureNormal
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureNormal_PixelShader();
    }
}

technique DrawTextureMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureMask_PixelShader();
    }
}

technique DrawTextureSpecularMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularMask_PixelShader();
    }
}

technique DrawTextureSpecularNormalMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularNormalMask_PixelShader();
    }
}

technique DrawTextureNormalMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureNormalMask_PixelShader();
    }
}

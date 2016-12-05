//Lightshader Bounty Road 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "helper.fx"

float4x4  World;
float4x4  WorldViewProj;
float3x3  WorldViewIT; //Inverse Transposed

float3 Camera;
float2 Resolution;

float Roughness = 0.3f;
float Metallic = 0;
int MaterialType = 0;

const float CLIP_VALUE = 0.99;

float4 DiffuseColor = float4(0.8f, 0.8f, 0.8f, 1);

Texture2D<float4> Texture;

Texture2D<float4> NormalMap;

Texture2D<float4> MetallicMap;

Texture2D<float4> RoughnessMap;

Texture2D<float4> DisplacementMap;

Texture2D<float4> Mask;

sampler TextureSampler
{
	Texture = (Texture);
	Filter = Anisotropic;
	MaxAnisotropy = 8;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler TextureSamplerTrilinear
{
	Texture = (NormalMap);
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
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
    float2 Depth : TEXCOORD0;
};

struct Render_IN
{
    float4 Position : SV_POSITION0;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0;
    float2 Depth : DEPTH;
    float Metallic : TEXCOORD1;
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
    Output.Depth = float2(Output.Position.z, Output.Position.w);
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
    Output.Depth = float2(Output.Position.z, Output.Position.w);
    return Output;
}

float3 GetNormalMap(float2 TexCoord)
{
	//This gets normalized anyways, so it doesn't matter that it's technically only half the length
	return NormalMap.Sample(TextureSamplerTrilinear, TexCoord).rgb - float3(0.5f, 0.5f, 0.5f);
}


PixelShaderOutput Lighting(Render_IN input)
{
    //float4 gamma_color = pow(abs(input.Color), 2.2f);

    //float4 finalValue = gamma_color*(float4(0.2,0.2,0.2,1));
                 
    float4 finalValue = input.Color;

    //Deferred MRT

    PixelShaderOutput Out;

    Out.Color = finalValue;

    Out.Color.a = encodeMetallicMattype(input.Metallic, MaterialType);

    Out.Normal.rgb =  encode(input.Normal); //

    Out.Normal.a = input.roughness;

    Out.Depth = (1- input.Depth.x / input.Depth.y);

    return Out;
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTexture_PixelShader(DrawBasic_VSOut input)
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;
 
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = Roughness;
    
    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecular_PixelShader(DrawBasic_VSOut input) 
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;

    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = RoughnessTexture;

    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecularMetallic_PixelShader(DrawBasic_VSOut input) 
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;
    float metallicTexture = MetallicMap.Sample(TextureSampler, input.TexCoord).r;

    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.Metallic = metallicTexture;
    renderParams.roughness = RoughnessTexture;

    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecularNormal_PixelShader(DrawNormals_VSOut input) 
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;

    // NORMAL MAP ////
	float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = RoughnessTexture;

    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureSpecularNormalMetallic_PixelShader(DrawNormals_VSOut input) 
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;
    float metallicTexture = MetallicMap.Sample(TextureSampler, input.TexCoord).r;
    // NORMAL MAP ////
    float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.Metallic = metallicTexture;
    renderParams.roughness = RoughnessTexture;

    return Lighting(renderParams);
}



[earlydepthstencil]      //experimental
PixelShaderOutput DrawTextureNormal_PixelShader(DrawNormals_VSOut input) 
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    // NORMAL MAP ////
    float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = Roughness;
    
    return Lighting(renderParams);
}

      //experimental
PixelShaderOutput DrawTextureMask_PixelShader(DrawBasic_VSOut input) 
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float mask = Mask.Sample(TextureSampler, input.TexCoord).r;
    if (mask < CLIP_VALUE)
        clip(-1);
 
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = Roughness;
    
    return Lighting(renderParams);
}


PixelShaderOutput DrawTextureSpecularMask_PixelShader(DrawBasic_VSOut input) 
{
    Render_IN renderParams;

	float mask = Mask.Sample(TextureSampler, input.TexCoord).r;
	if (mask < CLIP_VALUE)
	clip(-1);

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;

    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = RoughnessTexture; // 1 - (RoughnessTexture.r+RoughnessTexture.b+RoughnessTexture.g) / 3;
    
    return Lighting(renderParams);
}

      //experimental
PixelShaderOutput DrawTextureSpecularNormalMask_PixelShader(DrawNormals_VSOut input) 
{
    Render_IN renderParams;

	float mask = Mask.Sample(TextureSampler, input.TexCoord).r;
	if (mask < CLIP_VALUE)
	clip(-1);

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;


    // NORMAL MAP ////
    float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = RoughnessTexture;

    return Lighting(renderParams);
}

    //experimental
PixelShaderOutput DrawTextureNormalMask_PixelShader(DrawNormals_VSOut input) 
{
    Render_IN renderParams;

	float mask = Mask.Sample(TextureSampler, input.TexCoord).r;

	//Branching has shown to make no difference here
	if (mask < CLIP_VALUE)
	{
		clip(-1);
	}

	float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
	float4 outputColor = textureColor; //* input.Color;

	float3x3 worldSpace = input.WorldToTangentSpace;


	// NORMAL MAP ////
	float3 normalMap = GetNormalMap(input.TexCoord);
	normalMap = normalize(mul(normalMap, worldSpace));

	renderParams.Position = input.Position;
	renderParams.Color = outputColor;
	renderParams.Normal = normalMap;
	renderParams.Depth = input.Depth;
	renderParams.Metallic = Metallic;
	renderParams.roughness = Roughness;

	return Lighting(renderParams);
}

PixelShaderOutput DrawBasic_PixelShader(DrawBasic_VSOut input)
{
    Render_IN renderParams;

    float4 outputColor = DiffuseColor; //* input.Color;

         
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.roughness = Roughness;

    return Lighting(renderParams);
}


//DISPLACEMENT / POM
PixelShaderOutput DrawTextureDisplacement_PixelShader(DrawNormals_VSOut input) 
{
    Render_IN renderParams;

    float3x3 worldToTangent = input.WorldToTangentSpace;

    float3 tangentPos = mul(input.Position.xyz, worldToTangent);
    float3 tangentCamera = mul(Camera, worldToTangent);
    
    float3 viewDir = normalize(tangentPos - tangentCamera);
    //POM
    float height = DisplacementMap.Sample(TextureSampler, input.TexCoord).r;
    float2 p = viewDir.xy / viewDir.z * height * 0.01f;

    float2 texCoordPOM = input.TexCoord - p;

    //float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;
    //float metallicTexture = MetallicMap.Sample(TextureSampler, input.TexCoord).r;

    
    float4 textureColor = Texture.Sample(TextureSampler, texCoordPOM);
    float4 outputColor = textureColor; //* input.Color;

    // NORMAL MAP ////
    float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldToTangent));

   
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
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

technique DrawTextureSpecularMetallic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularMetallic_PixelShader();
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

technique DrawTextureSpecularNormalMetallic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularNormalMetallic_PixelShader();
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
        PixelShader = compile ps_4_0 DrawTextureMask_PixelShader();
    }
}

technique DrawTextureSpecularMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureSpecularMask_PixelShader();
    }
}

technique DrawTextureSpecularNormalMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureSpecularNormalMask_PixelShader();
    }
}

technique DrawTextureNormalMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureNormalMask_PixelShader();
    }
}

technique DrawTextureDisplacement
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureDisplacement_PixelShader();
    }
}
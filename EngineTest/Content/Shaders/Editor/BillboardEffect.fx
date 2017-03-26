////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Billboard Shader
//  Draws camera-oriented quads.

// Camera parameters.
float4x4 WorldViewProj;
float4x4 WorldView;

//Will be overwritten
float AspectRatio = 1.777;
float FarClip;

float2 ResolutionRcp = float2(1.0f / 1280, 1.0f / 800);

// Particle texture and sampler.
Texture2D Texture;

// For mouse picking we draw the object's id as color to the screen
float3 IdColor;

SamplerState Sampler = sampler_state
{
    MinFilter = Linear;
    MagFilter = Anisotropic;

    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D DepthMap;
SamplerState DepthSampler = sampler_state
{
    MinFilter = Point;
    MagFilter = Point;

    AddressU = Clamp;
    AddressV = Clamp;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float4 Color : COLOR0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

VertexShaderOutput BillboardVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProj);
    output.Position /= output.Position.w;

	float4 PositionVS = mul(input.Position, WorldView);

	//Get the screen coordinates of the billboards position
    float2 texCoord = 0.5f * (float2(output.Position.x, -output.Position.y) + 1);
        
	//Average the depth of this and the 2 neighbor pixels (x)
    float vDepthMap = DepthMap.SampleLevel(DepthSampler, texCoord, 0).r;
    vDepthMap += DepthMap.SampleLevel(DepthSampler, texCoord + float2(ResolutionRcp.x, 0), 0).r;
    vDepthMap += DepthMap.SampleLevel(DepthSampler, texCoord - float2(ResolutionRcp.x, 0), 0).r;
    vDepthMap /= 3;

	//If we are behind that -> don't draw
    float vLocalDepth = PositionVS.z /- FarClip;
       
	//If we are in front
    if (vLocalDepth < vDepthMap)
    {
		//Expand each edge vertex depending on texture coordinate
        output.Position.xy += (input.TexCoord - float2(0.5f, 0.5f)) * float2(1, AspectRatio) * 0.075f;
    }
    
    output.TextureCoordinate = float2(input.TexCoord.x, 1-input.TexCoord.y);

    output.Color = input.Color;

    return output;
}

//------------------------ PIXEL SHADER ----------------------------------------

float4 BillboardPixelShader(VertexShaderOutput input) : SV_TARGET
{
    float4 color = Texture.Sample(Sampler, input.TextureCoordinate);

    if (color.a < 0.95f)
        clip(-1);

    return float4(color.rgb * input.Color.rgb * IdColor,1);
}

float4 IdPixelShader(VertexShaderOutput input) : SV_TARGET
{
    float4 color = Texture.Sample(Sampler, input.TextureCoordinate);

    if (color.a < 0.95f)
        clip(-1);

    return float4(IdColor, 1);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES

technique Billboard
{
    pass P0
    {
        VertexShader = compile vs_5_0 BillboardVertexShader();
        PixelShader = compile ps_5_0 BillboardPixelShader();
    }
}

technique Id
{
    pass P0
    {
        VertexShader = compile vs_5_0 BillboardVertexShader();
        PixelShader = compile ps_5_0 IdPixelShader();
    }
}
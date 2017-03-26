//Shadow Mapping for point & directional lights

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES

matrix WorldViewProj;
matrix WorldView;
matrix World;

float3 LightPositionWS = float3(0,0,0);

float FarClip = 200;
float SizeBias = 0.005f; //0.005f * 2048 / ShadowMapSize

Texture2D MaskTexture;

SamplerState texSampler
{
	Texture = (MaskTexture);
	AddressU = WRAP;
	AddressV = WRAP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
};

struct DrawBasic2_VSIn
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD;
};

struct DrawLinear_VSOut
{
    float4 Position : SV_POSITION;
    float Depth : TEXCOORD0;
	float3 Normal : NORMAL;
};

struct DrawLinear2_VSOut
{
	float4 Position : SV_POSITION;
	float3 WorldPosition : TEXCOORD0;
	float3 Normal : NORMAL;
};

struct DrawLinear3_VSOut
{
	float4 Position : SV_POSITION;
	float3 WorldPosition : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

DrawLinear_VSOut DrawLinear_VertexShader(DrawBasic_VSIn input)
{
	DrawLinear_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
	Output.Depth = mul(input.Position, WorldView).z / -FarClip;
	Output.Normal = mul(float4(input.Normal, 0), WorldView).xyz;
    return Output;
}

DrawLinear2_VSOut DrawDistance_VertexShader(DrawBasic_VSIn input)
{
	DrawLinear2_VSOut Output;
	float4 position = mul(input.Position, WorldViewProj);
	Output.Position = position;
	Output.WorldPosition = mul(input.Position, World).xyz;
	Output.Normal = mul(float4(input.Normal, 0), World).xyz;
	return Output;
}

DrawLinear3_VSOut DrawDistance_VertexShaderAlpha(DrawBasic2_VSIn input)
{
	DrawLinear3_VSOut Output;
	float4 position = mul(input.Position, WorldViewProj);
	Output.Position = position;
	Output.WorldPosition = mul(input.Position, World).xyz;
	Output.TexCoord = input.TexCoord;
	return Output;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

// VSM
//float4 DrawBasicVSM_PixelShader(DrawBasic_VSOut input) : SV_TARGET
//{
//    float depth = input.Depth.x / input.Depth.y;
//
//    //depth = Projection._43 / (depth - Projection._33);
//
//    float depthsq = depth * depth;
//
//    float dx = ddx(depth);
//    float dy = ddy(depth);
//
//    //depth -= 0.00002f * transparent;
//
//    depthsq += 0.25 * (dx * dx + dy * dy);
//    return float4(1 - depth, 1 - depthsq, 0, 0);
//}

float4 DrawLinear_PixelShader(DrawLinear_VSOut input) : SV_TARGET
{
	//relevant is only z component of the normal, right?
	float depth = input.Depth;

	//Calculate bias
	float3 normal = normalize(input.Normal);
	float bias = (1 - abs(normal.z)) * SizeBias;

	depth += bias;

	return float4(depth,0,0,0);
}

float4 DrawDistance_PixelShader(DrawLinear2_VSOut input) : SV_TARGET
{
	float dist = length(input.WorldPosition - LightPositionWS) / FarClip;
	return 1-dist;
}

float4 DrawDistance_PixelShaderAlpha(DrawLinear3_VSOut input) : SV_TARGET
{
	if (MaskTexture.Sample(texSampler, input.TexCoord).r < 0.49f) discard;

	float dist = length(input.WorldPosition - LightPositionWS) / FarClip;
	return 1 - dist;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique DrawLinearDepth
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 DrawLinear_VertexShader();
        PixelShader = compile ps_5_0 DrawLinear_PixelShader();
    }
}

technique DrawDistanceDepth
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 DrawDistance_VertexShader();
		PixelShader = compile ps_5_0 DrawDistance_PixelShader();
	}
}

technique DrawDistanceDepthAlpha
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 DrawDistance_VertexShaderAlpha();
		PixelShader = compile ps_5_0 DrawDistance_PixelShaderAlpha();
	}
}


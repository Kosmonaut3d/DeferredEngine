
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
//      PROJECTION

matrix WorldViewProj;
matrix WorldView;
matrix World;

float3 LightPositionWorld = float3(0,0,0);

static bool transparent = false;

float FarClip = 200;
float SizeBias = 0.005f; //0.005f * 2048 / ShadowMapSize


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
};

struct DrawLinear_VSOut
{
    float4 Position : SV_POSITION;
    float Depth : TEXCOORD0;
	float3 Normal : NORMAL;
};

struct DrawZW_VSOut
{
	float4 Position : SV_POSITION;
	float2 Depth : TEXCOORD0;
	float3 Normal : NORMAL;
};

struct DrawLinear2_VSOut
{
	float4 Position : SV_POSITION;
	float3 WorldPosition : TEXCOORD0;
	float3 Normal : NORMAL;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
DrawLinear_VSOut DrawLinear_VertexShader(DrawBasic_VSIn input)
{
	DrawLinear_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
    /*Output.Depth.x = Output.Position.z;
    Output.Depth.y = Output.Position.w;*/
	Output.Depth = mul(input.Position, WorldView).z / -FarClip;
	Output.Normal = mul(float4(input.Normal, 0), WorldView).xyz;
    return Output;
}

DrawLinear2_VSOut DrawZW_VertexShader(DrawBasic_VSIn input)
{
	DrawLinear2_VSOut Output;
	Output.Position = mul(input.Position, WorldViewProj);
	Output.WorldPosition = mul(input.Position, World).xyz;
	Output.Normal = mul(float4(input.Normal, 0), World).xyz;
	return Output;

	/*DrawZW_VSOut Output;
	Output.Position = mul(input.Position, WorldViewProj);
	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;
	Output.Normal = mul(float4(input.Normal, 0), WorldViewProj).xyz;
	return Output;*/
}

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

float4 DrawZW_PixelShader(DrawLinear2_VSOut input) : SV_TARGET
{
	float dist = length(input.WorldPosition - LightPositionWorld) / FarClip;
	return 1-dist;
	/*float depth = input.Depth.x / input.Depth.y;

	return float4(1-depth, 0, 0, 0);*/
}

technique DrawDepthLinear
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 DrawLinear_VertexShader();
        PixelShader = compile ps_5_0 DrawLinear_PixelShader();
    }
}

technique DrawDepthZW
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 DrawZW_VertexShader();
		PixelShader = compile ps_5_0 DrawZW_PixelShader();
	}
}

//technique DrawVSM
//{
//    pass Pass1
//    {
//        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
//        PixelShader = compile ps_5_0 DrawBasic_PixelShader();
//    }
//}


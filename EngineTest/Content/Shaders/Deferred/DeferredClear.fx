////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
	float2 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position.xy,1, 1);
	return output;
}

//------------------------ PIXEL SHADER ----------------------------------------

float4 BasePixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
	return float4(0,0,0,0);
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES

technique Base
{
	pass Pass1
	{

		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 BasePixelShaderFunction();
	}
}
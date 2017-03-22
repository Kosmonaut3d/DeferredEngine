////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
	float3 Position : POSITION0;
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


//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader

technique Base
{
	pass Pass1
	{

		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 BasePixelShaderFunction();
	}
}
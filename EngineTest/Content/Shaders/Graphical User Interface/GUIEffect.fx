//
//Texture2D BaseTexture;
//SamplerState u_texture
//{
//    Texture = <ScreenTexture>; 
//
//	MagFilter = LINEAR;
//	MinFilter = LINEAR;
//	Mipfilter = LINEAR;
//
//	AddressU = CLAMP;
//	AddressV = CLAMP;
//};

float3 Color;

struct VertexShaderInput
{
	float3 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Color : TEXCOORD0;
}; 

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	output.Color = Color;
	return output;
}

float4 FlatPS(VertexShaderOutput input) : SV_TARGET0
{
	return float4(input.Color, 0);
}


technique Flat
{
	pass Flat
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 FlatPS();
	}
}
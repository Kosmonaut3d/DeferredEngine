////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Draw colored lines
//  Either defined by vertex color or by GlobalColor

matrix WorldViewProj;
float4 GlobalColor;

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
                 
    output.Position = mul(input.Position, WorldViewProj);

    output.Color = input.Color;
	return output;
}

float4 VertexShaderFunctionColor(float4 Position : SV_Position) : SV_Position
{
    float4 outPosition = mul(Position, WorldViewProj);

    return outPosition;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
	return input.Color; //+ AmbientColor * AmbientIntensity;
}


float4 PixelShaderFunctionColor(float4 SV_POSITION: SV_Position) : SV_TARGET0
{
    return GlobalColor;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique VertexColor
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0  PixelShaderFunction();
	}
}

technique GlobalColor
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunctionColor();
        PixelShader = compile ps_5_0 PixelShaderFunctionColor();
    }
}

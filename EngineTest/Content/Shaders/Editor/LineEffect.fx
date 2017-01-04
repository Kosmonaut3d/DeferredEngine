//float4x4 World;
//float4x4 View;
//float4x4 Projection;

matrix WorldViewProj;

float4 GlobalColor;

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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
                 
    output.Position = mul(input.Position, WorldViewProj);

    output.Color = input.Color;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
    return input.Color; //+ AmbientColor * AmbientIntensity;
}

/// ONLY ONE COLOR, PREDEFINED IN Color!

float4 VertexShaderFunctionColor(float4 Position : SV_Position) : SV_Position
{
    float4 outPosition = mul(Position, WorldViewProj);

    return outPosition;
}

float4 PixelShaderFunctionColor(float4 SV_POSITION: SV_Position) : SV_TARGET0
{
    return GlobalColor;
}


technique Line
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0  PixelShaderFunction();
	}
}

technique OneColor
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunctionColor();
        PixelShader = compile ps_5_0 PixelShaderFunctionColor();
    }
}

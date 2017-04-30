//Bloomfilter 2016, Thekosmonaut

Texture2D ScreenTexture;
SamplerState u_texture
{
    Texture = <ScreenTexture>; 

	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;

	AddressU = CLAMP;
	AddressV = CLAMP;
};

float2 InverseResolution;
float Threshold = 0.8f;
float Radius = 1.0f;
float Strength = 1.0f;
float StreakLength = 1;

struct VertexShaderInput
{
	float2 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
}; 

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uint id:SV_VERTEXID)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 0, 1);
	output.TexCoord.x = (float)(id / 2) * 2.0;
	output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;

	return output;
}

float4 Box4(float4 p0, float4 p1, float4 p2, float4 p3)
{
	return (p0 + p1 + p2 + p3) * 0.25f;
}

float4 ExtractPS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = ScreenTexture.Sample(u_texture, texCoord);

	float avg = (color.r + color.g + color.b) / 3;

	if (avg>Threshold)
	{
		return color /** (avg - Threshold) / (10 - Threshold)*/;// * (avg - Threshold);
	}

	return float4(0, 0, 0, 0);
}

float GetLuma(float3 rgb)
{
	return (0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b);
}

float4 ExtractLuminancePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 color = ScreenTexture.Sample(u_texture, texCoord);

	float luminance = GetLuma(color.rgb);

    if(luminance>Threshold)
    {
		return float4(color.rgb, luminance);// *(luminance - Threshold);
        //return saturate((color - Threshold) / (1 - Threshold));
    }

    return float4(0, 0, 0, 0);
}

float4 DownsamplePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float2 offset = float2(StreakLength * InverseResolution.x, 1 * InverseResolution.y);
        
    float4 c0 = ScreenTexture.Sample(u_texture, texCoord + float2(-2, -2) * offset);
    float4 c1 = ScreenTexture.Sample(u_texture, texCoord + float2(0,-2)*offset);
    float4 c2 = ScreenTexture.Sample(u_texture, texCoord + float2(2, -2) * offset);
    float4 c3 = ScreenTexture.Sample(u_texture, texCoord + float2(-1, -1) * offset);
    float4 c4 = ScreenTexture.Sample(u_texture, texCoord + float2(1, -1) * offset);
    float4 c5 = ScreenTexture.Sample(u_texture, texCoord + float2(-2, 0) * offset);
    float4 c6 = ScreenTexture.Sample(u_texture, texCoord);
    float4 c7 = ScreenTexture.Sample(u_texture, texCoord + float2(2, 0) * offset);
    float4 c8 = ScreenTexture.Sample(u_texture, texCoord + float2(-1, 1) * offset);
    float4 c9 = ScreenTexture.Sample(u_texture, texCoord + float2(1, 1) * offset);
    float4 c10 = ScreenTexture.Sample(u_texture, texCoord + float2(-2, 2) * offset);
    float4 c11 = ScreenTexture.Sample(u_texture, texCoord + float2(0, 2) * offset);
    float4 c12 = ScreenTexture.Sample(u_texture, texCoord + float2(2, 2) * offset);

    return Box4(c0, c1, c5, c6) * 0.125f +
    Box4(c1, c2, c6, c7) * 0.125f +
    Box4(c5, c6, c10, c11) * 0.125f +
    Box4(c6, c7, c11, c12) * 0.125f +
    Box4(c3, c4, c8, c9) * 0.5f;
}

float4 UpsamplePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float2 offset = float2(StreakLength * InverseResolution.x, 1 * InverseResolution.y) * Radius;

    float4 c0 = ScreenTexture.Sample(u_texture, texCoord + float2(-1, -1) * offset);
    float4 c1 = ScreenTexture.Sample(u_texture, texCoord + float2(0, -1) * offset);
    float4 c2 = ScreenTexture.Sample(u_texture, texCoord + float2(1, -1) * offset);
    float4 c3 = ScreenTexture.Sample(u_texture, texCoord + float2(-1, 0) * offset);
    float4 c4 = ScreenTexture.Sample(u_texture, texCoord);
    float4 c5 = ScreenTexture.Sample(u_texture, texCoord + float2(1, 0) * offset);
    float4 c6 = ScreenTexture.Sample(u_texture, texCoord + float2(-1,1) * offset);
    float4 c7 = ScreenTexture.Sample(u_texture, texCoord + float2(0, 1) * offset);
    float4 c8 = ScreenTexture.Sample(u_texture, texCoord + float2(1, 1) * offset);

    //Tentfilter  0.0625f    
    return 0.0625f * (c0 + 2 * c1 + c2 + 2 * c3 + 4 * c4 + 2 * c5 + c6 + 2 * c7 + c8) * Strength; //+ 0.5f * ScreenTexture.Sample(c_texture, texCoord);

}

//-------------------------- TECHNIQUES ----------------------------------------

technique Extract
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 ExtractPS();
	}
}

technique ExtractLuminance
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 ExtractLuminancePS();
	}
}

technique Downsample
{
    pass Pass1
    {
		VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 DownsamplePS();
    }
}

technique Upsample
{
    pass Pass1
    {
		VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 UpsamplePS();
    }
}


texture2D ScreenTexture;
sampler u_texture = sampler_state
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
	float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
}; 

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord;
	return output;
}

float4 Box4(float4 p0, float4 p1, float4 p2, float4 p3)
{
	return (p0 + p1 + p2 + p3) * 0.25f;
}

float4 ExtractPS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(u_texture, texCoord);

	float avg = (color.r + color.g + color.b) / 3;

	if (avg>Threshold)
	{
		return color * (avg - Threshold) / (1 - Threshold);// * (avg - Threshold);
	}

	return float4(0, 0, 0, 0);
}

float4 ExtractLuminancePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 color = tex2D(u_texture, texCoord);

    float luminance = color.r * 0.21f + color.g * 0.72f + color.b * 0.07f;

    if(luminance>Threshold)
    {
		return color * (luminance - Threshold) / (1 - Threshold);// *(luminance - Threshold);
        //return saturate((color - Threshold) / (1 - Threshold));
    }

    return float4(0, 0, 0, 0);
}

float4 DownsamplePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float2 offset = float2(StreakLength * InverseResolution.x, 1 * InverseResolution.y);
        
    float4 c0 = tex2D(u_texture, texCoord + float2(-2, -2) * offset);
    float4 c1 = tex2D(u_texture, texCoord + float2(0,-2)*offset);
    float4 c2 = tex2D(u_texture, texCoord + float2(2, -2) * offset);
    float4 c3 = tex2D(u_texture, texCoord + float2(-1, -1) * offset);
    float4 c4 = tex2D(u_texture, texCoord + float2(1, -1) * offset);
    float4 c5 = tex2D(u_texture, texCoord + float2(-2, 0) * offset);
    float4 c6 = tex2D(u_texture, texCoord);
    float4 c7 = tex2D(u_texture, texCoord + float2(2, 0) * offset);
    float4 c8 = tex2D(u_texture, texCoord + float2(-1, 1) * offset);
    float4 c9 = tex2D(u_texture, texCoord + float2(1, 1) * offset);
    float4 c10 = tex2D(u_texture, texCoord + float2(-2, 2) * offset);
    float4 c11 = tex2D(u_texture, texCoord + float2(0, 2) * offset);
    float4 c12 = tex2D(u_texture, texCoord + float2(2, 2) * offset);

    return Box4(c0, c1, c5, c6) * 0.125f +
    Box4(c1, c2, c6, c7) * 0.125f +
    Box4(c5, c6, c10, c11) * 0.125f +
    Box4(c6, c7, c11, c12) * 0.125f +
    Box4(c3, c4, c8, c9) * 0.5f;
}

//IDEA: Why don't we sample the distance based on brightness?
float4 DownsampleLuminancePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{

	float4 c6 = tex2D(u_texture, texCoord);

	float2 offset = float2(StreakLength * InverseResolution.x, 1 * InverseResolution.y);

	float4 c0 = tex2D(u_texture, texCoord + float2(-2, -2) * offset);
	float4 c1 = tex2D(u_texture, texCoord + float2(0, -2) * offset);
	float4 c2 = tex2D(u_texture, texCoord + float2(2, -2) * offset);
	float4 c3 = tex2D(u_texture, texCoord + float2(-1, -1) * offset);
	float4 c4 = tex2D(u_texture, texCoord + float2(1, -1) * offset);
	float4 c5 = tex2D(u_texture, texCoord + float2(-2, 0) * offset);
	float4 c7 = tex2D(u_texture, texCoord + float2(2, 0) * offset);
	float4 c8 = tex2D(u_texture, texCoord + float2(-1, 1) * offset);
	float4 c9 = tex2D(u_texture, texCoord + float2(1, 1) * offset);
	float4 c10 = tex2D(u_texture, texCoord + float2(-2, 2) * offset);
	float4 c11 = tex2D(u_texture, texCoord + float2(0, 2) * offset);
	float4 c12 = tex2D(u_texture, texCoord + float2(2, 2) * offset);

	return saturate(Box4(c0, c1, c5, c6) * 0.125f +
		Box4(c1, c2, c6, c7) * 0.125f +
		Box4(c5, c6, c10, c11) * 0.125f +
		Box4(c6, c7, c11, c12) * 0.125f +
		Box4(c3, c4, c8, c9) * 0.5f);
}

float4 UpsamplePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float2 offset = float2(StreakLength * InverseResolution.x, 1 * InverseResolution.y) * Radius;

    float4 c0 = tex2D(u_texture, texCoord + float2(-1, -1) * offset);
    float4 c1 = tex2D(u_texture, texCoord + float2(0, -1) * offset);
    float4 c2 = tex2D(u_texture, texCoord + float2(1, -1) * offset);
    float4 c3 = tex2D(u_texture, texCoord + float2(-1, 0) * offset);
    float4 c4 = tex2D(u_texture, texCoord);
    float4 c5 = tex2D(u_texture, texCoord + float2(1, 0) * offset);
    float4 c6 = tex2D(u_texture, texCoord + float2(-1,1) * offset);
    float4 c7 = tex2D(u_texture, texCoord + float2(0, 1) * offset);
    float4 c8 = tex2D(u_texture, texCoord + float2(1, 1) * offset);

    //Tentfilter  0.0625f    
    return 0.0625f * (c0 + 2 * c1 + c2 + 2 * c3 + 4 * c4 + 2 * c5 + c6 + 2 * c7 + c8) * Strength + float4(0, 0,0,0); //+ 0.5f * tex2D(c_texture, texCoord);

}



float4 UpsampleLuminancePS(float4 pos : SV_POSITION,  float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 c4 = tex2D(u_texture, texCoord);  //middle one
 /*
    float luminance = c4.r * 0.21f + c4.g * 0.72f + c4.b * 0.07f;
    luminance = max(luminance, 0.4f);
*/
	float2 offset = float2(StreakLength * InverseResolution.x, 1 * InverseResolution.y) * Radius;
    float4 c0 = tex2D(u_texture, texCoord + float2(-1, -1) * offset);
    float4 c1 = tex2D(u_texture, texCoord + float2(0, -1) * offset);
    float4 c2 = tex2D(u_texture, texCoord + float2(1, -1) * offset);
    float4 c3 = tex2D(u_texture, texCoord + float2(-1, 0) * offset);
    float4 c5 = tex2D(u_texture, texCoord + float2(1, 0) * offset);
    float4 c6 = tex2D(u_texture, texCoord + float2(-1, 1) * offset);
    float4 c7 = tex2D(u_texture, texCoord + float2(0, 1) * offset);
    float4 c8 = tex2D(u_texture, texCoord + float2(1, 1) * offset);
 
    return 0.0625f * (c0 + 2 * c1 + c2 + 2 * c3 + 4 * c4 + 2 * c5 + c6 + 2 * c7 + c8) * Strength + float4(0, 0, 0, 0); //+ 0.5f * tex2D(c_texture, texCoord);

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

technique DownsampleLuminance
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 DownsampleLuminancePS();
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


technique UpsampleLuminance
{
    pass Pass1
    {
		VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 UpsampleLuminancePS();
    }
}

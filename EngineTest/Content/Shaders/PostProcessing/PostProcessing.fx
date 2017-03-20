//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing

Texture2D ScreenTexture;

// Our sampler for the texture, which is just going to be pretty simple
sampler TextureSampler = sampler_state
{
    Texture = <ScreenTexture>; 
};

float ChromaticAbberationStrength = 10;

float SCurveStrength; //= -0.05f;

float WhitePoint = 1.1f;

float Exposure = 2;

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

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

float2 brownConradyDistortion(float2 uv)
{
    // positive values of K1 give barrel distortion, negative give pincushion
    float barrelDistortion1 = 0.1; // K1 in text books
    float barrelDistortion2 = -0.025; // K2 in text books
    float r2 = uv.x * uv.x + uv.y * uv.y;
    uv *= 1.0 + barrelDistortion1 * r2 + barrelDistortion2 * r2 * r2;
    
    // tangential distortion (due to off center lens elements)
    // is not modeled in this function, but if it was, the terms would go here
    return uv;
}

float3 ColorSCurve(float3 color)
{
    [branch]
    if (abs(SCurveStrength) <= 0.01)
        return color;
    //brighness (luminance)
    //float brightness = color.r * 0.21f + color.g * 0.72f + color.b * 0.07f;
    //float brightness = (color.r+color.b+color.g) / 3;
    float brightness = max(color.r, max(color.g, color.b));

    float brightnessCurve = brightness - sin(brightness * 2 * 3.1414f) * SCurveStrength + sin(brightness*3.141)*0.1;

    brightness = brightnessCurve / brightness;

    return color * float3(brightness, brightness, brightness);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	//align texture coordinates
	output.TexCoord = input.TexCoord;
	return output;
}

//------------------------ PIXEL SHADER ----------------------------------------
// This pixel shader will simply look up the color of the texture at the
// requested point, and turns it into a shade of gray

float radiusX = 0.6;
float radiusY = 0.2;

//float4 VignettePixelShaderFunction(float4 pos : SV_POSITION, float2 texCoord : TEXCOORD0) : SV_TARGET0
//{
//    float3 base = tex2D(TextureSampler, texCoord.xy).rgb;
//
//    base = ColorSCurve(base);
//
//    return float4(base,1);
//}

float3 ToneMapFilmic_Hejl2015(float3 hdr, float whitePt)
{
	float4 vh = float4(hdr, whitePt);
	float4 va = (1.425 * vh) + 0.05f;
	float4 vf = ((vh * va + 0.004f) / ((vh * (va + 0.55f) + 0.0491f))) - 0.0821f;
	return vf.rgb / vf.www;
}

float GetLuma(float3 rgb)
{
	return (0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b);
}

//http://www.cs.utah.edu/~reinhard/cdrom/tonemap.pdf

float3 ReinhardTonemap(float3 hdr)
{
	float x = GetLuma(hdr);
	return hdr * (x / (x + 1));
}

float3 ReinhardTonemap(float3 hdr, float WhitePoint)
{
	float x = GetLuma(hdr);
	return hdr * (x * ( 1 + x / (WhitePoint*WhitePoint)) / (x + 1));
}

float3 Uncharted2Tonemap(float3 x)
{
	float A = 0.15;

	float B = 0.50;

	float C = 0.10;

	float D = 0.20;

	float E = 0.02;

	float F = 0.30;

	return ((x*(A*x + C*B) + D*E) / (x*(A*x + B) + D*F)) - E / F;
}

float4 VignetteChromaShiftPixelShaderFunction(float4 pos : SV_POSITION, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float3 base = tex2D(TextureSampler, texCoord).rgb ;


    //float chromaStrength = (base.r + base.g + base.b) / 3;
	float dist = distance(texCoord, float2(0.5.xx));

	if (dist > 0.1)
	{
		float2 chromaDist = (texCoord - float2(0.5, 0.5)) * ChromaticAbberationStrength * 0.1f; //*(0.5f+chromaStrength);

		float chromaR = tex2D(TextureSampler, texCoord.xy + chromaDist).r;

		base.r = chromaR;
	}

	base.rgb = ToneMapFilmic_Hejl2015(base.rgb * Exposure, WhitePoint);
	base = pow(abs(base), 0.4545454545f);

	//base.rgb = //ReinhardTonemap(base.rgb * Exposure, WhitePoint);
	//		   //Uncharted2Tonemap(base.rgb * Exposure) / Uncharted2Tonemap(WhitePoint.xxx);
	//base = pow(abs(base), 0.4545454545f);

    base = ColorSCurve(base);

    dist *= 0.60f;
    base.rgb *= smoothstep(radiusX, radiusY, dist);

    return float4(base,1);
}

float4 BasePixelShaderFunction(float4 pos : SV_POSITION, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float3 base = tex2D(TextureSampler, texCoord).rgb;

	base = pow(abs(base), 0.4545454545f);
	base.rgb = //ReinhardTonemap(base.rgb * Exposure, WhitePoint);
			   		   Uncharted2Tonemap(base.rgb * Exposure) / Uncharted2Tonemap(WhitePoint.xxx);

	base = ColorSCurve(base);
	return float4(base,1);
}


//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader
technique Vignette
{
    pass Pass1
    {

		VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 BasePixelShaderFunction();
    }
}

technique Base
{
	pass Pass1
	{

		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 BasePixelShaderFunction();
	}
}

technique VignetteChroma
{
    pass Pass1
    {

		VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 VignetteChromaShiftPixelShaderFunction();
    }
}
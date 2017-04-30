// Basic Postprocessing shader
// Combines chroma shift and vignette effect, plus Tonemapping from HDR to LDR and gamma conversion from 1.0 to 2.2

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

Texture2D ScreenTexture;

// Our sampler for the texture, which is just going to be pretty simple
SamplerState LinearSampler
{
    Texture = (ScreenTexture); 
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = POINT;
};

float ChromaticAbberationStrength = 10;

float SCurveStrength; //= -0.05f;

float WhitePoint = 1.1f;

//Note this should be computed as Pow(2, Exposure) as an input. I do not compute the power in this shader
float PowExposure = 2;

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
	float2 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uint id:SV_VERTEXID)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 0, 1);
	output.TexCoord.x = (float)(id / 2) * 2.0;
	output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;
	return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
	Almost everything below is taken from somewhere. I apologize for not providing sources for each helper function.
*/


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

    float brightnessCurve = brightness - sin(brightness * 2 * 3.1414f) * SCurveStrength * 0.1f + sin(brightness*3.141)*0.1;

    brightness = brightnessCurve / brightness;

    return color * float3(brightness, brightness, brightness);
}

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

float3 InverseReinhardTonemap(float3 ldr)
{
	float x = GetLuma(ldr);
	return ldr * ((x + 1) / x);
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

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Main Post Processing
////////////////////////////////////////////////////////////////////////////////////////////////////////////


float4 VignetteChromaShiftPixelShaderFunction(float4 pos : SV_POSITION, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	int3 texCoordInt = int3(pos.xy, 0);

	//Load base color
    float3 base = ScreenTexture.Load(texCoordInt).rgb;

	//Calculate distance to center
	float dist = distance(texCoord, float2(0.5.xx));

	//Chroma shift / fringe effect
	if (dist > 0.1)
	{
		//Depending on distance to center, we substitute our red channel for another pixel from a slight offset
		float2 distcr = (texCoord - float2(0.5, 0.5)) ;
		float2 chromaDist = distcr*dist * float2(1.6f, 1) * ChromaticAbberationStrength * 0.1f; //*(0.5f+chromaStrength);

		float chromaR = ScreenTexture.Sample(LinearSampler, texCoord.xy + chromaDist).r;

		base.r = chromaR;
	}

	//Apply Tonemapping!

	//base.rgb = base.rgb * PowExposure;
	//base.rgb = //ReinhardTonemap(base.rgb * PowExposure, WhitePoint);
	//		   //Uncharted2Tonemap(base.rgb * PowExposure) / Uncharted2Tonemap(WhitePoint.xxx);
	base.rgb = ToneMapFilmic_Hejl2015(base.rgb * PowExposure, WhitePoint);

	//Convert back to 2.2 Gamma!

	base = pow(abs(base), 0.4545454545f);

	//Apply SCurve
    base = ColorSCurve(base);

	//Apply "Vignette" Effect
	const float radiusX = 0.6;
	const float radiusY = 0.2;
    dist *= 0.60f;
    base.rgb *= smoothstep(radiusX, radiusY, dist);

    return float4(base,1);
}

//Only apply Tonemapping and SCurve
float4 BasePixelShaderFunction(float4 pos : SV_POSITION, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	int3 texCoordInt = int3(pos.xy, 0);

	//Load base color
	float3 base = ScreenTexture.Load(texCoordInt).rgb;

	base.rgb = //ReinhardTonemap(base.rgb * PowExposure, WhitePoint);
		//Uncharted2Tonemap(base.rgb * PowExposure) / Uncharted2Tonemap(WhitePoint.xxx);
		ToneMapFilmic_Hejl2015(base.rgb * PowExposure, WhitePoint);

	base = pow(abs(base), 0.4545454545f);

	base = ColorSCurve(base);

	return float4(base,1);
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
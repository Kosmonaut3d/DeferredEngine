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
    if (SCurveStrength == 0)
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

float4 VignettePixelShaderFunction(float4 pos : SV_POSITION, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float3 base = tex2D(TextureSampler, texCoord.xy).rgb;

    base = ColorSCurve(base);

    return float4(base,1);
}

float4 VignetteChromaShiftPixelShaderFunction(float4 pos : SV_POSITION, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float3 base = tex2D(TextureSampler, texCoord).rgb ;

    //float chromaStrength = (base.r + base.g + base.b) / 3;

    float2 chromaDist = (texCoord - float2(0.5, 0.5)) * ChromaticAbberationStrength; //*(0.5f+chromaStrength);

    float chromaR = tex2D(TextureSampler, texCoord.xy + chromaDist).r;
    
	base.r = chromaR;

	base = pow(abs(base), 1 / 2.2f);

    base = ColorSCurve(base);

    float dist = distance(texCoord, float2(0.5.xx)) * 0.60f;
    base.rgb *= smoothstep(radiusX, radiusY, dist);

    return float4(base,1);
}


//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader
technique Vignette
{
    pass Pass1
    {

		VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 VignettePixelShaderFunction();
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
static const int BlurKernelSize = 13;
static const float2 BlurKernel[BlurKernelSize] =
{
    { -6, 0 },
    { -5, 0 },
    { -4, 0 },
    { -3, 0 },
    { -2, 0 },
    { -1, 0 },
    { 0, 0 },
    { 1, 0 },
    { 2, 0 },
    { 3, 0 },
    { 4, 0 },
    { 5, 0 },
    { 6, 0 }
};
static const float BlurWeights[BlurKernelSize] =
{
    0.002216f, 0.008764f, 0.026995f, 0.064759f, 0.120985f, 0.176033f, 0.199471f,
    0.176033f, 0.120985f, 0.064759f, 0.026995f, 0.008764f, 0.002216f
};

Texture2D TargetMap;

SamplerState SceneSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
float2 InverseResolution;

struct VI
{
    float2 Position : POSITION0;
};

struct VO
{
    float4 Position : POSITION0;
    float2 TexCoord : TexCoord;
};

VO VS(VI input, uint id:SV_VERTEXID)
{
    VO output;

	output.Position = float4(input.Position, 0, 1);
	output.TexCoord.x = (float)(id / 2) * 2.0;
	output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;

    return output;
}

float4 HorizontalPS(VO input) : COLOR0
{
    float4 outputColor = float4(0, 0, 0, 0);
    
    [unroll] 
    for (int i = 0; i < BlurKernelSize; i++)
    {
        float2 offset = BlurKernel[i].xy * InverseResolution.xy;
    
        float4 sample = TargetMap.Sample(SceneSampler, input.TexCoord + offset);
        sample *= BlurWeights[i];
		
        outputColor += sample;
    }
    
    return outputColor;
}

float4 VerticalPS(VO input) : COLOR0
{
    float4 outputColor = float4(0, 0, 0, 0);
    
    [unroll] 
    for (int i = 0; i < BlurKernelSize; i++)
    {
        float2 offset = BlurKernel[i].yx * InverseResolution.xy;
    
        float4 sample = TargetMap.Sample(SceneSampler, input.TexCoord + offset);
        sample *= BlurWeights[i];

        outputColor += sample;
    }
    
    return outputColor;
}

technique GaussianBlur
{
    pass Horizontal
    {
        VertexShader = compile vs_4_0 VS();
        PixelShader = compile ps_4_0 HorizontalPS();
    }

    pass Vertical
    {
        VertexShader = compile vs_4_0 VS();
        PixelShader = compile ps_4_0 VerticalPS();
    }
}

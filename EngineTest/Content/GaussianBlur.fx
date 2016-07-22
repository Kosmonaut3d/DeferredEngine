static const int BlurKernelSize = 13;
static const float2 BlurKernel[ BlurKernelSize ] = 
{
    { -6, 0 }, { -5, 0 }, { -4, 0 }, { -3, 0 }, { -2, 0 }, { -1, 0 }, {  0, 0 },
    {  1, 0 }, {  2, 0 }, {  3, 0 }, {  4, 0 }, {  5, 0 }, {  6, 0 } 
};
static const float BlurWeights[ BlurKernelSize ] = 
{
    0.002216f, 0.008764f, 0.026995f, 0.064759f, 0.120985f, 0.176033f, 0.199471f,
    0.176033f, 0.120985f, 0.064759f, 0.026995f, 0.008764f, 0.002216f
};

texture SceneMap;

sampler SceneSampler = sampler_state
{
    Texture = (SceneMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
float2 InverseRenderTargetDimension : INVRTDIM;

struct VI
{
	float4 Position : POSITION0;
	float2 TexCoord0 : TEXCOORD0;
};

struct VO
{
    float4 Position : POSITION0;
	float2 TexCoord0 : TEXCOORD0;
};

VO VS( VI input )
{
    VO output;

	output.Position = input.Position;
	output.TexCoord0 = input.TexCoord0;

    return output;
}

float4 HorizontalPS( VO input ) : COLOR0
{
	float4 outputColor = float4( 0, 0, 0, 0 );
    
    [unroll] 
	for( int i = 0 ; i < BlurKernelSize ; i++ )
    {   
		float2 offset = BlurKernel[ i ].xy * InverseRenderTargetDimension.xy;
    
		float4 sample = tex2D( SceneSampler, input.TexCoord0 + offset );
		sample *=  BlurWeights[ i ];
		
		outputColor += sample;
    }    
    
	return outputColor ;
}

float4 VerticalPS( VO input ) : COLOR0
{
	float4 outputColor = float4( 0, 0, 0, 0 );
    
    [unroll] 
	for( int i = 0 ; i < BlurKernelSize ; i++ )
    {   
		float2 offset = BlurKernel[ i ].yx * InverseRenderTargetDimension.xy;
    
		float4 sample = tex2D( SceneSampler, input.TexCoord0 + offset );
		sample *=  BlurWeights[ i ];

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

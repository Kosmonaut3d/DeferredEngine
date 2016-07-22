
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
//      PROJECTION

matrix  WorldViewProj;

matrix Projection;

bool transparent = false;


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
	float4 Position : SV_POSITION0;
};

struct DrawBasic_VSOut
{
    float4 Position : SV_POSITION0;
    float2 Depth : TEXCOORD2;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
DrawBasic_VSOut DrawBasic_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
    Output.Depth.x = Output.Position.z;
    Output.Depth.y = Output.Position.w;
    return Output;
}


float4 DrawBasic_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    float depth = input.Depth.x / input.Depth.y;

    float depthsq = depth * depth ;

    float dx = ddx(depth);
    float dy = ddy(depth);

    //depth -= 0.0002f * transparent;

    depthsq += 0.25 * (dx * dx + dy * dy);
    return float4(1-depth, 1-depthsq, 0, 0);
}

technique DrawDepth
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawBasic_PixelShader();
    }
}

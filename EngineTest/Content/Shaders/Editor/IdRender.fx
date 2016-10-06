matrix  WorldViewProj;
matrix World;

float4 ColorId = float4(0.8f, 0.8f, 0.8f, 1);

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
	float4 Position : SV_POSITION0;
};

struct DrawNormal_VSIn
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

DrawBasic_VSIn DrawBasic_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSIn Output;
    Output.Position = mul(input.Position, WorldViewProj);
    
    return Output;
}

float4 Id_PixelShader(DrawBasic_VSIn input): SV_Target
{
    return float4(ColorId.xyz, 0);
}

//Outline
DrawBasic_VSIn DrawOutline_VertexShader(DrawNormal_VSIn input)
{
    DrawBasic_VSIn Output;

    float4 normal = mul(float4(input.Normal, 0), WorldViewProj);

    if(normal.z < 0.03f)
        normal *= 0;

    //if (normal.w < -0.2f)
    //    normal.w = -normal.w;

    Output.Position = mul(input.Position, WorldViewProj) + normalize(normal) * 0.2f;
    

    return Output;
}

float4 Outline_PixelShader(DrawBasic_VSIn input) : SV_Target
{
    return ColorId;
}

technique DrawId
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_4_0 Id_PixelShader();
    }
}

technique DrawOutline
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawOutline_VertexShader();
        PixelShader = compile ps_4_0 Outline_PixelShader();
    }
}
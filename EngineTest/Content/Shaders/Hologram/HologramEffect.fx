//Draw Hologram effect (UNUSED RIGHT NOW)

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
//      PROJECTION  

matrix  World;
matrix  WorldViewProj;

//bool shade = true;


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
	float4 Position : POSITION0;
	float3 Normal   : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct DrawBasic_VSOut
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
DrawBasic_VSOut DrawBasic_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSOut Output;
    Output.Position = mul(input.Position, WorldViewProj);
    Output.Normal = mul(float4(input.Normal, 0), World).xyz;
    return Output;
}


float4 DrawBasic_PixelShader(DrawBasic_VSOut input) : SV_TARGET
{
    float color = 1;

    //[branch]
    //if(shade)
    color = saturate(dot(input.Normal, float3(-1, 0, 1)));

    //float color = 0.5f*saturate(dot(input.Normal, normalize(float3(-1, 0.5, 1))));
    //color += 0.5f*saturate(dot(input.Normal, normalize(float3(-1, -0.5, 1))));
    color *= color*color;

    return float4(color, color, color, 1);
}

technique DrawBasic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawBasic_PixelShader();
    }
}
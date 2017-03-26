////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Draw meshes with color == id
//  Or
//  Draw selected meshes with a a color overlay and outlines

matrix  WorldViewProj;
matrix World;

float4 ColorId = float4(0.8f, 0.8f, 0.8f, 1);
float OutlineSize = 0.4f;

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasic_VSIn
{
	float4 Position : POSITION;
};

struct DrawNormal_VSIn
{
    float4 Position : POSITION;
    float3 Normal : NORMAL0;
};

struct DrawNormal_VSOut
{
	float4 Position : SV_POSITION;
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

//For outlines extrude the backfacing normals
DrawNormal_VSOut DrawOutline_VertexShader(DrawNormal_VSIn input)
{
	DrawNormal_VSOut Output;

    float4 normal = mul(float4(input.Normal, 0), WorldViewProj);

    /*if(normal.z < 0.03f)
        normal *= 0;
*/
    //if (normal.w < -0.2f)
    //    normal.w = -normal.w;

	float factor = 0;
	if (normal.z > 0.03f)
		factor = OutlineSize;

	//if (normal.w < -0.2f)
	//    normal.w = -normal.w;

	Output.Position = mul(input.Position, WorldViewProj) + normalize(float4(normal.rgb,0)) * factor;

	//Output.Position = mul(input.Position + float4(input.Normal, 0)* OutlineSize, WorldViewProj); /*+ float4(normalize(normal.rgb),0) * OutlineSize*/;
	//Output.Position = mul(input.Position, WorldViewProj); /*+ float4(normalize(normal.rgb),0) * OutlineSize*/;
	Output.Normal = normal.rgb;
    

    return Output;
}

//------------------------ PIXEL SHADER ----------------------------------------

float4 Id_PixelShader(DrawBasic_VSIn input) : SV_Target
{
	return float4(ColorId.xyz, 0);
}


float4 Outline_PixelShader(DrawNormal_VSOut input) : SV_Target
{
	if (input.Normal.z > 0.03) return float4(ColorId.rgb,0.4f);

    if (input.Position.x % 8 + input.Position.y % 8 > 6) return float4(0, 0, 0, 0);
    return ColorId;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES

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
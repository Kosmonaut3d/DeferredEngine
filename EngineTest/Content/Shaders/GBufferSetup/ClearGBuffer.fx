struct VertexShaderInput
{
    float3 Position : POSITION0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    return output;
}
struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    //black color
    output.Color = 0;
    //when transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal = 0;
    //max depth
    output.Depth = 1.0f;
    return output;
}
technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

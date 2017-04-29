//Environment cube maps, TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"

float3 CameraPosition;
//In World Space
float3 FrustumCorners[4];

Texture2D VolumeTex;
Texture2D DepthMap;

float3 VolumeTexPositionWS;
float3 VolumeTexSize;
float3 VolumeTexResolution = float3(2,2,2);

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 ViewDir : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

float3 GetFrustumRay(float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;
    output.ViewDir = GetFrustumRay(input.TexCoord);
    return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  BASE FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

float MaxManhattan(float3 vec)
{
	return max(abs(vec.x), max(abs(vec.y), abs(vec.z)));
}

float4 PixelShaderFunctionBasic(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	int3 texCoordInt = int3(input.Position.xy, 0);

	float linearDepth = DepthMap.Load(texCoordInt).r;

	float3 PositionWS = linearDepth * input.ViewDir + CameraPosition;

	//Assume axis-aligned square

	float3 relativePosition = PositionWS - VolumeTexPositionWS;

	relativePosition /= VolumeTexSize;

	//Out of bounds
	if (abs(MaxManhattan(relativePosition)) > 1) discard;

	//Get Texcoordinate from that, normalize to texcoords first
	relativePosition = (relativePosition + float3(1, 1, 1)) * 0.5f * (VolumeTexResolution-float3(1,1,1));

	//x and y are correct, the z determines how much x is shifted.
	float x = trunc(relativePosition.x);
	float xfrac = frac(relativePosition.x);
	float y = trunc(relativePosition.y);
	float yfrac = frac(relativePosition.y);
	float z = trunc(relativePosition.z);
	float zfrac = frac(relativePosition.z);

	float x0y0z0;
	float x1y0z0;
	float x0y1z0;
	float x1y1z0;
	float x0y0z1;
	float x1y0z1;
	float x0y1z1;
	float x1y1z1;

	x += VolumeTexResolution.x * z;
	 x0y0z0 = VolumeTex.Load(int3(x, y, 0)).r;
	 x1y0z0 = VolumeTex.Load(int3(x+1, y, 0)).r;
	 x0y1z0 = VolumeTex.Load(int3(x, y+1, 0)).r;
	 x1y1z0 = VolumeTex.Load(int3(x+1, y + 1, 0)).r;

	x += VolumeTexResolution.x ;
	 x0y0z1 = VolumeTex.Load(int3(x, y, 0)).r;
	 x1y0z1 = VolumeTex.Load(int3(x + 1, y, 0)).r;
	 x0y1z1 = VolumeTex.Load(int3(x, y + 1, 0)).r;
	 x1y1z1 = VolumeTex.Load(int3(x + 1, y + 1, 0)).r;


	float4 lerpz0 = lerp( lerp(x0y0z0, x1y0z0, xfrac), lerp(x0y1z0, x1y1z0, xfrac), yfrac);
	float4 lerpz1 = lerp(lerp(x0y0z1, x1y0z1, xfrac), lerp(x0y1z1, x1y1z1, xfrac), yfrac);
	float4 lerpout = lerp(lerpz0, lerpz1, zfrac) / 1000;
	//

    return lerpout;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique Basic
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunctionBasic();
    }
}

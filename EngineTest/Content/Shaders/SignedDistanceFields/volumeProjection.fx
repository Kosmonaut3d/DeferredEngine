//Environment cube maps, TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"

float3 CameraPosition;

Texture2D VolumeTex;
Texture2D DepthMap;

float2 TriangleTexResolution;
float TriangleAmount;

float3 VolumeTexPositionWS;
float4x4 VolumeTexInverseMatrix;
float3 VolumeTexSize;
float3 VolumeTexScale;
float3 VolumeTexResolution = float3(2,2,2);
float FarClip = 500;

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
    float3 ViewDir : TEXCOORD1;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uint id:SV_VERTEXID)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 0, 1);
	output.TexCoord.x = (float)(id / 2) * 2.0;
	output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;

    output.ViewDir = GetFrustumRay(id);
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

//http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
//float dBox(float3 p, float3 b)
//{
//	return length(max(abs(p), 0.0f));
//}

float GetMinDistance(float3 PositionWS)
{
	float3 relativePosition = PositionWS;

	relativePosition /= VolumeTexSize;

	float initialDistance = 0;

	//Out of bounds
	if (abs(MaxManhattan(relativePosition)) > 1) 
	{
		float3 clamped = clamp(relativePosition, float3(-1,-1,-1), float3(1,1,1));

		initialDistance = length(relativePosition - clamped) * VolumeTexSize * 0.5f; //AGAIN //////////////////////////////////////////////////////
		
		relativePosition = clamped;
	}

	//Get Texcoordinate from that, normalize to texcoords first
	relativePosition = (relativePosition + float3(1, 1, 1)) * 0.5f * (VolumeTexResolution - float3(1, 1, 1));

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
	x1y0z0 = VolumeTex.Load(int3(x + 1, y, 0)).r;
	x0y1z0 = VolumeTex.Load(int3(x, y + 1, 0)).r;
	x1y1z0 = VolumeTex.Load(int3(x + 1, y + 1, 0)).r;

	x += VolumeTexResolution.x;
	x0y0z1 = VolumeTex.Load(int3(x, y, 0)).r;
	x1y0z1 = VolumeTex.Load(int3(x + 1, y, 0)).r;
	x0y1z1 = VolumeTex.Load(int3(x, y + 1, 0)).r;
	x1y1z1 = VolumeTex.Load(int3(x + 1, y + 1, 0)).r;

	float lerpz0 = lerp(lerp(x0y0z0, x1y0z0, xfrac), lerp(x0y1z0, x1y1z0, xfrac), yfrac);
	float lerpz1 = lerp(lerp(x0y0z1, x1y0z1, xfrac), lerp(x0y1z1, x1y1z1, xfrac), yfrac);
	float lerpout = lerp(lerpz0, lerpz1, zfrac);

	return lerpout + initialDistance;
}

//http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
float sdBox(float3 p, float3 b)
{
	float3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float4 PixelShaderFunctionBasic(VertexShaderOutput input) : COLOR0
{
	float3 startPoint = CameraPosition;
	float3 endPoint = CameraPosition + input.ViewDir;

	float3 dir = normalize(endPoint - startPoint);

	//normalize
	//dir /= FarClip;

	//Get min dist to box
	float3 p = startPoint;

	float marchingDistance = 0;

	//Raymarch
	for (int i = 0; i < 128; i++)
	{
		const float precis = 0.005f;

		float3 q = mul(float4(p,1), VolumeTexInverseMatrix).xyz;

		//float step = sdBox(q / VolumeTexScale, VolumeTexSize) * VolumeTexScale;

		//IMPORTANT -> this won't work then!
		//CHANGE
		float step = GetMinDistance(q / VolumeTexScale) * VolumeTexScale; //AGAIN //////////////////////////////////////////////////////

		marchingDistance += step;

		if (step <= precis)  return marchingDistance / FarClip;

		if(marchingDistance > FarClip) discard;

		p += step * dir;
	}

	return float4(0,0,0, 1);

}

float4 PixelShaderFunctionDrawDistanceField(VertexShaderOutput input) : COLOR0
{
	float3 startPoint = CameraPosition;
	float3 endPoint = CameraPosition + input.ViewDir;

	float3 dir = endPoint - startPoint;
	float farClip = length(dir);

	//normalize
	dir /= farClip;
	float3 p = startPoint;

	float marchingDistance = 0;

	//Raymarch
	for (int i = 0; i < 512; i++)
	{
		const float precis = 0.0005;

		float step = GetMinDistance(p);

		marchingDistance += step;

		if (step <= precis) break;
		if(marchingDistance > farClip) discard;

		p += step * dir;
	}

	float output = marchingDistance / farClip;

	return float4(output.xxx, 1);
}

float4 PixelShaderFunctionDrawToSurface(VertexShaderOutput input) : COLOR0
{
	int3 texCoordInt = int3(input.Position.xy, 0);

	float linearDepth = DepthMap.Load(texCoordInt).r;

	float3 PositionWS = linearDepth * input.ViewDir + CameraPosition;

	//Assume axis-aligned square

	return GetMinDistance(PositionWS);
	
}

float3 GetVertex(float vertexIndex)
{
	return VolumeTex.Load(int3(vertexIndex % TriangleTexResolution.x, vertexIndex / TriangleTexResolution.x, 0)).xyz;
}

float dot2(in float3 v) { return dot(v, v); }

float4 PixelShaderFunctionGenerateSDF(VertexShaderOutput input) : COLOR0
{
	//Generate SDFs
	float2 pixel = trunc(input.Position.xy);

	//Get 3d position from our position
	
	float x = trunc(pixel.x % VolumeTexResolution.x);
	float y = pixel.y;
	float z = trunc(pixel.x / VolumeTexResolution.x);

	//Used as offset here
	float3 offset = VolumeTexPositionWS;
	float3 p = offset + float3(x * VolumeTexSize.x * 2.0f / (VolumeTexResolution.x - 1) - VolumeTexSize.x * 1.0f,
		y * VolumeTexSize.y * 2.0f / (VolumeTexResolution.y - 1) - VolumeTexSize.y * 1.0f,
		z * VolumeTexSize.z * 2.0f / (VolumeTexResolution.z - 1) - VolumeTexSize.z * 1.0f);

	//Go through all triangles and find the closest one.

	int vertexIndex = 0;
	
	float minvalue = 100000;

	for (int i = 0; i < TriangleAmount; i++, vertexIndex+=3)
	{
		float3 a = GetVertex(vertexIndex);
		float3 b = GetVertex(vertexIndex + 1);
		float3 c = GetVertex(vertexIndex + 2);

		float3 ba = b - a; float3 pa = p - a;
		float3 cb = c - b; float3 pb = p - b;
		float3 ac = a - c; float3 pc = p - c;
		float3 nor = cross(ba, ac);

		float value = 
			(sign(dot(cross(ba, nor), pa)) +
				sign(dot(cross(cb, nor), pb)) +
				sign(dot(cross(ac, nor), pc))<2.0f)
			?
			min(min(
				dot2(ba*saturate(dot(ba, pa) / dot2(ba)) - pa),
				dot2(cb*saturate(dot(cb, pb) / dot2(cb)) - pb)),
				dot2(ac*saturate(dot(ac, pc) / dot2(ac)) - pc))
			:
			dot(nor, pa)*dot(nor, pa) / dot2(nor);

		//Inside?
		float signum = sign(dot(pa, nor));

		value = abs(value) * signum;

		if (abs(value) < abs(minvalue))
		{
			minvalue = value;
		}
	}

	float output = sqrt(abs(minvalue)) * sign(minvalue);

	//Check neighbors
	

	return output.xxxx;

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

technique DrawToSurface
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunctionDrawToSurface();
	}
}

technique GenerateSDF
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunctionGenerateSDF();
	}
}


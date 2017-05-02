//Environment cube maps, TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"

float3 CameraPosition;

Texture2D VolumeTex;
float3 VolumeTexSize;
float3 VolumeTexResolution = float3(2, 2, 2);
Texture2D DepthMap;

//Generation
float2 TriangleTexResolution;
float TriangleAmount;
float3 MeshOffset;


#define MAXINSTANCES 40

float4x4 InstanceInverseMatrix[MAXINSTANCES];
float3 InstanceScale[MAXINSTANCES];

float InstancesCount = 0;

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


float InterpolateSDF(float3 texCoords)
{
	//x and y are correct, the z determines how much x is shifted.
	float x = trunc(texCoords.x);
	float xfrac = frac(texCoords.x);
	float y = trunc(texCoords.y);
	float yfrac = frac(texCoords.y);
	float z = trunc(texCoords.z);
	float zfrac = frac(texCoords.z);

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
	return lerp(lerpz0, lerpz1, zfrac);
}

float GetMinDistance(float3 Position)
{
	float3 relativePosition = Position;

	relativePosition /= VolumeTexSize;

	float distanceToBounds = 0;

	//FS = field space
	float3 outsideFS;


	//Out of bounds
	if (abs(MaxManhattan(relativePosition)) > 1) 
	{
		float3 clamped = clamp(relativePosition, float3(-1,-1,-1), float3(1,1,1));
		outsideFS = (relativePosition - clamped);
		distanceToBounds = length(outsideFS * VolumeTexSize);

		relativePosition = clamped;
	}

	//Get Texcoordinate from that, normalize to texcoords first
	//These are not in [0...1] but instead in [0 ... VolumeTexResolution]
	float3 texCoords = (relativePosition + float3(1, 1, 1)) * 0.5f * (VolumeTexResolution - float3(1, 1, 1));

	float value = InterpolateSDF(texCoords);

	//Get back to our initial problem - are we outside?

	[branch]
	//must always be larger than 0 if we are outside
	if (abs(distanceToBounds) > 0)
	{
		//normalize
		//outsideFS /= distanceToBounds;

		//We went one step inside, opposite to our direction to the field
		float value2 = InterpolateSDF(texCoords - normalize(outsideFS)/*(outsideFS * VolumeTexSize) /distanceToBounds*/);
		//Same
		//float value3 = InterpolateSDF(((relativePosition - outsideFS / distanceToBounds / VolumeTexResolution*2) + float3(1, 1, 1)) * 0.5f * (VolumeTexResolution - float3(1, 1, 1)));

		//gradient, this is the change per 1 texel
		float gradient = value - value2;

		//how many texels have we advanced?
		float texelsCovered =  length(outsideFS * VolumeTexSize * VolumeTexResolution);

		return distanceToBounds + value; /// gradient; ///*texelsCovered * gradient*/value + texelsCovered * gradient;
	}


	return value;
}

//http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
float sdBox(float3 p, float3 b)
{
	float3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float FindMin(float3 ro)
{
	float minimum = FarClip;

	for (uint i = 0; i < InstancesCount; i++)
	{
		float3 q = mul(float4(ro, 1), InstanceInverseMatrix[i]).xyz;

		//Note: Minimum could be precomputed
		float dist = GetMinDistance(q / InstanceScale[i]) * min(InstanceScale[i].x, min(InstanceScale[i].y, InstanceScale[i].z));

		if (dist < minimum) minimum = dist;
	}

	return minimum;
}

float4 PixelShaderFunctionVisualizeVolume(VertexShaderOutput input) : COLOR0
{
	float3 startPoint = CameraPosition;
	float3 endPoint = CameraPosition + input.ViewDir;

	float3 dir = normalize(endPoint - startPoint);

	int3 texCoordInt = int3(input.Position.xy, 0);

	float linearDepth = DepthMap.Load(texCoordInt).r;

	//Get min dist to box
	float3 p = startPoint;

	float marchingDistance = 0;

	//Raymarch
	for (int i = 0; i < 128; i++)
	{
		const float precis = 0.005f;

		float step = FindMin(p);

		marchingDistance += step;

		if (step <= precis)  return marchingDistance / FarClip;

		if (marchingDistance > FarClip) discard;

		p += step * dir;

		//if (marchingDistance > linearDepth * FarClip) return float4(1.0, marchingDistance/FarClip, 0, 1);
	}

	return float4(0,0,0, 1);

}

float4 PixelShaderFunctionDrawShadow(VertexShaderOutput input) : COLOR0
{
	int3 texCoordInt = int3(input.Position.xy, 0);

	float linearDepth = DepthMap.Load(texCoordInt).r;

	float3 p = linearDepth * input.ViewDir + CameraPosition;

	float3 light = float3(26, 0, 10);

	float3 dir = light - p;
	float maxdist = length(dir);

	//normalize
	dir /= maxdist;

	float marchingdistance = 0;

	float t = 0.05;

	float k = 32;
	float res = 1.0f;

	//
	float step = FindMin(p);

	return float4(frac(step).xxx, 1);


	/*
	while (t<maxdist-1)
	{
		const float precis = 0.005f;

		float3 ro = p + t * dir;

		float step = FindMin(ro);

		if (step <= precis)  return float4(0.0f, 0.0f, 0.0f, 1);

		res = min(res, k * step / t);

		t += step;
	}
	

	return float4(res.xxx, 1);*/
}

float3 GetVertex(float vertexIndex)
{
	return VolumeTex.Load(int3(vertexIndex % TriangleTexResolution.x, vertexIndex / TriangleTexResolution.x, 0)).xyz;
}

float dot2(in float3 v) { return dot(v, v); }

float RayCast(float3 a, float3 b, float3 c, float3 origin, float3 dir)
{
	//https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
	const float EPSILON = 0.0000001f;

	float3 edge1 = b - a;
	float3 edge2 = c - a;
	float3 pvec = cross(dir, edge2);
	float det = dot(edge1, pvec);

	if (det > -EPSILON && det < EPSILON) return 0.0f;

	float inv_det = 1.0f / det;
	float3 tvec = origin - a;
	float u = dot(tvec, pvec) * inv_det;

	if (u < 0 || u > 1) return 0.0f;
	float3 qvec = cross(tvec, edge1);
	float v = dot(dir, qvec) * inv_det;
	if (v < 0 || u + v > 1) return 0.0f;

	float t = dot(edge2, qvec) * inv_det;

	if (t > EPSILON) return 1.0f;

	return 0.0f;
}

float4 PixelShaderFunctionGenerateSDF(VertexShaderOutput input) : COLOR0
{
	//Generate SDFs
	float2 pixel = input.Position.xy;

	//Get 3d position from our position
	
	float x = trunc(pixel.x % VolumeTexResolution.x);
	float y = trunc(pixel.y);
	float z = trunc(pixel.x / VolumeTexResolution.x);

	//Used as offset here
	float3 offset = MeshOffset;
	float3 p = offset + float3(x * VolumeTexSize.x * 2.0f / (VolumeTexResolution.x - 1) - VolumeTexSize.x * 1.0f,
		y * VolumeTexSize.y * 2.0f / (VolumeTexResolution.y - 1) - VolumeTexSize.y * 1.0f,
		z * VolumeTexSize.z * 2.0f / (VolumeTexResolution.z - 1) - VolumeTexSize.z * 1.0f);

	//Go through all triangles and find the closest one.

	int vertexIndex = 0;
	
	float minvalue = 100000;

	float3 ray = float3(1, 0, 0);
	float intersections = 0.0f;


	//Ray cast to find out if we are inside or outside... complicated but safe

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

		intersections += RayCast(a, b, c, p, ray);
		//Inside?
		/*float signum = sign(dot(pa, nor));

		value = abs(value) * signum;*/

		if (abs(value) < abs(minvalue))
		{
			minvalue = value;
		}
	}

	int signum = intersections % 2 == 0 ? 1 : -1;

	float output = sqrt(abs(minvalue)) * signum;

	//Check neighbors
	

	return output.xxxx;

}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique Volume
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunctionVisualizeVolume();
    }
}

technique Distance
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunctionDrawShadow();
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


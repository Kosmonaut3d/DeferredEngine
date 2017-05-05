//Environment cube maps, TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"
#include "../Common/sdf.fx"

float3 CameraPosition;

Texture2D DepthMap;

//Generation
float2 TriangleTexResolution;
float TriangleAmount;
float3 MeshOffset;

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
	/*float step = FindMin(p);

	return float4(frac(step).xxx, 1);
*/

	
	while (t<maxdist-1)
	{
		const float precis = 0.005f;

		float3 ro = p + t * dir;

		float step = FindMin(ro);

		if (step <= precis)  return float4(0.0f, 0.0f, 0.0f, 1);

		res = min(res, k * step / t);

		t += step;
	}
	

	return float4(res.xxx, 1);
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

	float3 baseSize = VolumeTexSize[0];
	float3 resolution = VolumeTexResolution[0];
	
	float x = trunc(pixel.x % resolution.x);
	float y = trunc(pixel.y);
	float z = trunc(pixel.x / resolution.x);

	//Used as offset here
	float3 offset = MeshOffset;
	float3 p = offset + float3(x * baseSize.x * 2.0f / (resolution.x - 1) - baseSize.x * 1.0f,
		y * baseSize.y * 2.0f / (resolution.y - 1) - baseSize.y * 1.0f,
		z * baseSize.z * 2.0f / (resolution.z - 1) - baseSize.z * 1.0f);

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


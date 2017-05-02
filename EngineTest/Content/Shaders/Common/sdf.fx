//SDFs, TheKosmonaut 2017

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

Texture2D VolumeTex;

float3 CameraPosition;

float3 VolumeTexPositionWS;
float4x4 VolumeTexInverseMatrix;
float3 VolumeTexSize;
float3 VolumeTexScale;
float3 VolumeTexResolution = float3(2,2,2);
float FarClip;

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

float GetMinDistance(float3 PositionWS)
{
	float3 relativePosition = PositionWS;

	relativePosition /= VolumeTexSize;

	float initialDistance = 0;

	//Out of bounds
	if (abs(MaxManhattan(relativePosition)) > 1) 
	{
		float3 clamped = clamp(relativePosition, float3(-1,-1,-1), float3(1,1,1));

		initialDistance = length((relativePosition - clamped)*VolumeTexSize); //AGAIN //////////////////////////////////////////////////////
		
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

//origin, destination, distance(dest-ori), softness factor
float RaymarchSoft(float3 p, float3 destination, float distanceToDestination, float k)
{
	float3 dir = destination - p;
	float maxdist = distanceToDestination;

	//normalize
	dir /= maxdist;

	float marchingdistance = 0;

	float t = 0.05;
	float res = 1.0f;

	while (t<maxdist-1)
	{
		const float precis = 0.005f;

		float3 ro = p + t * dir;

		float3 q = mul(float4(ro, 1), VolumeTexInverseMatrix).xyz;

		//float step = sdBox(q / VolumeTexScale, VolumeTexSize) * VolumeTexScale;
		
		// should step always be < distance to light?
		float step = GetMinDistance(q / VolumeTexScale) * min(VolumeTexScale.x, min(VolumeTexScale.y, VolumeTexScale.z));

		//Idea

		if (step <= precis)  return 0.0f;

		res = min(res, k * step / t);

		t += step;
	}

	return res;
}

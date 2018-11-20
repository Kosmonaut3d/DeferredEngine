//SDFs, TheKosmonaut 2017

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define MAXTEXTURES 40

Texture2D VolumeTex;
float3 VolumeTexSize[MAXTEXTURES];
float4 VolumeTexResolution[MAXTEXTURES];

#define MAXINSTANCES 40

float4x4 InstanceInverseMatrix[MAXINSTANCES];
float3 InstanceScale[MAXINSTANCES];
float InstanceSDFIndex[MAXINSTANCES];

float InstancesCount = 0;

float FarClip = 500;

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

float InterpolateSDF(float3 texCoords, float3 resolution)
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

	x += resolution.x * z;
	x0y0z0 = VolumeTex.Load(int3(x, y, 0)).r;
	x1y0z0 = VolumeTex.Load(int3(x + 1, y, 0)).r;
	x0y1z0 = VolumeTex.Load(int3(x, y + 1, 0)).r;
	x1y1z0 = VolumeTex.Load(int3(x + 1, y + 1, 0)).r;

	x += resolution.x;
	x0y0z1 = VolumeTex.Load(int3(x, y, 0)).r;
	x1y0z1 = VolumeTex.Load(int3(x + 1, y, 0)).r;
	x0y1z1 = VolumeTex.Load(int3(x, y + 1, 0)).r;
	x1y1z1 = VolumeTex.Load(int3(x + 1, y + 1, 0)).r;

	float lerpz0 = lerp(lerp(x0y0z0, x1y0z0, xfrac), lerp(x0y1z0, x1y1z0, xfrac), yfrac);
	float lerpz1 = lerp(lerp(x0y0z1, x1y0z1, xfrac), lerp(x0y1z1, x1y1z1, xfrac), yfrac);
	return lerp(lerpz0, lerpz1, zfrac);
}

float GetMinDistance(float3 Position, float SDFIndex)
{
	float3 samplePosition = Position;

	float3 baseSize = VolumeTexSize[SDFIndex];
	float3 resolution = VolumeTexResolution[SDFIndex].xyz;
	float y_offset = VolumeTexResolution[SDFIndex].w;

	samplePosition /= baseSize;

	float distanceToBounds = 0;

	//FS = field space
	float3 outsideFS;


	//Out of bounds
	if (abs(MaxManhattan(samplePosition)) > 1)
	{
		float3 clamped = clamp(samplePosition, float3(-1, -1, -1), float3(1, 1, 1));
		outsideFS = (samplePosition - clamped);
		distanceToBounds = length(outsideFS * baseSize);

		samplePosition = clamped;
	}

	//Get Texcoordinate from that, normalize to texcoords first
	//These are not in [0...1] but instead in [0 ... VolumeTexResolution]
	float3 texCoords = (samplePosition + float3(1, 1, 1)) * 0.5f * (resolution - float3(1, 1, 1));

	float value = InterpolateSDF(texCoords + float3(0, y_offset, 0), resolution);

	//Get back to our initial problem - are we outside?

	[branch]
	//must always be larger than 0 if we are outside
	if (abs(distanceToBounds) > 0)
	{
		////normalize
		////outsideFS /= distanceToBounds;

		return distanceToBounds + value; /// gradient; ///*texelsCovered * gradient*/value + texelsCovered * gradient;
	}

	return value;
}

//returns true if position is inside SDF of given index
bool GetIsInsideVolume(float3 Position, float SDFIndex)
{
	float3 samplePosition = Position;

	float3 baseSize = VolumeTexSize[SDFIndex];

	samplePosition /= baseSize;

	//Inside bounds?
	return (abs(MaxManhattan(samplePosition)) < 1.0f);
}

//http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
float sdBox(float3 p/*, float3 b*/)
{
	float3 d = abs(p)/* - b*/;
	return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float FindMin(float3 ro)
{
	float minimum = FarClip;

	/*for (uint i = 0; i < InstancesCount; i++)
	{
		float3 q = mul(float4(ro, 1), InstanceInverseMatrix[i]).xyz;

		float dist = sdBox(q / InstanceScale[testId]) * min(InstanceScale[testId].x, min(InstanceScale[testId].y, InstanceScale[testId].z));

		if (dist < minimum)
		{
			minimum = dist;
			testId = i;
		}
	}

	float3 q = mul(float4(ro, 1), InstanceInverseMatrix[testId]).xyz;

	minimum = GetMinDistance(q / InstanceScale[testId], InstanceSDFIndex[testId]) * min(InstanceScale[testId].x, min(InstanceScale[testId].y, InstanceScale[testId].z));*/

	for (float i = 0; i < InstancesCount; i++)
	{
		float3 q = mul(float4(ro, 1), InstanceInverseMatrix[i]).xyz;

		//Note: Minimum could be precomputed
		float dist = GetMinDistance(q / InstanceScale[i], InstanceSDFIndex[i]) * min(InstanceScale[i].x, min(InstanceScale[i].y, InstanceScale[i].z));

		if (dist < minimum) minimum = dist;
	}

	return minimum;
}


float FindMinBoundingBox(float3 ro)
{
	for (float i = 0; i < InstancesCount; i++)
	{
		float3 q = mul(float4(ro, 1), InstanceInverseMatrix[i]).xyz;

		//Note: Minimum could be precomputed
		bool dist = GetIsInsideVolume(q / InstanceScale[i], InstanceSDFIndex[i]);

		if (dist) return i;
	}

	return -1;
}

//origin, destination, distance(dest-ori), softness factor
float RaymarchAO(float3 p, float3 destination, float distanceToDestination)
{
	float3 dir = destination - p;
	float maxdist = distanceToDestination;

	const float precis = 0.005f;

	
	//whatever

	//for static
	float minVis = 0;

	const float iterations = 8.0f;
	const float rcp_iterations = 1 / iterations;

	float stepsize = maxdist * rcp_iterations;

	for (float i = 1; i <= iterations; i++)
	{
		float step = FindMin(p + dir * i *rcp_iterations);

		//float value = step / (stepsize * i);

		////modify
		//minVis += value * rcp_iterations;

		if (step < precis /*|| step < stepsize*/) return 0;



	}


	return 1;
	
	

	
	//Idea - "find smallest angle"

	/*
	//normalize
	dir /= maxdist;

	float t = 0.15;
	float minstep = 0.15f;
	float minVis = 0;

	while (t<maxdist)
	{
		float3 ro = p + t * dir;

		float laststep = minstep;

		minstep = FindMin(ro);

		minVis += saturate(minstep / t) * laststep / maxdist;

		if (minstep <= precis) break;

		t += minstep;
	}

	if (minstep > precis)
	{
		t = maxdist;
		minVis += saturate(FindMin(destination) / t) * minstep / maxdist;
	}

	//const float PI4 = 12.566f;

	////To sphere
	//minVis = minVis*minVis*minVis;

	return minVis;*/
}

//origin, destination, distance(dest-ori), softness factor
float RaymarchSoft(float3 p, float3 destination, float distanceToDestination, float k)
{
	float3 dir = destination - p;
	float maxdist = distanceToDestination;

	//normalize
	dir /= maxdist;

	//float marchingdistance = 0;

	float t = 0.15f;
	float res = 1.0f;

	while (t<maxdist - 1)
	{
		const float precis = 0.005f;

		float3 ro = p + t * dir;

		float step = FindMin(ro);

		if (step <= precis)  return 0;

		res = min(res, k * step / t);

		t += step;
	}

	return res;
}

//origin, destination, distance(dest-ori), softness factor
float Subsurface(in float3 p, in float3 destination, in float maxdist, out float3 exitpoint)
{
	float3 dir = destination - p;

	//normalize
	dir /= maxdist;

	float t = 1;

	float step = -1;

	const float precis = -0.005f;

	while (step < precis)
	{
		float3 ro = p + t * dir;

		step = FindMin(ro);

		t -= step;
	}

	exitpoint = p + (t + 0.45f) * dir ;

	return t;
}

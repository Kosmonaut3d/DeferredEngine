//Helper functions for a deferred Renderer shader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

float3 FrustumCorners[4]; //In Viewspace!

float2 InverseResolution = float2(1.0f / 1280.0f, 1.0f / 800.0f);

#define SAMPLE_COUNT 9
static float2 SampleOffsets[9] =
{
    float2(-1, -1), float2(0, -1), float2(1, -1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, 1), float2(0, 1), float2(1, 1)
};

static float SampleWeights[9] =
{
    0.077847f,
    0.123317f,
    0.077847f,
    0.123317f,
    0.195346f,
    0.123317f,
    0.077847f,
    0.123317f,
    0.077847f,
};

float Time = 0;
float3 randomNormal2(float2 tex)
{
	tex = frac(tex * Time);
	float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f)) * 2 - 1;
	float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f)) * 2 - 1;
	float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f)) * 2 - 1;
	return normalize(float3(noiseX, noiseY, noiseZ));
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  HELPER FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

float3 GetFrustumRay(float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

float3 GetFrustumRay(uint id)
{
	//Bottom left
	if (id < 1)
	{
		return FrustumCorners[2];
	}
	else if (id < 2) //Top left
	{
		return FrustumCorners[2] + (FrustumCorners[0] - FrustumCorners[2]) * 2;
	}
	else
	{
		return FrustumCorners[2] + (FrustumCorners[3] - FrustumCorners[2]) * 2;
	}

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//GBUFFER

//Simply converting from [-1, 1] to [0, 1]

//float3 encode(float3 n)
//{
//	return   0.5f * (n + 1.0f);
//}
//
//float3  decode(float3 n)
//{
//	return 2.0f * n.xyz - 1.0f;
//}

//Leaving out the third component, since view space normals have a z that always looks towards the camera
//it's length can be inferred from the other 2 components.

//float3 encode(float3 n)
//{
//	return float3((n.xy + float2(1.0f, 1.0f)) * 0.5f, 0);
//}

//float3 decode(float3 enc)
//{
//	float3 n;
//	n.xy = enc.xy * 2.0f - float2(1.0f, 1.0f);
//	n.z = sqrt(1.0f - dot(n.xy, n.xy));
//	return n;
//}

//Normal encodings http://aras-p.info/texts/CompactNormalStorage.html
//maybe worth checking out https://knarkowicz.wordpress.com/2014/04/16/octahedron-normal-vector-encoding/
//Spheremap Transform
float3 encode(float3 n)
{
	float f = sqrt(8 * n.z + 8);
	return float3(n.xy / f + 0.5, 0);
}

float3 decode(float3 enc)
{
	float2 fenc = enc.xy * 4 - 2;
	float f = dot(fenc, fenc);
	float g = sqrt(1 - f / 4);
	float3 n;
	n.xy = fenc*g;
	n.z = 1 - f / 2;
	return n;
}

//Since we encode in fp16 renderformat we can use values > 1.
//Since I don't plan to have a lot of mattypes I use the mattype as the int and the metalness as the fractional
float encodeMetallicMattype(float metalness, float /*int*/ mattype)
{
	return metalness * 0.9f + mattype;
}

float decodeMetalness(float input)
{
	const float recip = 1.0f / 0.9f;
	return saturate(frac(input) * recip);
}

float decodeMattype(float input)
{
	return trunc(input);
}

//Consider using bitmaps, or moving to another, high-precision channel
//float encodeMetallicMattype(float metalness, float mattype)
//{
//	return metalness * 0.1f * 0.5f + mattype * 0.1f;
//}
//
//float decodeMetalness(float input)
//{
//	input *= 10;
//	return frac(input) * 2;
//}
//
//float decodeMattype(float input)
//{
//	input *= 10;
//	return trunc(input);
//}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//SHADOW MAP STRIP

float2 GetSampleOffsetY(float2 coord, float yOffset)
{
	const float sixth = 1.0f / 6;

	float yOffsetNormalized = yOffset * sixth;

	float2 output = coord + float2(0, yOffsetNormalized);

	//If inside, we are done
	
	if (trunc(output.y * 6) == trunc(coord.y * 6) && output.y > 0) return output;

	//PositiveX
	if (coord.y < sixth)
	{
		//going down
		if (yOffset > 0)
		{
			//straight up to slice 5
			return coord + float2(0, (yOffset + 4) * sixth);
		}
		else //flip x, then go down in slice 4 (aka top view)
		{
			return float2(1-coord.x, -(coord.y + yOffsetNormalized)  + 4 * sixth);
		}
	}
	else if (coord.y < 2 * sixth) //Negative X
	{
		//going down
		if (yOffset > 0)
		{
			
			return float2(1 - coord.x, 8 * sixth - output.y);
		}
		else 
		{
			return float2(coord.x, output.y + 4*sixth);
		}
	}
	else if (coord.y < 3 * sixth) //Positive y
	{
		//going down
		if (yOffset > 0)
		{

			return float2((output.y - 3*sixth)*6, (6-coord.x) * sixth);
		}
		else
		{
			return float2(1-(2 * sixth-output.y)*6, (5 - coord.x) * sixth);
		}
	}
	else if (coord.y < 4 * sixth) //negative y
	{
		//going down
		if (yOffset > 0)
		{

			return float2(1 - (output.y - 4 * sixth) * 6, (5 + coord.x) * sixth);
		}
		else
		{
			return float2(3*sixth - output.y, (coord.x+4)*sixth);
		}
	}
	else if (coord.y < 5 * sixth) //Positive z
	{
		//going down
		if (yOffset > 0)
		{

			return float2(coord.x, output.y-4*sixth);
		}
		else
		{
			return float2(1 - coord.x, -(output.y - 4 * sixth));
		}
	}
	else
	{
		//going down
		if (yOffset > 0)
		{

			return float2(1 - coord.x, 8 * sixth-(output.y));
		}
		else
		{
			return float2(coord.x, (output.y - 4 * sixth));
		}
	}

	return coord;
}

// Our sampler for the texture, which is just going to be pretty simple
float2 GetSampleOffsetX(float2 coord, float xOffset)
{
	float2 output = coord + float2(xOffset, 0);

	//If inside, we are done
	if (output.x == saturate(output.x)) return output;

	// a possible precision problem?
	const float sixth = 1.0f / 6;

	float overlap = output.x - saturate(output.x);

	//Otherwise we need to traverse to other projections/slides
	if (output.y < sixth)
	{
		//Positive X 
		//Go to the right
		if (overlap > 0)
		{
			return float2(overlap, coord.y + 3 * sixth);
		}
		else
		{
			return float2(1 + overlap, coord.y + 2 * sixth);
		}
	}
	else if (output.y < 2 * sixth)
	{
		//Negative X
		if (overlap > 0)
		{
			return float2(overlap, coord.y + sixth);
		}
		else
		{
			return float2(1 + overlap, coord.y + 2 * sixth);
		}
	}
	else if (output.y < 3 * sixth)
	{
		//Positive Y 
		//Go to the right
		if (overlap > 0)
		{
			return float2(overlap, coord.y - 2 * sixth); //pos->
		}
		else
		{
			return float2(1 + overlap, coord.y - sixth);
		}
	}
	else if (output.y < 4 * sixth)
	{
		//Negative Y 
		//Go to the right
		if (overlap > 0)
		{
			return float2(overlap, coord.y - 2 * sixth);
		}
		else
		{
			return float2(1 + overlap, coord.y - 3 * sixth);
		}
	}
	else if (output.y < 5 * sixth)
	{
		//Positive Z 
		if (overlap > 0) //Goes into Positive Y
		{
			float2 flip = float2((coord.y - 4 * sixth) * 6, (overlap + 2) * sixth);
			flip.x = 1 - flip.x;
			return flip;
		}
		else
		{
			float2 flip = float2((coord.y - 4 * sixth) * 6, (-overlap + 3) * sixth);
			//flip.x = 1 - flip.x;
			return flip;
		}
	}
	else
	{
		if (overlap > 0) //Goes into Positive Y
		{
			float2 flip = float2((coord.y - 5 * sixth) * 6, (-overlap + 4) * sixth);
			/*flip.x = 1 - flip.x;*/
			return flip;
		}
		else
		{
			float2 flip = float2((coord.y - 5 * sixth) * 6, (overlap + 3) * sixth);
			flip.x = 1 - flip.x;
			return flip;
		}
	}

	return coord;
}

float2 GetSampleOffset(float2 coord, float2 offset)
{
	coord = GetSampleOffsetX(coord, offset.x);
	coord = GetSampleOffsetY(coord, offset.y);

	return coord;
}

//vec3 doesn't have to be normalized,
//Translates from world space vector to a coordinate inside our 6xsize shadow map 
float2 GetSampleCoordinate(float3 vec3)
{
	float2 coord;
	float slice;
	vec3.z = -vec3.z;

	if (abs(vec3.x) >= abs(vec3.y) && abs(vec3.x) >= abs(vec3.z))
	{
		vec3.y = -vec3.y;
		if (vec3.x > 0) //Positive X
		{
			slice = 0;
			vec3 /= vec3.x;
			coord = vec3.yz;
		}
		else
		{
			vec3.z = -vec3.z;
			slice = 1; //Negative X
			vec3 /= vec3.x;
			coord = vec3.yz;
		}
	}
	else if (abs(vec3.y) >= abs(vec3.x) && abs(vec3.y) >= abs(vec3.z))
	{
		if (vec3.y > 0)
		{
			slice = 2; // PositiveY;
			vec3 /= vec3.y;
			coord = vec3.xz;
		}
		else
		{
			vec3.z = -vec3.z;
			slice = 3; // NegativeY;
			vec3 /= vec3.y;
			coord = vec3.xz;
		}
	}
	else
	{
		vec3.y = -vec3.y;
		//Z
		if (vec3.z < 0) //Pos Z
		{
			slice = 4;
			vec3 /= vec3.z;
			coord = vec3.yx;
		}
		else
		{
			vec3.x = -vec3.x;
			slice = 5; // NegativeY;
			vec3 /= vec3.z;
			coord = vec3.yx;
		}
	}


	// a possible precision problem?
	const float sixth = 1.0f / 6;

	//now we are in [-1,1]x[-1,1] space, so transform to texCoords
	coord = (coord + float2(1, 1)) * 0.5f;

	//now transform to slice position
	coord.y = coord.y * sixth + slice * sixth;
	return coord;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//LIGHTING

float3 SpecularCookTorrance(float NdotL, float3 normal, float3 negativeLightDirection, float3 cameraDirectionP, float diffuseIntensity, float3 diffuseColor, float f0, float roughness)
{
    float3 specular = float3(0, 0, 0);

    [branch]
    if (NdotL > 0.0f)
    {
        float3 halfVector = normalize(negativeLightDirection + cameraDirectionP);

        float NdotH = saturate(dot(normal, halfVector));
        float NdotV = saturate(dot(normal, cameraDirectionP));
        float VdotH = saturate(dot(cameraDirectionP, halfVector));
        float mSquared = roughness * roughness;


        //Trowbridge-Reitz
        float D_lowerTerm = (NdotH * NdotH * (mSquared * mSquared - 1) + 1);
        float D = mSquared * mSquared / (3.14 * D_lowerTerm * D_lowerTerm);

        //fresnel        (Schlick)
        float F = pow(1.0 - VdotH, 5.0);
        F *= (1.0 - f0);
        F += f0;

        //Schlick Smith
        float k = (roughness + 1) * (roughness + 1) / 8;
        float g_v = NdotV / (NdotV * (1 - k) + k);
        float g_l = NdotL / (NdotL * (1 - k) + k);

        float G = g_l * g_v;

        specular = max(0, (D * F * G) / (4 * NdotV * NdotL)) * diffuseIntensity * diffuseColor * NdotL; //todo check this!!!!!!!!!!! why 3.14?j only relevant if we have it 
        
        //http://malcolm-mcneely.co.uk/blog/?p=214
    }
    return specular;
}

float3 DiffuseOrenNayar(float NdotL, float3 normal, float3 lightDirection, float3 cameraDirection, float lightIntensity, float3 lightColor, float roughness)
{
    const float PI = 3.14159;
    
    // calculate intermediary values
    float NdotV = dot(normal, cameraDirection);

    float angleVN = acos(NdotV);
    float angleLN = acos(NdotL);
    
    float alpha = max(angleVN, angleLN);
    float beta = min(angleVN, angleLN);
    float gamma = dot(cameraDirection - normal * NdotV, lightDirection - normal * NdotL);
    
    float roughnessSquared = roughness * roughness;
    
    // calculate A and B
    float A = 1.0 - 0.5 * (roughnessSquared / (roughnessSquared + 0.57));

    float B = 0.45 * (roughnessSquared / (roughnessSquared + 0.09));
 
    float C = sin(alpha) * tan(beta);
    
    // put it all together
    float L1 = max(0.0, NdotL) * (A + B * max(0.0, gamma) * C);
    
    // get the final color 
    return L1 * lightColor * lightIntensity / 4;
}

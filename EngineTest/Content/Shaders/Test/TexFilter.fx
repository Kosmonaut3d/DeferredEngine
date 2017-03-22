//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing

Texture2D ScreenTexture;
float4x4 WorldViewProj;

//float2 GetSampleOffset(float2 coord, float2 offset)
//{
//	coord = GetSampleOffsetX(coord, offset.x);
//	coord = GetSampleOffsetY(coord, offset.y);
//
//	return coord;
//}

float2 GetSampleOffset(float2 coord, float yOffset)
{
	float2 output = coord + float2(0, yOffset);

	//If inside, we are done
	if (trunc(output.y * 6) == trunc(coord.y * 6) && output.y > 0) return output;


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
			return float2(1+overlap, coord.y + 2 * sixth);
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
			return float2(1+overlap, coord.y + 2 * sixth);
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
			return float2(1+overlap, coord.y - sixth);
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
			return float2(1+overlap, coord.y - 3 * sixth);
		}
	}
	else if (output.y < 5 * sixth)
	{
		//Positive Z 
		if (overlap > 0) //Goes into Positive Y
		{
			float2 flip = float2((coord.y - 4 * sixth)*6, (overlap + 2) * sixth);
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
			float2 flip = float2((coord.y - 5 * sixth) * 6, (- overlap + 4) * sixth);
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

	//now we are in [-1,1]x[-1,1] space, so transform to texCoords
	coord = (coord + float2(1, 1)) * 0.5f;

	//now transform to slice position
	coord.y = coord.y * 1 / 6 + slice * 1 / 6;
	return coord;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = mul(input.Position, WorldViewProj);
	output.Normal = input.Normal;
	return output;
}

//------------------------ PIXEL SHADER ----------------------------------------

float4 BasePixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
	//filter
	float2 coord = GetSampleCoordinate(input.Normal);
	float3 base;

	/*for (int i = 0; i < 10; i++)
	{
		base += ScreenTexture.Load(int3(GetSampleOffset(coord, i / 1024.0f ) * float2(512.0, 3072.0)*2, 0)).rgb;
	}*/
	for (int i = 0; i < 20; i++)
	{
		base += ScreenTexture.Load(int3(GetSampleOffset(coord, -i / 1024.0f) * float2(512.0, 3072.0) * 2, 0)).rgb;
	}
	/*for (i = 0; i < 40; i++)
	{
		base += ScreenTexture.Load(int3(GetSampleOffset(GetSampleCoordinate(input.Normal), i / 256.0f) * float2(256.0, 1536.0), 0)).rgb;
	}*/

	base /= 20;

	return float4(base,1);
}


//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader

technique Base
{
	pass Pass1
	{

		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 BasePixelShaderFunction();
	}
}
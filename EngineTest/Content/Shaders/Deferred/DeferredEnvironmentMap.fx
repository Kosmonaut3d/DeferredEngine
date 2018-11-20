//Environment cube maps, TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/helper.fx"
#include "../Common/sdf.fx"

//We want to get from VS to WS. Usually this would mean an inverted VS. To get to 3x3 it's useful to use TI on this one.
//So it's T I I = T
float3x3 TransposeView;

Texture2D AlbedoMap;
Texture2D NormalMap;
Texture2D ReflectionMap;

//SDF
bool UseSDFAO;
Texture2D DepthMap;

float2 Resolution = { 1280, 800 };

float3 SkyColor = float3(0.1385, 0.3735f, 0.9805f);
float3 CameraPositionWS;

bool FireflyReduction;
float FireflyThreshold = 0.1f;

float EnvironmentMapSpecularStrength = 1.0f;
float EnvironmentMapSpecularStrengthRcp = 1.0f;
float EnvironmentMapDiffuseStrength = 0.2f;

TextureCube ReflectionCubeMap;
SamplerState ReflectionCubeMapSampler
{
    texture = <ReflectionCubeMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

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

float GetLuma(float3 rgb)
{
	return (0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b)*10;
}

float4 GetSSR(float2 TexCoord)
{
	
	//Just a bit shorter
	int3 texCoord = int3(TexCoord * Resolution, 0);

	//Get our current Position in viewspace
	float4 similarSampleAcc = ReflectionMap.Load(texCoord);

	float alpha = similarSampleAcc.a;

	if(similarSampleAcc.a <= 0.01) return float4(0,0,0,0);

	if (!FireflyReduction) 
		return similarSampleAcc;

	float similarLuma = GetLuma(similarSampleAcc.rgb);

	//Ignore rest for dark values
	//if (similarLuma < 0.1f) return similarSampleAcc;

	float4 neighbor;
	float neighborLuma;

	float similarSamples = 1;

	float4 differentSampleAcc;
	float differentSamples = 0;

	[loop]
	for (int x = -2; x <= 2; x++)
	{
		for (int y = -2; y <= 2; y++)
		{
			//Don't sample mid again
			if (y == 0 && x == 0) continue;

			neighbor = ReflectionMap.Load(int3(texCoord.x + x, texCoord.y + y, 0));
			neighborLuma = GetLuma(neighbor.rgb);

			float weight = 1;

			if (abs(x) > 1 || abs(y) > 1) weight = 0.5f;

			//is similar?
			if (abs(neighborLuma - similarLuma) < FireflyThreshold)
			{
				//Join to similar samples
				similarSamples += weight;
				similarSampleAcc += neighbor * weight;
			}
			else
			{
				differentSamples += weight;
				differentSampleAcc += neighbor * weight;
			}
		}
	}

	similarSampleAcc /= similarSamples;
	differentSampleAcc /= differentSamples;

	return float4(similarSamples > differentSamples ? similarSampleAcc.rgb : differentSampleAcc.rgb, alpha);
}

//float GetNormalVariance(float2 texCoord, float3 baseNormal, float offset)
//{
//    float variance = 0;
//
//    float3 normalTest;
//    for (int i = 0; i < SAMPLE_COUNT; i++)
//    {
//        normalTest = NormalMap.Sample(normalSampler, texCoord.xy + offset*
//                     SampleOffsets[i] * InverseResolution).rgb;
//        normalTest = decode(normalTest.xyz);
//
//        variance += 1-dot(baseNormal, normalTest);
//    }
//
//    return variance/SAMPLE_COUNT;
//}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  BASE FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

PixelShaderOutput PixelShaderFunctionBasic(VertexShaderOutput input)
{
    PixelShaderOutput output;
	int3 texCoordInt = int3(input.Position.xy, 0);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Load(texCoordInt);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

	//We use this to fake a sky color in the specular component
    if (normalData.x + normalData.y <= 0.001f)
    {
            output.Diffuse = float4(0, 0, 0, 0);
			output.Specular = output.Specular = float4(SkyColor, 0) * 0.5f;/*float4(0.4072f, 0.6392f, 0.9911f, 0)*/ ;//float4(0.6706f, 0.8078f, 0.9216f,0)*0.05f; //float4(0, 0.4431f, 0.78, 0) * 0.05f;
            return output;
    }

    //get metalness
    float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
    float4 color = AlbedoMap.Load(texCoordInt);

    float metalness = decodeMetalness(normalData.b);

    float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

    //float materialType = decodeMattype(color.a);

	//The incoming vector from the camera //EDIT: In world space now
    float3 incident = normalize(input.ViewDir - CameraPositionWS);

	//Transform the reflectionVector from VS to WS
	normal = mul(normal, TransposeView);

	//The reflected vector which points to our cube map
    float3 reflectionVector = reflect(incident, normal);

	//Fresnel
    float VdotH = saturate(dot(normal, incident));
    float fresnel = pow(1.0 - VdotH, 5.0);
    fresnel *= (1.0 - f0);
    fresnel += f0;

    reflectionVector.z = -reflectionVector.z;

    //roughness from 0.05 to 0.5, coarsest of approximations
    float mip = roughness / 0.04f;

	float4 specularReflection = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, mip);

	specularReflection *= (1 - roughness) * fresnel; //* NdotC * NdotC * NdotC;

	float4 diffuseReflection = ReflectionCubeMap.SampleLevel(ReflectionCubeMapSampler, reflectionVector, 9);

	diffuseReflection *= (roughness) * fresnel; //* NdotC * NdotC * NdotC;

	//Sample our screen space reflection map and use the environment map only as fallback
	float4 ssreflectionMap = GetSSR(input.TexCoord);
	
	if (ssreflectionMap.a > 0) specularReflection.rgb = ssreflectionMap.rgb * EnvironmentMapSpecularStrengthRcp;

	float ao = 1;

	[branch]
	if (UseSDFAO)
	{
		//Compute WS position 
		float linearDepth = DepthMap.Load(texCoordInt).r;
		float3 PositionWS = CameraPositionWS + linearDepth * input.ViewDir;

		float3 aoDirection = normal;

		float3 random = randomNormal2(input.Position.xy / 2000.0f);

		if (dot(random, normal) < 0) random = -random;

		aoDirection = random;

		ao = RaymarchAO(PositionWS, PositionWS + normalize(aoDirection) * 10, 10);

		ao = smoothstep(0, 1, ao);
	}
	
    output.Diffuse = float4(diffuseReflection.xyz, 0) * EnvironmentMapDiffuseStrength * ao;
    output.Specular = float4(specularReflection.xyz, 0) *EnvironmentMapSpecularStrength * (ao * 0.5f + 0.5f);

    return output;
}

PixelShaderOutput PixelShaderFunctionSky(VertexShaderOutput input)
{
	PixelShaderOutput output;
	int3 texCoordInt = int3(input.Position.xy, 0);

	//get normal data from the NormalMap
	float4 normalData = NormalMap.Load(texCoordInt);

	//tranform normal back into [-1,1] range
	float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

											//We use this to fake a sky color in the specular component
	if (normalData.x + normalData.y <= 0.001f)
	{
		output.Diffuse = float4(0, 0, 0, 0);
		output.Specular = float4(SkyColor, 0) * 0.5f;/*float4(0.4072f, 0.6392f, 0.9911f, 0)*///float4(0.6706f, 0.8078f, 0.9216f,0)*0.05f; //float4(0, 0.4431f, 0.78, 0) * 0.05f;
		return output;
	}

	output.Diffuse = 0;
	output.Specular = 0;

	return output;
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

technique Sky
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunctionSky();
	}
}


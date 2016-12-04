float4x4 World;
float4x4 ViewProjection;
//color of the light 
float3 lightColor;
//position of the camera, for specular light
float3 cameraPosition = float3(0,0,0);
//this is used to compute the world-position
float4x4 InvertViewProjection;
//this is the position of the light
float3 lightPosition = float3(0,0,0);
//how far does this light reach
float lightRadius = 0;
//control the brightness of the light
float lightIntensity = 1.0f;

float4 lightPositionVS;
float2 lightPositionTexCoord;

matrix LightViewProjectionPositiveX;
matrix LightViewProjectionNegativeX;
matrix LightViewProjectionPositiveY;
matrix LightViewProjectionNegativeY;
matrix LightViewProjectionPositiveZ;
matrix LightViewProjectionNegativeZ;

float2 Resolution = float2(1280, 800);

// diffuse color, and specularIntensity in the alpha channel
Texture2D AlbedoMap;
// normals, and specularPower in the alpha channel
Texture2D NormalMap;

bool inside = false;

#include "helper.fx"
       

//depth
Texture2D DepthMap;
sampler colorSampler = sampler_state
{
    Texture = (AlbedoMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler depthSampler = sampler_state
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
TextureCube shadowCubeMap;
sampler shadowCubeMapSampler = sampler_state
{
    texture = <shadowCubeMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    Mipfilter = LINEAR;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
    float4 Position : POSITION0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
    //float3 viewDirection : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
	float4 Volume : COLOR2;
};

struct PixelShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    //float3 viewDirection : TEXCOORD1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    //processing geometry coordinates
    float4 worldPosition = mul(float4(input.Position.rgb, 1), World);
    output.Position = mul(worldPosition, ViewProjection);
    //need t
    output.ScreenPosition = output.Position;

    //output.viewDirection = normalize(cameraPosition - worldPosition.xyz);
    return output;
}


PixelShaderInput BaseCalculations(VertexShaderOutput input)
{
    
     //obtain screen position
    input.ScreenPosition.xyz /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);

    /////////////////////////////////////////

    //read depth
    float depthVal = 1 - tex2D(depthSampler, texCoord).r;

        //compute screen-space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    //////////////////////////////////////

    PixelShaderInput output;
    output.Position = position;
    output.TexCoord = texCoord;
    //output.viewDirection = input.viewDirection;

        /////////////////////////////////////
    //CULL?
    float lightDepth = input.ScreenPosition.z;

    float insideMult = inside;
    if (insideMult <= 0)
        insideMult = -1;

    [branch]
   // if(lightDepth*inside < depthVal*inside)
    if (lightDepth * insideMult < depthVal * insideMult)
    {
        clip(-1);
        return output;
    }


    return output;
}

PixelShaderOutput BasePixelShaderFunction(PixelShaderInput input)
{
    PixelShaderOutput output;
    
    //surface-to-light vector
    float3 lightVector = lightPosition - input.Position.xyz;

    float lengthLight = length(lightVector);

    [branch]
    if (lengthLight > lightRadius)
    {
        clip(-1);
        return output;
    }
    else
    {
        int3 texCoordInt = int3(input.TexCoord * Resolution, 0);

        //get normal data from the NormalMap
        float4 normalData = NormalMap.Load(texCoordInt);
        //tranform normal back into [-1,1] range
        float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
        //get metalness
        float roughness = normalData.a;
        //get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Load(texCoordInt);

        float metalness = decodeMetalness(color.a);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        //compute attenuation based on distance - linear attenuation
        float attenuation = saturate(1.0f - lengthLight / lightRadius);

        //normalize light vector
        lightVector /= lengthLight;

        float3 cameraDirection = normalize(cameraPosition - input.Position.xyz); //input.viewDirection; //
        //compute diffuse light
        float NdL = saturate(dot(normal, lightVector));

        float3 diffuseLight = float3(0, 0, 0);

        [branch]
        if (metalness < 0.99)
        {
            diffuseLight = DiffuseOrenNayar(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
        }
        float3 specular = SpecularCookTorrance(NdL, normal, lightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
    
        //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
        output.Diffuse.rgb = (attenuation * diffuseLight * (1 - f0)) * 0.01f; //* (1 - f0)) * (f0 + 1) * (f0 + 1);
        output.Specular.rgb = specular * attenuation * 0.01f;

        return output;
    }
}

float chebyshevUpperBound(float distance, float3 texCoord)
{
		// We retrive the two moments previously stored (depth and depth*depth)
    float2 moments = 1 - shadowCubeMap.Sample(shadowCubeMapSampler, texCoord).rg;
		
		// Surface is fully lit. as the current fragment is before the light occluder
    //if (distance <= moments.x)
    //    return 1.0;

		// The fragment is either in shadow or penumbra. We now use chebyshev's upperBound to check
		// How likely this pixel is to be lit (p_max)
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, 0.000002);
	
    float d = distance - moments.x;
    float p_max = variance / (variance + d * d);
	
    return p_max;
}

PixelShaderOutput BasePixelShaderFunctionShadow(PixelShaderInput input)
{
    
    PixelShaderOutput output;
    
    //get normal data from the NormalMap
    float4 normalData = tex2D(normalSampler, input.TexCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
    //get metalness
    float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
    float4 color = tex2D(colorSampler, input.TexCoord);

    float metalness = decodeMetalness(color.a);
    
    float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

    //surface-to-light vector
    float3 lightVector = lightPosition - input.Position.xyz;

    float lengthLight = length(lightVector);

    [branch]
    if (lengthLight > lightRadius)
    {
        clip(-1);
        return output;
    }
    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - lengthLight / lightRadius);

    //normalize light vector
    lightVector /= lengthLight;

    float3 cameraDirection = normalize(cameraPosition - input.Position.xyz); //input.viewDirection; //
    //compute diffuse light
    float NdL = saturate(dot(normal, lightVector));

    //take into account attenuation and lightIntensity.
        
    //float zw = depthSample;
    //float linearZ = Projection._43 / (zw - Projection._33);
    //return linearZ;
    matrix lightProjection = LightViewProjectionPositiveX;

    lightVector = -lightVector;

    [branch]
    if (lightVector.z >= abs(lightVector.x) && lightVector.z >= abs(lightVector.y))
    {
        lightProjection = LightViewProjectionNegativeZ;
    }
    else
    [branch] if (lightVector.z <= -abs(lightVector.x) && lightVector.z <= -abs(lightVector.y))
    {
        lightProjection = LightViewProjectionPositiveZ;
    }
    else
    [branch] if (lightVector.y >= abs(lightVector.x) && lightVector.y >= abs(lightVector.z))
    {
        lightProjection = LightViewProjectionPositiveY;
    }
    else
    [branch] if (lightVector.y <= -abs(lightVector.x) && lightVector.y <= -abs(lightVector.z))
    {
        lightProjection = LightViewProjectionNegativeY;
    }
    else
    [branch] if (lightVector.x <= -abs(lightVector.y) && lightVector.x <= -abs(lightVector.y))
    {
        lightProjection = LightViewProjectionNegativeX;
    }

    float4 positionInLS = mul(input.Position, lightProjection);
    float depthInLS = (positionInLS.z / positionInLS.w);


    lightVector.z = -lightVector.z;
    //float shadowVSM = 
    //float shadowDepth = 1-shadowCubeMap.Sample(shadowCubeMapSampler, -lightVector).g; // chebyshevUpperBound(distanceToLightNonLinear, -lightVector);

    float shadowVSM = chebyshevUpperBound(depthInLS, lightVector);
       
    float3 diffuseLight = float3(0, 0, 0);
    float3 specular = float3(0, 0, 0);
         
    lightVector.z = -lightVector.z;

    [branch]
    if (shadowVSM > 0.01f && lengthLight < lightRadius)
    {
    [branch]
        if (metalness < 0.99)
        {
            diffuseLight = DiffuseOrenNayar(NdL, normal, -lightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
        }
        specular = SpecularCookTorrance(NdL, normal, -lightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
    }

    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
    output.Diffuse.rgb = (attenuation * diffuseLight * (1 - f0)) * 0.01f * shadowVSM; //* (1 - f0)) * (f0 + 1) * (f0 + 1);
    output.Specular.rgb = specular * attenuation * 0.01f * max(shadowVSM - 0.1f, 0);

    return output;
   //return float4(color.rgb * (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1) *0.5f + specular*attenuation, 1);

}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : SV_TARGET
{
    PixelShaderInput p_input = BaseCalculations(input);
 
    PixelShaderOutput Output;

    Output = BasePixelShaderFunction(p_input);

    return Output;
}


PixelShaderOutput PixelShaderFunctionShadowed(VertexShaderOutput input) : SV_TARGET
{
    PixelShaderInput p_input = BaseCalculations(input);


    PixelShaderOutput Output;

    Output = BasePixelShaderFunctionShadow(p_input);

    return Output;
}

///VOLUMETRIC
struct VertexShaderOutput2
{
	float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
};

VertexShaderOutput2 VertexShaderFunction2(VertexShaderInput input)
{
	VertexShaderOutput2 output;
	//processing geometry coordinates
	float4 worldPosition = mul(float4(input.Position.rgb, 1), World);
	output.Position = mul(worldPosition, ViewProjection);
	output.ScreenPosition = output.Position;
	output.WorldPosition = worldPosition;
	return output;
}

float integrateVolume(float d2, float d1, float radius)
{
	// 1 - x/r -> x - x^2 / r*2
	return max(d2 - d2*d2 / (radius*2) - d1 + d1*d1 / (radius*2),0);
}

PixelShaderOutput VolumetricPixelShaderFunction(VertexShaderOutput2 input)
{
	PixelShaderOutput output;
	input.ScreenPosition.xyz /= input.ScreenPosition.w;
	float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);

	//read depth
	float depthVal = 1 - tex2D(depthSampler, texCoord).r;

	//compute screen-space position
	float4 position;
	position.xy = input.ScreenPosition.xy;
	position.z = depthVal;
	position.w = 1.0f;
	//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;

	//The way our camera is looking
	float3 cameraDirection = normalize(position.xyz - cameraPosition);

	//Entry into the volumetric field?
	float insideMult = inside;
	if (insideMult <= 0)
		insideMult = -1;

	//If inside = 0 we look from the outside, so we enter into the field
	float3 toLightCenter = normalize(lightPosition - input.WorldPosition.xyz);

	/*float3 perpVector = cross(toLightCenter, cameraDirection);

	float3 perpVector2 = cross(cameraDirection, perpVector);
*/

	float3 cameraToLight = lightPosition - cameraPosition;
	float distanceLtoC = length(cameraToLight);

	//normalize
	cameraToLight /= distanceLtoC;

	float alpha = dot(cameraDirection, cameraToLight);

	//P is the vertex position (interpolated)
	//B is the halfway in the sphere
	//R is the reconstructed position from the depthmap

	float distanceCtoP = distance(cameraPosition, input.WorldPosition.xyz);
	float distanceCtoB = alpha * distanceLtoC;
	float distanceCtoR = distance(cameraPosition, position.xyz);

	float3 b_vector = distanceCtoB * cameraDirection + cameraPosition;

	// 1 - x / radius


	float totalVolumePassed = 0;

	float distanceLtoP = distance(lightPosition, input.WorldPosition.xyz);
	float distanceLtoB = distance(lightPosition, b_vector);
	float distanceLtoR = distance(lightPosition, position.xyz);

	[branch]
	if (inside <= 0)
	{
		[branch]
		if (distanceCtoR > distanceCtoP + (distanceCtoB - distanceCtoP) * 2)
		{
			totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoB, lightRadius) - integrateVolume(distanceLtoP, lightRadius, lightRadius);
		}

		else
		{
			if (distanceCtoR > distanceCtoB)
			{
				totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoB, lightRadius) + integrateVolume(distanceLtoB, distanceLtoR, lightRadius);
			}

			else if (distanceCtoR > distanceCtoP)
			{
				totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoR, lightRadius);
			}
		}
	}
	else
	{
		
		//object behind our volume
		if (distanceCtoR > distanceCtoP )
		{
			//is camera behind mid of volume?
			if (alpha<0)
			{
				totalVolumePassed = integrateVolume(distanceLtoP, distanceLtoC, lightRadius)/2;
			}
			else
			{
				totalVolumePassed = 0.5f *( integrateVolume(distanceLtoP, distanceLtoB, lightRadius) + integrateVolume(distanceLtoC, distanceLtoB, lightRadius));
			}
		}
		else
		{
			if (alpha<0)
			{
				totalVolumePassed = integrateVolume(distanceLtoR, distanceLtoC, lightRadius) / 2;
			}
			else
			{
				if (distanceCtoR < distanceCtoB)
				{
					totalVolumePassed = 0.5f*integrateVolume(distanceLtoC, distanceLtoR, lightRadius);
				}
				else
				{
					totalVolumePassed = 0.5f*(integrateVolume(distanceLtoR, distanceLtoB, lightRadius) + integrateVolume(distanceLtoC, distanceLtoB, lightRadius));
				}
			}
		}
	}

	float3 lightVector = lightPosition - position.xyz;

	output.Diffuse = 0;
	output.Specular = 0;
	output.Volume = float4((totalVolumePassed * 0.001) * lightColor , 0);


	[branch]
	if (distanceLtoR < lightRadius)
	{
		int3 texCoordInt = int3(texCoord * Resolution, 0);

		//get normal data from the NormalMap
		float4 normalData = NormalMap.Load(texCoordInt);
		//tranform normal back into [-1,1] range
		float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad
												//get metalness
		float roughness = normalData.a;
		//get specular intensity from the AlbedoMap
		float4 color = AlbedoMap.Load(texCoordInt);

		float metalness = decodeMetalness(color.a);

		float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

		//compute attenuation based on distance - linear attenuation
		float attenuation = saturate(1.0f - distanceLtoR / lightRadius);

		//normalize light vector
		lightVector /= distanceLtoR;

		//float3 cameraDirection = normalize(cameraPosition - input.Position.xyz); //input.viewDirection; //
																				 //compute diffuse light
		float NdL = saturate(dot(normal, lightVector));

		float3 diffuseLight = float3(0, 0, 0);

		[branch]
		if (metalness < 0.99)
		{
			diffuseLight = DiffuseOrenNayar(NdL, normal, lightVector, -cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
		}
		float3 specular = SpecularCookTorrance(NdL, normal, lightVector, -cameraDirection, lightIntensity, lightColor, f0, roughness);

		//return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
		output.Diffuse.rgb = (attenuation * diffuseLight * (1 - f0)) * 0.01f; //* (1 - f0)) * (f0 + 1) * (f0 + 1);
		output.Specular.rgb = specular * attenuation * 0.01f;

	}
	return output;

}

   
technique Unshadowed
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique UnshadowedVolume
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction2();
		PixelShader = compile ps_4_0 VolumetricPixelShaderFunction();
	}
}

technique Shadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunctionShadowed();
    }
}





//PixelShaderOutput PixelShaderFunctionClassic(VertexShaderOutput input) : COLOR0
//{
//    //obtain screen position
//    input.ScreenPosition.xy /= input.ScreenPosition.w;
//    //obtain textureCoordinates corresponding to the current pixel
//    //the screen coordinates are in [-1,1]*[1,-1]
//    //the texture coordinates need to be in [0,1]*[0,1]
//    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    
//    //get normal data from the NormalMap
//    float4 normalData = tex2D(normalSampler, texCoord);
//    //tranform normal back into [-1,1] range
//    float3 normal = 2.0f * normalData.xyz - 1.0f; //could do mad
//    //get specular power
//    float f0 = normalData.a;
//    //get specular intensity from the AlbedoMap
//    float4 color = tex2D(colorSampler, texCoord);

//    float roughness = decodeMetalness(color.a);

//    //read depth
//    float depthVal = 1 - tex2D(depthSampler, texCoord).r;
           
//    PixelShaderOutput output;

//    //compute screen-space position
//        float4 position;
//        position.xy = input.ScreenPosition.xy;
//        position.z = depthVal;
//        position.w = 1.0f;
//    //transform to world space
//        position = mul(position, InvertViewProjection);
//        position /= position.w;
//    //surface-to-light vector
//        float3 lightVector = lightPosition - position.xyz;
//    //compute attenuation based on distance - linear attenuation
//        float attenuation = saturate(1.0f - length(lightVector) / lightRadius) * lightIntensity;
//    //normalize light vector
//        lightVector = normalize(lightVector);

//        float3 cameraDirection = normalize(cameraPosition - position.xyz);
//    //compute diffuse light
//        float NdL = saturate(dot(normal, lightVector));

//        float3 diffuseLight = NdL * lightColor.rgb;
    
//        float3 reflectionVector = normalize(reflect(-lightVector, normal));
//    //camera-to-surface vector
//        float3 specular = /*(1-roughness)*/(1 - roughness) * (1 - roughness) * 4 * pow(saturate(dot(reflectionVector, cameraDirection)), (1 - roughness) * (1 - roughness) * 60); //+ (1 - NdL) * (1 - NdL) * f0;

//    //take into account attenuation and lightIntensity.

//    //return attenuation * lightIntensity * float4(diffuseLight.rgb, specular);
//        output.Diffuse.rgb = attenuation * diffuseLight * 0.01f;
//        output.Specular.rgb = specular * lightColor * attenuation * 0.01f;
    
//    return output;
//   //return float4(color.rgb * (attenuation * diffuseLight * (1 - f0)) * (f0 + 1) * (f0 + 1) *0.5f + specular*attenuation, 1);

//}
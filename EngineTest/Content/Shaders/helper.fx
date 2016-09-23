       
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

//float2 encode(float3 n)
//{
//    half2 enc = normalize(n.xy) * (sqrt(-n.z * 0.5 + 0.5));
//    enc = enc * 0.5 + 0.5;
//    return enc;
//}

//float3 decode(float4 enc)
//{
//    half4 nn = enc * half4(2, 2, 0, 0) + half4(-1, -1, 1, -1);
//    half l = dot(nn.xyz, -nn.xyw);
//    nn.z = l;
//    nn.xy *= sqrt(l);
//    return nn.xyz * 2 + half3(0, 0, -1);
//}

//half4 encode(half3 n)
//{
//    return half4(n.xy * 0.5 + 0.5, 0, 0);
//}

//half3 decode(half2 enc)
//{
//    half3 n;
//    n.xy = enc * 2 - 1;
//    n.z = sqrt(1 - dot(n.xy, n.xy));
//    return n;
//}
 

float3 encode(float3 n)
{
    return   0.5f * (n + 1.0f);
}

float3  decode(float3 n)
{
    return 2.0f * n.xyz - 1.0f;
}

float encodeMetallicMattype(float metalness, float mattype)
{
    return metalness * 0.1f * 0.5f + mattype * 0.1f;
}

float decodeMetalness(float input)
{
    input *= 10;
    return frac(input) * 2;
}

float decodeMattype(float input)
{
    input *= 10;
    return trunc(input);
}

     
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

//half2 encode(float3 n)
//{
//    half f = sqrt(8 * n.z + 8);
//    return n.xy / f + 0.5;
//}
//half3 decode(half4 enc)
//{
//    half2 fenc = enc * 4 - 2;
//    half f = dot(fenc, fenc);
//    half g = sqrt(1 - f / 4);
//    half3 n;
//    n.xy = fenc * g;
//    n.z = 1 - f / 2;
//    return n;
//}
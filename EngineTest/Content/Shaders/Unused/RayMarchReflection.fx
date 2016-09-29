float4x4 InvertViewProjection;
float4x4 ViewProjection;
float4x4 Projection;
float4x4 View;
float4x4 SSProjection;
Texture2D colorMap;

float2 InverseResolution = (1.0f / 1280.0f, 1.0f / 800.0f);

float3 cameraPosition;
float3 cameraDir;

float cb_nearPlaneZ= -1;
float cb_farPlaneZ = 300;

float2 cb_depthBufferSize = { 1280, 800 };
float cb_zThickness = 0.000001f;
float cb_strideZCutoff = 200;
float cb_maxDistance = 299;
float cb_maxSteps = 40;
float cb_stride = 3;

#define STEPS 9

Texture2D normalMap;
//depth
Texture2D depthMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 posH : SV_Position;
    float3 viewRay : VIEWRAY;
    float2 tex : TEXCOORD0;
};




////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.posH = float4(input.Position);
    output.viewRay = float3(input.Position.xy / input.Position.z, 1.0f);
    output.tex = input.TexCoord;
    return output;
}

float linearizeDepth(float depthSample)
{
    //depthSample = 2.0 * depthSample - 1.0;
    //float zLinear = 2.0 * cb_nearPlaneZ * cb_farPlaneZ / (cb_farPlaneZ + cb_nearPlaneZ - depthSample * (cb_farPlaneZ - cb_nearPlaneZ));

    float zLinear = depthSample * (cb_farPlaneZ - cb_nearPlaneZ) + cb_nearPlaneZ;
    return zLinear;
    //float zw = depthSample;
    //float linearZ = Projection._43 / (zw - Projection._33);
    //return linearZ;
}

float distanceSquared(float2 a, float2 b)
{
    a -= b;
    return dot(a, a);
}
 
bool intersectsDepthBuffer(float z, float minZ, float maxZ)
{
 /*
 * Based on how far away from the camera the depth is,
 * adding a bit of extra thickness can help improve some
 * artifacts. Driving this value up too high can cause
 * artifacts of its own.
 */
    float depthScale = min(1.0f, z * cb_strideZCutoff);
    z += cb_zThickness + lerp(0.0f, 2.0f, depthScale);
    return (maxZ >= z) && (minZ - cb_zThickness <= z);
}
 
void swap(inout float a, inout float b)
{
    float t = a;
    a = b;
    b = t;
}
 
float linearDepthTexelFetch(int2 hitPixel)
{
 // Load returns 0 for any value accessed out of bounds
    return linearizeDepth(1 - depthMap.Load(int3(hitPixel, 0)).r);
}


bool traceScreenSpaceRay(
 // Camera-space ray origin, which must be within the view volume
 float3 csOrig,
 // Unit length camera-space ray direction
 float3 csDir,
 // Number between 0 and 1 for how far to bump the ray in stride units
 // to conceal banding artifacts. Not needed if stride == 1.
 float jitter,
 // Pixel coordinates of the first intersection with the scene
 out float2 hitPixel,
 // Camera space location of the ray hit
 out float3 hitPoint)
{
 // Clip to the near plane
    float rayLength = ((csOrig.z + csDir.z * cb_maxDistance) < cb_nearPlaneZ) ?
 (cb_nearPlaneZ - csOrig.z) / csDir.z : cb_maxDistance;
    float3 csEndPoint = csOrig + csDir * rayLength;
 
 // Project into homogeneous clip space
    float4 H0 = mul(float4(csOrig, 1.0f), SSProjection);
    H0.xy *= cb_depthBufferSize;
    float4 H1 = mul(float4(csEndPoint, 1.0f), SSProjection);
    H1.xy *= cb_depthBufferSize;
    float k0 = 1.0f / H0.w;
    float k1 = 1.0f / H1.w;
 
 // The interpolated homogeneous version of the camera-space points
    float3 Q0 = csOrig * k0;
    float3 Q1 = csEndPoint * k1;
 
 // Screen-space endpoints
    float2 P0 = H0.xy * k0;
    float2 P1 = H1.xy * k1;
 
 // If the line is degenerate, make it cover at least one pixel
 // to avoid handling zero-pixel extent as a special case later
    P1 += (distanceSquared(P0, P1) < 0.0001f) ? float2(0.01f, 0.01f) : 0.0f;
    float2 delta = P1 - P0;
 
 // Permute so that the primary iteration is in x to collapse
 // all quadrant-specific DDA cases later
    bool permute = false;
    if (abs(delta.x) < abs(delta.y))
    {
 // This is a more-vertical line
        permute = true;
        delta = delta.yx;
        P0 = P0.yx;
        P1 = P1.yx;
    }
 
    float stepDir = sign(delta.x);
    float invdx = stepDir / delta.x;
 
 // Track the derivatives of Q and k
    float3 dQ = (Q1 - Q0) * invdx;
    float dk = (k1 - k0) * invdx;
    float2 dP = float2(stepDir, delta.y * invdx);
 
 // Scale derivatives by the desired pixel stride and then
 // offset the starting values by the jitter fraction
    float strideScale = 1.0f - min(1.0f, csOrig.z * cb_strideZCutoff);
    float stride = 1.0f + strideScale * cb_stride;
    dP *= stride;
    dQ *= stride;
    dk *= stride;
 
    P0 += dP * jitter;
    Q0 += dQ * jitter;
    k0 += dk * jitter;
 
 // Slide P from P0 to P1, (now-homogeneous) Q from Q0 to Q1, k from k0 to k1
    float4 PQk = float4(P0, Q0.z, k0);
    float4 dPQk = float4(dP, dQ.z, dk);
    float3 Q = Q0;
 
 // Adjust end condition for iteration direction
    float end = P1.x * stepDir;
 
    float stepCount = 0.0f;
    float prevZMaxEstimate = csOrig.z;
    float rayZMin = prevZMaxEstimate;
    float rayZMax = prevZMaxEstimate;
    float sceneZMax = rayZMax + 100.0f;
    for (;
 ((PQk.x * stepDir) <= end) && (stepCount < cb_maxSteps) &&
 !intersectsDepthBuffer(sceneZMax, rayZMin, rayZMax) &&
 (sceneZMax != 0.0f);
 ++stepCount)
    {
        rayZMin = prevZMaxEstimate;
        rayZMax = (dPQk.z * 0.5f + PQk.z) / (dPQk.w * 0.5f + PQk.w);
        prevZMaxEstimate = rayZMax;
        if (rayZMin > rayZMax)
        {
            swap(rayZMin, rayZMax);
        }
 
        hitPixel = permute ? PQk.yx : PQk.xy;
 // You may need hitPixel.y = depthBufferSize.y - hitPixel.y; here if your vertical axis
 // is different than ours in screen space
        sceneZMax = linearDepthTexelFetch(int2(hitPixel));
 
        PQk += dPQk;
    }
 
 // Advance Q based on the number of steps
    Q.xy += dQ.xy * stepCount;
    hitPoint = Q * (1.0f / PQk.w);
    return intersectsDepthBuffer(sceneZMax, rayZMin, rayZMax);
}


float4 PixelShaderFunction(VertexShaderOutput pIn) : SV_TARGET
{
    int3 loadIndices = int3(pIn.posH.xy, 0);
    float3 normalVS = normalMap.Load(loadIndices).xyz * 2 - 1;
    if (!any(normalVS))
    {
        return 0.0f;
    }
 
    normalVS = mul(float4(normalVS, 0), ViewProjection).xyz;

    float depth = 1 - depthMap.Load(loadIndices).r;
    float3 rayOriginVS = float4(pIn.viewRay, 1) * linearizeDepth(depth);
 
 /*
 * Since position is reconstructed in view space, just normalize it to get the
 * vector from the eye to the position and then reflect that around the normal to
 * get the ray direction to trace.
 */
    float3 toPositionVS = normalize(rayOriginVS);

    toPositionVS = mul(float4(cameraDir,1), ViewProjection);

    float3 rayDirectionVS = normalize(reflect(toPositionVS, normalVS));
 
 // output rDotV to the alpha channel for use in determining how much to fade the ray
    float rDotV = dot(rayDirectionVS, toPositionVS);
 
 // out parameters
    float2 hitPixel = float2(0.0f, 0.0f);
    float3 hitPoint = float3(0.0f, 0.0f, 0.0f);
 
    float jitter = 
    cb_stride > 1.0f ? float(int(pIn.posH.x + pIn.posH.y) & 1) * 0.5f : 0.0f;
 
 // perform ray tracing - true if hit found, false otherwise
    bool intersection = traceScreenSpaceRay(rayOriginVS, rayDirectionVS, jitter, hitPixel, hitPoint);
 
    depth = 1 - depthMap.Load(int3(hitPixel, 0)).r;
 
 // move hit pixel from pixel position to UVs
    hitPixel *= InverseResolution; //float2(texelWidth, texelHeight);
    if (hitPixel.x > 1.0f || hitPixel.x < 0.0f || hitPixel.y > 1.0f || hitPixel.y < 0.0f)
    {
        intersection = false;
    }
 
    return colorMap.Sample(colorSampler, hitPixel);
    //return float4(hitPixel, depth, rDotV) * (intersection ? 1.0f : 0.0f);
}

//float4 raytrace(in float3 startPos,
//              in float3 endPos,
//              uniform float4x4 mat_proj)
//{

//    float4 startPosSS = mul(mat_proj, float4(startPos, 1));
//    startPosSS /= startPosSS.w;
//    startPosSS.xy = startPosSS.xy * ;
//    float4 endPosSS = mul(mat_proj, float4(endPos, 1));
//    endPosSS /= endPosSS.w;
//    endPosSS.xy = endPosSS.xy * texpad_albedo.xy + texpad_albedo.xy;
//    // Reflection vector in the screen space
//    float3 vectorSS = normalize(endPosSS.xyz - startPosSS.xyz) * stepSize;
    
//    // Init vars for cycle
//    float2 samplePos = 0; // texcoord for the depth and color
//    float sampleDepth = 0; // depth from texture
//    float currentDepth = 0; // current depth calculated with reflection vector
//    float deltaD = 0;
//    float4 color = 0;
//    for (int i = 1; i < 35; i++)
//    {
//        samplePos = (startPosSS.xy + vectorSS.xy * i);
//        currentDepth = linearizeDepth(startPosSS.z + vectorSS.z * i);
//        sampleDepth = linearizeDepth(f1tex2D(depth, samplePos));
//        deltaD = currentDepth - sampleDepth;
//        if (deltaD > 0 && deltaD < maxDelta)
//        {
//            color = tex2D(albedo, samplePos);
//            color.a *= fade / i;
//            break;
//        }
//    }


//}


//float4 PixelShaderFunction(VertexShaderOutput input)
//{
//    float4 color = colorMap.Sample(colorSampler, input.tex);
//    float4 normal = normalMap.Sample(normalSampler, input.tex) * 2 - 1;
//    float4 depth = depthMap.Sample(depthSampler, input.tex);

//    float4 position;

//    position = input.posH;
//    position.z = depth;
//    position.w = 1;
//    position = mul(SSProjection, position);
//    position /= position.w;

//    float3 V = normalize(position.xyz);

//    //normal
//    float3 N = mul(float4(normal.xyz, 0), ViewProjection).xyz;

//    //Reflection vector in camera space
//    float R = normalize(reflect(V.xyz, N.xyz));

//    return raytrace(position.xyz, position.xyz + R, Projection);


//}


technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}


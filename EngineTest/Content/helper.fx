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

float encodeRoughnessMattype(float roughness, float mattype)
{
    return roughness * 0.1f + mattype * 0.1f;
}

float decodeRoughness(float input)
{
    input *= 10;
    return frac(input);
}

float decodeMattype(float input)
{
    input *= 10;
    return round(trunc(input));
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
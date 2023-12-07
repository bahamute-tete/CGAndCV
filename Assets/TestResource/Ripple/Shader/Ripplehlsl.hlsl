
#ifndef CUSTOM_RIPPLE_INCLUDED
#define CUSTOM_RIPPLE_INCLUDED

#include "UnityInput.hlsl"



float3 TransformObjectToWorld (float3 positionOS) {
	return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
}

float4 TransformWorldToHClip (float3 positionWS) {
	return mul(unity_MatrixVP, float4(positionWS, 1.0));
}

float4 UnlitPassVertex (float3 positionOS :POSITION):SV_POSITION
{
    float3 positionWS = TransformObjectToWorld(positionOS.xyz);
    return TransformWorldToHClip(positionWS);;
}

float4 UnlitPassFragment () : SV_Target
{
    
    float4 col =0;
    return col;
}


#endif

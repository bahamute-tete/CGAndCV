

#if !defined(MY_SHADOW_INCLUDE)
#define MY_SHADOW_INCLUDE
#include "UnityCG.cginc"

            struct vertexData
            {
                float4 position:POSITION;
                float3 normal:NORMAL;
            };


            #if defined(SHADOWS_CUBE)

            struct v2f
            {
                float4 position:SV_POSITION;
                float3 lightVec:TEXCOORD0;
            };

            v2f vert(vertexData v)
            {
                v2f i;

                i.position =UnityObjectToClipPos(v.position);
                i.lightVec =mul(unity_ObjectToWorld,v.position).xyz-_LightPositionRange.xyz;
                return i;
            }


            float4 frag(v2f i):SV_TARGET
            {
                float depth = length(i.lightVec)+unity_LightShadowBias.x;
                depth*= _LightPositionRange.w;//_LightPositionRange.w variable contains the inverse of its range, so we have to multiply by this value. 
                return UnityEncodeCubeShadowDepth(depth);
            } 

            #else

            float4 vert(vertexData v):SV_POSITION
            {
                
                float4 position =UnityClipSpaceShadowCasterPos(v.position.xyz,v.normal);//To  support the normal bias
 

                return UnityApplyLinearShadowBias(position); //To support the depth bias
            }


            float4 frag():SV_TARGET
            {

                return 0;
            } 
            #endif
#endif
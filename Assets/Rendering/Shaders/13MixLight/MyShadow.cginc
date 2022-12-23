

#if !defined(MY_SHADOW_INCLUDE)
    #define MY_SHADOW_INCLUDE
    #include "UnityCG.cginc"

    #if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
        #if defined(_SEMITRANSPARENT_SHADOWS)
            #define SHADOWS_SEMITRANSPARENT 1
        #else
            #define _RENDERING_CUTOUT
        #endif
        
    #endif



    #if SHADOWS_SEMITRANSPARENT || defined(_RENDERING_CUTOUT)
     #if !defined(_SMOOTHNESS_ALBEDO)
        #define SHADOWS_NEED_UV 1
     #endif
    #endif

#endif


            struct vertexData
            {
                float4 position:POSITION;
                float3 normal:NORMAL;
                float2 uv:TEXCOORD0;
            };



            struct v2fVertex
            {
                float4 position : SV_POSITION;
                
                 #if defined(SHADOWS_NEED_UV)
                    float2 uv:TEXCOORD0;
                #endif

                #if defined(SHADOWS_CUBE)
                    float3 lightVec : TEXCOORD1;
                #endif
            };

               struct v2f
            {
                #if SHADOWS_SEMITRANSPARENT
                    //The UNITY_VPOS_TYPE macro is defined in HLSLSupport. 
                    //It's usually a float4, except for Direct3D 9, which needs it to be a float2.
                    UNITY_VPOS_TYPE vpos : VPOS;
                #else
                    float4 positions : SV_POSITION;
                #endif
                
                 #if defined(SHADOWS_NEED_UV)
                    float2 uv:TEXCOORD0;
                #endif

                #if defined(SHADOWS_CUBE)
                    float3 lightVec : TEXCOORD1;
                #endif
            };


            float4 _Color;
            sampler2D _Texture;
            float4 _Texture_ST;
            float _Cutoff;

            sampler3D _DitherMaskLOD;

            v2fVertex vert(vertexData v)
            {
                v2fVertex i;
                #if defined(SHADOWS_CUBE)
                i.position =UnityObjectToClipPos(v.position);
                i.lightVec =mul(unity_ObjectToWorld,v.position).xyz-_LightPositionRange.xyz;
                #else
                 i.position =UnityClipSpaceShadowCasterPos(v.position.xyz,v.normal);//To  support the normal bias
                 i.position = UnityApplyLinearShadowBias(i.position); //To support the depth bias
                #endif

                #if SHADOWS_NEED_UV
                    i.uv = TRANSFORM_TEX(v.uv,_Texture);
                #endif

                return i;
            }


            float GetAlpha(v2f i)
            {
                float alpha = _Color.a;
                #if SHADOWS_NEED_UV
                    alpha *= tex2D (_Texture,i.uv.xy).a;
                #endif
                return alpha;

            }

            float4 frag(v2f i):SV_TARGET
            {   
                float alpha = GetAlpha(i);

                #if defined (_RENDERING_CUTOUT)
                    clip(alpha - _Cutoff);
                #endif

                #if SHADOWS_SEMITRANSPARENT
                   //The alpha channel of the dither texture is zero when a fragment should be discarded. 
                   //So subtract a small value from it and use that to clip.
                   //_DitherMaskLOD and use second pattern.
                   //magnify it by a factor of 100, which is done by multiplying the position by 0.01
                   //A spotlight shadow allows us to get a good look at it.
                   //The shadows get fully clipped at 0, and are fully rendered at 0.9375.
                    float dither = tex3D(_DitherMaskLOD, float3(i.vpos.xy*0.25 , alpha * 0.9375)).a;

                    clip(dither-0.01);
                #endif

                #if defined (SHADOWS_CUBE)
                    float depth = length(i.lightVec)+unity_LightShadowBias.x;
                    depth*= _LightPositionRange.w;//_LightPositionRange.w variable contains the inverse of its range, so we have to multiply by this value. 
                    return UnityEncodeCubeShadowDepth(depth);
                #else
                    return 0;
                #endif
            } 

      

        
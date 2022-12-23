#if !defined(MY_DEFERRED_SHADING)
#define MY_DEFERRED_SHADING

#include "UnityPBSLighting.cginc"

 struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv:TEXCOORD0;//for sample G_Buffer
                float3 ray:TEXCOORD1;
            };

           
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            sampler2D _CameraGBufferTexture0;
            sampler2D _CameraGBufferTexture1;
            sampler2D _CameraGBufferTexture2;

            float4 _LightColor,_LightDir,_LightPos;

            sampler2D _LightTextureB0;//attenuation is stored in a lookup texture, which is made available via _LightTextureB0.

            #if defined(POINT_COOKIE)
                samplerCUBE _LightTexture0;
            #else
                sampler2D _LightTexture0;
            #endif

            float4x4 unity_WorldToLight;
            float _LightAsQuad;

            //It is already defined for point and spotlight shadows in UnityShadowLibrary
            //should not define it ourselves, except when working with shadows for directional lights.
            #if defined(SHADOWS_SCREEN)
            sampler2D _ShadowMapTexture;
            #endif

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =ComputeScreenPos(o.vertex);
          
                o.ray =lerp(
                        UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1),
                        v.normal,
                        _LightAsQuad);
                return o;
            }

            UnityLight CreatLight(float2 uv,float3 worldPos,float viewZ)
            {
                UnityLight light;
                float shadowAttenuation =1;
                float attenuation =1;

                bool shadowed = false;

                #if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
                    light.dir = -_LightDir;

                    #if defined(DIRECTIONAL_COOKIE)
                        float2 uvCookie = mul(unity_WorldToLight,float4(worldPos,1)).xy;
                        //artifacts appear when there is a large difference 
                        //between the cookie coordinates of adjacent fragments. 
                        //attenuation *= tex2D(_LightTexture0,uvCookie).w;
                        //use Bias 
                        attenuation *= tex2Dbias(_LightTexture0,float4( uvCookie,0,-8)).w;
                    #endif

                    #if defined(SHADOWS_SCREEN)
                        shadowed =true;
                        shadowAttenuation = tex2D(_ShadowMapTexture,uv).r;
                       
                    #endif
                #else

                    float3 lightVec = _LightPos.xyz -worldPos;
                    light.dir = normalize(lightVec);

                    //The range is stored in the fourth component of _LightPos.
                    attenuation *= tex2D(_LightTextureB0,(dot(lightVec,lightVec)*_LightPos.w).rr).UNITY_ATTEN_CHANNEL;

                    #if defined(SPOT)
                        float4 uvCookie = mul(unity_WorldToLight,float4(worldPos,1));
                        uvCookie.xy /=uvCookie.w; 
                        attenuation *= tex2Dbias(_LightTexture0,float4( uvCookie.xy,0,-8)).w;
                        attenuation *= uvCookie.w < 0;

                        #if defined(SHADOWS_DEPTH)
                            shadowed= true;
                            shadowAttenuation = UnitySampleShadowmap(mul(unity_WorldToShadow[0],float4(worldPos,1)));
                        #endif
                    #else

                        #if defined(POINT_COOKIE)
                            float3 uvCookie = mul(unity_WorldToLight,float4(worldPos,1)).xyz;
                             attenuation *= texCUBEbias(_LightTexture0,float4( uvCookie,-8)).w;
                        #endif

                        #if defined(SHADOWS_CUBE)
                            shadowed = true;
                            shadowAttenuation = UnitySampleShadowmap(-lightVec);
                        #endif
                    #endif

                #endif

                if (shadowed)
                {       
                        float shadowFadeDistance =UnityComputeShadowFadeDistance(worldPos,viewZ);
                        //The shadow fade factor is a value from 0 to 1, 
                        //which indicates how much the shadows should fade away.
                        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
                        shadowAttenuation = saturate(shadowAttenuation+shadowFade);

                        #if defined(UNITY_FAST_COHERENT_DYNAMIC_BRANCHING) && defined(SHADOWS_SOFT)
                        UNITY_BRANCH
                            if (shadowFade >0.99)
                            {
                                shadowAttenuation =1;
                            }
                        #endif
                }
                 
                light.color = _LightColor.rgb * (attenuation*shadowAttenuation);
                return light;

            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv.xy/i.uv.w;

                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv);
                depth = Linear01Depth(depth);

                float3 rayToFarPlane = i.ray* _ProjectionParams.z/i.ray.z;
                float3 viewPos = rayToFarPlane *depth;

                float3 worldPos = mul(unity_CameraToWorld,float4(viewPos,1)).xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos- worldPos);

                float3 albedo = tex2D(_CameraGBufferTexture0,uv).rgb;
                float3 specularTint = tex2D(_CameraGBufferTexture1,uv).rgb;
                float3 smoothness = tex2D(_CameraGBufferTexture1,uv).a;
                float3 normal = tex2D(_CameraGBufferTexture2,uv).rgb*2-1;

                //surface reflectivity. We derive that from the specular tint. 
                //It's simply the strongest color component. 
                //We can use the SpecularStrength function to extract it.
                float oneMinusReflectivity = 1 - SpecularStrength(specularTint);

                UnityLight light = CreatLight(uv,worldPos,viewPos.z);
                
                UnityIndirect indirectLight;
                indirectLight.diffuse = 0;
                indirectLight.specular =0;

                float4 color = UNITY_BRDF_PBS
                (
                    albedo,specularTint,oneMinusReflectivity,smoothness,normal,
                    viewDir,light,indirectLight
                );
                return color;
            }

#endif
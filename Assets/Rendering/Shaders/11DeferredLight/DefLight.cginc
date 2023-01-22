#if !defined(MY_DEFFRRED_SHADING)
#define MY_DEFFRRED_SHADING

//#include "UnityCG.cginc""
//Computing BRDF
#include "UnityPBSLighting.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                //rays for the four vertices of the quad are supplied as normal vectors. 
                //we can just pass them through the vertex program and interpolate them.
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
               
                //for sample G_Buffer
                float4 uv:TEXCOORD0;
                float3 ray:TEXCOORD1;
            };

           // find the depth value in the fragment program by 
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            //we need access to the G-buffers to retrieve the surface properties
            sampler2D _CameraGBufferTexture0;
            sampler2D _CameraGBufferTexture1;
            sampler2D _CameraGBufferTexture2;

            float4 _LightColor,_LightDir,_LightPos;

            //attenuation is stored in a lookup texture, which is made available via _LightTextureB0.
            sampler2D _LightTextureB0;

            #if defined(POINT_COOKIE)
                samplerCUBE _LightTexture0;
            #else
                sampler2D _LightTexture0;
            #endif
            //The transformation for that is made available via the unity_WorldToLight matrix variable.
            // convert from world to light space to  sample ligthTexture
            float4x4 unity_WorldToLight;

            //if it's set to 1, we're dealing with a quad and can use the normals
            float _LightAsQuad;

            //It is already defined for point and spotlight shadows in UnityShadowLibrary
            //should not define it ourselves, except when working with shadows for directional lights.
            #if defined(SHADOWS_SCREEN)
            //we relied on the macros from AutoLight to determine the light attenuation caused by shadows. 
            //Unfortunately, that file wasn't written with deferred lights in mind. 
            //So we'll do the shadow sampling ourselves.
            // The shadow map can be accessed via the _ShadowMapTexture variable.
            sampler2D _ShadowMapTexture;
            #endif

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Unity doesn't supply light passes with convenient texture coordinates.
                // Instead, we have to derive them from the clip-space position.
                o.uv =ComputeScreenPos(o.vertex);
                

                //if we  consider Direction Light as camera , cameras are orthographic
                // ray stored in the normal channel only for full-screen quad
                //o.ray =v.normal;
                //converting the points to view space, 
                //for which we can use the UnityObjectToViewPos function.
                // this produces rays with the wrong orientation. We have to negate their X and Y coordinates.
                //o.ray = UnityObjectToViewPos(v.vertex) * float3(-1,-1, 1);

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
                //For our calculations, we need the direction from the surface to the light, 
                //so the opposite.
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
                    //Spot light
                    float3 lightVec = _LightPos.xyz -worldPos;
                    light.dir = normalize(lightVec);

                    //The range is stored in the fourth component of _LightPos.
                    //The texture is designed so it has to be sampled with the squared light distance, scaled by the light's range.
                    //Which of the texture's channels should be used varies per platform 
                    //and is defined by the UNITY_ATTEN_CHANNEL macro. 
                    attenuation *= tex2D(_LightTextureB0,(dot(lightVec,lightVec)*_LightPos.w).rr).UNITY_ATTEN_CHANNEL;

                    //The spotlight's conic attenuation is created via a cookie texture
                    //whether it's the default circle or a custom cookie
                    #if defined(SPOT)
                        //This is done with a perspective transformation. 
                        //So the matrix multiplication produces 4D homogeneous coordinates
                        float4 uvCookie = mul(unity_WorldToLight,float4(worldPos,1));
                        uvCookie.xy /=uvCookie.w; 
                        attenuation *= tex2Dbias(_LightTexture0,float4( uvCookie.xy,0,-8)).w;
                        // This actually results in two light cones, one forward and one backward.
                        // We only want the forward cone, which corresponds with a negative W coordinate.
                        attenuation *= uvCookie.w < 0;

                        #if defined(SHADOWS_DEPTH)
                            shadowed= true;
                            //we can use UnitySampleShadowmap to take care of the details of sampling hard or soft shadows. 
                            //We have to supply it with the fragment position in shadow space. 
                            //The first matrix in the unity_WorldToShadow array can be used to convert from world to shadow space.
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
                
                // Because we're manually sampling the shadow map, 
                //our shadows get truncated when the edge of the map is reached. 
                //The result is that shadows get sharply cut off or are missing beyond the fade distance.
                //To fade the shadows, we must first know the distance at which they should be completely gone
                // Stable Fit mode the fading is spherical, centered on the middle of the map. 
                // Close Fit mode it's based on the view depth.
                if (shadowed)
                {       //The UnityComputeShadowFadeDistance function can figure out the correct metric for us. 
                        //It has the world position and view depth as parameters.
                        //return the distance from the shadow center, or the unmodified view depth.

                        ////////////////////////////////////////////
                        // float UnityComputeShadowFadeDistance (float3 wpos, float z) {
                        //          float sphereDist = distance(wpos, unity_ShadowFadeCenterAndType.xyz);
                        //          return lerp(z, sphereDist, unity_ShadowFadeCenterAndType.w);
                        // }
                        ////////////////////////////////////////////
                        float shadowFadeDistance =UnityComputeShadowFadeDistance(worldPos,viewZ);
                        //The shadow fade factor is a value from 0 to 1, 
                        //which indicates how much the shadows should fade away.
                         ////////////////////////////////////////////
                         // half UnityComputeShadowFade(float fadeDist) {
                         // return saturate(fadeDist * _LightShadowData.z + _LightShadowData.w);
                        // }
                        ////////////////////////////////////////////
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
                //true Screen position
                float2 uv = i.uv.xy/i.uv.w;

                //sampling the _CameraDepthTexture texture linearizing it
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv);
                depth = Linear01Depth(depth);

                //a big difference is that we supplied rays that reached the far plane to our fog shader. 
                //In this case, we are supplied with rays that reach the near plane. 
                //We have to scale them so we get rays that reach the far plane. 
                float3 rayToFarPlane = i.ray* (_ProjectionParams.z/i.ray.z);
                //Scaling this ray by the depth value gives us a position.
                float3 viewPos = rayToFarPlane *depth;
                //unity_CameraToWorld matrix, which is defined in ShaderVariables.
                float3 worldPos = mul(unity_CameraToWorld,float4(viewPos,1)).xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos- worldPos);

                //read from buffers
                float3 albedo = tex2D(_CameraGBufferTexture0,uv).rgb;
                float3 specularTint = tex2D(_CameraGBufferTexture1,uv).rgb;
                float3 smoothness = tex2D(_CameraGBufferTexture1,uv).a;
                float3 normal = tex2D(_CameraGBufferTexture2,uv).rgb*2-1;

                //surface reflectivity. We derive that from the specular tint. 
                //It's simply the strongest color component. 
                //We can use the SpecularStrength function to extract it.
                float oneMinusReflectivity = 1 - SpecularStrength(specularTint);

                /////////////lightData
                UnityLight light = CreatLight(uv,worldPos,viewPos.z);
                
                UnityIndirect indirectLight;
                indirectLight.diffuse = 0;
                indirectLight.specular =0;
                /////////////

                float4 color = UNITY_BRDF_PBS
                (
                    albedo,specularTint,oneMinusReflectivity,smoothness,normal,
                    viewDir,light,indirectLight
                );

                //for LDR  must changing the blend mode of our shader to Blend DstColor Zero.
                #if !defined(UNITY_HDR_ON)
                    color = exp2(-color);
                #endif

                return color;
            }
#endif
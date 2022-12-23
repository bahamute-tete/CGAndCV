// Upgrade NOTE: replaced 'defined POINT' with 'defined (POINT)'



#if !defined(My_LIGHTING_INCLUDE)
#define My_LIGHTING_INCLUDE
//we have to make sure that UnityCG is included first.
//because UnityShadowLibrary depends on UnityCG, but doesn't explicitly include it.
//otherwise Point Light Shadow can not compiler correct
//We can do this by including UnityPBSLighting before including AutoLight 
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc" 
#endif

            float4 _Tint;
            sampler2D _Texture, _DetailTexture,_NormalDetailMap;
            float4 _Texture_ST, _DetailTexture_ST;
            float _Smoothness;
            float4 _SpecularTint;

            sampler2D _MetallicMap;
            float _Metallic;

            sampler2D _NormalMap;
            float _BumpScale;
            float _NormalDetailScale;
            //sampler2D _ShadowMapTexture;
          
            sampler2D _EmissionMap;
            float3 _Emission;

            sampler2D _OcclusionMap;
            float _OcclusionStrength;

            sampler2D _DetailMask;

            float _AlphaCutoff;

            struct vertexData
            {
                float4 vertex:POSITION;//vertex is fixed because some Macros need this name
                float2 uv:TEXCOORD0;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
                
                
            };

      

            struct vertex2Fragment
            {
                float4 pos:SV_POSITION;//pos is fixed because some Macros need this name
                float4 uv:TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
               

                #if defined (BINORMAL_PER_FRAGMENT)    
                float4 worldTangent:TEXCOORD2;
                #else
                float3 worldTangent:TEXCOORD2;
                float3 binormal:TEXCOORD3;
                #endif

                float3 worldPosition:TEXCOORD4;

                // #if defined (SHADOWS_SCREEN)
                // float4 shadowCoordinates : TEXCOORD5;
                // #endif
                SHADOW_COORDS(5)

                #if defined (VERTEXLIGHT_ON)
                float3 vertexlightColor:TEXCOORD6;
                #endif
            };

/////////////////////////////////////////////

            void ComputeVertexLight (inout vertex2Fragment i)
            {
                #if defined (VERTEXLIGHT_ON)

                 i.vertexlightColor = Shade4PointLights(unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
                 unity_LightColor[0].rgb,unity_LightColor[1].rgb,unity_LightColor[2].rgb,unity_LightColor[3].rgb,
                 unity_4LightAtten0,i.worldPostion,i.worldNormal);
                #endif
            }

            float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) 
            {
                 
                    return cross(normal, tangent.xyz) *(binormalSign * unity_WorldTransformParams.w);
            }

            vertex2Fragment vert(vertexData v)
            {
                vertex2Fragment o;
                o.pos =UnityObjectToClipPos(v.vertex);
 
                
              
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPosition = mul(unity_ObjectToWorld,v.vertex);
               
                

                #if defined (BINORMAL_PER_FRAGMENT)    
                  o.worldTangent = float4(UnityObjectToWorldDir(v.tangent.xyz),v.tangent.w);////w is sign  no need to change 
                #else
                  o.worldTangent =UnityObjectToWorldDir(v.tangent.xyz);
                  o.binormal = CreateBinormal(o.worldNormal,o.worldTangent,v.tangent.w);
                #endif

                o.uv.xy = TRANSFORM_TEX(v.uv,_Texture);
                o.uv.zw = TRANSFORM_TEX(v.uv,_DetailTexture);

                
                TRANSFER_SHADOW(o);
                ComputeVertexLight(o);
                return o; 
            }

//////////////////////////////////////////////////////

             float GetMetallic (vertex2Fragment i)
            {
                #if defined (_METALLIC_MAP)
                    return tex2D(_MetallicMap,i.uv.xy).r;
                #else
                    return _Metallic;
                #endif
            }

            float GetSmoothness (vertex2Fragment i)
            {
                float smoothness =1;

                #if defined (_SMOOTHNESS_ALBEOD )
                    smoothness = tex2D(_Texture,i.uv.xy).a ;
                #elif defined (_SMOOTHNESS_METALLIC) && defined (_METALLIC_MAP)
                    smoothness = tex2D(_MetallicMap,i.uv.xy).a ;
                #endif

                return smoothness*_Smoothness;

            }

            float GetOcclusion(vertex2Fragment i)
            {
                #if defined(_OCCLUSION_MAP)
                  return tex2D(_OcclusionMap,i.uv.xy).g;
                #else 
                    return lerp(1,tex2D(_OcclusionMap,i.uv.xy).g,_OcclusionStrength);
                #endif
            }

            float GetDetailMask(vertex2Fragment i)
            {
                #if defined(_DETAIL_MASK)
                    return tex2D(_DetailMask,i.uv.xy).a;
                #else
                    return 1;
                #endif
            }

            float3 GetAlbedo(vertex2Fragment i)
            {
                float3 albedo = tex2D (_Texture,i.uv.xy).rgb*_Tint.rgb;

                #if defined(_DETAIL_ALBEDO_MASK)

                float3 detail = tex2D (_DetailTexture,i.uv.zw)*unity_ColorSpaceDouble;
                albedo = lerp(albedo,albedo*detail,GetDetailMask(i));

                #endif

                return albedo;
            }

            float3 GetAlpha(vertex2Fragment i)
            {
                float alpha = _Tint.a;

                #if !defined(_SMOOTHNESS_ALBEDO)

                 alpha  *=  tex2D (_Texture,i.uv.xy).a;

                #endif

                return alpha;
            }

            UnityLight CreateLight(vertex2Fragment i)
            {
                UnityLight light;
                
                #if defined (POINT) || defined (SPOT)|| defined (POINT_COOKIE)
                light.dir= normalize(_WorldSpaceLightPos0.xyz-i.worldPosition);
                #else
                light.dir= _WorldSpaceLightPos0.xyz;
                #endif

               
                //the UNITY_LIGHT_ATTENUATION macro already uses SHADOW_ATTENUATION So we can su"ce with using just that macro
                UNITY_LIGHT_ATTENUATION(attenuation,i,i.worldPosition);

                //attenuation *=GetOcclusion(i); //it makes sense that it is only applied to indirect light. 

                light.color=_LightColor0.rgb*attenuation;
                light.ndotl=DotClamped(i.worldNormal,light.dir);
                return light;
            }

            float3 BoxProject(float3 direction,float3 position,float4 cubemapPosition,float3 boxMin,float3 boxMax)
            {

                #if UNITY_SPECCUBE_BOX_PROJECTION
                
                //Combine
                UNITY_BRANCH
                if (cubemapPosition.w>0)//Unity stores this information in a fourth component of the cube map position. If that component is larger than zero, then the probe should use box projection.
                {
                    float3 factors = ((direction>0? boxMax:boxMin)-position)/direction;
                    float  scalar = min(min(factors.x,factors.y),factors.z);
                     direction =direction * scalar + (position - cubemapPosition);//Finding the new projection direction. 
                }
                
                #endif
                return  direction;
            }

            UnityIndirect CreatIndirectLight(vertex2Fragment i,float3 viewDir)
            {
                UnityIndirect lightIndirect;
                lightIndirect.diffuse=0;
                lightIndirect.specular=0;

                #if defined (VERTEXLIGHT_ON)
                    lightIndirect.diffuse = i.vertexlightColor;
                #endif

                #if defined (FORWARD_BASE_PASS)
                lightIndirect.diffuse += max(0,ShadeSH9(float4(i.worldNormal,1)));
                
                float3 reflectionDir = reflect(-viewDir,i.worldNormal);

                
                //Unity Code
                Unity_GlossyEnvironmentData envData;
                envData.roughness = 1-GetSmoothness(i);
                envData.reflUVW = BoxProject(reflectionDir,i.worldPosition,unity_SpecCube0_ProbePosition,unity_SpecCube0_BoxMin,unity_SpecCube0_BoxMax);//direction
                float3 probe0  = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0),unity_SpecCube0_HDR,envData);

                #if UNITY_SPECCUBE_BLENDING
                //Unity calculates this for us, and stored the interpolator in the fourth coordinate of unity_SpecCube0_BoxMin. 
                float interpolator = unity_SpecCube0_BoxMin.w;

                UNITY_BRANCH
                if (interpolator<0.9999)
                {
                  envData.reflUVW = BoxProject(reflectionDir,i.worldPosition,unity_SpecCube1_ProbePosition,unity_SpecCube1_BoxMin,unity_SpecCube1_BoxMax);//direction
                  //Use the UNITY_PASS_TEXCUBE_SAMPLER macro to combine the second probe's texture with the only sampler that we have to gets rid of the error.
                  float3 probe1  = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0),unity_SpecCube1_HDR,envData);
                   lightIndirect.specular = lerp(probe1,probe0,unity_SpecCube0_BoxMin.w);
                }
                else
                {
                    lightIndirect.specular = probe0;
                }
                #else
                    lightIndirect.specular = probe0;
                #endif


                    float occlusion = GetOcclusion(i);
                    lightIndirect.diffuse *= occlusion;
                    lightIndirect.specular *= occlusion;
                #endif

                return lightIndirect;

            }


            float3 GetTangentSpaceNormal(vertex2Fragment i)
            {   
                float3 normal = float3(0,0,1);

                #if defined(_NORMAL_MAP)

                 //Use Function in UnityStandardUtils 
                 normal = UnpackScaleNormal(tex2D(_NormalMap,i.uv.xy),_BumpScale);

                #endif

                #if defined(_DETAIL_NORMAL_MAP)
                float3 detailNormal = UnpackScaleNormal(tex2D(_NormalDetailMap,i.uv.zw),_NormalDetailScale);
                
                detailNormal = lerp(float3(0,0,1),detailNormal,GetDetailMask(i));
               
                //Use Function in UnityStandardUtils no need to Normalize
                 normal = BlendNormals(normal, detailNormal);

                #endif 

                return normal;
            }

            void InitializeFragmentNormal(inout vertex2Fragment i)
            {
                
                
               float3 tangentSpaceNormal = GetTangentSpaceNormal(i);
                tangentSpaceNormal = tangentSpaceNormal.xzy;

                
                #if defined (BINORMAL_PER_FRAGMENT)    
                 float3 binormal = CreateBinormal(i.worldNormal,i.worldTangent.xyz,i.worldTangent.w);
                #else
                 float3 binormal = i.binormal;
                #endif
                

                i.worldNormal =normalize(
                                    tangentSpaceNormal.x * i.worldTangent +
                                    tangentSpaceNormal.y * i.worldNormal +
                                    tangentSpaceNormal.z * binormal 
                                        );    
                

               
            }

            float3 GetEmission(vertex2Fragment i)
            {
                #if defined (FORWARD_BASE_PASS)
                    #if defined (_EMISSION_MAP)
                        return tex2D(_EmissionMap,i.uv.xy)*_Emission;
                    #else
                        return _Emission;
                    #endif
                #else
                    return 0;
                #endif

            }
           

            float4 frag(vertex2Fragment i):SV_TARGET
            {
                
                float alpha = GetAlpha(i);

                #if defined (_RENDERING_CUTOUT)//if we're really rendering a cutout material. Fully opaque materials don't need it.
                clip(alpha-_AlphaCutoff);
                #endif

                InitializeFragmentNormal(i);

                float4 tex = tex2D(_Texture,i.uv.xy)*_Tint;
                i.worldNormal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos- i.worldPosition);


               
                
                

                float3 specularTint;
                float oneMinusReflectivity;
                float3 albedo=DiffuseAndSpecularFromMetallic(GetAlbedo(i),GetMetallic(i),specularTint,oneMinusReflectivity );
                
                #if defined (_RENDERING_TRANSPARENT)
                    albedo  *= alpha;
                    alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
                #endif

                float4 col = UNITY_BRDF_PBS(albedo,specularTint,oneMinusReflectivity,GetSmoothness(i),i.worldNormal,viewDir,CreateLight(i),CreatIndirectLight(i,viewDir));

                col.rgb += GetEmission(i);

                #if defined (_RENDERING_FADE) || defined (_RENDERING_TRANSPARENT)
                  col.a = alpha;
                #endif

             

                return col;
            } 
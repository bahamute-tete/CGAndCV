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
            float _Metallic;
            sampler2D _HeightMap;
            float4 _HeightMap_TexelSize;
            sampler2D _NormalMap;
            float _BumpScale;
            float _NormalDetailScale;
            //sampler2D _ShadowMapTexture;
          

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

                light.color=_LightColor0.rgb*attenuation;
                light.ndotl=DotClamped(i.worldNormal,light.dir);
                return light;
            }

            float3 BoxProject(float3 direction,float3 position,float4 cubemapPosition,float3 boxMin,float3 boxMax)
            {

                #if UNITY_SPECCUBE_BOX_PROJECTION
                // boxMin -=position;//relative to the surface position.
                // boxMax -=position;//relative to the surface position.

                // float x = (direction.x>0? boxMax.x: boxMin.x)/direction.x;//Dividing the appropriate bounds by the X component of the direction gives us the scalar that we need.
                // float y = (direction.y>0? boxMax.y: boxMin.y)/direction.y;
                // float z = (direction.z>0? boxMax.z: boxMin.z)/direction.z;

                // float scalar = min(min(x,y),z);//use distance from the bounds face  closest  

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

                //Our method 
                // float roughness = 1 - _Smoothness;
                // roughness *=1.7 -0.7*roughness;//1.7*r − 0.7r*r unity formula
                // float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0,reflectionDir,roughness*UNITY_SPECCUBE_LOD_STEPS);//sample skyBox with LOD relative roughness
                // lightIndirect.specular = DecodeHDR( envSample,unity_SpecCube0_HDR);//reflection convert HDR to RGB 
                //Unity Code
                Unity_GlossyEnvironmentData envData;
                envData.roughness = 1-_Smoothness;
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
               
                #endif

                return lightIndirect;

            }



            void InitializeFragmentNormal(inout vertex2Fragment i)
            {
                
                
                //Use Function in UnityStandardUtils 
                float3 normal = UnpackScaleNormal(tex2D(_NormalMap,i.uv.xy),_BumpScale);
                float3 detailNormal = UnpackScaleNormal(tex2D(_NormalDetailMap,i.uv.zw),_NormalDetailScale);
                
               
                //Use Function in UnityStandardUtils no need to Normalize
                float3 tangentSpaceNormal = BlendNormals(normal, detailNormal);
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

            float4 frag(vertex2Fragment i):SV_TARGET
            {

                InitializeFragmentNormal(i);

                float4 tex = tex2D(_Texture,i.uv.xy)*_Tint;
                i.worldNormal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos- i.worldPosition);


               
                float3 albedo =tex2D(_Texture,i.uv.xy).rgb*_Tint.rgb;
                albedo *= tex2D(_DetailTexture,i.uv.zw)*unity_ColorSpaceDouble;
                

                float3 specularTint;
                float oneMinusReflectivity;
                albedo=DiffuseAndSpecularFromMetallic(albedo,_Metallic,specularTint,oneMinusReflectivity );

                return UNITY_BRDF_PBS(albedo,specularTint,oneMinusReflectivity,_Smoothness,i.worldNormal,viewDir,CreateLight(i),CreatIndirectLight(i,viewDir));
            } 
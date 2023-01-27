// Upgrade NOTE: replaced 'defined POINT' with 'defined (POINT)'



#if !defined(My_LIGHTING_INCLUDE)
#define My_LIGHTING_INCLUDE
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
          

            struct vertexData
            {
                float4 position:POSITION;
                float2 uv:TEXCOORD0;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
                
                
            };

      

            struct vertex2Fragment
            {
                float4 position:SV_POSITION;
                float4 uv:TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
               

                #if defined (BINORMAL_PER_FRAGMENT)    
                float4 worldTangent:TEXCOORD2;
                #else
                float3 worldTangent:TEXCOORD2;
                float3 binormal:TEXCOORD3;
                #endif

                float3 worldPostion:TEXCOORD4;

                #if defined (VERTEXLIGHT_ON)
                float3 vertexlightColor:TEXCOORD5;
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
                o.position =UnityObjectToClipPos(v.position);
 
                
              
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPostion = mul(unity_ObjectToWorld,v.position);
               
                

                #if defined (BINORMAL_PER_FRAGMENT)    
                  o.worldTangent = float4(UnityObjectToWorldDir(v.tangent.xyz),v.tangent.w);////w is sign  no need to change 
                #else
                  o.worldTangent =UnityObjectToWorldDir(v.tangent.xyz);
                  o.binormal = CreateBinormal(o.worldNormal,o.worldTangent,v.tangent.w);
                #endif

                o.uv.xy = TRANSFORM_TEX(v.uv,_Texture);
                o.uv.zw = TRANSFORM_TEX(v.uv,_DetailTexture);
                ComputeVertexLight(o);
                return o; 
            }


            UnityLight CreateLight(vertex2Fragment i)
            {
                UnityLight light;
                
                #if defined (POINT) || defined (SPOT)|| defined (POINT_COOKIE)
                light.dir= normalize(_WorldSpaceLightPos0.xyz-i.worldPostion);
                #else
                light.dir= _WorldSpaceLightPos0.xyz;
                #endif

                //Canculate attenuation from self
                //float3 lightVector = _WorldSpaceLightPos0.xyz-i.worldPostion;
                //float attenuation =1/(1+dot(lightVector,lightVector));//1/(d*d) is infinite for d = zero so use 1/(1+dot(lightVector,lightVector)) to get the attenuation
                //Use UnityMarcro
                UNITY_LIGHT_ATTENUATION(attenuation,0,i.worldPostion);

                light.color=_LightColor0.rgb*attenuation;
                light.ndotl=DotClamped(i.worldNormal,light.dir);
                return light;
            }

            UnityIndirect CreatIndirectLight(vertex2Fragment i)
            {
                UnityIndirect lightIndirect;
                lightIndirect.diffuse=0;
                lightIndirect.specular=0;

                #if defined (VERTEXLIGHT_ON)
                    lightIndirect.diffuse = i.vertexlightColor;
                #endif

                #if defined (FORWARD_BASE_PASS)
                lightIndirect.diffuse += max(0,ShadeSH9(float4(i.worldNormal,1)));
                #endif

                return lightIndirect;

            }

            void InitializeFragmentNormal(inout vertex2Fragment i)
            {
                //Forward Difference on U direction
                // float2 delta = float2(_HeightMap_TexelSize.x,0);
                // float h1 = tex2D(_HeightMap,i.uv);
                // float h2  = tex2D(_HeightMap,i.uv+delta);
                //float3 tangent = float3(delta.x,h2-h1,0);//because we're normalizing anyway, we can scale our tangent vector by δ. This eliminates a division and improves precision.
                //float3 tangent = float3(1,h2-h1,0);//scale some factor
                //i.worldNormal =float3(h1-h2,1,0);//rotate the tangent vector 90 degrees 

                //Center Difference
                float2 du = float2(_HeightMap_TexelSize.x*0.5,0);
                float u1 = tex2D(_HeightMap,i.uv-du);
                float u2  = tex2D(_HeightMap,i.uv+du);
                float3 tu = float3(1,u2-u1,0);

                float2 dv = float2(_HeightMap_TexelSize.y*0.5,0);
                float v1 = tex2D(_HeightMap,i.uv-dv);
                float v2  = tex2D(_HeightMap,i.uv+dv);
                float3 tv = float3(0,v2-v1,1);

                //float3 n  = cross(tv, tu);
                i.worldNormal = normalize(float3(u1 - u2, 1, v1 - v2)); //construct the vector directly 
            }

            void InitializeFragmentNormal2(inout vertex2Fragment i)
            {
                
                // i.worldNormal.xy = tex2D(_NormalMap,i.uv).wy*2-1;//DXT5NM x in Alpha Channel y in Green channel
                // i.worldNormal.xy *= _BumpScale;
                // i.worldNormal.z = sqrt(1-saturate(dot(i.worldNormal.xy,i.worldNormal.xy)));
                //Use Function in UnityStandardUtils 
                float3 normal = UnpackScaleNormal(tex2D(_NormalMap,i.uv.xy),_BumpScale);
                float3 detailNormal = UnpackScaleNormal(tex2D(_NormalDetailMap,i.uv.zw),_NormalDetailScale);
                
/////////////////////////////////////////////////////////NormalBlend
// tangent vector  = [1,f`(u),0]
// bitangent vector = [0,f`(v),1]
// normal vector = tangent  corss bitangent = [-f`(u),1,-f`(v)]
// in normal map should swap Y and Z component which wanted use world derivative representation
//===>[-f`(u),-f`(v),1]
// because  normalized normal vector is equal multiply a scalar S so original normal vector ==>[-Sf`(u),-Sf`(v),S]
// if we divide Z component we should get derivatives f`(u) and f`(v)
///////////////////////////////////////////////////////////whiteout methord///////
//so 2 normal vector blend ==>[M(x)/M(z)+D(x)/D(z),M(y)/M(y)+D(y)/D(z),1] (M= mainTexnormal D= detailTexNormal)
// if we multiply the new normal by M(z)D(Z) we can get [M(x)D(z)+D(x)M(z),M(y)D(z)+D(y)M(z),M(z)D(z)]
//drop the scaling of X and Y, ==>[M(x)+D(x),M(y)+D(y),M(z)D(z)]
//////////////////////////////////////////////////////////

                //i.worldNormal = (normal + detailNormal) * 0.5;//average 

                //i.worldNormal = float3(normal.xy/normal.z+detailNormal.xy/detailNormal.z,1);//derivative Add

                // i.worldNormal = float3(normal.xy+detailNormal.xy,normal.z*detailNormal.z);//whiteout blending
                // i.worldNormal = normalize( i.worldNormal.xzy);//Swap the Y and Z 

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

                InitializeFragmentNormal2(i);

                float4 tex = tex2D(_Texture,i.uv.xy)*_Tint;
                i.worldNormal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos- i.worldPostion);


               
                float3 albedo =tex2D(_Texture,i.uv.xy).rgb*_Tint.rgb;
                albedo *= tex2D(_DetailTexture,i.uv.zw)*unity_ColorSpaceDouble;
                // float3 height = tex2D(_HeightMap,i.uv);
                // albedo *=height;

                float3 specularTint;
                float oneMinusReflectivity;
                albedo=DiffuseAndSpecularFromMetallic(albedo,_Metallic,specularTint,oneMinusReflectivity );

                return UNITY_BRDF_PBS(albedo,specularTint,oneMinusReflectivity,_Smoothness,i.worldNormal,viewDir,CreateLight(i),CreatIndirectLight(i));
            } 
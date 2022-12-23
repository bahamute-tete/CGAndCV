// Upgrade NOTE: replaced 'defined POINT' with 'defined (POINT)'



#if !defined(My_LIGHTING_INCLUDE)
#define My_LIGHTING_INCLUDE
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc" 
#endif

            float4 _Tint;
            sampler2D _Texture;
            float4 _Texture_ST;
            float _Smoothness;
            float4 _SpecularTint;
            float _Metallic;
           

            struct vertexData
            {
                float4 position:POSITION;
                float2 uv:TEXCOORD0;
                float3 normal:NORMAL;
                
                
            };

            struct vertex2Fragment
            {
                float4 position:SV_POSITION;
                float2 uv:TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
                float3 worldPostion:TEXCOORD2;

                #if defined (VERTEXLIGHT_ON)
                float3 vertexlightColor:TEXCOORD3;
                #endif
            };


            void ComputeVertexLight (inout vertex2Fragment i)
            {
                #if defined (VERTEXLIGHT_ON)
                //One Light
                // float3 lightPos = float3(unity_4LightPosX0.x,unity_4LightPosY0.x,unity_4LightPosZ0.x);//unity_4LightPosX0, unity_4LightPosY0, and unity_4LightPosZ0 in UnityShaderVariables
                //  float3 lightVector = lightPos -i.worldPostion;
                //  float3 lightDir = normalize(lightVector);
                //  float ndotl = DotClamped(lightDir,i.worldNormal);
                //  float attenuation = 1/(1+dot(lightVector,lightVector)*unity_4LightAtten0.x);//UnityShaderVariables provides another variable, unity_4LightAtten0
                //  i.vertexlightColor = unity_LightColor[0].rgb * attenuation *ndotl;//UnityShaderVariables defines an array of vertex light colors
                //Canculate four light 
                 i.vertexlightColor = Shade4PointLights(unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
                 unity_LightColor[0].rgb,unity_LightColor[1].rgb,unity_LightColor[2].rgb,unity_LightColor[3].rgb,
                 unity_4LightAtten0,i.worldPostion,i.worldNormal);
                #endif
            }

            vertex2Fragment vert(vertexData v)
            {
                vertex2Fragment o;
                o.position =UnityObjectToClipPos(v.position);
 
                o.uv =TRANSFORM_TEX(v.uv,_Texture);
              
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPostion = mul(unity_ObjectToWorld,v.position);
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

            float4 frag(vertex2Fragment i):SV_TARGET
            {

                

                float4 tex = tex2D(_Texture,i.uv)*_Tint;
                i.worldNormal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos- i.worldPostion);


               
                float3 albedo =tex2D(_Texture,i.uv).rgb*_Tint.rgb;

        

                float3 specularTint;
                float oneMinusReflectivity;
                albedo=DiffuseAndSpecularFromMetallic(albedo,_Metallic,specularTint,oneMinusReflectivity );
                
      
                // float3 shColor = ShadeSH9(float4(i.worldNormal,1));
                // return float4 (shColor,1);
                

                return UNITY_BRDF_PBS(albedo,specularTint,oneMinusReflectivity,_Smoothness,i.worldNormal,viewDir,CreateLight(i),CreatIndirectLight(i));
            }


        
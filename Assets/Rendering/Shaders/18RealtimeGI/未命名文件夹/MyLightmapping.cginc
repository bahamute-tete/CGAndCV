// Upgrade NOTE: replaced 'defined FOG_DEPTH' with 'defined (FOG_DEPTH)'

// Upgrade NOTE: replaced 'defined POINT' with 'defined (POINT)'



#if !defined(MY_LIGHTMAPPING_INCLUD)
    #define MY_LIGHTMAPPING_INCLUD
 
#include "UnityPBSLighting.cginc"
//we can use the UnityMetaFragment function defined in the UnityMetaPass include file. 
//It has a UnityMetaInput structure as parameter which contains both the albedo and emission. 
//The function will decide which one to output and how to encode it.
//UnityMetaInput also contains the specular color, even though it isn't stored in the lightmap. 
//It's used for some editor visualizations,
#include "UnityMetaPass.cginc"




            float4 _Color;
            sampler2D _Texture, _DetailTexture,_DetailMask;
            float4 _Texture_ST, _DetailTexture_ST;


            sampler2D _MetallicMap;
            float _Metallic;
            float _Smoothness;

            sampler2D _EmissionMap;
            float3 _Emission;

           

            struct VertexData
            {
                float4 vertex:POSITION;//vertex is fixed because some Macros need this name
                float2 uv:TEXCOORD0;
                float2 uv1:TEXCOORD1;//Lightmap uv
                float2 uv2:TEXCOORD2;
                
                
            };

      

            struct Interpolators
            {
                float4 pos:SV_POSITION;//pos is fixed because some Macros need this name
                float4 uv:TEXCOORD0;
            };


//////////////////////////////////////////////////////

             float GetMetallic (Interpolators i)
            {
                #if defined (_METALLIC_MAP)
                    return tex2D(_MetallicMap,i.uv.xy).r;
                #else
                    return _Metallic;
                #endif
            }

            float GetSmoothness (Interpolators i)
            {
                float smoothness =1;

                #if defined (_SMOOTHNESS_ALBEOD )
                    smoothness = tex2D(_Texture,i.uv.xy).a ;
                #elif defined (_SMOOTHNESS_METALLIC) && defined (_METALLIC_MAP)
                    smoothness = tex2D(_MetallicMap,i.uv.xy).a ;
                #endif

                return smoothness*_Smoothness;

            }



            float GetDetailMask(Interpolators i)
            {
                #if defined(_DETAIL_MASK)
                    return tex2D(_DetailMask,i.uv.xy).a;
                #else
                    return 1;
                #endif
            }

            float3 GetAlbedo(Interpolators i)
            {
                float3 albedo = tex2D (_Texture,i.uv.xy).rgb*_Color.rgb;

                #if defined(_DETAIL_ALBEDO_MASK)

                float3 detail = tex2D (_DetailTexture,i.uv.zw)*unity_ColorSpaceDouble;
                albedo = lerp(albedo,albedo*detail,GetDetailMask(i));

                #endif

                return albedo;
            }


            float3 GetEmission(Interpolators i)
            {
               
                    #if defined (_EMISSION_MAP)
                        return tex2D(_EmissionMap,i.uv.xy)*_Emission;
                    #else
                        return _Emission;
                    #endif
            }
/////////////////////////////////////////////////////////////////
            Interpolators vert(VertexData v)
            {
                Interpolators o;
                
                // v.vertex.xy = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
                // v.vertex.z = v.vertex.z>0?0.0001:0;

                o.pos = UnityMetaVertexPosition(v.vertex,v.uv1,v.uv2,unity_LightmapST,unity_DynamicLightmapST);
                //o.pos =UnityObjectToClipPos(v.vertex);
 
            
                o.uv.xy = TRANSFORM_TEX(v.uv,_Texture);
                o.uv.zw = TRANSFORM_TEX(v.uv,_DetailTexture);

               
                return o; 
            }
           
//////////////////////////////////////////////////////////////
            float4 frag(Interpolators i):SV_TARGET
            {
                UnityMetaInput surfaceData = (UnityMetaInput)0;
                
                //return (1 - smoothness) * (1 - smoothness);
                float roughness = SmoothnessToRoughness(GetSmoothness(i)*0.5);
	            float oneMinusReflectivity;
	            surfaceData.Albedo = DiffuseAndSpecularFromMetallic( GetAlbedo(i), GetMetallic(i),
		                                                            surfaceData.SpecularColor, oneMinusReflectivity
	                                                                );

                surfaceData.Albedo +=surfaceData.SpecularColor *roughness;

               //Material.globalIlluminationFlags must Set to Bake
	            surfaceData.Emission = GetEmission(i);
              
            
                return UnityMetaFragment(surfaceData);
            } 

#endif
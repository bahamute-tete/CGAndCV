
Shader "Unlit/RealtimeGI"
{
    Properties
    {
        _Color("Color",Color) =(1,1,1,1)//The lightmapper expects the alpha cutoff value to be stored in the _Color property, 
        _Texture("Albedo",2D) ="white"{}
        _Smoothness("smoothness",Range(0,1)) =0.5
        _SpecularTint("specular",Color) =(1,1,1,1)

        [NoScaleOffsets]_MetallicMap("Metallic",2D) ="white"{}
        [Gamma]_Metallic("Metallic",Range(0,1)) =0.5

        [NoScaleOffsets]_NormalMap("Normal",2D) ="bump"{}
        _BumpScale("BumpSclae",Range(0,1)) =1
        _DetailTexture("Detail Albedo",2D) ="gray"{}
        [NoScaleOffsets]_NormalDetailMap("Detail Normal",2D) ="bump"{}
        _NormalDetailScale("DetailBumpSclae",range(0,1)) =1

        [NoScaleOffsets] _EmissionMap("Emission",2D) ="Black"{}
        _Emission("Emission",Color) = (0,0,0)

        [NoScaleOffsets]_OcclusionMap("Occlusion",2D) ="white"{}
        _OcclusionStrength  ("Occlusion Strength",Range(0,1)) =1

        [NoScaleOffsets] _DetailMask("Detail Mask",2D) ="white"{}

        _Cutoff("Alpha Cutoff",Range(0,1)) =0.5//The lightmapper expects the alpha cutoff value to be stored in the _Cutoff property, 

        [HiddenInspector]_SrcBlend ("_SrcBlend",float) =1
        [HiddenInspector]_DstBlend ("_DstBlend",float) =0

        [HiddenInspector]_ZWrite ("_ZWrite",float) = 1

    }

    CGINCLUDE
     #define BINORMAL_PER_FRAGMENT
     #define FOG_DISTANCE
    ENDCG

    SubShader
    {
        

        Pass
        {
            Tags { "LightMode" ="ForwardBase" }
            LOD 100
            Blend  [_SrcBlend]  [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma shader_feature _METALLIC_MAP
            #pragma shader_feature _ _SMOOTHNESS_ALBEOD _SMOOTHNESS_METALLIC
            #pragma shader_feature _NORMAL_MAP
            #pragma shader_feature _EMISSION_MAP
            #pragma shader_feature  _OCCULISION_MAP
            #pragma shader_feature _DETAIL_MASK
            #pragma shader_feature _DETAIL_ALBEDO_MAP
            #pragma shader_feature _DETAIL_NORMAL_MAP
            #pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT

        //     #pragma multi_compile _ SHADOWS_SCREEN
           
        //    //Unity will never include vertex lights. 
        //    //Their keywords are mutually exclusive. 
        //    //So we don't need a variant with both VERTEXLIGHT_ON and LIGHTMAP_ON at the same time.
        //     #pragma multi_compile _ LIGHTMAP_ON VERTEXLIGHT_ON
            
            //It will take care of all the lightmapping keywords, and also the VERTEXLIGHT_ON keyword.
            #pragma multi_compile_fwdbase
            #pragma multi_compile FORWARD_BASE_PASS

            #pragma multi_compile_fog
            

            #include "UnityCG.cginc" 
            
            #include "UnityStandardBRDF.cginc"  
            #include "MyRealtimeGILight.cginc"   
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" ="ForwardAdd" }
            LOD 100
            Blend  [_SrcBlend]  One
            ZWrite Off

            
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma shader_feature _METALLIC_MAP
            #pragma shader_feature _ _SMOOTHNESS_ALBEOD _SMOOTHNESS_METALLIC
            #pragma shader_feature _NORMAL_MAP
            #pragma shader_feature _DETAIL_MASK
            #pragma shader_feature _DETAIL_ALBEDO_MAP
            #pragma shader_feature _DETAIL_NORMAL_MAP
            #pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
            
            #pragma multi_compile_fwdadd_fullshadows
            
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            #include "UnityStandardBRDF.cginc"  

            
            #include "MyRealtimeGILight.cginc"      
            ENDCG
        }

        pass
        {
            Tags { "LightMode" ="Deferred" }

            
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma  exclude_renderers nomrt
            #pragma shader_feature _METALLIC_MAP
            #pragma shader_feature _ _SMOOTHNESS_ALBEOD _SMOOTHNESS_METALLIC
            #pragma shader_feature _NORMAL_MAP
            #pragma shader_feature _EMISSION_MAP
            #pragma shader_feature  _OCCULISION_MAP
            #pragma shader_feature _DETAIL_MASK
            #pragma shader_feature _DETAIL_ALBEDO_MAP
            #pragma shader_feature _DETAIL_NORMAL_MAP
            #pragma shader_feature _ _RENDERING_CUTOUT 

            // #pragma multi_compile _ SHADOWS_SCREEN
            // #pragma multi_compile _ VERTEXLIGHT_ON
            // #pragma multi_compile _ LIGHTMAP_ON
            
            
            // #pragma multi_compile _  UNITY_HDR_ON
            #define DEFERRED_PASS
            
            //#pragma multi_compile_prepassfinal directive. It takes care of the lightmapping and the HDR keywords.
            #pragma multi_compile_prepassfinal
            #include "UnityCG.cginc" 
            
            #include "UnityStandardBRDF.cginc"  
            #include "MyRealtimeGILight.cginc"   
            ENDCG

        }

        pass
        {
            Tags { "LightMode" ="ShadowCaster"}
            CGPROGRAM

            #pragma target 3.0

            #pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
            #pragma shader_feature _ _SMOOTHNESS_ALBEDO
            #pragma shader_feature _SEMITRANSPARENT_SHADOWS

            #pragma multi_compile_shadowcaster
            
            #pragma vertex vert
            #pragma fragment frag

            #include "MyShadow.cginc" 

            ENDCG
        }

        pass
        {   
            //figure out the surface color of objects, 
            //the lightmapper looks for a shader pass with its light mode set to Meta. 
            //This pass is only used by the lightmapper and isn't included in builds. 
             Tags { "LightMode" ="Meta"}

             Cull off

             CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag 

                #pragma shader_feature _METALLIC_MAP
                #pragma shader_feature _ _SMOOTHNESS_ALBEOD _SMOOTHNESS_METALLIC
                #pragma shader_feature _EMISSION_MAP
                #pragma shader_feature _DETAIL_MASK
                #pragma shader_feature _DETAIL_ALBEDO_MAP

                #include "MyLightmapping.cginc" 

             ENDCG
        }
    }

    CustomEditor "RealTimeGIGUI"
    
}

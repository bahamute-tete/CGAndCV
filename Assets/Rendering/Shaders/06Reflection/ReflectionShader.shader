

Shader "Unlit/Reflection"
{
    Properties
    {
        _Tint("Color",Color) =(1,1,1,1)
        _Texture("Albedo",2D) ="white"{}
        _Smoothness("smoothness",Range(0,1)) =0.5
        _SpecularTint("specular",Color) =(1,1,1,1)
        [Gamma]_Metallic("Metallic",Range(0,1)) =0.5
        [NoScaleOffsets]_HeightMap("Height",2D) ="gray"{}
        [NoScaleOffsets]_NormalMap("Normal",2D) ="bump"{}
        _BumpScale("BumpSclae",Range(0,1)) =1
        _DetailTexture("DetailTexture",2D) ="gray"{}
        [NoScaleOffsets]_NormalDetailMap("NormalDetailMap",2D) ="bump"{}
        _NormalDetailScale("DetailBumpSclae",range(0,1)) =1

    }

    CGINCLUDE
     #define BINORMAL_PER_FRAGMENT
    ENDCG

    SubShader
    {
        

        Pass
        {
            Tags { "LightMode" ="ForwardBase" }
            LOD 100
            CGPROGRAM
           
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma multi_compile _ SHADOWS_SCREEN
            #pragma multi_compile _ VERTEXLIGHT_ON
            
            #pragma multi_compile FORWARD_BASE_PASS
            #pragma target 3.0

            #include "UnityCG.cginc" 
            
            #include "UnityStandardBRDF.cginc"  
            #include "MyReflection.cginc"   
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" ="ForwardAdd" }
            LOD 100
            Blend One One
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma multi_compile_fwdadd_fullshadows
            #pragma target 3.0


            #include "UnityCG.cginc"
            
            #include "UnityStandardBRDF.cginc"  

            
            #include "MyReflection.cginc"      
            ENDCG
        }

        pass
        {
            Tags { "LightMode" ="ShadowCaster"}
            CGPROGRAM

            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            
            #pragma vertex vert
            #pragma fragment frag

            #include "MyShadow.cginc" 

            ENDCG
        }
    }
}

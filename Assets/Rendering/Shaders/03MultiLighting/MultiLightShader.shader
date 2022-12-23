

Shader "Unlit/MultiLightShader"
{
    Properties
    {
        _Tint("Color",Color) =(1,1,1,1)
        _Texture("Albedo",2D) ="white"{}
        _Smoothness("smoothness",Range(0,1)) =0.5
        _SpecularTint("specular",Color) =(1,1,1,1)
        [Gamma]_Metallic("Metallic",Range(0,1)) =0.5

    }
    SubShader
    {
        

        Pass
        {
            Tags { "LightMode" ="ForwardBase" }
            LOD 100
            CGPROGRAM
           
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile _ VERTEXLIGHT_ON
            #pragma multi_compile FORWARD_BASE_PASS
            #pragma target 3.0

            #include "UnityCG.cginc"
            
            #include "UnityStandardBRDF.cginc"  
            #include "MyLighting.cginc"   
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
            // make fog work
            #pragma multi_compile_fog
            // #pragma multi_compile DIRECTIONAL POINT SPOT DIRECTIONAL_COOKIE POINT_COOKIE
            #pragma multi_compile_fwdadd
            #pragma target 3.0


            #include "UnityCG.cginc"
            
            #include "UnityStandardBRDF.cginc"  

            
            #include "MyLighting.cginc"   
            ENDCG
        }
    }
}

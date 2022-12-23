// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/SplatTexture"
{
    Properties
    {
        _Tint("Color",Color) =(1,1,1,1)
        _Texture("SplatTexture",2D) ="white"{}
        [NoScaleOffset]_Texture01("Texture1" ,2D) ="white"{} 
        [NoScaleOffset]_Texture02("Texture2" ,2D) ="white"{} 
        [NoScaleOffset]_Texture03("Texture3" ,2D) ="white"{}
        [NoScaleOffset]_Texture04("Texture4" ,2D) ="white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
           
            sampler2D _Texture,_Texture01,_Texture02,_Texture03,_Texture04;
            float4 _Texture_ST;


            struct vertexData
            {
                float4 position:POSITION;
                float2 uv:TEXCOORD0;
            };

            struct vertex2Fragment
            {
                float4 position:SV_POSITION;
                float2 uv:TEXCOORD0;
                float2 uvSplat:TEXCOORD1;
            };

            vertex2Fragment vert(vertexData v)
            {
                vertex2Fragment o;
                o.position =UnityObjectToClipPos(v.position);
                // o.uv =v.uv*_Texture_ST.xy+_Texture_ST.zw;
                o.uv =TRANSFORM_TEX(v.uv,_Texture);
                o.uvSplat =v.uv;

                return o; 
            }

            float4 frag(vertex2Fragment i):SV_TARGET
            {
                
                float4 splat = tex2D(_Texture,i.uvSplat);
                float4 col = tex2D(_Texture01,i.uv)*splat.r+
                             tex2D(_Texture02,i.uv)*splat.g+
                             tex2D(_Texture03,i.uv)*splat.b+
                             tex2D(_Texture04,i.uv)*(1-splat.r- splat.g- splat.b);
                return col;
            }
            ENDCG
        }
    }
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/MyFirstShader"
{
    Properties
    {
        _Tint("Color",Color) =(1,1,1,1)
        _Texture("MainTexture",2D) ="white"{}
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
            float4 _Tint;
            sampler2D _Texture;
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
            };

            vertex2Fragment vert(vertexData v)
            {
                vertex2Fragment o;
                o.position =UnityObjectToClipPos(v.position);
                // o.uv =v.uv*_Texture_ST.xy+_Texture_ST.zw;
                o.uv =TRANSFORM_TEX(v.uv,_Texture);

                return o; 
            }

            float4 frag(vertex2Fragment i):SV_TARGET
            {
                // float4 col = float4 (i.uv,1,1);
                float4 tex = tex2D(_Texture,i.uv);
                return tex*_Tint;
            }
            ENDCG
        }
    }
}

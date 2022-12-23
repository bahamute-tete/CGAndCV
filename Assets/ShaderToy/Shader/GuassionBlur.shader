Shader "Unlit/GuassionBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE 
             #include "UnityCG.cginc"
            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f_Blur
            {
                float2 uv[5] : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

             v2f_Blur vert_horizon (a2v v)
            {
                v2f_Blur o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv[0] = v.uv;
                o.uv[1] = v.uv+_MainTex_TexelSize.xy*float2(-1,0);
                o.uv[2] = v.uv+_MainTex_TexelSize.xy*float2(1,0);
                o.uv[3] = v.uv+_MainTex_TexelSize.xy*float2(-2,0);
                o.uv[4] = v.uv+_MainTex_TexelSize.xy*float2(2,0);
                return o;
            }

              v2f_Blur vert_vertical (a2v v)
            {
                v2f_Blur o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv[0] = v.uv;
                o.uv[1] = v.uv+_MainTex_TexelSize.xy*float2(0,-1);
                o.uv[2] = v.uv+_MainTex_TexelSize.xy*float2(0,1);
                o.uv[3] = v.uv+_MainTex_TexelSize.xy*float2(0,-2);
                o.uv[4] = v.uv+_MainTex_TexelSize.xy*float2(0,2);
                return o;
            }


            float4 Convolution(v2f_Blur i):SV_Target
            {

                float3 col0 =tex2D(_MainTex, i.uv[0]).xyz*0.4026;
                float3 col1= tex2D(_MainTex, i.uv[1]).xyz*0.2442;
                float3 col2= tex2D(_MainTex, i.uv[2]).xyz*0.2442;
                float3 col3= tex2D(_MainTex, i.uv[3]).xyz*0545;
                float3 col4= tex2D(_MainTex, i.uv[4]).xyz*0545;

                return float4(col0+col1+col2+col3+col4,1);

            }

    ENDCG


    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Name "HORIZION"
        pass 
        {
            CGPROGRAM
            #pragma vertex vert_horizon
            #pragma fragment Convolution
            ENDCG

        }

        Name "VERTICAL"
        pass 
        {
            CGPROGRAM
            #pragma vertex vert_vertical
            #pragma fragment Convolution
            ENDCG

        }


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

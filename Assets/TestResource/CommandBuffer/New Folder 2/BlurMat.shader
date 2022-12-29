Shader "Unlit/BlurMat"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        _BlurRaidu("_BlurRaidu",Float)=0.1
        _RenderTex ("RenderTexture", 2D) = "white" {}
        _OutlineTex ("RenderTexture", 2D) = "white" {}
        _OutlineCol("_OutlineCol", Color) = (1,1,1,1)
        _LineIntensity("_LineIntensity", Float) =1
    }

    CGINCLUDE

        float _BlurRaidu;
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv[5] : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };




        v2f vert_Horizon (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            

            o.uv[0]= v.uv;
            o.uv[1]= v.uv+float2(1,0)*_BlurRaidu*_MainTex_TexelSize.xy;
            o.uv[2]= v.uv+float2(-1,0)*_BlurRaidu*_MainTex_TexelSize.xy;
            o.uv[3]= v.uv+float2(2,0)*_BlurRaidu*_MainTex_TexelSize.xy;
            o.uv[4]= v.uv+float2(-2,0)*_BlurRaidu*_MainTex_TexelSize.xy;

            return o;
        }

        v2f vert_Vertical(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            

            o.uv[0]= v.uv;
            o.uv[1]= v.uv+float2(0,1)*_BlurRaidu*_MainTex_TexelSize.xy;
            o.uv[2]= v.uv+float2(0,-1)*_BlurRaidu*_MainTex_TexelSize.xy;
            o.uv[3]= v.uv+float2(0,2)*_BlurRaidu*_MainTex_TexelSize.xy;
            o.uv[4]= v.uv+float2(0,-2)*_BlurRaidu*_MainTex_TexelSize.xy;

            return o;
        }



     
    half4 frag_Convolution (v2f i) : SV_Target 
    {
        float4 col =0;
        float4 col1 = tex2D(_MainTex,i.uv[0])*0.4026;
        float4 col2 = tex2D(_MainTex,i.uv[1])*0.2442;
        float4 col3 = tex2D(_MainTex,i.uv[2])*0.2442;
        float4 col4= tex2D(_MainTex,i.uv[3])*0.0545;
        float4 col5 = tex2D(_MainTex,i.uv[4])*0.0545;

        col = col1+col2+col3+col4+col5;

        return float4(col.xyz,1.);

    }


    ENDCG


    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Horizontal"
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert_Horizon
            #pragma fragment frag_Convolution
            ENDCG
        }

         Pass
        {
            Name "Vertical"
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert_Vertical
            #pragma fragment frag_Convolution
            ENDCG
        }


        Pass 
        {
            Name "SubTex"
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag

             struct appdata1
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f1
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f1 vert (appdata1 v)
            {
                v2f1 o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _OrginalTex;
            sampler2D _RenderTex;
            float4 _OutlineCol;
            float _LineIntensity;

            half4 frag (v2f1 i) : SV_Target 
            {
               
                
                float4 commadColor = tex2D(_RenderTex,i.uv);
                float4 orginalColor =tex2D(_OrginalTex,i.uv);
                float4 fColor =commadColor-orginalColor; 

                return float4(fColor.xyz,1.)*_OutlineCol*_LineIntensity;

            }
            ENDCG

        }

         Pass 
        {
            Name "LerpTexture"
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag

             struct appdata2
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f2
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f2 vert (appdata2 v)
            {
                v2f2 o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _OutlineTex;
            sampler2D _OrginalTex;

            half4 frag (v2f2 i) : SV_Target 
            {

                float4 outlineCol = tex2D(_OutlineTex,i.uv);
                float4 orginalColor =tex2D(_OrginalTex,i.uv);
                float4 fColor =outlineCol+orginalColor; 
                return float4(fColor.xyz,1.);

            }
            ENDCG

        }

        
    }
}

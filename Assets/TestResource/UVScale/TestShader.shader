Shader "Unlit/TestShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScreenAspect ("ScreenAspect",Float)=2
        _TexAspect ("TextureAspect",Float)=0.75
        _offsetsX ("OffsetsX", float) = 0.5 
        _offsetsY ("OffsetsY", float) = 0.5 

         _Scale ("_Scale", float) = 0.5 


        _scaleX("SclaeX",float)=1
        _scaleY("SclaeY",float)=1
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
            #pragma shader_feature _HORIZON
            #pragma shader_feature _VERTICAL

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _ScreenAspect;
            float _TexAspect;
            float _offsetsX;
            float _offsetsY;

            float _scaleX;
            float _scaleY;

            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.uv = v.uv*_MainTex_ST.xy +_MainTex_ST.zw;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // float smoothstep(float t1, float t2, float x)
            // {
            
            //     x = clamp((x - t1) / (t2 - t1), 0.0, 1.0); 
            
            //     return x * x * (3 - 2 * x);
            // }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float2 uv = i.uv*2.-1.;//[-1,1]
                
                float d1=0;
                float d2=0;
                uv *= float2(_ScreenAspect,1);

                #ifdef _HORIZON
                uv*= float2(1.,_TexAspect);
                uv/= _ScreenAspect*2.;
                #endif

                #ifdef _VERTICAL
                uv *=float2(_TexAspect,1.);
                uv /=2;
                #endif

                 uv +=0.5;

                float4 bg = 1;
                fixed4 col = tex2D(_MainTex, uv);
                
                #ifdef _HORIZON
                 d1  = smoothstep(0.005,0.01,uv.y);
                 d2  = smoothstep(0.01,0.005,uv.y-1);
                #endif

                #ifdef _VERTICAL
                  d1 = smoothstep(0.005,0.01,uv.x);
                  d2 = smoothstep(0.01,0.005,uv.x-1);
                #endif


                float t = d1*d2;
                float4 fcol = col*t +bg*(1-t);
                
                
                return fcol;
               
            }
            ENDCG
        }
    }
}

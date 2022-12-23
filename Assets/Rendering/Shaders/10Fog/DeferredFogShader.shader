Shader "Unlit/DeferredFogShader"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off
        ZTest Always
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // make fog work
            #pragma multi_compile_fog
            #define FOG_DISTANCE
            //#define FOG_SKYBOX
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
               

                #if defined(FOG_DISTANCE)
                    float3 ray:TEXCOORD1;
                #endif
            };

            sampler2D _MainTex,_CameraDepthTexture;
            float4 _MainTex_ST;

            float3 _FrustumCorners[4];


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                

                #if defined(FOG_DISTANCE)
                    o.ray = _FrustumCorners[v.uv.x+2*v.uv.y];//The coordinates are (0, 0), (1, 0), (0, 1), and (1, 1). So the index is u + 2v.
                #endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv);
                depth = Linear01Depth(depth);

                float viewDistance = depth * _ProjectionParams.z - _ProjectionParams.y;//near distance plane

                #if defined(FOG_DISTANCE)
                    viewDistance = length(i.ray*depth);
                #endif


                UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                unityFogFactor = saturate(unityFogFactor);  
                   
                  
                float3 col = tex2D(_MainTex, i.uv).rgb;

                #if !defined(FOG_SKYBOX)
                if (depth >0.999)
                {
                    unityFogFactor =1;
                }
                #endif

                #if !defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2)
                unityFogFactor =1;
                #endif

                float3 fogcolor = lerp(unity_FogColor.rgb, col, unityFogFactor);
     
                return float4(fogcolor,1);
            }
            ENDCG
        }
    }
}

Shader "Unlit/CSTest2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // specify an additional option, indicated by adding the #pragma instancing_options directive.
            #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
            #pragma editor_sync_compilation

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 color :COLOR0;
                float psize:PSIZE0;
            };

            struct ParticleData
            {
                float3 pos;
                float4 color;
            };

            // #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            // StructuredBuffer<ParticleData> _particleDataBuffer;
            // #endif
             StructuredBuffer<ParticleData> _particleDataBuffer;

            sampler2D _MainTex;
            float4 _MainTex_ST;


            
            v2f vert (uint id :SV_VertexID)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(_particleDataBuffer[id].pos,0));
                o.color = _particleDataBuffer[id].color;
                o.psize = 10;
                return o;
            }

            // v2f vert (appdata v, uint instanceID : SV_InstanceID)
            // {
            //     v2f o = (v2f)0;
            //     float4 pos = float4(_particleDataBuffer[instanceID].pos,0);
            //     o.vertex = UnityObjectToClipPos(v.vertex +pos); 
            //     o.color = _particleDataBuffer[instanceID].color;

            //     //o.psize = 10;
            //     return o;
            // }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 col = i.color;

                return float4(col,1);
            }
            ENDCG
        }
    }
}

Shader "Custom/CSSufferce"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation

  
        #pragma target 4.5

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _time;
        float _step;
        uint _count;



        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)

         struct ParticleData
            {
                float3 pos;
                float4 color;
            };

        StructuredBuffer<ParticleData> _particleDataBuffer;
        #endif

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void ConfigureProcedural()
        {
             #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            float3 position = _particleDataBuffer[unity_InstanceID].pos;

            unity_ObjectToWorld= 0;
            unity_ObjectToWorld._m03_m13_m23_m33 =float4(position,1.0);
            unity_ObjectToWorld._m00_m11_m22 =_step;

             #endif

        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {   

            o.Albedo =IN.worldPos*0.5+0.5;

            // #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            // o.Albedo = _particleDataBuffer[unit_InstanceID].color;
            // #endif
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

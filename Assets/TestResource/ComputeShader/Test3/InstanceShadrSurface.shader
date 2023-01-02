Shader "Custom/InstanceShadrSurface"
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
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.5

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        float step;
        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        struct Particel
        {
            float3 pos;
            float4 color;
        };
		StructuredBuffer<Particel> particels;
		#endif
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void ConfigureProcedural () 
		{
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				 float3 position = particels[unity_InstanceID].pos;

				unity_ObjectToWorld =0;
				unity_ObjectToWorld._m03_m13_m23_m33 =float4(position,1.0);
				unity_ObjectToWorld._m00_m11_m22 =step;
			#endif

			
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
             float4 c =1;
          	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				 float4 color = particels[unity_InstanceID].color;
                o.Albedo = color;
				
			#else
                 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
            #endif

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

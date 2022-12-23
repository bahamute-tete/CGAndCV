Shader "Custom/HashVisualization"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        
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

       

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		float4 _Config;

		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<uint> _Hashes;
			StructuredBuffer<float3> _Positions;
			StructuredBuffer<float3> _Normals;
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
				unity_ObjectToWorld = 0.0;
				unity_ObjectToWorld._m03_m13_m23_m33 = float4(_Positions[unity_InstanceID],1.0);
				unity_ObjectToWorld._m03_m13_m23 +=(_Config.z * ((1.0 / 255.0) * (_Hashes[unity_InstanceID] >> 24) - 0.5)) *_Normals[unity_InstanceID];
				unity_ObjectToWorld._m00_m11_m22 = _Config.y;
			#endif
		}

		float3 GetHashColor ()
		{
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			
			uint hash = _Hashes[unity_InstanceID];
			return(1.0/255.0)*float3(hash & 255,(hash>>8)&255,(hash >>16)&255);
			#else
			return  1.0;
			#endif
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
           
           
            o.Albedo = GetHashColor();
          
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha =1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

Shader "RayMarch/FractalGPU"
{
    Properties
    {
        _BaseColor ("_BaseColor", Color) = (1,1,1,1)
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
			float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _BaseColor;
		float _Step;

		float4 _SequenceNumbers;
		float4 _ColorA,_ColorB;


		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		StructuredBuffer<float4x4 > _Matrices;
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
				

				unity_ObjectToWorld =_Matrices[unity_InstanceID];
				
			#endif

			
		}

		float4  GetFractalColor()
		{
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float4  resColor;
				resColor.rgb = lerp(_ColorA.rgb,_ColorB.rgb, frac(unity_InstanceID*_SequenceNumbers.x+_SequenceNumbers.y));
				resColor.a = lerp(_ColorA.a,_ColorB.a, frac(unity_InstanceID*_SequenceNumbers.z+_SequenceNumbers.w));
				return resColor;
			#else
				return _ColorA;
			 #endif
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            
            //o.Albedo =IN.worldPos*0.5+0.5;
			 o.Albedo =GetFractalColor().rgb;
            
            o.Metallic = _Metallic;
            o.Smoothness = GetFractalColor().a;
			o.Emission =0.05;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

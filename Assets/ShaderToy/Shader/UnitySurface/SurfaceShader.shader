Shader "Custom/SurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Tex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Normal ("Normal",2D)="bump"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert finalcolor:fc


        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _Tex;
		sampler2D _Normal;

        struct Input
        {
            float2 uv_Tex;
			float2 uv_Normal;
			//float4 customPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v ,out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			float3 dir =normalize(v.vertex.xyz);
			//v.vertex.z = (sin(2*dis-_Time.y)*0.5+0.5);
			//o.customPos = v.vertex+ dot(dir,v.normal)*sin(length(v.vertex))*float4(v.normal,1);
			v.vertex = v.vertex+(0.5*sin(0.3*length(v.vertex)+_Time.y*0.2)+0.5)*float4(v.normal,1);
		}
		
		void fc (Input i, SurfaceOutputStandard o, inout  float4 c)
		{
			c = sqrt(c);
		}

        void surf (Input i, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_Tex, i.uv_Tex+float2(_Time.x,_Time.y)) * _Color;
			float3 n= UnpackNormal( tex2D (_Normal,i.uv_Normal+float2(_Time.x,_Time.y)));
            o.Albedo = c.rgb;
			o.Normal = n;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

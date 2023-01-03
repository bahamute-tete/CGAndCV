Shader "Custom/InstacneIndirectShader_Test"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Intensity ("Intensity", Range(0,2)) =1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        //Cull Back
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard  vertex:vert fullforwardshadows addshadow alpha:blend
        #pragma instancing_options  procedural:setup 
        

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;


        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

            struct myBuffers
            {
                float4 position;
                float4 color;

            };
            RWStructuredBuffer<myBuffers>MyBuffers;
        #endif

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float4,_Color)//property name);
        UNITY_INSTANCING_BUFFER_END(Props)


        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(r, s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            float4 data = MyBuffers[unity_InstanceID].position;

            float rotation = data.w * _Time.y *0.5f;
            rotate2D(data.xz,rotation);

        
            unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
            #endif

        }

        float _Intensity;
        void vert(inout appdata_full v)
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

            v.vertex.xyz += v.normal * _Intensity;

            float4 data = MyBuffers[unity_InstanceID].position;
            float rotation = data.w * _Time.y *3.;
            rotate2D(v.vertex.xz,rotation);
            rotate2D(v.vertex.yz,rotation);
            rotate2D(v.vertex.xy,rotation);
            
            #endif

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            fixed4 c=tex2D (_MainTex, IN.uv_MainTex);
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                c *= MyBuffers[unity_InstanceID].color;
            #else
                c *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            #endif

            
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

Shader "Unlit/CustomDeferredLight"
{
    Properties
    {
        
    }
    SubShader
    {
        

        Pass
        {
            Blend [_SrcBlend][_DstBlend]
            // Cull Off
            // ZTest Always
            ZWrite Off

            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma exclude_renderers nomrt

            #pragma multi_compile_lightpass
            #pragma multi_compile _ UNITY_HDR_ON

            #include "MyDeferredShading.cginc"

           
            ENDCG
        }

        pass
        {
            Cull Off
            ZTest Always
            ZWrite Off

            Stencil
            {
                Ref [_StencilNonBackground]
                ReadMask[_StencilNonBackground]
                CompBack Equal
                CompFront Equal
            }

            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma exclude_renderers nomrt

            #include "UnityCG.cginc"


            sampler2D _LightBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv:TEXCOORD0;
                
            };

            struct v2f
            {
             
                float4 pos : SV_POSITION;
                float uv : TEXCOORD0;
            };

           

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return -log2(tex2D(_LightBuffer, i.uv));
            }
            ENDCG
        }
    }
}

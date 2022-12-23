Shader "RayMarch/Stencil_Mask"
{
    Properties
    {
        _MainLayer0 ("Layer0", 2D) = "white" {}


    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		ColorMask 0

        Pass
        {
			ZWrite off
			Stencil
			{
				Ref 3
				Comp always
				Pass replace
			}



            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            };

            sampler2D _MainLayer0;
            float4 _MainLayer0_ST;
			float _scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = 0;
               fixed4 col = tex2D(_MainLayer0, i.uv);
			   //fixed4 col = tex2D(_MainLayer0, 2*i.uv);
              
                return col;
            }
            ENDCG
        }


		


    }
}

Shader "RayMarch/Stencil_Front"
{
    Properties
    {
        _MainLayer0 ("Layer0", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		
		Pass
        {
		
			Tags { "RenderType"="Opaque" }
			
			//Blend SrcAlpha OneMinusSrcAlpha
						Cull Front 
						//Lighting Off
						ZWrite on

			Stencil {
				Ref 1
				Comp Less
				Pass Keep
				Fail Keep
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


			float2x2 RotM (float a)
			{
				float s= sin(a);
				float c = cos(a);
				return float2x2 (c,-s,s,c);
			}

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
               fixed4 col = 1;
			    col = tex2D(_MainLayer0, i.uv*_MainLayer0_ST.xy+_MainLayer0_ST.zw);
              
                return col;
            }
            ENDCG
        }

	


    }
}

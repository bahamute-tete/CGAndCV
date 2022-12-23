Shader "RayMarch/Stencil_UI"
{
    Properties
    {
        _MainLayer0 ("Layer0", 2D) = "white" {}
		_MainLayer1 ("Layer1", 2D) = "white" {}
		_MainLayer2 ("Layer2", 2D) = "white" {}
		_t ("Layer1_Bend",Range(0,1))= 0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		

        Pass
        {
		
			Name "LAYER0"

			Stencil
			{
				Ref 1
				Comp Always
				Pass  Replace
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              
                fixed4 col = tex2D(_MainLayer0, i.uv);
              
                return col;
            }
            ENDCG
        }


		Pass
        {
		
			Name "LAYER1"

			Tags { "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil
			{
				Ref 1
				Comp Equal
				Pass  IncrSat
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

            sampler2D _MainLayer1;
            float4 _MainLayer1_ST;
			float _t;

			float2x2 RotM (float a)
			{
				float s= sin(a);
				float c = cos(a);
				return float2x2 (c,-s,s,c);
			}

			



            v2f vert (appdata v)
            {
                v2f o;
				
				v.vertex *=0.85;

				float s= sin(v.vertex.x*_t);
				float c = cos(v.vertex.x*_t);
				float2x2 m = float2x2 (c,-s,s,c);
				v.vertex.xy = mul (m,v.vertex.xy);

				
				v.vertex +=float4(0,1,0,1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              
                fixed4 col = tex2D(_MainLayer1, i.uv)*1;
                float  mask = tex2D(_MainLayer1, i.uv).r;
                return float4(col.rgb,mask);
            }
            ENDCG
        }

		Pass
        {
		
			Name "LAYER2"

			Stencil
			{
				Ref 2
				Comp Equal
				Pass  IncrSat
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

            sampler2D _MainLayer2;
            float4 _MainLayer2_ST;
		

			float2x2 RotM (float a)
			{
				float s= sin(a);
				float c = cos(a);
				return float2x2 (c,-s,s,c);
			}

			



            v2f vert (appdata v)
            {
                v2f o;

				v.vertex *=0.5;
				v.vertex +=float4(0,-2.5,0,1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              
                fixed4 col = tex2D(_MainLayer2, i.uv)*1;
              
                return col;
            }
            ENDCG
        }


    }
}

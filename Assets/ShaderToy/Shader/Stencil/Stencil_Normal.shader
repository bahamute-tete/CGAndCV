Shader "RayMarch/Stencil_Normal"
{
    Properties
    {
        _MainLayer0 ("Layer0", 2D) = "white" {}
		_MainLayer1 ("Layer1", 2D) = "white" {}
		_MainLayer2 ("Layer2", 2D) = "white" {}
		_t ("Layer1_Bend",Range(0,1))= 0
		_scale("_distance",Range(0,1)) =1

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
				float2 uv = 2*i.uv -1;
				float d= length(uv);
               fixed4 col = tex2D(_MainLayer0, uv*(sin(20*(d-_Time.x*1))));
			   //fixed4 col = tex2D(_MainLayer0, 2*i.uv);
              
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
			float _scale;

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

				float s= sin(v.vertex.x*_scale*0.1);
				float c = cos(v.vertex.x*_scale*0.1);
				float2x2 m = float2x2 (c,-s,s,c);
				v.vertex.xy = mul (m,v.vertex.xy);

				
				v.vertex +=float4(0,1*_scale+0.1,0,1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              
                fixed4 col = tex2D(_MainLayer1, i.uv);
				float mask;

				if (fwidth(col.a)>0.05)
				mask= 1;
				else
				mask=0;
                //float  mask = tex2D(_MainLayer1, i.uv).r;
                //return float4(col.rgb,mask);
				return float4(col.rbg+mask*float3(0.4,1,0.55)*(0.5*sin(_Time.y*4)+0.5),col.a);
            }
            ENDCG
        }

		Pass
        {
		
			Name "LAYER2"
			Tags { "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			Stencil
			{
				Ref 2
				Comp Equal
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

            sampler2D _MainLayer2;
            float4 _MainLayer2_ST;
			float _scale;
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

				v.vertex *=0.65;

				float s= sin(v.vertex.x*_scale*0.1);
				float c = cos(v.vertex.x*_scale*0.1);
				float2x2 m = float2x2 (c,-s,s,c);
				v.vertex.xy = mul (m,v.vertex.xy);

				v.vertex +=float4(0,2.5*_scale+0.15,0.4,1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              
                fixed4 col = tex2D(_MainLayer2, i.uv);
				//float L = dot(col,float3(0.2125,0.7154,0.0721));
				//float mask = 0;
				//if (fwidth(L)>0.01)
				//mask= 1;
				//else
				//mask=0;
				
                return col;
            }
            ENDCG
        }


    }
}

Shader "RayMarch/FadePlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        ZWrite On
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {

		

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;

                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 srcPos : TEXCOORD1;
				float3 vp : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


			
			float remap01(float a,float b,float t)//normalize t
			{
				return (t-a)/(b-a);
			}

            v2f vert (appdata v)
            {
                v2f o;
                
				o.vp = UnityObjectToViewPos(v.vertex);
			   
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.srcPos= ComputeScreenPos(o.vertex);
				o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
			    float2 maskuv = i.uv-0.5;
                fixed4 col = tex2D(_MainTex,i.uv);
			    float3 sp =i.srcPos.x/i.srcPos.w;
				 float d=length(sp.x-0.5);
				if (d>0.05)
				{
					float t=1-smoothstep(0,1,(d-0.05)/(0.5-0.05));
					col.a =t;	
				}

                return col;
            }
            ENDCG
        }
    }
}

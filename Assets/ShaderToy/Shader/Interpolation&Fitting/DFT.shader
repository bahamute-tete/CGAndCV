Shader "Unlit/DFT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
               
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			#define DFT_SIZE 32
			#define PI 3.14159265359
			#define avg(v) ((v.x+v.y+v.z)/3.0)

			float2 dft(float2 uv)
			{
				float2 complex = float2(0,0);
    
				uv *= float(DFT_SIZE);
    
				float size = float(DFT_SIZE);
    
				for(int x = 0;x < DFT_SIZE;x++)
				{
					for(int y = 0;y < DFT_SIZE;y++)
					{
						float a = 2.0 * PI * (uv.x * (float(x)/size) + uv.y * (float(y)/size));
						float3 samplev;
						//float3 samplev = tex2D(_MainTex,frac(float2(x,y)/size)).rgb;

						if(int (x+y) & 1 ==1 )
						{
							samplev = tex2D(_MainTex,frac(float2(x,y)/size)).rgb;
						}else
						{
							samplev =-tex2D(_MainTex,frac(float2(x,y)/size)).rgb;
						}

						complex += avg(samplev)*float2(cos(a),sin(a));
					}
				}
    
			return complex;
			}



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              
				
				float4 col =0;
				float2 uv = (i.uv);
				
    
				float3 dftcolor = 0;
    
    
				if(uv.x < 1.0 && uv.x > 0.0)
				{
					dftcolor = length(dft(uv))/float(DFT_SIZE);
				}
    
				col = float4(dftcolor,1.0);


                return col;
            }
            ENDCG
        }
    }
}

Shader "RayMarch/EasingTypeDisplay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Mix("_Mix",Range(0,1))=0
		_LineColor("_LineColor",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
		ZWrite off
		Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float B;
			float C;
			int easingType1;
			int easingType2;
			float _Mix;
			float4 _LineColor;
			int _ShaderPlay;
			float _T;

			float sdPoint (float2 p,float2 pc)
			{
				float2 d = p -pc;
				return length(d);
			}

			float sdLine (float2 p ,float2 a ,float2 b)
			{
				float2 pa = p-a;
				float2 ba = b-a;
				float h = clamp(dot(pa,ba)/dot(ba,ba),0,1);
				float2 d = pa -ba*h;
				return length(d);
			}

			float BezierCubic( float B, float C,float t)
			{
				float s1 = (1 - t) * (1 - t) * (1 - t);
				float s2 = (1 - t) * (1 - t) * t;
				float s3 = t * t * (1 - t);
				float s4 = t * t * t;
				return 3.0f* B * s2 +3.0f* C * s3 + s4;
			}

			float EaseOutBounce(float t)
			{
				float  n1 = 7.5625f;
				float d1 = 2.75f;

				if (t< 1 / d1) {
					return n1* t * t;
				} else if (t< 2 / d1) {
					return n1* (t -= 1.5f / d1) * t + 0.75f;
				} else if (t< 2.5f / d1) {
					return n1* (t -= 2.25f / d1) * t + 0.9375f;
				} else {
					return n1* (t -= 2.625f / d1) * t + 0.984375f;
				}
			}

			float EaseOutElastic(float t) 
			{
				float  a = (2.0f * 3.141592) / 3;

				return (t == 0)? 0: (t == 1) ? 1: pow(2, -10 * t) *sin((t* 10.0f - 0.75f) * a) + 1;
			}

			float2 PointPos (float t ,int type1,int type2,float mix)
			{
				float y1 =0;float y2 =0;
				switch (type1)
				{
					case 0:
					y1 = t;
					break;

					case 1:
					y1 = t*t;
					break;

					case 2:
					y1 = t*t*t;
					break;

					case 3:
					y1 = 1-(1-t)*(1-t);
					break;

					case 4:
					y1 = 1 - (1 - t) * (1 - t)*(1-t);
					break;

					case 5:
					y1 = 16.0 * (1.0 - t) * (1.0 - t) * t * t;
					break;

					case 6:
					y1= BezierCubic(B,C,t);
					
					break;

					case 7:
					y1 =(t*t*(1-t))/0.1481;
					break;

					case 8:
					y1 = (t*(1-t)*(1-t))/0.1481;
					break;

					case 9:
					y1= EaseOutBounce(t);
					break;

					case 10:
					y1= EaseOutElastic(t);
					break;


				}
				switch (type2)
				{
					case 0:
					y2 = t;
					break;

					case 1:
					y2 = t*t;

					break;

					case 2:
					y2 = t*t*t;
					break;

					case 3:
					y2 = 1-(1-t)*(1-t);
					break;

					case 4:
					y2 = 1 - (1 - t) * (1 - t)*(1-t);
					break;

					case 5:
					y2 = 16.0 * (1.0 - t) * (1.0 - t) * t * t;
					break;

					case 6:
					y2 = BezierCubic(B,C,t);
					break;

					case 7:
					y2 =(t*t*(1-t))/0.1481;
					break;

					case 8:
					y2 = (t*(1-t)*(1-t))/0.1481;
					break;

					case 9:
					y2= EaseOutBounce(t);
					break;

					case 10:
					y2= EaseOutElastic(t);
					break;
				}

				if (type1==type2 || mix ==0)
				{
					return float2 (t,y1);
				}
				else
				{
					float by = y1 * (1 - mix) + y2 * mix;
					return float2 (t,by);
				}
			}

			float sdBox( float2  p,  float2 b )
			{
				float2 d = abs(p)-b+0.2;
				return length(max(d,0.0)) + min(max(d.x,d.y),0.0)-0.2;
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
                // sample the texture
                fixed4 col = 0;
				float2 uv = i.uv*1.1-0.05;

				float2 maskuv = 2*i.uv -1;
				float db = sdBox(maskuv,float2(1,1));
				float mask = smoothstep(0.005,0.003,db);

				float2 po = 0 ;
				float2 fp = 0;
				float t =0;

	
				float d = 1000.0;
				float2 pc = 0 ;
				float dp =0;
				#define SAMPLENUM 64
				for(int  j = 0  ; j <=SAMPLENUM ; j ++)
				{
					float  h  = (float) j/SAMPLENUM;
				    pc = 0;
					pc +=PointPos(h,easingType1,easingType2,_Mix);
					
					d = sdLine(uv,po,pc);
					col += smoothstep(0.005,0.003,d)*_LineColor;
					po =pc;
				}

				dp = sdPoint(uv,fp+PointPos(_T,easingType1,easingType2,_Mix));
				col += smoothstep(0.05,0.04,dp)*_LineColor+float4(0.06,0.06,0.06,mask);

                return col;
            }
            ENDCG
        }
    }
}

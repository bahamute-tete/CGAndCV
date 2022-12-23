Shader "Custom/SmoothStep"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _radus("Radius",Range(0,10) )=8
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

            #define pi 3.1415926

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _radus;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


           


            float almostIdentity( float x, float m, float n )
            {
                //m be the threshold that anything above m stays unchanged
                // n the value things will take when the signal is zero.
                if( x>m ) return x;
                const float a = 2.0*n - m;
                const float b = 2.0*m - 3.0*n;
                const float t = x/m;
                return (a*t + b)*t*t + n;
            }

            float almostIdentity( float x, float n )
            {
                return sqrt(x*x+n);
            }

            float expImpulse( float x, float k )
            {
                const float h = k*x;
                return h*exp(1.0-h);
            }

            float cubicPulse( float c, float w, float x )
            {
                //like smoothstep(c-w,c,x)-smoothstep(c,c+w,x)
                x = abs(x - c);
                if( x>w ) return 0.0;
                x /= w;
                return 1.0 - x*x*(3.0-2.0*x);
            }

            float funCurve(float x)
            {
                float res = pow(x,5);
                res = smoothstep(-1,1,x);
                res = ceil(sin(pi*x+_Time.y));
                res = fmod(x,0.5);
                res = frac(x);
                res = ceil(x);
                res= sign(x);
                res = min(0.0,x);
                res = max(0.0,x);
                res = 1.0-pow(abs(x),3.5);
                res =clamp( almostIdentity(x,0.12,0.1),0,1.0);
                res = expImpulse(x,3);
                return  res;
            }

            // float CoordinateXY(float2 uv,float width)
            // {
            //     float d1 = cubicPulse(0,0.02,uv.y);
            //     float d2 = cubicPulse(0.0.02,uv.x);
            //     return d1+d2;
            // }

             float DrawCurve(float2 uv,float y)
            {
                //return smoothstep(x-0.02,x,uv.y)-smoothstep(x,x+0.02,uv.y);
                return cubicPulse(y,0.02,uv.y);
            }

            float sdBox(float2 uv, float2 size,float r)
            {
                float2 p = abs(uv)-size;
                float2 part1 = max(p,0.0);//3areas,max make one axies value =0,only effective for outter point
                float2 part2= min(max(p.x,p.y),0.0);//make inner Point to d =0
                return abs(length(part1)+part2-r);
            }

            float sdPie( float2 p, float2 c,  float r )
            {
                p.x = abs(p.x);
                float l = length(p) - r;
                float m = length(p-c*clamp(dot(p,c),0.0,r)); //c = sin/cos
                return max(l,m*sign(c.y*p.x-c.x*p.y));
            }



            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
                uv*=1;
                float3 col = 0;

                

               
                float y  = funCurve(uv.x);
                col =y;//background

                float xa = smoothstep(-0.02,0,uv.y);
                float xb = smoothstep(0,0.02,uv.y);
                
                // float d1 = cubicPulse(0,0.02,uv.y);
                // float d2 = cubicPulse(0,0.02,uv.x);
                // //float d = d1+d2;
                // //float d = DrawCurve(uv,y);

                // col =d2*float3(0,1,0)+(1-d2)*col;
                // float circle1 = smoothstep(0.3,0.28,length(uv));
                // float circle2 = smoothstep(0.17,0.16,length(uv));
                // col = circle1-circle2;

                float box = sdBox(uv,float2(0.5,0.5),0.2);

                float  angel = 60*pi/180;
                float pie =abs(sdPie(uv,float2(sin(angel),cos(angel)),0.2)-0.01);

                col = smoothstep(0.01,0.009,pie);

                return float4(col,1);
            }
            ENDCG
        }
    }
}

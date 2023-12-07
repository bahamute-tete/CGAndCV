Shader "RayMarching/AlienOrb"
{
    Properties
    {
        //_Background ("CubeMap", Cube) = "white" {}
       // _Texture("MainTex",2D)="white"{}
        //_Texture2("SubTex",2D)="white"{}
       // _DisTexture("DisTex",2D)="white"{}
        _CameraPos("CameraPosition", Vector) = (4.5,4.5,-7,1)
        _Target("CameraTarget",Vector) =(0,0,0)
        //_ColorShift("ColorShift",Range(0,0.5))=0
        //_Density("Density",Range(0,1))=0
        //_Color("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            samplerCUBE _Background;
            float4 _CameraPos;
            float3 _Target;
            float _ColorShift;
            float _Density;
            float3 _Color;
            sampler2D _Texture;
            sampler2D _Texture2;
            sampler2D _DisTexture;
            #define SURFANCE_DIS 1e-3
            #define MAX_STEPS 128
            #define MAX_DISTACNE 50

            float2 FlipUVWithDirection(float2 uv,float2 n)
            {
                n = normalize(n);
                //uv.x*n.x+uv.y*n.y
                // if n =(1,0) dot(uv,n)==>uv.x
                // if n =(0,1) dot(uv,n)==>uv.y
                //也就是说 得到的是一个距离，点P到（0,0）的距离
                float d = dot(uv,n);
                //2倍的距离 想当于轴对称
                uv -= 2.*n* min(0,d);
                return uv;
            }

            float2x2 Rot(float angel)
            {
                float s = sin(angel);
                float c = cos(angel);
                return float2x2(c, -s, s, c);
            }

            float2 Min2(float2 a,float2 b)
            {
                return a.x<b.x?a:b;
            }

            float3 Camera_float(float2 uv,float3 origin, float3 look_at,float zoom)
            {
                float3 forward = normalize(look_at -origin);
                float3 right = cross(forward,float3(0,1,0));
                float3 up = cross(right,forward);

                float3 center = origin + forward * zoom;
                float3 intersection = center + right * uv.x+ up * uv.y;
                return  intersection-origin;
            }

            float SDFSphere_float(float3 p, float3 center, float radius)
            {
                return length(p - center) - radius;
            }

            float SDFPlane_float(float3 p, float3 n, float height)
            {
                return dot(p,normalize(n))+height;
            }

            float SDFBox_float(float3 p, float3 boxSize, float radius)
            {
                p =abs(p)-boxSize;
                return length(max(p,0.))+ min(max(p.x,max(p.y,p.z)),0) - radius;
            }

            float SDFBox2D_float(float2 p, float2 boxSize, float radius)
            {
                return length(max(abs(p)-boxSize,0))+min(max(p.x,p.y),0) - radius;
            }

            float SDFRing_float(float3 p, float radius1, float radius2)
            {
                float x = length(p.xy-float2(0,0.15))-radius1;
                float y = p.z;
                float dring = length(float2(x, y)) - radius2;
                return dring;
            }   

            float SDLineSeg(float3 p ,float3 a ,float3 b)
            {
                float3 pa =p-a;
                float3 ab= b-a;
                float t = dot(pa,ab)/dot(ab,ab);
                t = clamp(t,0,1);
                return length(pa - ab*t);
            }

            float smin(float a, float b, float k)
            {
                float h = clamp(0.5+0.5*(b-a)/k, 0, 1);
                return lerp(b, a, h) - k*h*(1-h);
            }

            float BallGyroid(float3 p)
            {
                p.yz = mul(Rot(_Time.y),p.yz);
                //空间缩小
                p*=10.;
                float g =0.7* dot(sin(p),cos(p.yzx));
                //距离也要跟着缩小
                //abs反转内部的面，添加一个厚度
                return  abs(g/10.)-0.03;
            }

            float3 RayPlane(float3 ro,float3 rd,float3 p ,float3 n)
            {
                // make a plane cross the sphere
                float t =max(0,dot(p-ro,n)/dot(rd,n));
                return ro+rd*t;
            }

            float GetDistance_float(float3  p)
            {
                
                float d =0;
                float sphere =  SDFSphere_float(p,float3(0,0.,0),1.);
                sphere = abs(sphere)-0.03;//反转内部 并添加一个厚度，球壳
                float gyroid = BallGyroid(p);
                float ball= smin(sphere,gyroid,-0.03);//boolean intersection
                float ground = p.y+1.;
                p.z-=_Time.y;
                p*=4.;
                p.y+=sin(p.z)*.5;
                float y =abs( dot(sin(p),cos(p.yzx))*0.1);
                ground+=y;

                d=min(ball,ground*0.9);  
                return d;
            }



            float RayMarching_float(float3 ray_origin, float3 ray_direction,float side )
            {
                float  d=0.0;
                float index =0;

                for (int i = 0; i < MAX_STEPS && d < MAX_DISTACNE; i++)
                {
                    float3 p = ray_origin + ray_direction * d;
                    //side:inside =-1;outside=1;
                    float currentDistance =GetDistance_float(p)*side ;
                    
                    if (currentDistance< SURFANCE_DIS) break;
                    d += currentDistance;
                }

                return d;
            }

            float3 GetNormal(float3 p) // for function f(p)
            {
                float h = 0.001; // replace by an appropriate value
                float2 k = float2(1,-1);
                return normalize( k.xyy*GetDistance_float( p + k.xyy*h)+ 
                                  k.yyx*GetDistance_float( p + k.yyx*h)+ 
                                  k.yxy*GetDistance_float( p + k.yxy*h)+ 
                                  k.xxx*GetDistance_float( p + k.xxx*h) );
            }

            float3 GetNormal2(float3 p) 
            {
                float d = GetDistance_float(p);
                float2 e=float2(0.01,0);

                float3 n= d-float3(
                GetDistance_float(p-e.xyy),
                GetDistance_float(p-e.yxy),
                GetDistance_float(p-e.yyx));

                return normalize(n);
            }


            float Shadow (float3 ro,float3 rd,float mint,float maxt,float k)
            {
                float d = mint;
                float res = 1.0;
                for (int i = 0; i < MAX_STEPS && d <maxt; i++)
                {
                    float current_d = GetDistance_float(ro + rd * d);
                    if (current_d < 0.001) return 0.0;
                    d += current_d;
                    //距离物体越近 raymarching的圈的半径就越小，也就是 currentD越小
                    res =min(res,k*current_d/d);
                }
                return res;
            }

            float calcAO( float3 pos, float3 nor)
            {
                float occ = 0.0;
                float sca = 1.0;
                for( int i=0; i<5; i++ )
                {
                    //沿着法线方向给定5个步长
                    float h = 0.01 + 0.12*float(i)/4.0;
                    //计算 这5个采样点的距离
                    float d = GetDistance_float( pos + h*nor);
                    occ += (h-d)*sca;
                    sca *= 0.95;
                    if( occ>0.35 ) break;
                }
                return clamp( 1.0 - 3.0*occ, 0.0, 1.0 ) * (0.5+0.5*nor.y);
            }

            float Hash21(float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453);
            }
            float Glitter(float2 uv,float a )
            {
            // a = phase of sparkles;
                uv*= 10.0;
                float2 id = floor(uv);
                float  n = Hash21(id);//random Value[0,1]
                uv = frac(uv)-.5;

                float d  = length(uv);
                float m = smoothstep(0.5*n,0,d);

                m*= pow(sin(frac(n*10.)*2*UNITY_PI+a)*.5+.5,100.);//make like a pulse
                return m;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 aspect = _ScreenParams/_ScreenParams.y;
                float2 uv = (i.uv-0.5) *aspect;
                float squareCenterUVDistance =dot(uv,uv);
                float4 col=0;
                float3 target =_Target;
                float3 ro = _CameraPos;    
				float3 rd = Camera_float(uv,ro,target,_CameraPos.w);
                
                //col.rgb =texCUBE(_Background,rd);
                float d = RayMarching_float(ro,rd,1.);

                if (d<MAX_DISTACNE)
                {
                    float3 p = ro+rd*d;
                    float3 n = GetNormal(p);
                    float3 l = 0-p;
                    float3 r = reflect(rd,n);
                    float3 h = normalize(l-rd);


                    float spec =0.1+ pow(saturate(dot(n,h)),8.);
                    float fresnel =pow(1+dot(n,rd),2.);
                    float diffusion = dot(n,normalize(l))*0.5+0.5;

                    float centerDistance = length(p);
                    //col=lerp(0.0, diffusion*spec,fresnel);
                    col =diffusion*spec; 

                    //hit the floor
                    if (centerDistance>1.04)
                    {
                        //col *=float4(1,0,0,1);
                        //检查从光源出来的到物体的点的距离，如果是在距离函数里是正的，在距离还是外输出的是负
                        //normalize(l) 因为半径为1 的球，单位化后正好长度为1；
                        float s= BallGyroid(normalize(l));
                        float w = centerDistance*0.01;
                        float shadow =smoothstep(-w,w,s);
                        col *=shadow*0.9+.1;
                        //sparles
                        p.z -=_Time.y;
                        col += Glitter(p.xz*5.,dot(ro,float3(1,1,1)))*3.;
                        col /=centerDistance*centerDistance;
                    }
                    else
                    {
                        float sss = smoothstep(.01,0 ,squareCenterUVDistance);
                        sss*=sss;
                        float s = BallGyroid(p+sin(p*10.)*0.02);
                        sss*=smoothstep(-0.03,0,s);
                        col.rgb += sss*float3(1.,.2,.3);
                    }
                    
                }
                //center light 2Dlight
                float3 lightColor = float3(1,.8,.7);
                float light =1e-3/squareCenterUVDistance;
                col.rgb += light*smoothstep(0,0.5,(d-4))*lightColor;
                //glare
                float s = BallGyroid(normalize(ro));
                col.rgb+= light*smoothstep(0.,.03,s)*lightColor;

                //volumetrics
                uv = mul(Rot(_Time.y),uv);
                //2D methord
               // float a = atan2(uv.y,uv.x);//(-pi,pi]
               // float starburst = sin(a*11.-_Time.y)*sin(a*7.+_Time.y)*sin(a*5.+_Time.y*2.);

               //3D methord
               //Face to ro
               float3 intersectionPlanePoint = RayPlane(ro,rd,float3(0,0,0),normalize(ro));
               //because the Ball radius = 1，so normalize make mask sapce juse mask ball
               //otherwise must multiply by radius
                float starburst = BallGyroid(normalize(intersectionPlanePoint));
                starburst *= smoothstep(.1,1,length(uv));
                col.rgb +=max(2*starburst,0);
                //col.rgb = pow(col,0.4545);
                col*=1- squareCenterUVDistance;

                //col.rgb = Glitter(uv);
                return col;
            }
            ENDCG
        }
    }
}

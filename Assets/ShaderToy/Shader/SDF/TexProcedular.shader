Shader "RayMarching/TexProcedular"
{
    Properties
    {
        _Background ("CubeMap", Cube) = "white" {}
        _Texture("MainTex",2D)="white"{}
        _Texture2("SubTex",2D)="white"{}
        _DisTexture("DisTex",2D)="white"{}
        _CameraPos("CameraPosition", Vector) = (4.5,4.5,-7,1)
        _Target("CameraTarget",Vector) =(0,0,0)
        _ColorShift("ColorShift",Range(0,0.5))=0
        _Density("Density",Range(0,1))=0
        _Color("Color",Color) = (1,1,1,1)
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
            #define SURFANCE_DIS 1e-2
            #define MAX_STEPS 128
            #define MAX_DISTACNE 50

            float2 FlipUVWithDirection(float2 uv,float2 n)
            {
                n = normalize(n);
                //uv.x*n.x+uv.y*n.y
                // if n =(1,0) dot(uv,n)==>uv.x
                // if n =(0,1) dot(uv,n)==>uv.y
                //也就是说 得到的是一个距离
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



            float2 GetDistance_float(float3  p)
            {
                float2 aspect = _ScreenParams/_ScreenParams.y;
                float mat =0;
                float d =0;
                float box = SDFBox_float(p-float3(0,2,0),float3(1,1,1),0);
                float sphere1 =  SDFSphere_float(p,float3(0,0,0),1.);
                float sphere2 =SDFSphere_float(p-float3(0,-2,0),float3(0,0,0),1.);

                float pane =SDFPlane_float(p,float3(0,0,-1),0.);
                d=min(min(box,sphere1),sphere2);

                if (d==box)
                    mat =0;
                else if (d==sphere1)
                    mat =1;
                else if (d==sphere2)
                {
                    p =p-float3(0,-2,0);
                    float2 polarUV = float2(4*atan2(p.z,p.x)/2./UNITY_PI,2.*p.y)*aspect+0.5;
                    float dis=tex2D(_DisTexture,polarUV).r;
                    //dis*= smoothstep(1.1,0.9,abs(p.y));
                    d+=dis*0.2;
                    
                    //d += sin(5.*p.y+2.*_Time.y)*.1 ;
                    mat= 2;
                }
                    
                   
                return float2(d*0.7,mat);
            }

            int GetMat(float3  p)
            {
                
            }

            float2 RayMarching_float(float3 ray_origin, float3 ray_direction,float side )
            {
                float  d=0.0;
                float index =0;

                for (int i = 0; i < MAX_STEPS && d < MAX_DISTACNE; i++)
                {
                    float3 p = ray_origin + ray_direction * d;
                    //side:inside =-1;outside=1;
                    float currentDistance =GetDistance_float(p).x*side ;
                    
                    if (currentDistance< SURFANCE_DIS) break;
                    d += currentDistance;
                    index= GetDistance_float(p).y;
                }

                return float2(d,index);
            }

            float3 GetNormal(float3 p) // for function f(p)
            {
                float h = 0.001; // replace by an appropriate value
                float2 k = float2(1,-1);
                return normalize( k.xyy*GetDistance_float( p + k.xyy*h).x+ 
                                  k.yyx*GetDistance_float( p + k.yyx*h).x+ 
                                  k.yxy*GetDistance_float( p + k.yxy*h).x+ 
                                  k.xxx*GetDistance_float( p + k.xxx*h).x );
            }

            float3 GetNormal2(float3 p) 
            {
                float d = GetDistance_float(p);
                float2 e=float2(0.01,0);

                float3 n= d-float3(
                GetDistance_float(p-e.xyy).x,
                GetDistance_float(p-e.yxy).x,
                GetDistance_float(p-e.yyx).x);

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

            float4 frag (v2f i) : SV_Target
            {
                float2 aspect = _ScreenParams/_ScreenParams.y;
                float2 uv = (i.uv-0.5) *aspect;
                float4 col=0;
                float3 target =_Target;
                float3 ro = _CameraPos;
				float3 rd = Camera_float(uv,ro,target,_CameraPos.w);
                
                //col.rgb =texCUBE(_Background,rd);
                float2 res = RayMarching_float(ro,rd,1.);

                if (res.x<MAX_DISTACNE)
                {
                    float3 p = ro+rd*res.x;
                    float3 n = GetNormal(p);
                    float3 l = float3(1,2,4);
                    float3 r = reflect(rd,n);
                    float3 h = normalize(l-rd);

                    float spec = pow(saturate(dot(n,h)),8.);
                    float fresnel =pow(1+dot(n,rd),5.);
                    float diffusion = dot(n,normalize(l))*0.5+0.5;

                    if(res.y ==0)
                    {
                        n = abs(n);
                          
                        float3 t1 = tex2D(_Texture,p.xz*0.5+0.5).rgb;
                        float3 t2 = tex2D(_Texture,p.yz*0.5+0.5).rgb;
                        float3 t3 = tex2D(_Texture,p.xy*0.5+0.5).rgb;
                        col.rgb = t1*n.y+t2*n.x+t3*n.z;
                    
                    }else if (res.y==1)
                    {
                        n = abs(n);
                        //n=n*n*n*n*n*n;
                        n*=pow(n,20);
                        n/=n.x+n.y+n.z;
                           
                        float3 t1 = tex2D(_Texture,p.xz*0.5+0.5).rgb;
                        float3 t2 = tex2D(_Texture,p.yz*0.5+0.5).rgb;
                        float3 t3 = tex2D(_Texture,p.xy*0.5+0.5).rgb;
                        col.rgb = t1*n.y+t2*n.x+t3*n.z;
                        //col.rgb = n;
                    }else if (res.y==2)
                    {
                        p =p-float3(0,-2,0);
                        float2 polarUV = float2(4*atan2(p.z,p.x)/2./UNITY_PI,2.*p.y)*aspect+0.5;
                        polarUV.x = frac(polarUV.x-_Time.y);
                        float3 t1 =  tex2D(_Texture2,polarUV).rgb;
                        col.rgb = t1;
                    }
                     col*= diffusion;
                }
               
                return col;
            }
            ENDCG
        }
    }
}

Shader "RayMarching/Terrain"
{

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



            #define SURFANCE_DIS 1e-2
            #define MAX_STEPS 256
            #define MAX_DISTACNE 500
            #define SC (250.0)

            float hash(float2 p)
            {
                return  abs(frac(sin(dot(p, float2(27.1, 511.7)))*78523.45));
            }

            float hash21(float2 p)
            {
                float3 a= frac(p.xyx*.1021);
                a+= dot(a, a.yzx+33.33);
                return frac(a.x*a.z+a.y*a.z);
               
            }

            float3 noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float2 t = f*f*(3.0-2.0*f);
                float2 dt = 6.0*f*(1.0-f);

                float2 t2= f*f*f*(f*(f*6.0-15.0)+10.0);
                float2 dt2 = 30.0*f*f*(f*(f-2.0)+1.0);


                float a = hash21(i);
                float b = hash21(i+float2(1.0, 0.0));
                float c = hash21(i+float2(0.0, 1.0));
                float d = hash21(i+float2(1.0, 1.0));

                float part1= lerp(a,b,t.x);
                float part2= (c-a)*t.y*(1.0-t.x) + (d-b)*t.x*t.y;
                float version1 = part1+part2;

                float part3= lerp(c,d,t.x);
                float version2= lerp(part1,part3,t.y);

                float2 grad= dt*(float2(b-a,c-a)+(a-b-c+d)*t.yx);
                return float3(version1,grad);


            }

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

            float3x3 SetCamera(float3 ro,float3 target ,float angel)
            {
                float3 f = normalize(target-ro);
                float3 up =float3(sin(angel),cos(angel),0.0);
                float3 r = cross(f,up);
                float3 u = cross(r,f);

                float3 r1 =float3(r.x,u.x,f.x);
                float3 r2 =float3(r.y,u.y,f.y);
                float3 r3 =float3(r.z,u.z,f.z);


                return float3x3(r1,r2,r3);
            }


            float3x3 setCamera( float3 ro, float3 ta, float cr )
            {
                float3 cw = normalize(ta-ro);
                float3 cp = float3(sin(cr), cos(cr),0.0);
                float3 cu = normalize( cross(cw,cp) );
                float3 cv =          ( cross(cu,cw) );
                return float3x3( cu, cv, cw );
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

           
            float sdGroundH(float2 x)
            {
                //return p.y - 2*sin(p.x)*sin(p.z);
                float2 p =0.003*x;
                float a=0;
                float b=1.;
                float2 grad =0;

                const float2x2 m2 = float2x2(0.8,-0.6,0.6,0.8); 
               
                for(int i=0;i<16;i++)
                {
                    float3 n= noise(p);
                    grad +=n.yz;
                   
                    a+= b*(n.x)/(1.+dot(grad,grad));
                    b*=0.5;
                    p =mul(m2,p*2.0);
                }

                return 120*a;
            }

            float sdGroundM(float2 x)
            {
                //return p.y - 2*sin(p.x)*sin(p.z);
                float2 p =0.003*x;
                float a=0;
                float b=1.;
                float2 grad =0;

                const float2x2 m2 = float2x2(0.8,-0.6,0.6,0.8); 
               
                for(int i=0;i<8;i++)
                {
                    float3 n= noise(p);
                    grad +=n.yz;
                   
                    a+= b*(n.x)/(1.+dot(grad,grad));
                    b*=0.5;
                    p =mul(m2,p*2.0);
                }

                return 120*a;
            }

            float sdGroundL(float2 x)
            {
                //return p.y - 2*sin(p.x)*sin(p.z);
                float2 p =0.003*x;
                float a=0;
                float b=1.;
                float2 grad =0;

                const float2x2 m2 = float2x2(0.8,-0.6,0.6,0.8); 
               
                for(int i=0;i<3;i++)
                {
                    float3 n= noise(p);
                    grad +=n.yz;
                   
                    a+= b*(n.x)/(1.+dot(grad,grad));
                    b*=0.5;
                    p =mul(m2,p*2.0);
                }

                return 120*a;
            }

            
            float fbm(float2 p)
            {
                float a =0;
                float factor=1.0;
                const float2x2 m2 = float2x2(0.8,-0.6,0.6,0.8); 
                for(int i=0;i<4;i++)
                {
                    a+=factor*noise(p).x;
                    p =mul(m2,p*2.0);
                    factor*=0.5;
                }

                return a;
            }

            float3 SkyRender(float3 ro, float3 rd)
            {
                float3 col = float3(.3,.5,.85) -(rd.y *rd.y)*0.5;
                col = lerp(col,0.85*float3(0.7,0.75,0.85),pow(1.-max(rd.y,0),4.));

                float3 sunLight= normalize(float3(-0.8,0.4,-0.3));
                float  sundot =saturate(dot(sunLight,rd));//太阳和视线的夹角

                //夹角越大，强度越大
                col+=0.25*float3(1.,.7,.4)*pow(sundot,5.);
                col+=0.25 *float3(1.,.8,.6)*pow(sundot,64);
                col+=0.25 *float3(1.,.8,.6)*pow(sundot,512);
                //天际线
                col = lerp(col,0.68*float3(0.4,0.65,1.),pow(1.-max(rd.y,0),8.));

                //Cloud
                float cloudHeight = 400;
                float3 cloudUV = ro+(cloudHeight-ro.y)/rd.y*rd;
                col=lerp(col,float3(1.,.95,1.),0.5*smoothstep(0.8,0.5,fbm(cloudUV.xz*0.001)));

                
                return col;

            }

            float GetDistance_float(float3  p)
            {
                float d =0;
                
                float sphere = SDFSphere_float(p,float3(0,0,0),0.5);
                float ground = sdGroundH(p);
                d = min(sphere,ground);
                return d;
            }



            float RayMarching_float(float3 ray_origin, float3 ray_direction,float side ,float tmin,float tmax)
            {
                float  d=tmin;

                for (int i = 0; i < MAX_STEPS && d < tmax; i++)
                {
                    float3 p = ray_origin + ray_direction * d;
                    //side:inside =-1;outside=1;
                    float currentDistance =(p.y-sdGroundH(p.xz))*side ;
                    if (abs(currentDistance) < SURFANCE_DIS) break;
                     d += 0.2*currentDistance;
                    
                }

                return d;
            }

            float3 GetNormal(float3 p) // for function f(p)
            {
                float h = 0.01; // replace by an appropriate value
                float2 k = float2(1,-1);
                return normalize( k.xyy*GetDistance_float( p + k.xyy*h) + 
                                k.yyx*GetDistance_float( p + k.yyx*h) + 
                                k.yxy*GetDistance_float( p + k.yxy*h) + 
                                k.xxx*GetDistance_float( p + k.xxx*h) );
            }

            float3 GetNormal2(float3 p,float t) // for function f(p)
            {
                float2 e= float2(0.001*t,0);//根据距离缩放法线的精度
                float3 n = float3(
                        sdGroundH(p.xz-e.xy)-sdGroundH(p.xz+e.xy),
                        2.0*e.x,
                        sdGroundH(p.xz-e.yx)-sdGroundH(p.xz+e.yx)
                    );

                return normalize(n);
            }



            float Shadow (float3 ro,float3 rd,float mint,float maxt,float k)
            {
                float d = mint;
                float res = 1.0;
                for (int i = 0; i < MAX_STEPS && d <maxt; i++)
                {
                    float3 p = ro + rd*d;
                    float current_d = p.y- sdGroundM(p.xz);
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



            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv-0.5) *_ScreenParams/_ScreenParams.y;
                float4 col =0;

                float tmin =0.002;
                float tmax= MAX_DISTACNE;

                

                float an = _Time.y*.01;
                float r = 500;
                float2 posXZ =float2(r*sin(an),r*cos(an));
                float h = sdGroundL(posXZ)+22.;

                float3 ro =float3(posXZ.x,h,posXZ.y);
                float3 ta =float3(r*sin(an+0.01),h,r*cos(an+0.01));



                float3x3 ca = SetCamera( ro, ta, 0.0 );
				float3 rd = mul(ca,normalize( float3(uv,1.0)));
               // float3 rd =normalize( Camera_float(uv,ro,target,_CameraPos.w));

                float maxH=300.;//山的最大高度
                float tp = (maxH-ro.y)/rd.y;//视点到山顶的距离

                if (tp>0.)
                    if(rd.y>0.)
                        tmax = min(tmax,tp);
                    else
                        tmin = max(tmin,tp);

                float d = RayMarching_float(ro,rd,1.0,tmin,tmax);


                float3 sunLight = normalize(float3(-0.8,0.4,-0.3));
                float3 sky = SkyRender(ro,rd);

                col.rgb= sky;

                if (d<MAX_DISTACNE)
                {
                    float3 pos = ro + rd * d;
                    float3 nor = GetNormal2(pos,d);
                   
                    //float sunLightDot = saturate(dot(rd,sunLight));

                    col.rgb= float3(0.08,0.05,0.03);   


                    // snow
                    float h = smoothstep(55.0,80.0,pos.y + 25.0*fbm(0.01*pos.xz) );
                    float e = smoothstep(1.0-0.5*h,1.0-0.1*h,nor.y);
                    float o = 0.3 + 0.7*smoothstep(0.0,0.1,nor.x+h*h);
                    float s = h*e*o;
                    col.rgb = lerp( col.rgb, 0.29*float3(0.62,0.65,0.7), smoothstep( 0.1, 0.9, s ) );


                    float3 linearColorSpace =0;

                    float diffusion =0.8*(saturate(dot(nor,sunLight)));
                    float sh = Shadow(pos+sunLight*0.05,sunLight,0.01,5.,8.);
                    float amb = clamp(0.5+0.5*nor.y,0.0,1.0);
                    float bacgroundLight = saturate( 0.2 + 0.8*dot( normalize( float3(-sunLight.x, 0.0, sunLight.z ) ), nor ));
                    
                    linearColorSpace+=10*float3(1.,0.8,0.5)*diffusion*float3( sh, sh*sh*0.5+0.5*sh, sh*sh*0.8+0.2*sh );
                    linearColorSpace+=amb *float3(0.40,0.60,1.00)*1.2;
                    linearColorSpace+=bacgroundLight *float3(0.40,0.50,0.60);

                    col.rgb *=linearColorSpace;
                    //FOG
                    col.rgb= lerp(col.rgb,float3(.3,.5,.85),1.-exp(-pow(0.002*d,1.5)));
                }


                //col.rgb = noise(uv*20);
                //sun scatter
                col.rgb +=0.3*float3(1.0,0.7,0.3)*pow(saturate(dot(sunLight,rd)),8.0); 
                return col;
            }
            ENDCG
        }
    }
}

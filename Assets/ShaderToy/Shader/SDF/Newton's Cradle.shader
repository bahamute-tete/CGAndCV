Shader "RayMarching/NewtonCradle"
{
    Properties
    {
        _Background ("CubeMap", Cube) = "white" {}
        _CameraPos("CameraPosition", Vector) = (4.5,4.5,-7,1)
        _Target("CameraTarget",Vector) =(0,0,0)
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
            #define SURFANCE_DIS 1e-2
            #define MAX_STEPS 128
            #define MAX_DISTACNE 30


            #define MAT_DEFAULT 0
            #define MAT_BASE 1
            #define MAT_BAR 2
            #define MAT_LINE 3
            #define MAT_BALL 4

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

            float3 Camera_float(float2 uv,float3 origin, float3 look_at,float zoom){
                float3 forward = normalize(look_at -origin);
                float3 right = cross(forward,float3(0,1,0));
                float3 up = cross(right,forward);

                float3 center = origin + forward * zoom;
                float3 intersection = center + right * uv.x+ up * uv.y;
                return  intersection-origin;
            }

            float SDFSphere_float(float3 p, float3 center, float radius){
                return length(p - center) - radius;
            }

            float SDFPlane_float(float3 p, float3 n, float height)
            {
                return dot(p,normalize(n))+height;
            }

            float SDFBox_float(float3 p, float3 boxSize, float radius)
            {
                return length(max(abs(p)-boxSize,0))+min(max(p.x,max(p.y,p.z)),0) - radius;
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


            float2 SDNewtonBalls(float3 p, float3 center, float radius,float angle)
            {
                float offsetY =1.01;
                p =p-center;
                p.y = p.y -offsetY;
                p.xy = mul(p.xy,Rot(angle));
                p.y =p.y +offsetY;

                float dBall =length(p)-0.15;
                float dRing =SDFRing_float(p,0.03,0.01);
                dBall = min(dBall,dRing);

                p.z= abs(p.z);
                float dLine = SDLineSeg(p,float3(0,0.15,0),float3(0,offsetY,0.4))-0.001;

                float d =min(dBall,dLine);

                return float2(d,d==dBall?MAT_BALL:MAT_LINE);
            }

            float GetDistance_float(float3  p)
            {
                float base = SDFBox_float(p,float3(1,0.1,0.5),0.1);
               
                //excude the rectagle
                float bar = length(float2(SDFBox2D_float(p.xy,float2(0.8,1.4),0.15),abs(p.z)-0.4))-0.04;


                float angle =sin( 3*_Time.y);
                float angle1 =min(0,angle);
                float angle5 =max(0,angle); 
                float angle2 =(angle1+angle)*0.02;
                float angle3 =0;
                float angle4 =(angle5+angle)*0.02;
                
                float ball1 = SDNewtonBalls(p,float3(-0.6,0.5,0),0.15,angle1).x;
                float ball2 = SDNewtonBalls(p,float3(-0.3,0.5,0),0.15,angle2).x;
                float ball3 = SDNewtonBalls(p,float3(0,0.5,0),0.15,angle3).x;
                float ball4 = SDNewtonBalls(p,float3(0.3,0.5,0),0.15,angle4).x;
                float ball5 = SDNewtonBalls(p,float3(0.6,0.5,0),0.15,angle5).x;

                float balls = min(ball1,min(ball2,min(ball3,min(ball4,ball5))));
                float d = min(base,bar);
                //base = max(base,-p.y);

                d = min(d,balls);
                //CutoffBottom
                d= max(d,-p.y);
                return d;
            }

            int GetMat(float3  p)
            {
                float base = SDFBox_float(p,float3(1,0.1,0.5),0.1);
                base += sin(6*p.x)*0.02;
                //excude the rectagle
                float bar = length(float2(SDFBox2D_float(p.xy,float2(0.8,1.4),0.15),abs(p.z)-0.4))-0.04;


                float angle =sin( 1.3*_Time.y);
                float angle1 =min(0,angle);
                float angle5 =max(0,angle); 
                float angle2 =(angle1+angle)*0.05;
                float angle3 =0;
                float angle4 =(angle5+angle)*0.05;
                
                float2 ball1 = SDNewtonBalls(p,float3(-0.6,0.5,0),0.15,angle1);
                float2 ball2 = SDNewtonBalls(p,float3(-0.3,0.5,0),0.15,angle2);
                float2 ball3 = SDNewtonBalls(p,float3(0,0.5,0),0.15,angle3);
                float2 ball4 = SDNewtonBalls(p,float3(0.3,0.5,0),0.15,angle4);
                float2 ball5 = SDNewtonBalls(p,float3(0.6,0.5,0),0.15,angle5);

                float2 balls = Min2(ball1,Min2(ball2,Min2(ball3,Min2(ball4,ball5))));
                float d = min(base,bar);

                d = min(d,balls.x);
                //CutoffBottom
                d= max(d,-p.y);

                int mat =MAT_DEFAULT;
                if (d == base)
                    mat =MAT_BASE;
                else if (d ==bar)
                    mat =MAT_BAR;
                else if (d ==balls.x)
                    mat =balls.y;

                return mat;
            }

            float RayMarching_float(float3 ray_origin, float3 ray_direction)
            {
                float  d=0.0;

                for (int i = 0; i < MAX_STEPS && d < MAX_DISTACNE; i++)
                {
                    float3 p = ray_origin + ray_direction * d;
                    float currentDistance =GetDistance_float(p) ;
                    if (abs(currentDistance) < SURFANCE_DIS) break;
                     d += currentDistance;
                    
                }

                return d;
            }

            float3 GetNormal(float3 p) 
            {
                float h = 0.0001; // replace by an appropriate value
                float2 k = float2(1,-1);
                return normalize(   k.xyy*GetDistance_float( p + k.xyy*h) + 
                                    k.yyx*GetDistance_float( p + k.yyx*h) + 
                                    k.yxy*GetDistance_float( p + k.yxy*h) + 
                                    k.xxx*GetDistance_float( p + k.xxx*h) );
            }


            float Shadow (float3 ro,float3 rd,float mint,float maxt,float k){
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

            float calcAO( float3 pos, float3 nor){
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

            float3 GetLight (float3 p,float3 rd, float3 lightPos) {
                float3 lin = 0;
                float3 col = texCUBE(_Background,rd);

                float3 _lightPos =float3(lightPos.x+sin(_Time.y),lightPos.y,lightPos.z+cos(_Time.y));

                float3 l = normalize(_lightPos - p);

                float3 n = GetNormal(p);

                float3 ref = reflect( rd, n );

                float3 h = normalize(l+rd);

                float sha =Shadow( p, l, 0.02, 2.5,8 );

                float occ =calcAO(p,n);

                //sun
                float  hal = normalize( l-rd );
                float dif = clamp( dot( n, l ), 0.0, 1.0 );
                dif *= Shadow( p, l, 0.02, 2.5,8 );
                float spe = pow( clamp( dot( n, hal ), 0.0, 1.0 ),8.0);
                    spe *= dif;
                    spe *= 0.04+0.96*pow(clamp(1.0-dot(hal,l),0.0,1.0),18);
                //spe *= 0.04+0.96*pow(clamp(1.0-sqrt(0.5*(1.0-dot(rd,lig))),0.0,1.0),5.0);
                lin += col*1.0*dif*float3(1.0,1.00,0.7);
                lin +=     5*spe*float3(1.30,1.00,0.70);
                
                // sky
                float dif2 = sqrt(clamp( 0.5+0.5*n.y, 0.0, 1.0 ));
                dif2 *= occ;
                float spe2 = smoothstep( -0.2, 0.2, ref.y );
                spe2 *= dif2;
                spe2 *= 0.04+0.96*pow(clamp(1.0+dot(n,rd),0.0,1.0), 5.0 );//fresnel
                spe2 *= Shadow( p, ref, 0.02, 2.5,8 );//反射的计算方式和阴影的计算方式一样


                lin += col*1*dif2*float3(0.40,0.60,1.15);
                lin +=     5.00*spe2*float3(0.40,0.60,1.30);


                 // back
                float dif3 = clamp( dot( n, normalize(float3(0.5,0.0,0.6))), 0.0, 1.0 )*clamp( 1.0-p.y,0.0,1.0);
                dif3 *= occ;
                lin += col*0.55*dif3*float3(0.25,0.25,0.25);
        
                // sss
                float dif4 = pow(clamp(1.0+dot(n,rd),0.0,1.0),2.0);
                dif4 *= occ;
                lin += col*0.25*dif*float3(1.00,1.00,1.00);

                /*
                float  d=0.0;

                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 startPoint = (p + n * SURFANCE_DIS) + l * d;
                    float currentDistance =SDFSphere_float(startPoint,float3(0.,0.,0.),1); ;
                    if (currentDistance < SURFANCE_DIS || d > MAX_DISTACNE)
                        break;

                    d += currentDistance;
                }

                if (d < length(lightPos - p)) dif *=0.1;
                */
                col =lin;
                col = lerp( col, float3(0.7,0.7,0.9), 1.0-exp( -0.0001 ) );
                return  col;
            }


            float3 Render(inout float3 ro, inout float3 rd,inout float3 reflective){
                float3 col =texCUBE(_Background,rd).rgb;

                float d = RayMarching_float(ro,rd);
                reflective =0;
				if (d<MAX_DISTACNE)
				{
                    float3 p = ro+rd*d;
                    float3 n = GetNormal(p);
                    float3 refRay = reflect(rd,n);
                    //float3 refTex=texCUBE(_Background,refRay).rgb;

                    float fresnel =pow(saturate(1- dot(n,-rd)),5);
                    int matIndex = GetMat(p);
                    float3 lightPos = float3(1,2,3);
                    float diffusion = dot(n,normalize(lightPos))*0.5+0.5;

                    col = diffusion;
                   
                    if (matIndex ==MAT_BASE)
                    {
                        col =0.01*diffusion;
                        float ff =lerp(0.01,0.5,fresnel);
                        reflective = float3(ff,ff,ff);
                    }
                    else if (matIndex ==MAT_BAR)
                    {
                        col=0.1*diffusion;
                        reflective =float3(0.9,0.9,0.9);
                    }
                    else if (matIndex ==MAT_BALL)
                    {
                        col=0.1*diffusion;
                        reflective =float3(1,0.7,0.3);
                    }
                    else if (matIndex ==MAT_LINE)
                    {
                        col=0.05*diffusion;
                        reflective =0;
                    }

                    ro = p+n*SURFANCE_DIS*3;
                    rd = refRay;
				}


                return col;
            }


            fixed4 frag (v2f i) : SV_Target{
                float2 uv = (i.uv-0.5) *_ScreenParams/_ScreenParams.y;
                float4 col =0;

                float3 target =_Target;
                float3 ro = _CameraPos;
				float3 rd = Camera_float(uv,ro,target,_CameraPos.w);
				
                float3 kr=0;
                float3 decent=1;
                col.rgb =Render(ro,rd,kr);//第一次运行完毕后 ro 和rd 的值被改变了，因为inout 关键字

                for(int i=0;i<2;i++)
                {
                    decent*=kr;
                    float3 bounceBall= decent* Render(ro,rd,kr);
                    col.rgb+=bounceBall;
                }




                //col.rgb =pow(col,float3(0.4545,0.4545,0.4545));
/*              
                float3 n = GetNormal(p);
                float dif=sqrt(clamp( 0.5+0.5*n.y, 0.0, 1.0 ));
       

                float occ =calcAO(p,n);
                dif *= occ;


                float3 ref = reflect( rd, n );
                float spe = smoothstep( -0.2, 0.2, ref.y );
                spe *= dif;
                spe *= 0.04+0.96*pow(clamp(1.0+dot(n,rd),0.0,1.0), 5.0 );

                col.rgb = spe;
                //col.rgb = float3(0.7, 0.7, 0.9) - max(rd.y,0.0)*0.3;
                */
                return col;
            }
            ENDCG
        }
    }
}

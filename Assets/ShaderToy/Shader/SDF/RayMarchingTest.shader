Shader "RayMarching/RayMarchingTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Background ("CubeMap", Cube) = "white" {}
        _CameraPos("CameraPosition", Vector) = (4.5,4.5,-7,1)
        _Target("CameraTarget",Vector) =(0,0,0)
        _lightDir("_lightDir",Vector)=(1,2,3)
        _K("K",Range(0,1))= 0

        _Sphere1Pos("Sph1Pos",vector) =(-1,0,0,1)
        _Sphere2Pos("Sph2Pos",vector) =(1,0,0,1)
        _Sphere3Pos("Sph3Pos",vector) =(1,1,0,1)
        __GroundColor("_GroundColor",Color)=(1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

            #define SURFANCE_DIS 1e-3
            #define MAX_STEPS 64
            #define MAX_DISTACNE 30

            #define MAT_DEFAULT 0
            #define MAT_GROUND 1
            #define MAT_BALL 2
            #define MAT_BALL2 3
            #define PI 3.1415926

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
            float4 _CameraPos;
            float3 _Target,_lightDir;
            samplerCUBE _Background;
            float _K;
            float4 _Sphere1Pos,_Sphere2Pos,_Sphere3Pos;
            float3 _GroundColor;
            float3 _SphereColor[8];

            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2x2 Rot(float angel)
            {
                float s = sin(angel);
                float c = cos(angel);
                return float2x2(c, -s, s, c);
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

            float3x3 CamerarMat (float3 ro,float3 target)
            {
                float3 forward = normalize(target -ro);
                float3 right = cross(forward,float3(0,1,0));
                float3 up = cross(right,forward);

                return float3x3(right,up,forward);

            }

            float SDFSphere_float(float3 p, float radius)
            {
                return length(p) - radius;
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



            float2 GetDistance_float(float3  p)
            {
                float d =0;
                float k = _K;
                float mat=MAT_DEFAULT;
                float ground = p.y+0.8;
                float sphere= SDFSphere_float(p-_Sphere1Pos.xyz,_Sphere1Pos.w);
                
                //float sphere2= SDFSphere_float(p-_Sphere2Pos.xyz,_Sphere2Pos.w);
                //float sphere3= SDFSphere_float(p-_Sphere3Pos.xyz,_Sphere3Pos.w);
                //d=smin(sphere3, smin(sphere2,smin(sphere1,ground,k),k),k);

                for (int i  = 1;i<8;i++)
                {
                    p.xz = mul(Rot(2.0*PI/8.0*i),p.xz);
                    float sphereAdd = SDFSphere_float(p-_Sphere1Pos.xyz,_Sphere1Pos.w);
                    sphere = smin(sphere,sphereAdd,k);
  
                }
                d = smin(sphere,ground,k);

                if (d == ground)
                    mat= MAT_GROUND;
                else if (d == sphere)
                    mat= MAT_BALL;
                else 
                    mat = MAT_DEFAULT;


               

                return float2(d,mat);
            }


            float RayMarching_float(float3 ray_origin, float3 ray_direction,float side )
            {
                float  d=0.0;

                for (int i = 0; i < MAX_STEPS && d < MAX_DISTACNE; i++)
                {
                    float3 p = ray_origin + ray_direction * d;
                    //side:inside =-1;outside=1;
                    float currentDistance =GetDistance_float(p)*side ;
                    if (abs(currentDistance) < SURFANCE_DIS) break;
                     d += currentDistance;
                    
                }

                return d;
            }

            float3 GetNormal(float3 p) // for function f(p)
            {
                float h = 0.01; // replace by an appropriate value
                float2 k = float2(1,-1);
                return normalize( k.xyy*GetDistance_float( p + k.xyy*h).x + 
                                k.yyx*GetDistance_float( p + k.yyx*h).x + 
                                k.yxy*GetDistance_float( p + k.yxy*h).x + 
                                k.xxx*GetDistance_float( p + k.xxx*h).x );
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

            float3 GetReflection(inout float3 ro,inout float3 rd,  float3 p,float3 diffuse,inout float3 reflective)
            {
                float3 col=0;

                float d = RayMarching_float(ro,rd,1.);

                p = ro+rd*d;
                float3 n = GetNormal(p);
                float3 r = reflect(rd,n);

                
                if (d <MAX_DISTACNE)
                {
                    col = diffuse;
                    reflective =float3(0.2,0.6,1);
                }

                ro = p+n*SURFANCE_DIS*3;
                rd = r;
         
                return col;
                
            }

#define IOR 1.45//high density=>low density

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                _SphereColor[0] =float3(1,0,0);
                _SphereColor[1] =float3(1,1,0);
                _SphereColor[2] =float3(1,1,1);
                _SphereColor[3] =float3(0,1,0);
                _SphereColor[4] =float3(0,1,1);
                _SphereColor[5] =float3(0,0,1);
                _SphereColor[6] =float3(1,0,1);
                _SphereColor[7] =float3(1,1,1);

                float3 ballColor = _SphereColor[5];

                float2 uv = (i.uv-0.5) *_ScreenParams/_ScreenParams.y;
                
                float3 target =_Target;
                float3 ro = _CameraPos;
				float3 rd = mul(float3(uv.x,uv.y,1),CamerarMat(ro,target));
                
                col.rgb =texCUBE(_Background,rd);
                float d = RayMarching_float(ro,rd,1.);

                if (d<MAX_DISTACNE)
                {
                    float3 p = ro+rd*d;
                    float3 n = GetNormal(p);
                    float3 l =normalize(p-_lightDir);
                    float3 r = reflect(rd,n);
                    float3 h = normalize(l-rd);
                    float shadow = Shadow(p,l,0.02,2.5,8);
                    float4 litRes = lit(dot(n,l),dot(h,n),8);//vector (ambient, diffuse, specular, 1)

                    float3 ref =0;
                    float3 refracOut =0;

                    ref=texCUBE(_Background,r)*0.3;

                    float ao = calcAO(p,n);
                    float diffusion = saturate(dot(n,l))*0.5+0.5;

                    float3 refraIn = refract(rd,n,1./IOR);//inverse IOR 
                    float3 pEnter =p - n*SURFANCE_DIS*3;
                    float dIn = RayMarching_float(pEnter,refraIn,-1.);

                    float3 pExit = pEnter + refraIn*dIn;//position inside
                    float3 nExit = GetNormal(pExit);//insidePoit normal

                    refracOut = refract(refraIn,nExit,IOR);
                    if (dot(refracOut,refracOut)==0.) refracOut = reflect(refraIn,nExit); 

                    
                   

                    float fresnel =0.04+0.96*pow(clamp(1.0+dot(n,rd),0.0,1.0), 2.0 );

                    float spe = 1.0*pow( clamp( dot( n, h ), 0.0, 1.0 ),32.0);
                    spe *= diffusion;
                    spe *= 0.04+0.96*pow(clamp(1.0-dot(h,l),0.0,1.0),18);

                    float matID =GetDistance_float(p).y;  



                    col.rgb = ballColor* 2*diffusion*ao*fresnel+spe;
                    col.rgb+=ref*0.5;

                    ro = p+n*SURFANCE_DIS*3;
                    rd = r;
                    float d = RayMarching_float(ro,rd,1.);
                    if (d<MAX_DISTACNE)
                    {
                        float3 p = ro+rd*d;
                        float3 n = GetNormal(p);
                        float ao = calcAO(p,n);
                        float diffusion = saturate(dot(n,l))*0.5+0.5;

                        float fresnel =0.04+0.96*pow(clamp(1.0+dot(n,rd),0.0,1.0), 15.0 );

                        float spe = 1.0*pow( clamp( dot( n, h ), 0.0, 1.0 ),32.0);
                        spe *= diffusion;
                        spe *= 0.04+0.96*pow(clamp(1.0-dot(h,l),0.0,1.0),18);

                        col.rgb =ballColor* diffusion*ao*fresnel+spe;
                        col.rgb+=ref;
                        col.rgb*= 0.4;
                        
                    }

  /*           

                    if (matID==MAT_GROUND)
                    {
  
                        col.rgb = _GroundColor* diffusion*ao*fresnel+spe;
                        col.rgb+=ref*0.5;

                        ro = p+n*SURFANCE_DIS*3;
                        rd = r;
                        float d = RayMarching_float(ro,rd,1.);
                        if (d<MAX_DISTACNE)
                        {
                            float3 p = ro+rd*d;
                            float3 n = GetNormal(p);
                            float ao = calcAO(p,n);
                            float diffusion = saturate(dot(n,l))*0.5+0.5;

                            float fresnel =0.04+0.96*pow(clamp(1.0+dot(n,rd),0.0,1.0), 15.0 );

                            float spe = 1.0*pow( clamp( dot( n, h ), 0.0, 1.0 ),32.0);
                            spe *= diffusion;
                            spe *= 0.04+0.96*pow(clamp(1.0-dot(h,l),0.0,1.0),18);

                            col.rgb =ballColor* diffusion*ao*fresnel+spe;
                            col.rgb+=ref*.7;
                            col.rgb*= 0.7;
                            
                        }

                    }
                    else if(matID ==MAT_BALL)
                    {
                        col.rgb = ballColor* diffusion*ao*fresnel+spe;
                        col.rgb+=ref*1;

                        ro = p+n*SURFANCE_DIS*3;
                        rd = r;
                        float d = RayMarching_float(ro,rd,1.);
                        if (d<MAX_DISTACNE)
                        {
                            float3 p = ro+rd*d;
                            float3 n = GetNormal(p);
                            float ao = calcAO(p,n);
                            float diffusion = saturate(dot(n,l))*0.5+0.5;

                            float fresnel =0.04+0.96*pow(clamp(1.0+dot(n,rd),0.0,1.0), 15.0 );

                            float spe = 1.0*pow( clamp( dot( n, h ), 0.0, 1.0 ),32.0);
                            spe *= diffusion;
                            spe *= 0.04+0.96*pow(clamp(1.0-dot(h,l),0.0,1.0),18);

                            col.rgb =ballColor* diffusion*fresnel+spe;
                            col.rgb +=ref*0.5;
                            col.rgb*= 0.8;
                            
                        }
                    }
                
    */    
                 
                  
                    float groundDistance =length(p.xz);
                    float atten =1.0/groundDistance/groundDistance;
                    col.rgb=lerp(0,col.rgb,atten);
                   
                }
                return col;
            }
            ENDCG
        }
    }
}

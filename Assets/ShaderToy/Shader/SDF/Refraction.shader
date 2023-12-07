Shader "RayMarching/Refraction"
{
    Properties
    {
        _Background ("CubeMap", Cube) = "white" {}
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
            #define SURFANCE_DIS 1e-2
            #define MAX_STEPS 128
            #define MAX_DISTACNE 30

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



            float GetDistance_float(float3  p)
            {
                float d =0;
                p.xz =mul(p.xz,Rot(_Time.y));
                //float box = SDFBox_float(p,float3(1,1,1),0);
                float c = cos(UNITY_PI/5.0),s = sqrt(0.75-c*c);
                float3 n = float3(-0.5,-c,s);
                p =abs(p);
                p-=2.0*min(dot(p,n),0.0)*n;

                p.xy = abs(p.xy);
                p-=2.0*min(dot(p,n),0.0)*n;

                p.xy = abs(p.xy);
                p-=2.0*min(dot(p,n),0.0)*n;

                d=p.z-1.0;
                return d;
            }

            int GetMat(float3  p)
            {
                
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
                return normalize( k.xyy*GetDistance_float( p + k.xyy*h) + 
                                k.yyx*GetDistance_float( p + k.yyx*h) + 
                                k.yxy*GetDistance_float( p + k.yxy*h) + 
                                k.xxx*GetDistance_float( p + k.xxx*h) );
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



            float3 Render(inout float3 ro, inout float3 rd)
            {
 
                
                
            }


            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv-0.5) *_ScreenParams/_ScreenParams.y;
                float4 col =0;
                float3 target =_Target;
                float3 ro = _CameraPos;
				float3 rd = Camera_float(uv,ro,target,_CameraPos.w);
                
                col.rgb =texCUBE(_Background,rd);
                float d = RayMarching_float(ro,rd,1.);
#define IOR 1.45//high density=>low density
                if (d<MAX_DISTACNE)
                {
                    float3 p = ro+rd*d;
                    float3 n = GetNormal(p);
                    float3 l = float3(1,2,3);
                    float3 r = reflect(rd,n);
                    float3 h = normalize(l-rd);

                    float3 refraIn = refract(rd,n,1./IOR);//inverse IOR 
                    float3 pEnter =p - n*SURFANCE_DIS*3;
                    float dIn = RayMarching_float(pEnter,refraIn,-1.);

                    float3 pExit = pEnter + refraIn*dIn;//position inside
                    float3 nExit = GetNormal(pExit);//insidePoit normal

                    //float3 refracOut = refract(refraIn,nExit,IOR);
                    //当折射光线入射角过大的时，且由大密度忘小密度方向折射，会出现无法折射的现象
                    //数学公式上sqrt(1-ior*ior*(1-dot(N,L)*dot(N,L))))项，IOR平方项会很大，导致根号下为负
                    //这个时候按照反射方向进行反射
                    //refract函数在无法输出的时候 输出0长度的向量
                    //if (dot(refracOut,refracOut)==0.) refracOut = reflect(refraIn,nExit);

                    float3 reflTex =0;
                    float3 refracOut =0; 
                    float add=_ColorShift;
                    //ColorShift
                    refracOut = refract(refraIn,nExit,IOR-add);
                    if (dot(refracOut,refracOut)==0.) refracOut = reflect(refraIn,nExit); 
                    reflTex.r = texCUBE(_Background,refracOut).r;

                     refracOut = refract(refraIn,nExit,IOR);
                    if (dot(refracOut,refracOut)==0.) refracOut = reflect(refraIn,nExit); 
                    reflTex.g = texCUBE(_Background,refracOut).g;    

                     refracOut = refract(refraIn,nExit,IOR+add);
                    if (dot(refracOut,refracOut)==0.) refracOut = reflect(refraIn,nExit); 
                    reflTex.b = texCUBE(_Background,refracOut).b;          

                    float spec = pow(dot(n,h),8.);
                    //float3 refTex = texCUBE(_Background,refracOut);
                    float diffusion = dot(n,normalize(l))*0.5+0.5;

                    //Density
                    float optDis =exp(dIn*_Density);

                    float fresnel =pow(1+dot(n,rd),5.);

                    col.rgb =lerp(reflTex*optDis*_Color,texCUBE(_Background,rd),fresnel);
                    //col.rgb = n*0.5+0.5;
                }
                return col;
            }
            ENDCG
        }
    }
}

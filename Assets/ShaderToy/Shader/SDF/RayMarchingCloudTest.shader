Shader "RayMarching/RayMarchingCloudTest"
{
    Properties
    {

        _CameraPos("CameraPosition", Vector) = (4.5,4.5,-7,1)
        
        _Target("CameraTarget",Vector) =(0,0,0)
        _LightPos("LightPos",Vector)=(1,2,3)
         _ShadowColor("ShadowColor",Color)=(1,1,1,1)
        _CloudColor("CloudColor",Color) =(1,1,1,1)

        [Toggle]_RotateCamera("RotateCamera",float)=0
        [Toggle]_RotateCloud("RotateCloud",float)=0

        [Space(30)]
        _VolumeTex("ShapeTexture",3D)="white"{}
        _Offset("TextureOffset",Vector)=(0,0,0,0)
        _UVWSclae("UVWSclae",vector)=(1,1,1,1)
        _Step("SampleStep",Range(0.01,0.5))=0.1

        [Space(30)]
        _desityScale("DesityScale",float)=1
        _LightAbsorb("LightAbsorb",float)= 0.2
        _Transmittance("Transmittance",float)=0.5
        //g>0 fforward scattering
        //g<0 back scattering
        _G("G",Range(-0.5,0.5))=0
        _LightStepSize("LightStepSize",Range(0.01,0.5))=0.1
        _DarknessThreshold("DarknessThreshold",Range(0,1))=0.5

        [Space(30)]
        _K("K",Range(0,1))= 0
        _Sphere1Pos("Sph1Pos",vector) =(-1,0,0,1)
        _Sphere2Pos("Sph2Pos",vector) =(-1,0,0,1)
        _BoxSize("BoxSize",Vector)=(1,1,1,1)

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
            #define MAX_STEPS 128
            #define LIGHT_MAX_STEPS 32
            #define MAX_DISTACNE 32

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


            float4 _CameraPos;
            float3 _Target,_LightPos;
            float3 _UVWSclae;

            float _K;
            float4 _Sphere1Pos,_Sphere2Pos;
            float4 _BoxSize;
            float _desityScale;

           sampler3D _VolumeTex;
           float3 _Offset;
           float _Step,_LightStepSize;
           float _DarknessThreshold,_LightAbsorb,_Transmittance;

           float3 _ShadowColor,_CloudColor;
           float _G;
           bool _RotateCamera,_RotateCloud;


            
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



            float smin(float a, float b, float k)
            {
                float h = clamp(0.5+0.5*(b-a)/k, 0, 1);
                return lerp(b, a, h) - k*h*(1-h);
            }

            float opUnion( float d1, float d2 )
            {
                return min(d1,d2);
            }
            float opSubtraction( float d1, float d2 )
            {
                return max(-d1,d2);
            }
            float opIntersection( float d1, float d2 )
            {
                return max(d1,d2);
            }
            float opXor(float d1, float d2 )
            {
                return max(min(d1,d2),-max(d1,d2));
            }



            float2 GetDistance_float(float3  p)
            {
                float d = 0 ;
                float mat =0;
                float box = SDFBox_float(p-_Sphere2Pos.xyz,_BoxSize.xyz,0);
               

                float sphere = SDFSphere_float(p-_LightPos,_Sphere1Pos.w);
                d =opUnion(sphere,box);
                //d = sphere;

                if  (d ==sphere)
                    mat = 1;
                else
                    mat = 0;
                return float2(d,mat);
            }


            float2 RayMarching_float(float3 ray_origin, float3 ray_direction,float side )
            {
                float  d=0.0;
                float mat =0;

                for (int i = 0; i < MAX_STEPS && d < MAX_DISTACNE; i++)
                {
                    float3 p = ray_origin + ray_direction * d;
                    //side:inside =-1;outside=1;
                    float currentDistance =GetDistance_float(p).x*side ;
                    mat = GetDistance_float(p).y;
                    if (abs(currentDistance) < SURFANCE_DIS) break;
                     d += currentDistance;
                    
                }

                return float2(d,mat);
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

                float d = RayMarching_float(ro,rd,1.).x;

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

            float HenyeyGreenstein(float3 inLightVector, float3 inViewVector, float inG)
            {
                float cos_angle = dot(normalize(inLightVector), normalize(inViewVector));
                return ((1.0 - inG * inG) / pow((1.0 + inG * inG - 2.0 * inG * cos_angle), 3.0/2.0))
                    / 4.0 * 3.1415;
            }

            float BeerPower(float light_samples)
            {
                float powder_sugar_effect = 1.0 - exp(-light_samples * 2.0);
                float beers_law = exp(-light_samples);
                float light_energy = 2.0 * beers_law * powder_sugar_effect;
                return light_energy;

            }



 float3 knightlyHash( float3 p )
{
	p = float3( dot(p,float3(127.1,311.7, 74.7)),
			  dot(p,float3(269.5,183.3,246.1)),
			  dot(p,float3(113.5,271.9,124.6)));
	p = -1.0 + 2.0*frac(sin(p)*43758.5453123);
#if 1
	float t=_Time.y*0.5;
	float2x2 m=float2x2(cos(t),-sin(t), sin(t),cos(t));
	p.xz=mul(m,p.xz);
#endif
	return p;
}


float noise( in float3 p )
{
    float3 i = floor( p );
    float3 f = frac( p );
	
	float3 u = f*f*(3.0-2.0*f);

    return lerp( lerp( lerp( dot( knightlyHash( i + float3(0.0,0.0,0.0) ), f - float3(0.0,0.0,0.0) ), 
                          dot( knightlyHash( i + float3(1.0,0.0,0.0) ), f - float3(1.0,0.0,0.0) ), u.x),
                     lerp( dot( knightlyHash( i + float3(0.0,1.0,0.0) ), f - float3(0.0,1.0,0.0) ), 
                          dot( knightlyHash( i + float3(1.0,1.0,0.0) ), f - float3(1.0,1.0,0.0) ), u.x), u.y),
                lerp( lerp( dot( knightlyHash( i + float3(0.0,0.0,1.0) ), f - float3(0.0,0.0,1.0) ), 
                          dot( knightlyHash( i + float3(1.0,0.0,1.0) ), f - float3(1.0,0.0,1.0) ), u.x),
                     lerp( dot( knightlyHash( i + float3(0.0,1.0,1.0) ), f - float3(0.0,1.0,1.0) ), 
                          dot( knightlyHash( i + float3(1.0,1.0,1.0) ), f - float3(1.0,1.0,1.0) ), u.x), u.y), u.z );
}

const float3x3 m = float3x3( 0.00,  0.80,  0.60,
                    -0.80,  0.36, -0.48,
                    -0.60, -0.48,  0.64 );

float fracalNoise( in float3 pos)
{
 	float3 q = pos + float3(0, -_Time.y*0.025, 0);
    float f  = 0.5000*noise( q ); q =mul(m,q)*2.01;
    f += 0.2500*noise( q ); q = mul(m,q)*2.02;
    f += 0.1250*noise( q ); q = mul(m,q)*2.03;
    f += 0.0625*noise( q ); q = mul(m,q)*2.01;

    //f = noise(pos*2.0);
	f = smoothstep( -0.7, 0.7, f ); 
    return f;
}


            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = 0;

                float2 uv = (i.uv-0.5) *_ScreenParams/_ScreenParams.y;
                
                float3 target =_Target;
               
                float r =_CameraPos.w;
                float3 ro =0;
                if (_RotateCamera)
                {
                    ro = float3(r*cos(_Time.y),_CameraPos.y,r*sin(_Time.y));
                }else
                {
                    ro = _CameraPos;
                }
				float3 rd = mul(float3(uv.x,uv.y,1),CamerarMat(ro,target));
                //float3 rd = mul(CamerarMat(ro,target),float3(uv.x,uv.y,1));
                float3 lightDir = normalize(_LightPos);
                float darknessThreshold =_DarknessThreshold;
                float lightAbsorb =_LightAbsorb;
                float transmittance =_Transmittance;
                float g = _G;

                float3 result =0;
                float mask =0;

                float  d =0;
                float  density =0;

                float phaseValue =0;
                float transmission = 0;
                float lightAccumulation = 0;
                float finalLight = 0;
                float lightTransmission=0;
                float shadow;
                float3 lightShape=0;

                float2 res= RayMarching_float(ro,rd,1);
                if (res.x<MAX_DISTACNE)
                {
                    float3 p = ro + rd*res.x;
                    float3 n = GetNormal(p);
                    if (res.y==1)
                        lightShape =2;
                    else
                        col.rgb = 0.01;

                }

                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 p =ro+rd*d;

                    
                    p*=_UVWSclae;

                    //float noise = ValueNoisy(p.xz);



                    float3 lightOriginalPos = p;
                    if(_RotateCloud)
                        p.xz = mul(Rot(_Time.y),p.xz);
                  

                    float sphereDist = GetDistance_float(p).x;
                    
                    float3 samplePos =p+_Offset;
                    float3 lightSamplePos =lightOriginalPos+_Offset;;

                    // if (sphereDist < 0)
                    //  density+=0.1; 
                     if (sphereDist < 0)
                    {
                        float sampleDensity = tex3D(_VolumeTex,samplePos).r;
                        //float mathdensity=smoothstep(0.5, 0.8, fracalNoise(p*3));
                         
                        //云的密度
                        density += sampleDensity*_desityScale;

                        mask =density;



                        //light loop
                        float3 lightRayOrigin = lightSamplePos;
                        
                        for(int j = 0; j < LIGHT_MAX_STEPS; j++)
                        {
                            lightRayOrigin += lightDir*_LightStepSize;
                            float lightDensity =  tex3D(_VolumeTex,lightRayOrigin).r;
                            //灯光的累积强度
                            lightAccumulation +=max( lightDensity*_LightStepSize,0);
                        }

                       
                        //Beer_Lambert Law
                        //光通过云层后的衰减
                        lightTransmission = BeerPower(lightAccumulation);

                        //通过一个阈值控制衰减和原本颜色的比例
                        shadow = darknessThreshold + lightTransmission * (1.0 - darknessThreshold);
                        

                        //相位函数用来模拟云层最外面的边缘会形成的一个亮边
                        phaseValue = HenyeyGreenstein(lightDir,rd,g);


                        finalLight += density*transmittance*shadow*phaseValue;


                        //Beer_Lambert Law
                        //透射率按照云层密度进行衰减
                        transmittance*=exp(-density*lightAbsorb);

                    }

                    d+=_Step;
                }

                //对总密度进行一个衰减 模拟power-sugar-effect
                transmission = exp(-density);

                //float shadowMask = saturate((1-shadow)*mask);
                //输出3个值
                result =  float3(finalLight, transmission, transmittance);


                col.rgb = lerp(_ShadowColor, _CloudColor, result.x);
                
                //col.a =result.y;
                col.rgb = lerp(float3(0,0,0), col.rgb,1- result.y);

                //col.rgb += result;
                
                //col.rgb =saturate(mask+lightShape);

                // col.rgb *=saturate(mask+lightShape);
                return col;
            }
            ENDCG
        }
    }
}

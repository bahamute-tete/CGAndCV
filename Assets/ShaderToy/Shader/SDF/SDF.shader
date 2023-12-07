Shader "RayMarching/SDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CameraPos("Pos", Vector) = (4.5,4.5,-7)
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

            sampler2D _MainTex;
            float3 _CameraPos;
            #define SURFANCE_DIS 1e-2
            #define MAX_STEPS 100
            #define MAX_DISTACNE 32

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
          
                return length(max(abs(p)-boxSize,0)) - radius;
            }

            float GetDistance_float(float3  p)
            {
                float d =0;
                float sphere = SDFSphere_float(p,float3(0.,0.,0.),1);
                float box = SDFBox_float(p-float3(1.5,0,-1.5),float3(1,2,1)*0.5,0.1);
                float plane = SDFPlane_float(p,float3(0,1,0),1);
                d =min(min(plane,sphere),box);
            return d;


            }

            float RayMarching_float(float3 ray_origin, float3 ray_direction)
            {
                float  d=0.0;

                for (int i = 0; i < MAX_STEPS && d < MAX_DISTACNE; i++)
                {
                float3 p = ray_origin + ray_direction * d;
                float currentDistance =GetDistance_float(p) ;
                if (currentDistance < SURFANCE_DIS) break;
                d += currentDistance;
                }

                return d;
            }

            float3 GetNormal(float3 p) // for function f(p)
            {
                float h = 0.0001; // replace by an appropriate value
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

            float3 GetLight (float3 p,float3 rd, float3 lightPos)
            {
                float3 lin = 0;
                float3 col = float3(0.7, 0.7, 0.9) - max(rd.y,0.0)*0.3;

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
                float spe = pow( clamp( dot( n, hal ), 0.0, 1.0 ),16.0);
                    spe *= dif;
                    spe *= 0.04+0.96*pow(clamp(1.0-dot(hal,l),0.0,1.0),5.0);
                //spe *= 0.04+0.96*pow(clamp(1.0-sqrt(0.5*(1.0-dot(rd,lig))),0.0,1.0),5.0);
                lin += col*1.00*dif*float3(1.30,1.00,0.70);
                lin +=     1*spe*float3(1.30,1.00,0.70);
                
                // sky
                float dif2 = sqrt(clamp( 0.5+0.5*n.y, 0.0, 1.0 ));
                dif2 *= occ;
                float spe2 = smoothstep( -0.2, 0.2, ref.y );
                spe2 *= dif2;
                spe2 *= 0.04+0.96*pow(clamp(1.0+dot(n,rd),0.0,1.0), 5.0 );//fresnel
                spe2 *= Shadow( p, ref, 0.02, 2.5,8 );//反射的计算方式和阴影的计算方式一样


                lin += col*0.60*dif2*float3(0.40,0.60,1.15);
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


            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv-0.5) *_ScreenParams/_ScreenParams.y;
                float4 col =float4(0,0,0,1);

                float3 target =float3(0,0,0);
				float3 _CameraZoom = 1;
                float3 ro = _CameraPos;
				float3 rd = Camera_float(uv,ro,target,_CameraZoom);
				
				float d = RayMarching_float(ro,rd);

				float3 p  = ro+rd*d;
				float AmbientLight = 0.07;
                float3 lightPos = float3(0,5,0);
                
				if (d>MAX_DISTACNE)
				{
					col.rgb = float3(0.7, 0.7, 0.9) - max(rd.y,0.0)*0.3;
				}
				else
				{
					col.rgb = GetLight(p,rd,lightPos);
				}

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
                return col ;
            }
            ENDCG
        }
    }
}

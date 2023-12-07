

Shader "RayMarching/RayMachingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #pragma target 5.0

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
                float3 ray:TEXCOORD1;
            };

            sampler2D _MainTex;
            uniform float4x4 _CameraFrustum,_CamToWorld;
            uniform float _maxDistance;
            uniform float3 _directionalLight;
            uniform sampler2D _CameraDepthTexture;
            uniform float3 _Center;
            uniform float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                half index = v.vertex.z;
                v.vertex.z =0.01;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _CameraFrustum[(int)index].xyz;
                o.ray /= abs(o.ray.z);
                o.ray =mul(_CamToWorld,o.ray);
                return o;
            }
            float sdSphere(float3 p,float r)
            {
                return length(p)-r;
            }

            float sdBox(float3 p,float3 b)
            {
                float3 d = abs(p) - b;
                return length(max(d,0.0)) + min(max(d.x,max(d.y,d.z)),0.0);
            }

            
            float distanceFiled(float3 p)
            {
                float3 P =p;
                P.xz =frac(p.xz)-0.5;
                float ID =floor(p);

                float sphere1 = sdSphere(P-_Center,_Radius);

                return sphere1;
            }

            float3 getNormal(float3 p)
            {
                const float2 offset =float2(0.001,0.0);
                float3 n= float3(
                    distanceFiled(p+offset.xyy)-distanceFiled(p-offset.xyy),
                    distanceFiled(p+offset.yxy)-distanceFiled(p-offset.yxy),
                    distanceFiled(p+offset.yyx)-distanceFiled(p-offset.yyx)
                );

                return normalize(n);
            }

            fixed4 rayMarching(float3 ro,float3 rd, float depth)
            {
                fixed4 result =1;

                const int max_interation =64;
                float t=0;

                for(int i=0;i<max_interation;i++)
                {
                  if (t > _maxDistance || t>=depth)
                  {
                    result = fixed4(rd,0);
                    break;
                  }
                  float3 p = ro + rd*t;
                  float d = distanceFiled(p);
                  if(d<0.01)
                  {
                    float3 n = getNormal(p);
                    float light =0.7 * (saturate (dot(-_directionalLight,n))*0.5+0.5);
                    result =fixed4( fixed3(1,1,1)*light,1.0);
                    break;
                  }
                  t += d;

                }
                return result;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture,i.uv).r);
                depth*= length(i.ray);


                fixed3 col = tex2D(_MainTex,i.uv);
                float3 rd = normalize(i.ray.xyz);
                float3  ro =_WorldSpaceCameraPos;
                fixed4 result = rayMarching(ro,rd,depth);
                return fixed4(col*(1.0 -result.w)+result.xyz*result.w,1.0);
            }
            ENDCG
        }
    }
}

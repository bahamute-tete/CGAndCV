Shader "Custom/Fog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("NoiseTex", 2D) = "white" {}
        _fogMax("FogMax",Float) =10
        _fogMin("FogMin",Float) =0
        _fogColor("FogColor",Color) = (1,1,1,1)
        _fogIntensity("_fogIntensity",Float)=1
        _Amount("_Amount",Float)=1
    }
    SubShader
    {
        // No culling or depth
        Cull Off 
        ZWrite Off ZTest Always

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
                float4 screenPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                

                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex,_NoiseTex;
            sampler2D _CameraDepthTexture;
            float3 _fogColor;
            float _fogMax,_fogMin,_fogIntensity,_Amount;

            float3 custom_reconstruct_method(float2 screenPos)
            {
                //screenPos / screenPos.w就是【0,1】的归一化屏幕坐标  //_CameraDepthTexture是获取的深度图
                //Linear01Depth将采样的非线性深度图变成线性的
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPos));
                //将【0，1】映射到【-1， 1】上，得到ndcPos的x，y坐标
                float2 ndcPosXY = screenPos * 2 - 1;
                //float3的z值补了一个1，代表远平面的NDC坐标  _ProjectionParams代表view空间的远平面, 我们知道裁剪空间的w和view空间的z相等，
                //相当于做了一次逆向透视除法，得到了远平面的clipPos
                float3 clipPos = float3(ndcPosXY.x, ndcPosXY.y, 1) * _ProjectionParams.z;

                float3 viewPos = mul(unity_CameraInvProjection, clipPos.xyzz).xyz * depth;  //远平面的clipPos转回远平面的viewPos， 再利用depth获取该点在viewPos里真正的位置
                //补一个1变成其次坐标，然后逆的V矩阵变回worldPos
                float4 worldPos = mul(UNITY_MATRIX_I_V, float4(viewPos, 1));

                return worldPos;
            }

            float hash21(float2 p)
            {
                float3 a= frac(p.xyx*.1021);
                a+= dot(a, a.yzx+33.33);
                return frac(a.x*a.z+a.y*a.z);
               
            }

            float3 random(float2 p)
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
                return float3(version2,grad);


            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float noise =0;
                //noise= tex2D(_NoiseTex, i.uv+_Time.y*0.3).r-0.5;
                float4 ndcPos = i.screenPos*2-1;
                float2 screenUV =  i.screenPos.xy/i.screenPos.w;
  /*
                float depth =Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv));

                float3 ndcfar = float3(ndcPos.x, ndcPos.y, 1)*_ProjectionParams.z;
                float3 viewPos = mul(unity_CameraInvProjection,ndcfar.xyzz).xyz*depth;
                float4 worldPos = mul(UNITY_MATRIX_I_V, float4(viewPos, 1));
                worldPos.xyz/=worldPos.w;
*/
                float3 worldPos = custom_reconstruct_method(screenUV);

                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				depth = Linear01Depth(depth);
                float viewDistance = depth * _ProjectionParams.z;
               // UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
				//unityFogFactor = saturate(unityFogFactor);
               // col.rgb = lerp(unity_FogColor.rgb, col.rgb, saturate(unityFogFactor));

                noise=random(worldPos.xy+float2(_Time.y*3,0)).x;

                //if (depth > 0.99) discard;
/*
                float viewDistance = depth * _ProjectionParams.z;
				
                float3 foggedColor =
					lerp(unity_FogColor.rgb, col.rgb, 1);
                    col.rgb =unityFogFactor;
*/
             
                float fogfactor = (_fogMax -(viewDistance))/(_fogMax - _fogMin);
                //float fogfactor2 = exp(-_fogMax*(abs(worldPos.y)));
                //float fogfactor3 = exp(-pow(_fogMax-(abs(worldPos.y)),2));
                fogfactor = saturate(fogfactor*_fogIntensity);



                col.rgb = lerp( unity_FogColor.rgb,col.rgb, fogfactor);
               // col.rgb = worldPos;
                // just invert the colors
                //col.rgb =unityFogFactor;
                return col;
            }
            ENDCG
        }
    }
}

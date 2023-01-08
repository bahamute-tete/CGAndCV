Shader "Hidden/DepthField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
         #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;//float4(1 / width, 1 / height, width, height)


        float _FocusDistance,_FocusRange;
        sampler2D _CameraDepthTexture;

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

            half3 Sample(float2 uv)
            {
                return tex2D(_MainTex,uv).rgb;
            }

            half3 SampleBox(float2 uv,float delta)
            {
                float4 o = _MainTex_TexelSize.xyxy*float2(-delta,delta).xxyy;//sign =》 -1 -1，+1，+1

                //Sample 4 Conner
                half3 s= Sample(uv + o.xy)+Sample(uv+o.zy)
                        + Sample(uv+o.xw)+Sample(uv+o.zw);
                return s*0.25;

            }

        



    ENDCG



    SubShader
    {
  
 
        Cull Off
        ZTest Always
        ZWrite Off
        Pass
        { //0 circleofConrusionPass
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
         //我们使用的纹理只有一个 R 通道，所以整个 CoC 可视化现在是红色的。
         //我们需要存储实际的 CoC 值，因此去除负值的着色。
         //此外，我们可以将片段函数的返回类型更改为单个值。
            half frag (v2f i) : SV_Target
            {

                half depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv);

                // depth = Linear01Depth(depth);//(0,1]
                depth = LinearEyeDepth(depth);//(near，far]

                //将深度中心移到焦距地方并进行一定的比例压缩
                //导致超出焦距的点的 CoC 值为正，而焦距前面的点的 CoC 值为负
                float coc = (depth-_FocusDistance)/_FocusRange;
                coc = clamp(coc,-1,1);

                // if (coc<0)
                //     return  coc *-half4(1,0,0,1);

                return coc;
            }
            ENDCG
        }

        Pass
        { //1 bokehPass
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define BOKEH_KERNEL_MEDIUM
            //定义一个数组，其中包含所有重要的偏移量并循环遍历它，而不是检查每个样本是否有效
            // From https://github.com/Unity-Technologies/PostProcessing/
            // blob/v2/PostProcessing/Shaders/Builtins/DiskKernels.hlsl

            #if defined(BOKEH_KERNEL_SMALL)
            static const int kernelSampleCount = 16;
            static const float2 kernel[kernelSampleCount] = 
            {
                float2(0, 0),
                float2(0.54545456, 0),
                float2(0.16855472, 0.5187581),
                float2(-0.44128203, 0.3206101),
                float2(-0.44128197, -0.3206102),
                float2(0.1685548, -0.5187581),
                float2(1, 0),
                float2(0.809017, 0.58778524),
                float2(0.30901697, 0.95105654),
                float2(-0.30901703, 0.9510565),
                float2(-0.80901706, 0.5877852),
                float2(-1, 0),
                float2(-0.80901694, -0.58778536),
                float2(-0.30901664, -0.9510566),
                float2(0.30901712, -0.9510565),
                float2(0.80901694, -0.5877853),
            };
            #elif defined(BOKEH_KERNEL_MEDIUM)
                static const int kernelSampleCount = 22;
					static const float2 kernel[kernelSampleCount] = {
						float2(0, 0),
						float2(0.53333336, 0),
						float2(0.3325279, 0.4169768),
						float2(-0.11867785, 0.5199616),
						float2(-0.48051673, 0.2314047),
						float2(-0.48051673, -0.23140468),
						float2(-0.11867763, -0.51996166),
						float2(0.33252785, -0.4169769),
						float2(1, 0),
						float2(0.90096885, 0.43388376),
						float2(0.6234898, 0.7818315),
						float2(0.22252098, 0.9749279),
						float2(-0.22252095, 0.9749279),
						float2(-0.62349, 0.7818314),
						float2(-0.90096885, 0.43388382),
						float2(-1, 0),
						float2(-0.90096885, -0.43388376),
						float2(-0.6234896, -0.7818316),
						float2(-0.22252055, -0.974928),
						float2(0.2225215, -0.9749278),
						float2(0.6234897, -0.7818316),
						float2(0.90096885, -0.43388376),
					};
            #endif


            half4 frag (v2f i) : SV_Target
            {
                half3 color = 0;
                // float weight =0;
                // for (int u = -4; u <=4;u++)
                // {
                //     for (int v = -4;v <=4;v++)
                //     {
                //         float2 uvOffset = float2(u,v);

                //         if (length(uvOffset)<4)
                //         {
                //             uvOffset *= _MainTex_TexelSize.xy*2;
                //             color +=tex2D(_MainTex,i.uv+uvOffset).rgb;
                //             weight+=1;
                //         }
                //     }  
                // }

                for (int  k =0;k<kernelSampleCount;k++)
                {
                    float2 uvOffset = kernel[k];
                    //保持相同的圆盘半径，偏移量乘以 8
                    //uvOffset *=_MainTex_TexelSize*8;

                    //因为在申请RT的时候做了一次减半的下采样，为了保持一样的大小，偏移量减半
                    uvOffset *=_MainTex_TexelSize*4;
                    color +=tex2D(_MainTex,i.uv+uvOffset).rgb;

                }

                color *=1./kernelSampleCount; 
                return half4(color,1);
               
            }
            ENDCG
        }


        Pass
        { //2 PostFilterPass
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            half4 frag (v2f i) : SV_Target
            {
               float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
					half4 s =
						tex2D(_MainTex, i.uv + o.xy) +
						tex2D(_MainTex, i.uv + o.zy) +
						tex2D(_MainTex, i.uv + o.xw) +
						tex2D(_MainTex, i.uv + o.zw);
					return s * 0.25;
            }
            ENDCG
        }





    }
}

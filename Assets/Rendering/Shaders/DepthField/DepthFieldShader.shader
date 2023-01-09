Shader "Hidden/DepthField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
         #include "UnityCG.cginc"

        sampler2D _MainTex,_CoCTex,_CameraDepthTexture,_DoFTex;
        float4 _MainTex_TexelSize;//float4(1 / width, 1 / height, width, height)
        float _FocusDistance,_FocusRange,_BokehRadius;



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
         
         //我们使用的纹理只有一�?? R 通道，所以整�?? CoC 可视化现在是红色的�?
         //我们需要存储实际的 CoC 值，因此去除负值的着色�?
         //此外，我们可以将片段函数的返回类型更改为单个值�?
            half frag (v2f i) : SV_Target
            {

                half depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv);

                // depth = Linear01Depth(depth);//(0,1]
                depth = LinearEyeDepth(depth);//(near，far]

       
                float coc = (depth-_FocusDistance)/_FocusRange;

                //scale the CoC value by the bokeh radius

                coc = clamp(coc,-1,1)*_BokehRadius;

                // if (coc<0)
                //     return  coc *-half4(1,0,0,1);

                return coc;
            }
            ENDCG
        }

        pass
        {//1 preFileterPass 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag



            half Weigh (half3 c)
            {
					return 1 / (1 + max(max(c.r, c.g), c.b));
		    }

            //sampling from the four high-resolution to  low-resolution  
            //and average them. 
            //Store the result in the alpha channel.
            half4 frag (v2f i) : SV_Target
            {
                    float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;

                    //Toning Down the Bokeh
                    half3 s0 = tex2D(_MainTex, i.uv + o.xy).rgb;
                    half3 s1 = tex2D(_MainTex, i.uv + o.zy).rgb;
                    half3 s2 = tex2D(_MainTex, i.uv + o.xw).rgb;
                    half3 s3 = tex2D(_MainTex, i.uv + o.zw).rgb;

                    half w0 = Weigh(s0);
                    half w1 = Weigh(s1);
                    half w2 = Weigh(s2);
                    half w3 = Weigh(s3);

                    half3 color = s0 * w0 + s1 * w1 + s2 * w2 + s3 * w3;
                    color /= max(w0 + w1 + w2 + w3, 0.00001);


					half coc0 = tex2D(_CoCTex, i.uv + o.xy).r;
					half coc1 = tex2D(_CoCTex, i.uv + o.zy).r;
					half coc2 = tex2D(_CoCTex, i.uv + o.xw).r;
					half coc3 = tex2D(_CoCTex, i.uv + o.zw).r;

                    //regular downsample
                    // half coc = (coc0+coc1+coc2+coc3)*0.25;

                    //take the most extreme CoC value of the four texels, 
                    //either positive or negative.

                   half cocMin = min(min(min(coc0, coc1), coc2), coc3);
				   half cocMax = max(max(max(coc0, coc1), coc2), coc3);
				   half coc = cocMax >= -cocMin ? cocMax : cocMin;

             
                return half4(color, coc);
            }
            ENDCG
        }


        Pass
        { //2 bokehPass
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define BOKEH_KERNEL_MEDIUM
            //定义一个数组，其中包含所有重要的偏移量并循环遍历它，而不是检查每个样本是否有�??
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


            half Weight(half coc ,half radius)
            {
                //Instead of completely discarding samples, 
                //we'll assign them a weight in the 0�C1 range. 
                return saturate((coc - radius + 2) / 2);;
            }

            half4 frag (v2f i) : SV_Target
            {
                half coc = tex2D(_MainTex,i.uv).a;
                half3 bgcolor = 0,fgcolor =0;
                float bgweight =0,fgweight=0;


                for (int  k =0;k<kernelSampleCount;k++)
                {
                    float2 uvOffset = kernel[k]*_BokehRadius;

                    half radius = length(uvOffset);
                    //保持相同的圆盘半径，偏移量乘�?? 8
                    //uvOffset *=_MainTex_TexelSize*8;
                    

                    //因为在申请RT的时候做了一次减半的下采样，为了保持一样的大小，偏移量减半
                    uvOffset *=_MainTex_TexelSize.xy;

                    half4 s = tex2D(_MainTex,i.uv+uvOffset);
                    //color +=tex2D(_MainTex,i.uv+uvOffset).rgb;

                    // if (abs(s.a)>= radius)
                    // {
                    //     color +=s.rgb;
                    //     weight +=1;
                    // }
                    half bgsw = Weight(max(0,min(s.a,coc)),radius);
                    bgcolor+=s.rgb*bgsw;
                    bgweight+=bgsw;

                     half fgsw = Weight(-s.a,radius);
                    fgcolor+=s.rgb*fgsw;
                    fgweight+=fgsw;
                }

                bgcolor *= 1./(bgweight+(bgweight==0));
                fgcolor *= 1./(fgweight+(fgweight==0));
                half bgfg = min(1,fgweight * 3.14159265359/kernelSampleCount);
                half3 color = lerp(bgcolor,fgcolor,bgfg);
                //put it in the alpha channel of the DoF texture
                return half4(color,bgfg);
               
            }
            ENDCG
        }


        Pass
        { //3 PostFilterPass
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


        Pass
        { //4 combinePass
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            half4 frag (v2f i) : SV_Target
            {
               half4 source = tex2D(_MainTex,i.uv);

               half coc = tex2D(_CoCTex,i.uv).r;
               half4 dof  = tex2D(_DoFTex,i.uv);

               half dofStrenth = smoothstep(0.1,1,coc);


               half3 color = lerp(source.rgb,dof.rgb,dofStrenth +dof.a-dofStrenth* dof.a);
               return half4(color,source.a);
            }
            ENDCG
        }




    }
}

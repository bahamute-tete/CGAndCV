Shader "Hidden/FXAA"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }


//FXAA works by blending high-contrast pixels. 
//This is not a straightforward blurring of the image. 
    CGINCLUDE
         #include "UnityCG.cginc"

        sampler2D _MainTex,_CoCTex,_CameraDepthTexture,_DoFTex;
        float4 _MainTex_TexelSize;//float4(1 / width, 1 / height, width, height)
       


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

            struct LuminanceData
            {
                // use 8 neighbors
                float m,n,e,s,w;//middle,north,east,south,west
                float ne, nw, se, sw;
                float highest, lowest,contrast;
            };

            //Contrast Threshold most aggressively skips pixels, 
            //Relative Threshold can skip higher contrast pixels in brighter regions
            //(_RelativeThreshold* l.highest)
            float _ContrastThreshold,_RelativeThreshold;
            float _SubpixelBlending;
            

              v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Sample(float2 uv)
            {
                //To guarantee that no amount of perspective filtering is applied, 
                //use tex2Dlod to access the texture without adjustment in Sample,
               // return tex2D(_MainTex,uv);
               return tex2Dlod(_MainTex,float4(uv,0,0));
            }

            float SampleLuminance(float2 uv )
            {
                #if defined (LUMINANCE_GREEN)
                    return Sample(uv).g;
                #else
                    return Sample(uv).a;
                #endif
            }

            float SampleLuminance(float2 uv ,float uOffset,float vOffset)
            {
                uv+=_MainTex_TexelSize * float2 (uOffset,vOffset);
                return SampleLuminance(uv);

            }
            //First, the local contrast has to be calculated. 
            //FXAA uses the direct horizontal and vertical neighbors
            //and the middle pixel itself—to determine the contrast.
            LuminanceData SampleLuminanceNeighborhood(float2 uv)
            {
                LuminanceData l;
                l.m = SampleLuminance(uv);
                l.n = SampleLuminance(uv,0,1);
                l.e = SampleLuminance(uv,1,0);
                l.s = SampleLuminance(uv,0,-1);
                l.w = SampleLuminance(uv,-1,0);

                l.ne = SampleLuminance(uv,  1,  1);
                l.nw = SampleLuminance(uv, -1,  1);
                l.se = SampleLuminance(uv,  1, -1);
                l.sw = SampleLuminance(uv, -1, -1);

                //diagonal neighbors are spatially further away from the middle, 
                //they should matter less.
                l.highest = max(max(max(max(l.n,l.e),l.w),l.s),l.m);
                l.lowest = min(min(min(min(l.n,l.e),l.w),l.s),l.m);
                l.contrast = l.highest-l.lowest;

                return l;
            }

            bool ShouldSkipPixel(LuminanceData l)
            {
                //simply compare the contrast with the maximum of both
                float threshold = max(_ContrastThreshold,_RelativeThreshold* l.highest);
                return l.contrast <threshold;

            }

            //Second—if there is enough contrast—a blend factor has to be chosen based on the contrast. 
            float DeteminePixelBlendFactor(LuminanceData l)
            {
                //because  4 neighbors are important so make weights double
                //onther leave 1;
                // low pass filter like
                // 1 2 1
                // 2   2
                // 1 2 1

                float filter = 2*(l.n+l.w+l.s+l.n);
                filter += l.ne +l.nw +l.se +l.sw;
                filter *= 1./12;
                //find the contrast between the middle and this average, via their absolute difference. 
                //The result has now become a high-pass filter.
                filter = abs(filter -l.m);

                //normalized relative to the contrast of the NESW cross,via a division
                //as we might end up with larger values thanks to the filter covering more pixels than the cross.
                //should clamp 
                filter = saturate(filter/l.contrast);

                // Use the smoothstep function to smooth it out, 
                //then square the result of that to slow it down.
                float blendFactor = smoothstep(0,1,filter);
                return blendFactor*blendFactor *_SubpixelBlending;
            }

            //Third, the local contrast gradient has to be investigated to determine a blend direction. 
            struct EdgeData
            {
                bool isHorizontal;
                //How far it is to the next pixel
                float pixelStep;
                //track of this gradient and the luminance on the other side
                float oppsiteLuminance,grandient;
            };

            //center difference 
            EdgeData DetermineEdge(LuminanceData l)
            {
                EdgeData e;

                //cross  weight is  2 , diagonal weight is 1
                float  horizontal =
                        abs(l.n+l.s -2* l.m)*2 +abs(l.ne+l.se -2*l.e)+abs(l.nw +l.sw -2*l.w);

                float  vertical =
                        abs(l.e + l.w - 2 * l.m) * 2 +abs(l.ne + l.nw - 2 * l.n) +abs(l.se + l.sw - 2 * l.s);

                e.isHorizontal = horizontal >= vertical;

                // determine whether we should blend in the positive or negative direction. 
                //We do this by comparing the contrast—the luminance gradient—
                //on either side of the middle in the appropriate dimension. 
                 float pLuminance = e.isHorizontal? l.n:l.e;
                 float nLuminance = e.isHorizontal? l.s:l.w;

                 float pGradient = abs(pLuminance-l.m);
                 float nGradient = abs(nLuminance-l.m);   

                //if it's horizontal, then we'll blend vertically across the edge.
                //vice versa 
                e.pixelStep = e.isHorizontal? _MainTex_TexelSize.y:_MainTex_TexelSize.x;

                if (pGradient<nGradient)
                {
                    e.pixelStep = -e.pixelStep;
                    //track of this gradient and the luminance on the other side
                    e.oppsiteLuminance = nLuminance;
                    e.grandient = nGradient;
                }
                else
                { 
                    //track of this gradient and the luminance on the other side
                    e.oppsiteLuminance = pLuminance;
                    e.grandient = pGradient;
                }

                return e;
            }

            //////////////////Unity's post effect stack v2 
            #if defined(LOW_QUALITY)
                #define EDGE_STEP_COUNT 4
                #define EDGE_STEPS 1, 1.5, 2, 4
                #define EDGE_GUESS 12
		    #else
                #define EDGE_STEP_COUNT 10
                #define EDGE_STEPS 1,1.5,2,2,2,2,2,2,2,4
                #define EDGE_GUESS 8
            #endif
            
            static const float edgeSteps[EDGE_STEP_COUNT] ={EDGE_STEPS};

            float DeteminanEdgeBlendFactor (LuminanceData l,EdgeData e,float2 uv)
            {
               
                float2 uvEdge = uv;
                float2 edgeStep;

                //average luminance exactly on the edge, samplePoint on pixel side
                // need move 0.5 unit of pixels center
                // track  direction is horizontal and vertical
                if (e.isHorizontal)
                {
                    uvEdge.y +=e.pixelStep*0.5;
                    edgeStep = float2(_MainTex_TexelSize.x,0);
                }
                else
                {
                    uvEdge.x +=e.pixelStep *0.5;
                    edgeStep = float2(0 ,_MainTex_TexelSize.y);
                } 

                //average luminance ==> centerPos of  two pixels 
                float edgeLuminance = (l.m+e.oppsiteLuminance)*0.5;
                // FXAA uses a quarter of the original gradient
                float gradientThreshold = e.grandient *0.25;


                // Positive Direction edgeUV
                float2 puv = uvEdge + edgeStep *edgeSteps[0];
                //caculate the delta between edgeluminance  
                float pLuminanceDelta = SampleLuminance(puv)-edgeLuminance;
                bool pAtEnd = abs(pLuminanceDelta)>=gradientThreshold;   
            
                //some pixels further than one step away from the positive end point
                //add a loop after the first search step, performing it up to nine more times, 
                //for a maximum of ten steps per pixel.

                //via the UNITY_UNROLL attribute
                //simply tell the shader compiler to unrolled the loops
                UNITY_UNROLL
              	for (int ia = 1; ia < EDGE_STEP_COUNT  && !pAtEnd; ia++)
                 {
                    puv += edgeStep*edgeSteps[ia];
                    pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
                    pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
			    }

                //increase our UV offset one more time when
                //staircase steps more than ten pixels wide
                if (!pAtEnd)
                {
                    puv +=edgeStep*EDGE_GUESS;
                }

                //Negative Direction edgeUV
                float2 nuv= uvEdge-edgeStep*edgeSteps[0];
                float nLuminanceDelta = SampleLuminance(nuv)-edgeLuminance;
                bool nAtEnd = abs(nLuminanceDelta)>=gradientThreshold;
                UNITY_UNROLL
                for (int ib = 1; ib < EDGE_STEP_COUNT && !nAtEnd; ib++)
                 {
                    nuv -= edgeStep*edgeSteps[ib];
                    nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
                    nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
			    }

                //increase our UV offset one more time when
                //staircase steps more than ten pixels wide
                if (!nAtEnd) 
                {
				    nuv -= edgeStep*EDGE_GUESS;
			    }




                // distance to the end point in UV space
                float pDistance, nDistance;
                

                if (e.isHorizontal) {
                    pDistance = puv.x - uv.x;
                    nDistance = uv.x - nuv.x;
                }
                else {
                    pDistance = puv.y - uv.y;
                    nDistance = uv.y - nuv.y;
                }
                
                //Distance to nearest edge end.
                float shortDistance;

                bool deltaSign;

                if (pDistance<=nDistance)
                {
                    shortDistance = pDistance;
                    deltaSign = pLuminanceDelta>=0;
                }
                else
                {
                    shortDistance = nDistance;
                    deltaSign =nLuminanceDelta>=0;
                }

                if (deltaSign ==(l.m -edgeLuminance>=0))
                {
                    return 0;
                }
                return 0.5 - shortDistance / (pDistance + nDistance);
            }


            //Finally, a blend is performed between the original pixel and one of its neighbors.
            float4 ApplyFXAA(float2 uv)
            {
               LuminanceData l = SampleLuminanceNeighborhood(uv);

               if (ShouldSkipPixel(l))
               {
                    //make sure to return the original pixel if we decided not to blend it.
                    return Sample(uv);
               }

               float pixelBlend = DeteminePixelBlendFactor(l);
               EdgeData e = DetermineEdge(l);
               float edgeBlend = DeteminanEdgeBlendFactor(l,e,uv);
               //final blend factor of FXAA is simply the maximum of both blend factors
               float finalBlend = max(pixelBlend,edgeBlend);
               
               //simply sampling the image with an offset equal to the pixel step scaled by the blend factor. 
               if (e.isHorizontal)
               {
                    uv.y += e.pixelStep*finalBlend;
               }
               else
               {
                 uv.x += e.pixelStep*finalBlend;
               }

               //kept the original luminance in the alpha channel, 
               //in case you want to use it for something else, but that's not necessary.
                return float4(Sample(uv).rgb,l.m);
            }
    ENDCG



    SubShader
    {
  
 
        Cull Off
        ZTest Always
        ZWrite Off
        Pass
        { //0 luminancePass
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ GAMMA_BLENDING
         
        
            float4 frag (v2f i) : SV_Target
            {   
                float4 sample = tex2D(_MainTex,i.uv);
                //FXAA expects luminance values to lie in the 0–1 range, 
                //but this isn't guaranteed when working with HDR colors.
                //use the clamped color to calculate luminance.

//// Convert rgb to luminance////////////////////////////////////////
// with rgb in linear space with sRGB primaries and D65 white point
// half LinearRgbToLuminance(half3 linearRgb) {
// 	return dot(linearRgb, half3(0.2126729f,  0.7151522f, 0.0721750f));
// }
//////////////////////////////////////////////////////

                sample.rgb = saturate(sample.rgb);
                sample.a = LinearRgbToLuminance(sample.rgb);

                //FXAA is about perception, not physics
                //blending in linear space can produce visually worse results compared to blending in gamma space.
                #if defined (GAMMA_BLENDING)
                    sample.rgb = LinearToGammaSpace(sample.rgb);
                #endif
                return sample;
            }
            ENDCG
        }


          Pass
        { //1 fxaaPass
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            #pragma multi_compile _ LUMINANCE_GREEN
            #pragma multi_compile _ LOW_QUALITY
            #pragma multi_compile _ GAMMA_BLENDING
        
            float4 frag (v2f i) : SV_Target
            {   
                float4 sample =ApplyFXAA(i.uv);
            //We have to convert it back to linear space 
            // because the rendering pipeline assumes that the output is in linear space.
               #if defined(GAMMA_BLENDING)
						sample.rgb = GammaToLinearSpace(sample.rgb);
					#endif
                return sample;
            }
            ENDCG
        }






    }
}

Shader "Unlit/BloomShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
         #include "UnityCG.cginc"

        sampler2D _MainTex,_SourceTex;
        float4 _MainTex_TexelSize;//float4(1 / width, 1 / height, width, height)
        half _Threshold,_SoftThreshold;
        half _Intensity;


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

            //阈值确定哪些像素对光晕效果有贡献,没有贡献的将它们转换为黑色
            //加入设置阈值为1，则表明，只有当材质的颜色大于1的时候才会产生发光效果，等价于需要开启自发光选项
            half3 Prefilter(half3 c)
            {
                half brightness = max(c.r ,max(c.g,c.b));

                //见desmos ==>SoftKneeCurve
                half knee =_Threshold*_SoftThreshold;
                half soft = brightness -_Threshold + knee;
                soft = clamp(soft,0,2*knee);
                soft = soft *soft /(4*knee+0.0001);

                //c= (b-t)/b ==> t= b: c =0  t=0:c =1
                half contribution = max(soft,brightness -_Threshold)/max(brightness,0.0001);

                return  c*contribution;
            }



    ENDCG



    SubShader
    {
  
 
        Cull Off
        ZTest Always
        ZWrite Off
        
        Pass
        { //0 prefilter
         
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            fixed4 frag (v2f i) : SV_Target
            {
               return half4( Prefilter( SampleBox(i.uv, 1)), 1);
            }
            ENDCG
        }


        Pass
        { //1 downsample
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            fixed4 frag (v2f i) : SV_Target
            {
                return half4(SampleBox(i.uv, 1), 1);
            }
            ENDCG
        }

        Pass
        { //2 upsample
            Blend One One//将当前的图叠加到上采样的结果上
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            fixed4 frag (v2f i) : SV_Target
            {
               return half4(SampleBox(i.uv, 0.5), 1);
            }
            ENDCG
        }

        Pass
        { //3 渲染到最终目的地时会出错，因为还没有渲染到它destination = null 所以需要单独为它准备一个通道
            //使用默认混合模式
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            fixed4 frag (v2f i) : SV_Target
            {
                half4 c = tex2D(_SourceTex,i.uv);
                c.rgb+=_Intensity*SampleBox(i.uv, 0.5);
               return c;
            }
            ENDCG
        }

        Pass
        { //4  将最后的上采样结果显示出来 而不用和源图结合做为DEBUG
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            fixed4 frag (v2f i) : SV_Target
            {
               return half4(_Intensity*SampleBox(i.uv, 0.5), 1);
            }
           
            ENDCG
        }
    }
}

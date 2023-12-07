Shader "Noise/Voronoi"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


            float2 Hash22(float2 p)
            {
                float3 a = frac(p.xyx*float3(123.32,345.23,765.67));
                a +=dot(a,a+34.45);
                return frac(float2(a.x*a.y,a.y*a.z));

                
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (2*i.uv-1.)*_ScreenParams/_ScreenParams.y;
                fixed4 col =0;
                float m = 0;
                float t= _Time.y;
                float minDis =100;
                float index =1;
                if (false)
                {
                    for (int i = 0. ;i <50.;i++)
                    {

                        float2 n= Hash22(float2(i,i));
                        float2 p= sin(n*t);
                        float d = length(uv-p);
                        m += smoothstep(0.02,0.01,d);

                        if (d < minDis) 
                        {
                            minDis = d;
                            index = i;

                        }
                        
                    }
                    col.rgb = index/50;
                }else
                {
                    uv*=10;
                    float2 gridUV= frac(uv)-0.5;
                    float2 id = floor(uv);

                    for (int y= -1 ;y<=1;y++)
                    {
                        for (int x= -1 ;x<=1;x++)
                        {
                            float2 offset = float2(x,y);
                            //sin值域是[-1,1],总长度是2 ，而uv 格子 是长度是1，所以是[-0.5,0.5]
                            float2 p =offset+ sin(Hash22(id+offset)*t)/2.;
                            float d = length(gridUV-p);
                            //m += smoothstep(0.02,0.01,d);

                            if (d < minDis) 
                            {
                                minDis = d;
                                //index = n;
                            }
                        }
                    }

                    col.rgb= minDis;
                }
                
                return col;
            }
            ENDCG
        }
    }
}

Shader "RayMarch/SDF2DMaskTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Width("MaskSizeWidth",range(0,0.5))=0.1
		_Height("MaskSizeHeight",range(0,1))=0.1
		_FilletedCorner("FilletedCorner",Range(0,1.0))=0.01
		_FadeIntensity("Fade",Range(0.0,0.2))=0.01
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnorProjector"="True" "RenderType"="Opaque" "DisableBatching"="True" }
        LOD 300
		ZWrite on
		Blend SrcAlpha OneMinusSrcAlpha
		
		
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float _Width;
			float _Height;
			float _FilletedCorner;
			float _FadeIntensity;

			float sdPoint (float2 p,float2 pc)
			{
				float2 d = p -pc;
				return length(d);
			}

			float sdLine (float2 p ,float2 a ,float2 b)
			{
				float2 pa = p-a;
				float2 ba = b-a;
				float h = clamp(dot(pa,ba)/dot(ba,ba),0,1);
				float2 d = pa -ba*h;
				return length(d);
			}

			float sdRec( float2  p,  float2 b )
			{
				float2 d = abs(p)-b;
				return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               
                fixed4 col = 1;
				float2 size = float2(_Width,_Height);
				float2 uv = 2*i.uv -1;
				float dbleft = sdRec(uv-float2(-0.5,0),size)-_FilletedCorner;
				float dbright = sdRec(uv-float2(0.5,0),size)-_FilletedCorner;
				//float rec =smoothstep(0,0.5, smoothstep(_FadeIntensity,0,dbleft)+smoothstep(_FadeIntensity,0,dbright));
				float rec = smoothstep(_FadeIntensity,0,dbleft)+smoothstep(_FadeIntensity,0,dbright);
				 //rec =rec*rec;

				float4 tex = tex2D(_MainTex,i.uv);
                return rec*tex;
            }
            ENDCG
        }
    }
}

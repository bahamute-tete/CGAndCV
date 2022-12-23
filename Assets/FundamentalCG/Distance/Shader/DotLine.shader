Shader "LineTest/DotLine"
{
    Properties
    {
       
		_Color ("LineColor",Color)  = (1,1,1,1)
		_LineWidth("_LineWidth",float)= 4
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" }
        LOD 100
		//Blend SrcAlpha OneMinusSrcAlpha
		//ZWrite on
		//GrabPass {"_Gtx"}
		Cull off
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

            
			float4  _Color;
			float _LineWidth;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

				float4 col = _Color;
               float2 uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
			   uv*= _LineWidth;
			 
			   float d  =smoothstep(0.5,0.51, frac(uv.x));
			   clip(d-1);
			
				col = sqrt(col);
                return col;
            }
            ENDCG
        }
    }
}

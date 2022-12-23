Shader "RayMarch/FireWorks"
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

			#define N(h) frac(sin(float4(6,9,1,0)*h) * 9e2) 

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
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

            fixed4 frag (v2f i) : SV_Target
            {
				//initialize o to 0,0,0,0 in the shortest way possible
				// o is what will hold the final pixel color
				float4 col = 0;

				float2 u = (i.uv)*_ScreenParams/_ScreenParams.y;
				u*=1;
				// loop iterator defined here because it saves characters
				// starts at -2 so it goes through 0, which gives the occasional rocket
				float e, d, r=-2.;

				for(float4 p; r++<9.;
					// e = the time since the shader started + an offset per explosion
					// d = the floored version of that, which stays the same for a second, then jumps
					d = floor(e = r*9.1+_Time.x*5),
					// the position of the explosion, as well as the color
					// which is a pseudo random number made by throwing a large number into a sine function
					// +.3 so the explosions are centered horizontally (because aspect ratio of screen)
					p = N(d)+0.3, 
					// turn e into the fractional component of time e.g. 10.546 -> 0.546
					e -= d)
				{
					for(d=0.; d++<30.;)
					// col = the color, 1.-e is a number that starts at 1 and goes to 0 
					// over the duration of the explosion, essentially fading the particle
					col += p*(1.-e) 
					 // divide by 1000, otherwise the pixel colors will get too bright
					/ 1e3 
					// divide by the distance to the particle, the farther away, the darker
					// note that this never gets to 0, each tiny particle has an effect over the
					// entire screen
					// dist to particle is the length of the vector from the current uv coordinate (u)
					// to the particle pos (p-e*(N(d*i)-.5)).xy
					// particle pos starts at p, when e is 0
					// N(d*i) gives a pseudo random vec4 in 0-1 range
					// d*i to give different vec4 for each particle
					// *i is not really necessary but when i=0 it gives 0 for the whole vec4
					// which makes the appearance of the occasional rocket
					// N(d*i)-.5 to go from 0-1 range to -.5 .5 range
					/ length(u-(p-e*(N(d*r)-.5)).xy);
					
				}
				

				// draw skyline
				// uv.x goes from 0 to 1.6  *i to make it larger i=9. (save a char cuz 9. is 2 chars)
				// +d+e   d+e = iTime  -> this will make the skyline scroll
				// ceil to go in steps (stay at one height, then jump to the next)
				// N(..) to make a value 0, 1, 2, 3.. etc into random numbers in 0-1 range
				// .x*4   N returns a vec4, but we only need a float, *.4 so buildings are lower
				// o -= o*u.y   o-=o would make the buildings pitch black, *u.y to fade them towards the 
				// bottom, creating a bit of a fog effect     
				u.y<N(ceil(u.x*r+d+e)).x*.4 ? col-=col*u.y : col;

				return col;

            }


            ENDCG
        }
    }
}

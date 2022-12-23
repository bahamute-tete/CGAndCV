Shader "RayMarch/GroundMark"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_cubeMap("CubeMap",Cube) ="_Skybox"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float length:TEXCOORD1;
				SHADOW_COORDS(2)
				float3 worldViewDir :TEXCOORD3;
				//float4 objPos:TEXCOORD4;
                float4 pos : SV_POSITION;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _CollisionPointSph;
			float4 _CollisionPointCube[4];
			int _Type;
			samplerCUBE  _cubeMap;
			float4x4 _WorldToLocal;


			float sdPoint (float2 p ,float2 a )
			{
				return length(a-p);
			}

			float sdCircle(float2 p,float2 cs,float r)
			{
				float d =length(p-cs)-r;
				return d;
			}

			float sdLine (float2 p ,float2 a ,float2 b)
			{
				float2 pa = p-a;
				float2 ba = b-a;
				float h = clamp(dot(pa,ba)/dot(ba,ba),0,1);
				float2 d = pa -ba*h;
				return length(d);
			}

			float3 sdRec(float2 p,float2 size)
			{
				float2 w = abs(p)-size;
				float2 s = float2(p.x>0?1:-1 ,p.y>0? 1:-1);//sign
				float g = max(w.x,w.y);//  if  g>0 ===> out rec   g<0 ===> inner rec 
				float2 q= max(w,0);
				float l = length(q)-0.012;
				float2  m  = s*(g>0?q/l : (w.x>w.y)? float2(1,0):float2(0,1));//if  inner w.x>w.y==>the point at the  leftside of diagonal line else  at rightside 
				return float3 ( (g)>0?l:g, m);
			}


            v2f vert (appdata v)
            {
                v2f o;
				float3 worldPos = mul(unity_ObjectToWorld,v.vertex);
				 //v.objPos = v.vertex;
				o.length = distance(worldPos,0);
				//o.length = distance(v.vertex,0);
				o.worldViewDir = UnityWorldSpaceViewDir(worldPos);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv*1.6);
				float shadow = SHADOW_ATTENUATION(i)*0.8;
				
				
				
				 fixed3 reflectionDir =reflect(normalize(-i.worldViewDir),float3(0,1,0));
				float4 reflectionColor =float4 (texCUBE( _cubeMap,reflectionDir ).rgb,1);
				col *= shadow;
				col = lerp(col,reflectionColor,0.2);


				float2 uv = 2*i.uv-1;
				float w =0.5* sqrt(2)*i.length;

				//float4 objSpace = mul(_WorldToLocal,_CollisionPointSph);
				//float2 objPos = float2(objSpace.x/objSpace.w,objSpace.z/objSpace.w);
				//float2 posSph = float2(objPos.x/w,objPos.y/w);
				float2 posSph = float2(_CollisionPointSph.x/w,_CollisionPointSph.z/w);
				
				float  sphd = sdPoint(uv,posSph);

				float2 posC0=0;
				float2 posC1=0;
				float2 posC2=0;
				float2 posC3=0;

				//float4 objSpaceCube0 = mul(_WorldToLocal,_CollisionPointCube[0]);
				//float4 objSpaceCube1 = mul(_WorldToLocal,_CollisionPointCube[1]);
				//float4 objSpaceCube2 = mul(_WorldToLocal,_CollisionPointCube[2]);
				//float4 objSpaceCube3 = mul(_WorldToLocal,_CollisionPointCube[3]);

				//float2 objPosCube0 = float2(objSpaceCube0.x/objSpaceCube0.w,objSpaceCube0.z/objSpaceCube0.w);
				//float2 objPosCube1 = float2(objSpaceCube1.x/objSpaceCube0.w,objSpaceCube1.z/objSpaceCube0.w);
				//float2 objPosCube2 = float2(objSpaceCube2.x/objSpaceCube0.w,objSpaceCube2.z/objSpaceCube0.w);
				//float2 objPosCube3 = float2(objSpaceCube3.x/objSpaceCube0.w,objSpaceCube3.z/objSpaceCube0.w);


				//posC0 = float2(objPosCube0.x/w,objPosCube0.y/w);
				//posC1 = float2(objPosCube1.x/w,objPosCube1.y/w);
				//posC2 = float2(objPosCube2.x/w,objPosCube2.y/w);
				//posC3 = float2(objPosCube3.x/w,objPosCube3.y/w);

				posC0 = float2(_CollisionPointCube[0].x/w,_CollisionPointCube[0].z/w);
				posC1 = float2(_CollisionPointCube[1].x/w,_CollisionPointCube[0].z/w);
				posC2 = float2(_CollisionPointCube[2].x/w,_CollisionPointCube[2].z/w);
				posC3 = float2(_CollisionPointCube[3].x/w,_CollisionPointCube[3].z/w);
				

				if (_Type==0)
				{
					float  cd0 = sdPoint(uv,posC0);
					col+= smoothstep(0.02,0.01,cd0)*float4(0,1,0,1);
					
				}else if (_Type==1)
				{
					
					float cl = sdLine(uv,posC0,posC1);
					float pix = fwidth(cl)*3;
					col+= smoothstep(pix,0,cl)*float4(0,1,0,1);
					
					
				}else			
				{
					float cl0 = sdLine(uv,posC0,posC1);
					float cl1 = sdLine(uv,posC1,posC2);
					float cl2 = sdLine(uv,posC2,posC3);
					float cl3 = sdLine(uv,posC3,posC0);
					float pix = fwidth(cl0)*3;
					
					
					col+= smoothstep(pix,0,cl0)*float4(0,1,0,1);
					col+= smoothstep(pix,0,cl1)*float4(0,1,0,1);
					col+= smoothstep(pix,0,cl2)*float4(0,1,0,1);
					col+= smoothstep(pix,0,cl3)*float4(0,1,0,1);
				}

				

				col+= smoothstep(0.02,0.01,sphd)*float4(1,0,0,1);

				//col = float4(posSph.y,posSph.y,posSph.y,1);

				//col = float4(w,w,w,1);
                return col;
            }
            ENDCG
        }

		
    }

	Fallback "Diffuse"
}

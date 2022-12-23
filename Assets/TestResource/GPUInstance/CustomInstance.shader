Shader "Unlit/CustomInstance"
{
    Properties
    {

        
    }


    CGINCLUDE
        
        #include "Autolight.cginc"
        #include "UnityCG.cginc"
        #include "Lighting.cginc"

         float3x3 AngleAxis3x3(float angle, float3 axis)
        {
            float c, s;
            sincos(angle, s, c);

            float t = 1 - c;
            float x = axis.x;
            float y = axis.y;
            float z = axis.z;

            return float3x3(
                t * x * x + c, t * x * y - s * z, t * x * z + s * y,
                t * x * y + s * z, t * y * y + c, t * y * z - s * x,
                t * x * z - s * y, t * y * z + s * x, t * z * z + c
                );
        }


        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float3 normal:NORMAL;

            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
            float3 normal:TEXCOORD1;
            unityShadowCoord4 _ShadowCoord :TEXCOORD2;
            /// use this to access instanced properties in the fragment shader.
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };


        UNITY_INSTANCING_BUFFER_START(Props)//buffername
        //Defines a per-instance shader property with the specified type and name. 
            UNITY_DEFINE_INSTANCED_PROP(float4,_Color)//property name);
            UNITY_DEFINE_INSTANCED_PROP(float ,_Theta)
        UNITY_INSTANCING_BUFFER_END(Props)

        v2f vert (appdata v)
        {
            v2f o;
            //Allows shader functions to access the instance ID. 
            //For vertex shaders, this macro is required at the beginning. 
            //For fragment shaders, this addition is optional. 
            UNITY_SETUP_INSTANCE_ID(v);

            //Copies the instance ID from the input structure to the output structure in the vertex
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            
            float theta = UNITY_ACCESS_INSTANCED_PROP(Props,_Theta);

            float angle = fmod(_Time.y,UNITY_TWO_PI);
            float3 newPos = mul(AngleAxis3x3(angle,float3(0,1,0)),v.vertex.xyz);

            // float3 worldPos = mul(_Object2World,v.vertex);
            // float3 worldDir = normalize(worldPos);
            
            float3 dir =normalize(newPos);
            newPos+=dir*sin(_Time.y+theta); 

            
                
            o.normal =normalize( UnityObjectToWorldNormal(v.normal));
            o.vertex = UnityObjectToClipPos(newPos);
            //o.vertex = UnityObjectToClipPos(v.vertex);
            o._ShadowCoord=ComputeScreenPos(o.vertex);



            return o;
        }


    ENDCG



    SubShader
    {
        Tags { 
                "RenderType" = "Opaque"
                "LightMode" = "ForwardBase"
            }

        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbased



            fixed4 frag (v2f i) : SV_Target
            {

                UNITY_SETUP_INSTANCE_ID(i);

                float NoL = dot(i.normal,_WorldSpaceLightPos0);
                float3 ambient = ShadeSH9(float4(i.normal,1));
                float4 albedo =float4( NoL*_LightColor0+ambient,0);


                float shadow = SHADOW_ATTENUATION(i)*0.8;
                float4 color = albedo*UNITY_ACCESS_INSTANCED_PROP(Props,_Color)*shadow;
                return color;
            }
            ENDCG
        }

        Pass
        {

                Tags { 
                         "LightMode" ="ShadowCaster"
                    }

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing
                #pragma multi_compile_shadowcaster

                float4 frag(v2f i):SV_Target
                {
                    SHADOW_CASTER_FRAGMENT(i);

                }

                ENDCG
        }
    }
}

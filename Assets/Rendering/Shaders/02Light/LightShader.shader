

Shader "Unlit/LightShader"
{
    Properties
    {
        _Tint("Color",Color) =(1,1,1,1)
        _Texture("Albedo",2D) ="white"{}
        _Smoothness("smoothness",Range(0,1)) =0.5
        _SpecularTint("specular",Color) =(1,1,1,1)
        [Gamma]_Metallic("Metallic",Range(0,1)) =0.5

    }
    SubShader
    {
        Tags { "LightMode" ="ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma target 3.0

            #include "UnityCG.cginc"
            //NonePBS
            #include "UnityStandardBRDF.cginc"     
            //dotClamped
            #include "UnityStandardUtils.cginc"
            //EnergyConservationBetweenDiffuseAndSpecular(albedo,specular,oneMinusReflectivity)
            //DiffuseAndSpecularFromMetallic()

            #include "UnityPBSLighting.cginc" 
            //Macro: UNITY_BRDF_PBS(albedo,specular,oneMinusReflectivity,smoothness,normal,viewDir,LightDir,LightIndir)

            float4 _Tint;
            sampler2D _Texture;
            float4 _Texture_ST;
            float _Smoothness;
            float4 _SpecularTint;
            float _Metallic;
           

            struct vertexData
            {
                float4 position:POSITION;
                float2 uv:TEXCOORD0;
                float3 normal:NORMAL;
                
                
            };

            struct vertex2Fragment
            {
                float4 position:SV_POSITION;
                float2 uv:TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
                float3 worldPostion:TEXCOORD2;
            };

            vertex2Fragment vert(vertexData v)
            {
                vertex2Fragment o;
                o.position =UnityObjectToClipPos(v.position);
                // o.uv =v.uv*_Texture_ST.xy+_Texture_ST.zw;
                o.uv =TRANSFORM_TEX(v.uv,_Texture);
                // o.normal = v.normal;
                //o.worldNormal =mul(unity_ObjectToWorld, float4(v.normal,0));
                //o.worldNormal =normalize(mul(transpose((float3x3)unity_WorldToObject),v.normal));
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPostion = mul(unity_ObjectToWorld,v.position);
                
                return o; 
            }

            float4 frag(vertex2Fragment i):SV_TARGET
            {
                // float4 col = float4 (i.uv,1,1);
                //float3 lightDir = float3(1,0,1);
                float3 lightDir =_WorldSpaceLightPos0.xyz;

                float4 tex = tex2D(_Texture,i.uv)*_Tint;
                i.worldNormal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos- i.worldPostion);
                //float4 light = max(0,dot(i.worldNormal,lightDir)) ;
                //float4 light = saturate(dot(i.worldNormal,lightDir)) ;
                //float4 light = DotClamped(i.worldNormal,normalize(lightDir)) ;

                float3 lightColor = _LightColor0.rgb;
                float3 albedo =tex2D(_Texture,i.uv).rgb*_Tint.rgb;

                /*Specular工作流
                //手动计算能量衰减，关联表面反射和高光
                //albedo *= 1- max(_SpecularTint.r,max(_SpecularTint.g,_SpecularTint.b));
                //利用Unity自己的函数计算 效果一样
                float oneMinusReflectivity;
                albedo=EnergyConservationBetweenDiffuseAndSpecular(albedo,_SpecularTint,oneMinusReflectivity);
                */

                //Metallic 工作流
                // float3 specularTint = albedo* _Metallic;
                // float oneMinusReflectivity =1 -_Metallic;
                // albedo *=oneMinusReflectivity;

                float3 specularTint;
                float oneMinusReflectivity;
                albedo=DiffuseAndSpecularFromMetallic(albedo,_Metallic,specularTint,oneMinusReflectivity );
                
                /* NonePBS
                float3 diffuse =albedo*lightColor*light;

                //float3 refletctionDir = reflect(-lightDir,i.worldNormal);//  D-2N（N dot D）
                 //return pow(DotClamped(viewDir,refletctionDir),_Smoothness*100);//perfect mirror reflection
                float3 halfVector =normalize(lightDir+viewDir);
                //float3 specular = _SpecularTint.rgb* pow(DotClamped(i.worldNormal,halfVector),_Smoothness*100);//blinn-phong
                float3 specular = specularTint* pow(DotClamped(i.worldNormal,halfVector),_Smoothness*100);//blinn-phong
                return float4(diffuse+ specular,1);
                //return float4(i.worldNormal*0.5+0.5,1);
                //return float4(refletctionDir*0.5+0.5,1);
                //return float4(diffuse,1);
                */

                UnityLight light;
                light.color=lightColor;
                light.dir=lightDir;
                light.ndotl=DotClamped(i.worldNormal,lightDir);
                UnityIndirect lightIndirect;
                lightIndirect.diffuse=0;
                lightIndirect.specular=0;

                return UNITY_BRDF_PBS(albedo,specularTint,oneMinusReflectivity,_Smoothness,i.worldNormal,viewDir,light,lightIndirect);
            }
            ENDCG
        }
    }
}

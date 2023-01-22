Shader "Custom/DirectionalFlow_Transparent"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        // notice: not compressed
        [NoScaleOffset] _FlowMap ("FlowMap(RG,A noise,B flowSpeed)",2D) = "black" {}//not need UVOffset
        // [NoScaleOffset] _NormalMap ("Normal",2D) = "bump" {}
        //not s(RGB)
        [NoScaleOffset] _DerivHeightMap ("Derive(AG) Height(B)",2D) = "black" {}
        [Toggle(_DUAL_GRID)] _DualGrid("DualGrid",int) =0
       
        _Tiling("Tilling",float) =1
        _TilingModulated("_TilingModulated",float) =1
        _GridResolution("GridResolution",float) = 10
        _Speed("Speed",Range(0,1))=1
        _FlowStrength("Flow Strength",Range(0,1))=1
       
        _HeightScale("Height Scale Constant",float)=1
        _HeightScaleModulated("Height Scale Modulated",float)=0.75

         _WaterFogColor("Water Fog Color",Color) =(0,0,0,0)
        _WaterFogDensity("Water Fog Density",Range(0,2)) =0.1

        _RefractionStrength("Refraction Strength",Range(0,1))=0.25

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200
        GrabPass{"_WaterBackground"}
        
        CGPROGRAM
    
        #pragma surface surf Standard alpha finalcolor:ResetAlpha
       

      
        #pragma target 3.0

         #pragma shader_feature _DUAL_GRID

        #include "Flow.cginc"
        #include "LookingThroughWater.cginc"

        sampler2D _MainTex,_FlowMap,_NormalMap,_DerivHeightMap;

        struct Input
        {
            float2 uv_MainTex;
             float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Tiling,_TilingModulated,_GridResolution,_Speed,_FlowStrength;
        float _HeightScale,_HeightScaleModulated;

        float3 UnpackDerivativeHeight(float4 textureData)
        {
            float3 dh= textureData.agb;
            dh.xy = dh.xy*2 -1;
            return dh;
        }

        float3 FLowCell(float2 uv,float2 offset,float time,bool gridB)
        {
            //center  samplerCenter 
            float2 shift = 1-offset;
            shift *= 0.5;

            offset *= 0.5;

            #if defined(_DUAL_GRID)
            if (gridB)
            {
                offset+=0.25;
                shift -=0.25;
            }
            #endif

            float2x2 derivRotation;

           
            float2 uvTiled = (floor(uv*_GridResolution+offset)+shift)/_GridResolution;

             //At this point we have clearly distinguishable grid cells, 
            //each containing an undistorted pattern. 
            //The next step is to blend them. 
            //This requires us to sample multiple cells per fragment.
            float3 flow = tex2D(_FlowMap,uvTiled).rgb;
            flow.xy = flow.xy*2-1;
            flow.z *= _FlowStrength;

            //Rapidly flowing streams have many small ripples,
            // while slower regions have fewer larger ripples. 
            // factoring the flow speed into the tiling.
            float tilling = flow.z *_TilingModulated+_Tiling;

            //we can obfuscate the repetition by jittering the UV coordinates 
            //when sampling the pattern per cell. 
            //Simply adding the cell offset suffices.
            float2 uvFlow = DirectionalFlowUV(uv+offset,flow, tilling,time,derivRotation);

            float3 dh= UnpackDerivativeHeight(tex2D(_MainTex,uvFlow));
            dh.xy = mul(derivRotation,dh.xy);
            dh *= flow.z *_HeightScaleModulated+_HeightScale;

            return dh;
                                                

        }

        float3 FlowGrid(float2 uv ,float time,bool gridB)
        {
            float3 dhA = FLowCell(uv,float2(0,0),time,gridB);
            float3 dhB = FLowCell(uv,float2(1,0),time,gridB);
            float3 dhC = FLowCell(uv,float2(0,1),time,gridB);
            float3 dhD = FLowCell(uv,float2(1,1),time,gridB);
            
            //These lines are artifacts caused by the 
            //sudden jump of the UV coordinates used to sample the flow map. 
            //The suddenly large UV delta triggers the GPU to select 
            //a different mipmap level along the grid line, 
            //corrupting the flow data.
            //We can hide the lines by making sure that the cell weights are zero at their edges

            float2 t= uv*_GridResolution;
            #if defined(_DUAL_GRID)
            if (gridB)
            {
                t+=0.25;
            }
            #endif

            //triangleWave fade the edge
            t = abs(2.*frac(t)-1.);
            //bilinear interpolation
            float wA = (1-t.x)*(1-t.y);
            float wB = t.x*(1-t.y);
            float wC= (1-t.x)*t.y;
            float wD = t.x*t.y;

            return dhA*wA+dhB*wB+dhC*wC+dhD*wD;

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float time = _Time.y*_Speed;
          
            float2 uv = IN.uv_MainTex;

           float3 dh = FlowGrid(uv,time,false);
            dh = (dh +FlowGrid(uv,time,true)*0.5);

            float4 c = pow(dh.z,2)*_Color;
            c.a = _Color.a;

            o.Normal = normalize(float3(-dh.xy,1));
            o.Albedo =c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = ColorBelowWater(IN.screenPos,o.Normal)*(1-c.a);
        }

         void ResetAlpha(Input IN,SurfaceOutputStandard o,inout fixed4 color)
        {
            color.a = 1;
        }
        ENDCG
    }

}

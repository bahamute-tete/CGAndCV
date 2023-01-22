Shader "Custom/DistortionFlow"
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

        _UJump("U jump per phase", Range(-0.25,0.25)) = 0.25
        _VJump("V jump per phase", Range(-0.25,0.25)) = 0.25
        _Tiling("Tilling",float) =1
        _Speed("Speed",Range(0,1))=1
        _FlowStrength("Flow Strength",Range(0,1))=1
        _FlowOffset("Flow Offset",float)=0
        _HeightScale("Height Scale Constant",float)=1
        _HeightScaleModulated("Height Scale Modulated",float)=0.75

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
    
        #pragma surface surf Standard fullforwardshadows

      
        #pragma target 3.0

        #include "Flow.cginc"

        sampler2D _MainTex,_FlowMap,_NormalMap,_DerivHeightMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _UJump,_VJump,_Tiling,_Speed,_FlowStrength,_FlowOffset;
        float _HeightScale,_HeightScaleModulated;

        float3 UnpackDerivativeHeight(float4 textureData)
        {
            float3 dh= textureData.agb;
            dh.xy = dh.xy*2 -1;
            return dh;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //U in R , V in G , B is  scaler FlowSpeed not a vector
            float3 flowVector = tex2D(_FlowMap,IN.uv_MainTex).rgb;
            //the vector is encoded the same way as in a normal map.
            flowVector.xy = flowVector.xy*2-1;
            flowVector *= _FlowStrength;
            //use a noise map  add to  the time to distort the black
            float noise = tex2D(_FlowMap,IN.uv_MainTex).a;
            float time = _Time.y *_Speed + noise;

            float2 jump =float2(_UJump,_VJump);

            //use a diffenrent offset UV to sample the MainTex 2 times
            float3 uvwA = FlowUV(IN.uv_MainTex,flowVector.xy,jump,_FlowOffset,_Tiling,time,false);
            float3 uvwB = FlowUV(IN.uv_MainTex,flowVector.xy,jump,_FlowOffset,_Tiling,time,true);


            //The flow speed is equal to the length of the flow vector. Multiply it by the modulating scale, 
            //then add the constant scale, and use that as the final scale for the derivatives plus height.
            //but we can save the speed in B Channel
            float finalHeightScale = length(flowVector.z)*_HeightScaleModulated+_HeightScale;
            // float3 normalA = UnpackNormal(tex2D(_NormalMap,uvwA.xy))*uvwA.z;
            // float3 normalB = UnpackNormal(tex2D(_NormalMap,uvwB.xy))*uvwB.z;
            //o.Normal = normalize(normalA+normalB);
            //the correct approach would be to convert the normal vectors to height derivatives, 
            //add them, and then convert back to a normal vector.

            float3 dhA = UnpackDerivativeHeight(tex2D(_DerivHeightMap,uvwA.xy))*(uvwA.z*finalHeightScale);
            float3 dhB = UnpackDerivativeHeight(tex2D(_DerivHeightMap,uvwB.xy))*(uvwB.z*finalHeightScale);
            //rotate tangent vector to normal direction
            o.Normal = normalize(float3(-(dhA.xy+dhB.xy),1));


            fixed4 texA = tex2D (_MainTex, uvwA.xy)*uvwA.z;
            fixed4 texB = tex2D (_MainTex, uvwB.xy)*uvwB.z;
            
            //fixed4 c = (texA+texB) * _Color;
            //o.Albedo = c.rgb;
            //albedo is  Gama,we need manuall change to linerSapce
            o.Albedo = pow(dhA.z+dhB.z,2)*_Color;
            //o.Albedo = float3(flowVector,0);
           
          

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

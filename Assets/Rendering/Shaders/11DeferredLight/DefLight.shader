Shader "Custom/DefLights"
{
    Properties
    {
       
    }
    SubShader
    {
        

        Pass
        {
            //the encoded LDR colors have to be multiplied into the light buffer, instead of added.
            //HDR use add 
            //we'll have to make the blend mode variable. Unity uses _SrcBlend and _DstBlend for this.
            //for LDR  must changing the blend mode of our shader : Blend DstColor Zero.
            Blend [_SrcBlend] [_DstBlend]
           // Cull Off
            //ZTest Always
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers nomrt

            //We need shader variants for all possible light configurations. 
            //The multi_compile_lightpass compiler directive creates all keyword combinations that we need.
            #pragma multi_compile_lightpass
            #pragma multi_compile _ UNITY_HDR_ON

            #include "DefLight.cginc"
           
            ENDCG
        }
        

        // light data is logarithmically encoded. 
        //A final pass is needed to reverse this encoding. 
        //That's what the second pass is for.
        // if you disabled HDR for the camera, 
        //the second pass of our shader will also be used
          Pass
        {
   
           
            Cull Off
            ZTest Always
            ZWrite Off
            //rendering in LDR mode,  the sky turn black. 
            //This can happen in the scene view or the game view. 
            //If the sky turns black, the conversion pass doesn't correctly use the stencil buffer as a mask 
            //To fix this, explicitly configure the stencil settings of the second pass. 
            Stencil
            {
                ref [_StencilNonBackground]
                ReadMask[_StencilNonBackground]
                CompBack Equal
                CompFront Equal
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv :TEXCOORD0;
            };

            struct v2f
            {
               
                float4 vertex : SV_POSITION;
                float2 uv:TEXCOORD0;
            };

          

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            //The light buffer itself is made available to the shader via the _LightBuffer variable.
            sampler2D _LightBuffer;

            fixed4 frag (v2f i) : SV_Target
            {
               //LDR colors are logarithmically encoded
                return  -log2( tex2D(_LightBuffer,i.uv));
            }
            ENDCG
        }
    }
}

Shader "Water/Ripple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {

        
        Pass
        {
      	    HLSLPROGRAM
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
            #include "Ripplehlsl.hlsl"
			ENDHLSL
        }
    }
}

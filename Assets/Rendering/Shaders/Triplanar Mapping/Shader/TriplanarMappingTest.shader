Shader "Custom/TriplanarMappingTest"
{
    Properties
    {	
		//Usually, this is done via the tiling and offset values of a single texture,
		//but this doesn't make much sense for triplanar mapping.
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _MOSHMap("MOSH,2D",2D)="white"{}
		[NoScaleOffset] _NormalMap("Normal,2D",2D)="white"{}


		[NoScaleOffset] _TopMainTex ("Top Albedo", 2D) = "white" {}
		[NoScaleOffset] _TopMOHSMap ("Top MOHS", 2D) = "white" {}
		[NoScaleOffset] _TopNormalMap ("Top Normals", 2D) = "white" {}
		
		//ensure that not all weights become negative
		//when all three components of the normal vector are equal.
		//sqrt(1/3) =0.577 so ues 0.5
		_BlendOffset("Blend Offset",Range(0,0.5)) =0.25

		//Another way to decrease the blend region is via exponentiation, 
		//by raising the weights to some power higher than 1 before normalizing.
		_BlendExponet("Blende Exponet",Range(1,8)) =2

		_BlendHeightStrength("Blend Height Strength",Range(0,0.99))= 0.5

		_MapScale("Map Scale",float)=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

      Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma shader_feature _SEPARATE_TOP_MAPS
			#define _SEPARATE_TOP_MAPS

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#define FORWARD_BASE_PASS

			#include "MyTriplanarMapping.cginc"
			#include "My Lighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			#pragma shader_feature _SEPARATE_TOP_MAPS

			#define _SEPARATE_TOP_MAPS

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "MyTriplanarMapping.cginc"
			#include "My Lighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "Deferred"
			}

			CGPROGRAM

			#pragma target 3.0
			#pragma exclude_renderers nomrt

			#pragma multi_compile_prepassfinal
			#pragma multi_compile_instancing
			#pragma shader_feature _SEPARATE_TOP_MAPS
			#define _SEPARATE_TOP_MAPS

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#define DEFERRED_PASS

			#include "MyTriplanarMapping.cginc"
			#include "My Lighting.cginc"

			ENDCG
		}


		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#include "My Shadows.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "Meta"
			}

			Cull Off

			CGPROGRAM

			#pragma vertex MyLightmappingVertexProgram
			#pragma fragment MyLightmappingFragmentProgram

			#pragma shader_feature _SEPARATE_TOP_MAPS

			#define META_PASS_NEEDS_NORMALS
			#define META_PASS_NEEDS_POSITION

			#include "MyTriplanarMapping.cginc"
			#include "My Lightmapping.cginc"

			ENDCG
		}
	}
}


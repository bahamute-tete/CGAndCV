#if !defined(MY_TRIPLANAR_MAPPING_INCLUDED)
#define MY_TRIPLANAR_MAPPING_INCLUDED

#define NO_DEFAULT_UV

#include "MySurface.cginc"
#include "My Lighting Input.cginc"

float _MapScale;
float _BlendOffset,_BlendExponent,_BlendHeightStrength;

sampler2D _TopMainTex, _TopMOHSMap, _TopNormalMap;

struct TriplanarUV
{
	float2 x,y,z;
};

TriplanarUV GetTriplanarUV (SurfaceParameters parameters)
{
	TriplanarUV triUV;
	float3 p = parameters.position *_MapScale;
	triUV.x = p.zy;
	triUV.y = p.xz;
	triUV.z = p.xy;

   //negating the U coordinate mirror problem 
	if (parameters.normal.x<0)
	{
		triUV.x.x =-triUV.x.x;
	}

	if (parameters.normal.y<0)
	{
		triUV.y.x =-triUV.y.x;
	}

	if (parameters.normal.z>=0)
	{
		triUV.z.x =-triUV.z.x;
	}

	//liminate such repetitions by offsetting the projections
	triUV.x.y += 0.5;
	triUV.z.x += 0.5;
	return triUV;
}

float3 GetTriplanarWeights(SurfaceParameters parameters,float heightX,float heightY,float heightZ)
{	
	 //use the normal to define the weights of all three projections. 
	 //We have to use the absolute of the normal vector, because a surface can face a negative direction.
	 // the total of the weights has to sum to 1, 
	 //normalize them via division by their total. 
	float3 triW = abs(parameters.normal);

	//Blend offset property to make this possible
	//If subtract the same amount from all weights, 
	//then smaller weights are affected more than larger weights, 
	//which changes their relative importance.
	triW = saturate(triW - _BlendOffset);
	triW  *= lerp(1,float3(heightX, heightY, heightZ),_BlendHeightStrength);
	//triW = pow(triW, _BlendExponent);

	return triW /(triW.x+triW.y+triW.z);
}

float3 BlendTriplanarNormal(float3 mappedNormal,float3 surfaceNormal)
{
	//using whiteout blending
	//It's described in Rendering 6, Bumpiness.
	float3 n;
	n.xy = mappedNormal.xy+surfaceNormal.xy;
	n.z = mappedNormal.z*surfaceNormal.z;
	return n;
}
sampler2D _MOSHMap;
void MyTriPlanarSurfaceFunction (
	inout SurfaceData surface, SurfaceParameters parameters
) {

	 TriplanarUV triUV = GetTriplanarUV (parameters);
	 float3 albedoZ= tex2D(_MainTex,triUV.z).rgb;
	 float3 albedoY = tex2D(_MainTex,triUV.y).rgb;
	 //To keep the orientation as expected, we have to use ZY instead. rotated 90
	 float3 albedoX = tex2D(_MainTex,triUV.x).rgb;

	float4 mohsX = tex2D(_MOSHMap,triUV.x);
	float4 mohsY = tex2D(_MOSHMap,triUV.y);
	float4 mohsZ = tex2D(_MOSHMap,triUV.z);

	float3 tangentNormalX = UnpackNormal(tex2D(_NormalMap,triUV.x));
	float4 rawNormalY = tex2D(_NormalMap, triUV.y);
	float3 tangentNormalZ = UnpackNormal(tex2D(_NormalMap, triUV.z));

	#if defined(_SEPARATE_TOP_MAPS)
		if (parameters.normal.y>0)
		{
			albedoY = tex2D(_TopMainTex,triUV.y).rgb;
			mohsY =tex2D(_TopMOHSMap, triUV.y);
			rawNormalY = tex2D(_TopNormalMap, triUV.y);
			
		}
	#endif

	float3 tangentNormalY = UnpackNormal(rawNormalY);


	//prevent normal mirrorin
	////if we not use whiteout Blend methord
	// flip the normal's up direction, as they're pointing inwards.
	if (parameters.normal.x < 0) {
		tangentNormalX.x = -tangentNormalX.x;
		//we end up multiplying two negative Z values, flipping the sign of the final Z
		// equivalent to not negating the sampled Z components to begin with
		//tangentNormalX.z = -tangentNormalX.z;
	}
	if (parameters.normal.y < 0) {
		tangentNormalY.x = -tangentNormalY.x;
		//we end up multiplying two negative Z values, flipping the sign of the final Z
		// equivalent to not negating the sampled Z components to begin with
		//tangentNormalY.z = -tangentNormalY.z;
	}
	if (parameters.normal.z >= 0) {
		tangentNormalZ.x = -tangentNormalZ.x;

	}
	////we end up multiplying two negative Z values, flipping the sign of the final Z
	// equivalent to not negating the sampled Z components to begin with
	// else
	// {
	// 	tangentNormalZ.z = -tangentNormalZ.z;
	// }
	//The tangent space normals are stored with their local up direction in the Z channel
	//in the case of the Y projection, the up direction corresponds to Y, not Z
	//in the case of the X projection, the up direction corresponds to X, not Z
	//we have to swap components
	// float3 worldNormalX = tangentNormalX.zyx;
	// float3 worldNormalY = tangentNormalY.xzy;
	// float3 worldNormalZ = tangentNormalZ;

	//Whiteout blending assumes Z is pointing up, 
	//convert the surface normal to the projected space
	//we end up multiplying two negative Z values, flipping the sign of the final Z
	// equivalent to not negating the sampled Z components to begin with
	float3 worldNormalX =BlendTriplanarNormal(tangentNormalX,parameters.normal.zyx).zyx;
	float3 worldNormalY =BlendTriplanarNormal(tangentNormalY,parameters.normal.xzy).xzy;
	float3 worldNormalZ =BlendTriplanarNormal(tangentNormalZ,parameters.normal);
	

	float3 triW = GetTriplanarWeights(parameters,mohsX.z, mohsY.z, mohsZ.z);
	surface.normal = normalize(worldNormalX * triW.x + worldNormalY * triW.y + worldNormalZ * triW.z);

	//surface.albedo = surface.normal * 0.5 + 0.5;
	
	 surface.albedo = albedoX*triW.x+albedoY*triW.y+albedoZ*triW.z;

	float4 mohs = mohsX * triW.x + mohsY * triW.y + mohsZ * triW.z;
	 surface.metallic = mohs.x;
	 surface.occlusion = mohs.y;
	 surface.smoothness = mohs.a;


	

}

#define SURFACE_FUNCTION MyTriPlanarSurfaceFunction

#endif
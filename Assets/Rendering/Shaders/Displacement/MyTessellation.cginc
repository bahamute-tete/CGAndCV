#if !defined(TESSELLATION_INCLUDED)
#define TESSELLATION_INCLUDED

float _TessellationUniform;
float _TessellationEdgeLength;

struct TessellationControlPoint {
	float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;

	//Shadow not needed so many vertexdata so Make macros  
	#if TESSELLATION_TANGENT
	float4 tangent : TANGENT;
	#endif
	float2 uv : TEXCOORD0;
	#if TESSELLATION_UV1
	float2 uv1 : TEXCOORD1;
	#endif
	#if TESSELLATION_UV2
	float2 uv2 : TEXCOORD2;
	#endif
};

struct TessellationFactors {
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

TessellationControlPoint MyTessellationVertexProgram (VertexData v) {
	TessellationControlPoint p;
	p.vertex = v.vertex;
	p.normal = v.normal;
	//Shadow not needed so many vertexdata so Make macros  
	#if TESSELLATION_TANGENT
	p.tangent = v.tangent;
	#endif
	p.uv = v.uv;
	#if TESSELLATION_UV1
	p.uv1 = v.uv1;
	#endif
	#if TESSELLATION_UV2
	p.uv2 = v.uv2;
	#endif
	return p;
}

float TessellationEdgeFactor (float3 p0, float3 p1) {
	#if defined(_TESSELLATION_EDGE)
		float edgeLength = distance(p0, p1);

		float3 edgeCenter = (p0 + p1) * 0.5;
		float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);

		return edgeLength * _ScreenParams.y /
			(_TessellationEdgeLength * viewDistance);
	#else
		return _TessellationUniform;
	#endif
}

bool TriangleIsBelowClipPlane(float3 p0,float3 p1,float3 p2,int planeIndex,float bias)
{

	// These planes form a system where the space 
	//inside the frustum is considered to lie above all six clip planes.

	// clip planes of the camera are made available via the unity_CameraWorldClipPlanes 
	//left, right, bottom, top, near, far planes
	float4 plane= unity_CameraWorldClipPlanes[planeIndex];

	//make the maximum displacement into consideration when determining 
	//whether a triangle lies below a clip plane or not. 
	//This can be done by adding a bias 
	return  dot(float4(p0,1),plane)< bias&&
			dot(float4(p1,1),plane)< bias &&
			dot(float4(p2,1),plane)< bias ;
}

//GPU frustum culling
bool TriangleIsCulled(float3 p0,float3 p1,float3 p2,float bias)
{

	 //it's not worth the extra effort to check the near plane and The far plane
	return  TriangleIsBelowClipPlane(p0,p1,p2,0,bias) ||
			 TriangleIsBelowClipPlane(p0,p1,p2,1,bias) ||
			  TriangleIsBelowClipPlane(p0,p1,p2,2,bias) ||
			   TriangleIsBelowClipPlane(p0,p1,p2,3,bias);
}

TessellationFactors MyPatchConstantFunction (
	InputPatch<TessellationControlPoint, 3> patch
) {
	float3 p0 = mul(unity_ObjectToWorld, patch[0].vertex).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patch[1].vertex).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patch[2].vertex).xyz;
	TessellationFactors f;
	//A factor of 1 corresponds with no triangles being added.
	// A higher factor results in more triangles. 
	//But it is also possible to use the factor 0. 
	//When one of the tessellation factors is zero,
	// the original triangle is discarded and doesn't get rendered at all.

	float bias =0;
	#if VERTEX_DISPLACEMENT
		bias = -0.5 *_DisplacementStrength;
	#endif
	 
    if (TriangleIsCulled(p0,p1,p2,bias))
	{	
		f.edge[0] =f.edge[1]=f.edge[2]=f.inside =0;
	}
	else
	{
		f.edge[0] = TessellationEdgeFactor(p1, p2);
		f.edge[1] = TessellationEdgeFactor(p2, p0);
		f.edge[2] = TessellationEdgeFactor(p0, p1);
		f.inside =
					(TessellationEdgeFactor(p1, p2) +
					TessellationEdgeFactor(p2, p0) +
					TessellationEdgeFactor(p0, p1)) * (1 / 3.0);
	}
 
	return f;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("fractional_odd")]
[UNITY_patchconstantfunc("MyPatchConstantFunction")]
TessellationControlPoint MyHullProgram (
	InputPatch<TessellationControlPoint, 3> patch,
	uint id : SV_OutputControlPointID
) {
	return patch[id];
}

[UNITY_domain("tri")]
InterpolatorsVertex MyDomainProgram (
	TessellationFactors factors,
	OutputPatch<TessellationControlPoint, 3> patch,
	float3 barycentricCoordinates : SV_DomainLocation
) {
	VertexData data;

	#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) data.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

	MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
	MY_DOMAIN_PROGRAM_INTERPOLATE(normal)

	#if TESSELLATION_TANGENT
	MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)
	#endif
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv)
	#if TESSELLATION_UV1
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv1)
	#endif
	#if TESSELLATION_UV2
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv2)
	#endif

	return MyVertexProgram(data);
}

#endif
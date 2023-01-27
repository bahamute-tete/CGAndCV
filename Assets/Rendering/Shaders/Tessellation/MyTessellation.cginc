#if !defined(TESSELETATION_INCLUDED)
#define TESSELETATION_INCLUDED

//if we specified a new vertex shader
//It will produce yet another compiler error, 
//complaining about a reuse of the position semantic.
//we have to use an alternative output struct for our vertex program, 
//which uses the INTERNALTESSPOS semantic for the vertex position. 
struct TessellationControlPoint
{
    float4  vertex:INTERNALTESSPOS;
    float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;

};

//GPU uses four tessellation factors.
struct TessellationFactors
{
    ////Each edge of the triangle patch gets a factor
    float edge[3] :SV_TessFactor;
    //A factor for the inside of the triangle
    float inside: SV_InsideTessFactor;

};


// we still have to specify a vertex program 

TessellationControlPoint MyTessellationVertexProgram (VertexData v)
{
    TessellationControlPoint p;
    p.vertex = v.vertex;
	p.normal = v.normal;
	p.tangent = v.tangent;
	p.uv = v.uv;
	p.uv1 = v.uv1;
	p.uv2 = v.uv2;
    return p;
}


float _TessellationUniform;
float _TessellationEdgeLength;
// float _TessellationEdgeLength_W;
// float _TessellationEdgeLength_S;

// float TessellationEdgeFactor1(TessellationControlPoint cp0,TessellationControlPoint cp1)
// {

//     #if defined(_TESSELLATION_EDGE_W)
//         float3 p0 = mul(unity_ObjectToWorld,float4(cp0.vertex.xyz,1)).xyz;
//         float3 p1 = mul(unity_ObjectToWorld,float4(cp1.vertex.xyz,1)).xyz;

//         float edgeLength = distance(p0,p1);
//         return edgeLength/_TessellationEdgeLength_W;

//     #elif defined(_TESSELLATION_EDGE_S)
//         float4 p0 = UnityObjectToClipPos(cp0.vertex);
//         float4 p1 = UnityObjectToClipPos(cp1.vertex);

//         float edgeLength = distance(p0.xy/p0.w,p1.xy/p1.w);
//         return edgeLength* _ScreenParams.y/_TessellationEdgeLength_S;
        

//     #elif defined(_TESSELLATION_EDGE_VIEW)
//         float3 p0 = mul(unity_ObjectToWorld,float4(cp0.vertex.xyz,1)).xyz;
//         float3 p1 = mul(unity_ObjectToWorld,float4(cp1.vertex.xyz,1)).xyz;

//         float edgeLength = distance(p0,p1);

//         float edgeCenter = (p0+p1)*0.5;
//         float viewDistance = distance(edgeCenter,_WorldSpaceCameraPos);
//         return edgeLength * _ScreenParams.y /(_TessellationEdgeLength_S*viewDistance);

//     #else

//         return _TessellationUniform;

//     #endif
// }

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

TessellationFactors MyPatchConstantFunction (InputPatch<TessellationControlPoint,3> patch)
{   

    float3 p0 = mul(unity_ObjectToWorld, patch[0].vertex).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patch[1].vertex).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patch[2].vertex).xyz;

    // 1 will instruct the tessellation stage to not subdivide the patch.
    TessellationFactors f;

    // //      1
    // //
    // // 0---------2

    // f.edge[0] =TessellationEdgeFactor(patch[1],patch[2]);
    // f.edge[1] =TessellationEdgeFactor(patch[2],patch[0]);
    // f.edge[2] =TessellationEdgeFactor(patch[0],patch[1]);


    // // f.edge[0] =TessellationEdgeFactor(patch[0],patch[1]);
    // // f.edge[1] =TessellationEdgeFactor(patch[1],patch[2]);
    // // f.edge[2] =TessellationEdgeFactor(patch[2],patch[0]);

    // f.inside =(f.edge[0]+f.edge[1]+f.edge[2])*(1./3.);


    f.edge[0] = TessellationEdgeFactor(p1, p2);
    f.edge[1] = TessellationEdgeFactor(p2, p0);
    f.edge[2] = TessellationEdgeFactor(p0, p1);
	f.inside =
		(TessellationEdgeFactor(p1, p2) +
		TessellationEdgeFactor(p2, p0) +
		TessellationEdgeFactor(p0, p1)) * (1 / 3.0);

    return f;

}



//InputPatch<VertexData,3>:
//stream parameter 
//specify the data format of the vertices.
//each patch will contain three vertices

//uint id :SV_OutputControlPointID===>specifies which control point (vertex) it should work with

//The job of the hull program is to pass the required vertex data to the tessellation stage.
//Although it is fed an entire patch, the function should output only a single vertex at a time

//explicitly tell it that it's working with triangles
[UNITY_domain("tri")]
//explicitly specify that we're outputting three control points per patch, 
//one for each of the triangle's corners.
[UNITY_outputcontrolpoints(3)]
//it needs to know whether we want them defined clockwise or counterclockwise.
[UNITY_outputtopology("triangle_cw")]
//GPU also needs to be told how it should cut up the patch
//interger make tesselation not smooth,step by step
//[UNITY_partitioning("integer")]
//odd factor will inset a  triangle even factor will inset a center vertex
//When using a whole odd factor, the fractional_odd partitioning mode produces the same results as the integer mode. 
//But when transitioning between odd factors, 
//extra edge subdivisions will be split off and grow, or shrink and merge.
[UNITY_partitioning("fractional_odd")]
//GPU also has to know into how many parts the patch should be cut.
//but This isn't a constant value
//We have to provide a function to evaluate this
//the patch constant function is only invoked once per patch, not once per control point.
[UNITY_patchconstantfunc("MyPatchConstantFunction")]
TessellationControlPoint MyHullProgram(InputPatch<TessellationControlPoint,3> patch,uint id :SV_OutputControlPointID)
{
    return patch[id];

}

//The hull shader is only part of what we need to get tessellation working.
//Once the tessellation stage has determined how the patch should be subdivided, 
//it's up to the geometry shader to evaluate the result and generate the vertices of the final triangles


//tessellation stage determines how the patch should be subdivided, it doesn't generated any new vertices. 
//Instead, it comes up with barycentric coordinates for those vertices
[UNITY_domain("tri")]
InterpolatorsVertex MyDomainProgram( TessellationFactors factors,OutputPatch<TessellationControlPoint,3> patch,
                    float3 barycentricCoord :SV_DomainLocation)
{
    VertexData data;

    #define MY_DOMAIN_PROGRAM_INTERPOLATE(filedName) data.filedName = \
        patch[0].filedName * barycentricCoord.x+ \
        patch[1].filedName * barycentricCoord.y+ \
        patch[2].filedName * barycentricCoord.z;

    MY_DOMAIN_PROGRAM_INTERPOLATE(vertex);
    MY_DOMAIN_PROGRAM_INTERPOLATE(normal);
    MY_DOMAIN_PROGRAM_INTERPOLATE(tangent);
    MY_DOMAIN_PROGRAM_INTERPOLATE(uv);
    MY_DOMAIN_PROGRAM_INTERPOLATE(uv1);
    MY_DOMAIN_PROGRAM_INTERPOLATE(uv2);
    
    //We now have a new vertex, 
    //which will be send to either the geometry program or the interpolator after this stage. 
    //But these programs need InterpolatorsVertex data,not VertexData
    //To solve this, we have the domain shader take over the responsibilities of the original vertex program. 
    //This is done by invoking MyVertexProgram inside it
    return  MyVertexProgram(data);
}
#endif
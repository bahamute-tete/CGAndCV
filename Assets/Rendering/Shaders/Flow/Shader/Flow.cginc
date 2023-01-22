#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED



float3 FlowUV(float2 uv, float2 flowVector,float2 jump ,float flowOffset,float tiling,float time,bool flowB)
{  
    //need two pulsing patterns
    //shifting the phase of B by half its period
    //all weight needs to be 1.0
    float phaseOffset = flowB ? 0.5:0;
    float progress = frac(time + phaseOffset);
    

    // we need fade in black 
    //use another componet to control the progress
    //use function in Desmos
    float3 uvw;
    uvw.xy = uv - flowVector*(progress+flowOffset);
    uvw.xy *= tiling;
    uvw.xy +=phaseOffset;
    //slide the UV coordinates based on time
    //avoid visual sliding by keeping the UV offset constant during each phase, 
    //and jumping to a new offset between phases.use (time-progress)get intger part;
    uvw.xy +=(time-progress)* jump;
    uvw.z =1 - abs(1-2*progress);

    return uvw;
}

float2 DirectionalFlowUV(float2 uv, float3 flowVectorAndSpeed,float tiling,float time,out float2x2 rotation)
{  
    float2 dir = normalize(flowVectorAndSpeed.xy);
    //clockwise direction
    rotation = float2x2(dir.y,dir.x,-dir.x,dir.y);
    //because rotate the UV  ther vector is towards the opposite dir ,so we need to countclockwise
    uv = mul(float2x2(dir.y,-dir.x,dir.x,dir.y),uv);
    uv.y -= time*flowVectorAndSpeed.z;
    return uv*tiling;
}

#endif
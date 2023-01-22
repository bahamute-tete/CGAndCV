#if !defined(LOOKING_THROUGH_WATER_INCLUDED)
#define LOOKING_THROUGH_WATER_INCLUDED

sampler2D _CameraDepthTexture,_WaterBackground;
float4 _CameraDepthTexture_TexelSize;

float3 _WaterFogColor;
float _WaterFogDensity;
float _RefractionStrength;

float2 AlignWithGrabTexel(float2 uv)
{
    #if UNITY_UV_STARTS_AT_TOP
        if (_CameraDepthTexture_TexelSize.y<0)
        {
            uv.y =1-uv.y;
        }
    #endif
    //multiplying the UV by the texture size, discarding the fractions, 
    //offsetting to the texel center, and then dividing by the texture size.
    return
    (floor(uv*_CameraDepthTexture_TexelSize.zw)+0.5)*abs(_CameraDepthTexture_TexelSize.xy);
}

float3 ColorBelowWater(float4 screenPos,float3 tangentSpaceNormal)
{   
    float2 uvOffset = tangentSpaceNormal.xy*_RefractionStrength;

    uvOffset.y *= _CameraDepthTexture_TexelSize.z/_CameraDepthTexture_TexelSize.w;

    //The screen position is simply the clip space position, 
    //with the range of its XY components changed from ?1¨C1 to 0¨C1.
    //we're dealing with homogeneous coordinates.
    //we have to divide XY by W to get the final depth texture coordinates. 
    float2 uv =AlignWithGrabTexel((screenPos.xy+uvOffset)/screenPos.w);
    

    //convert the raw value to the linear depth 
    float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));

    //We find it by taking the Z component of screenPos
    //which is the interpolated clip space depth
    //and converting it to linear depth via the UNITY_Z_0_FAR_FROM_CLIPSPACE macro.
    float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);

    //underwater depth is found by subtracting the surface depth from the background depth.
    float depthDifference = backgroundDepth - surfaceDepth;

    //detect hit the foreground by checking whether the depth difference 
    //that  the fog is negative
    // if (depthDifference <0)
    // {   
    //     // if  foreground  us no offset uv 
    //     uv =AlignWithGrabTexel(screenPos.xy/screenPos.w);
    //     //sample the depth again, with the reset UV, before determining the fog factor
    //     backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));
    //     depthDifference = backgroundDepth - surfaceDepth;
    // }

 
        uvOffset *= saturate(depthDifference);
        uv =AlignWithGrabTexel((screenPos.xy+uvOffset)/screenPos.w);
        //sample the depth again, with the reset UV, before determining the fog factor
        backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));
        depthDifference = backgroundDepth - surfaceDepth;
    


    float3 backgroundColor = tex2D(_WaterBackground,uv).rgb;

    // f = 1/(2^cd) ==>f = 2^-cd  where dis the fog's density
    float fogFactor = exp2(-_WaterFogDensity*depthDifference);
    return lerp(_WaterFogColor,backgroundColor,fogFactor);
}

#endif
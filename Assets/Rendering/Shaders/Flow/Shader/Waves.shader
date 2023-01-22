Shader "Custom/Waves"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        //_Amplitude ("Amplitude",float) = 1
        // _Steepness ("Steepness",Range(0,1)) = 0.5
        // _Wavelength ("Wavelength",float) = 10
        // _Direction ("Direction(2D)",vector) = (1,0,0,0)
       
        _WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
        _WaveB("Wave B",vector) =(0,1,0.25,20)
        _WaveC("Wave C",vector) =(1,1,0.15,10)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull OFF

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
       

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // float _Steepness,_Wavelength;
        // float2 _Direction;

        float4 _WaveA,_WaveB,_WaveC;
        

        float3 GerstnerWave(float4 wave,float3 p, inout float3 tangent,inout float3 bitangent)
        {

            float steepness = wave.z;
		    float wavelength = wave.w;

            float  k  = 2* UNITY_PI /wavelength;

            //In reality, waves don't have an arbitrary phase speed. 
            //It is related to the wave number
            //c = sqrt(g* (2pi/wavelength))

            //if wanted loop animation speed needs to be power of 2 of  wavelength
            // eg:c1=sqrt(g* (2pi/64))==> c1 = 8*sqrt(g/(2pi))
            // c2=sqrt(g* (2pi/16)==> c2 = 4*sqrt(g/(2pi))
            //so c1==2c2
            float c = sqrt(9.8/k);

            //introduce the direction vector [dx,dz]
            float2 d = normalize(wave.xy);  

            //f = k(D*p-ct) where D ==[Dx,Dz]and P[Px,Pz] is vector
            //partial derivative  in x  ===>f`(x) =kDx
            //partial derivative  in z ===>f`(z) =kDz
            float f = k * (dot(d,p.xz)-c*_Time.y);

            float a = steepness/k;


            //sine wave into a circle by using float2(a*cos(f),a*sin(f))
            // but that would collapse the entire plane into a single circle. 
            //Instead, we have to anchor each point on its original X coordinate
            //so we need  float2(x+a*cos(f),a*sin(f))

            //chain rule 
            // dirivtive f =  sin(ax) ==> f` = a*cos(x)
            // f=cos(ax)==>f`= a*sin(x)
            //f = x+a*cos(f) ==>f`= 1-asin(f);
            //float3 tangent = normalize(float3(1-_Steepness*sin(f),_Steepness*cos(f),0));

            //because first tangent and bitangent initial values are set,
            //so eliminating 1
            tangent += float3(
                -d.x*d.x*(steepness*sin(f)),
                 d.x*(steepness*cos(f)),
                -d.x*d.y*(steepness*sin(f))
                );
            //because unit vector  roles of the X and Z component are swapped
             bitangent += float3(
                -d.x*d.y*(steepness*sin(f)),
                d.y*(steepness*cos(f)),
                -d.y*d.y*(steepness*sin(f))
                );
            
            //adjust the horizontal offsets of px,pz
            // p.x += d.x * a * cos(f);
            // p.y = a*sin(f);
            // p.z += d.y * a * cos(f);
            return float3(d.x * a * cos(f),a*sin(f), d.y * a * cos(f));
        }

 
        void vert(inout appdata_full vertexData)
        {
            float3  gridPoint  = vertexData.vertex.xyz;
            float3 tangent = float3(1,0,0);
            float3 bitangent = float3(0,0,1);
            float3 p= gridPoint;
            p+=GerstnerWave(_WaveA,gridPoint,tangent,bitangent);
            p+=GerstnerWave(_WaveB,gridPoint,tangent,bitangent);
            p+=GerstnerWave(_WaveC,gridPoint,tangent,bitangent);

            //float3 normal = float3(-tangent.y,tangent.x,0);
            float3 normal = normalize(cross(bitangent,tangent));
          
            vertexData.vertex.xyz= p;
            vertexData.normal = normal;

        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
           
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
          
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public static partial class Noise
{

    public interface IVoronoiDistance
    {
        float4 GetDistance(float4 x);
        float4 GetDistance(float4 x,float4 y);
        float4 GetDistance(float4 x,float4 y,float4 z);

        float4x2 Finalize1D(float4x2 minima);
        float4x2 Finalize2D(float4x2 minima);
        float4x2 Finalize3D(float4x2 minima);


    }

    public interface IVoronoiFunction
    {
        float4 Evaluate(float4x2 minma);
    }


    public struct F1 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 distances)
        {
            return distances.c0;
        }
    }


    public struct F2 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 distances)
        {
            return distances.c1;
        }
    }

    public struct F2MinusF1 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 distances)
        {
            return distances.c1-distances.c0;
        }
    }


    public struct Worley : IVoronoiDistance
    {
        public float4 GetDistance(float4 x) => abs(x);
        public float4 GetDistance(float4 x,float4 y) => sqrt( x*x+y*y);
        public float4 GetDistance(float4 x,float4 y,float4 z) => sqrt(x * x + y * y+ z * z);

        public float4x2 Finalize1D(float4x2 minima) => minima;

        public float4x2 Finalize2D(float4x2 minima)
        {
            minima.c0 = min(minima.c0, 1f);
            minima.c1 = min(minima.c1, 1f);
            return minima;
        }
        public float4x2 Finalize3D(float4x2 minima) => Finalize2D(minima);
    }


    public struct Chebyshev : IVoronoiDistance
    {
        public float4 GetDistance(float4 x) => abs(x);
        public float4 GetDistance(float4 x, float4 y) => max(abs(x),abs(y));
        public float4 GetDistance(float4 x, float4 y, float4 z) => max (max(abs(x), abs(y)),abs(z));

        public float4x2 Finalize1D(float4x2 minima) => minima;

        public float4x2 Finalize2D(float4x2 minima) => minima;

        public float4x2 Finalize3D(float4x2 minima) => minima;
    }


    public struct Voronoi1D<L,D,F> : INoise where L : struct, ILattice
                                            where D:struct,IVoronoiDistance
                                            where F : struct, IVoronoiFunction
    {
        
        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            var d = default(D);
            LatticeSpan4 x = l.GetLatticeSpan4(positions.c0, frequency);

            float4x2 minma = 2f;

            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 h = hash.Eat(l.ValidateSingleStep(x.p0+u,frequency));
                minma = UpdateVoronoiMinima(minma, d.GetDistance(h.Floats01A + u - x.g0));
            }

            
            return default(F).Evaluate(d.Finalize1D( minma));
        }

    }

   


    public struct Voronoi2D<L,D,F> : INoise where L : struct, ILattice
                                            where D : struct, IVoronoiDistance
                                            where F : struct, IVoronoiFunction
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            var d = default(D);

            LatticeSpan4 x = l.GetLatticeSpan4(positions.c0, frequency);
            LatticeSpan4 z = l.GetLatticeSpan4(positions.c2, frequency);

            float4x2 minma = 2f;

            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 hx = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
                float4 xOffset = u - x.g0;

                for (int v = -1; v <= 1; v++)
                {
                    SmallXXHash4 h = hx.Eat(l.ValidateSingleStep(z.p0 + v, frequency));
                    float4 zOffset = v - z.g0;

                    minma = UpdateVoronoiMinima(minma, d.GetDistance(h.Floats01A+xOffset,h.Floats01B+zOffset));

                    minma = UpdateVoronoiMinima(minma, d.GetDistance(h.Floats01C + xOffset, h.Floats01D+ zOffset));
                }
            }
           
            return default(F).Evaluate(d.Finalize2D(minma));
        }
    }


    public struct Voronoi3D<L,D,F> : INoise where L : struct, ILattice
                                            where D : struct, IVoronoiDistance
                                            where F : struct, IVoronoiFunction
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            var d = default(D);
            LatticeSpan4 x = l.GetLatticeSpan4(positions.c0, frequency);
            LatticeSpan4 y = l.GetLatticeSpan4(positions.c1, frequency);
            LatticeSpan4 z = l.GetLatticeSpan4(positions.c2, frequency);


            float4x2 minma = 2f;

            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 hx = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
                float4 xOffset = u - x.g0;

                for (int v = -1; v <= 1; v++)
                {
                    SmallXXHash4 hy = hx.Eat(l.ValidateSingleStep(y.p0 + v, frequency));
                    float4 yOffset = v - y.g0;

                    for (int w = -1; w <= 1; w++)
                    {
                        SmallXXHash4 h = hy.Eat(l.ValidateSingleStep(z.p0 + w, frequency));
                        float4 zOffset = w - z.g0;

                        minma = UpdateVoronoiMinima(minma, d.GetDistance( h.GetBitsAsFloats01(5,0) + xOffset, 
                                                                        h.GetBitsAsFloats01(5,5) + yOffset,
                                                                        h.GetBitsAsFloats01(5,10) + zOffset));

                        minma = UpdateVoronoiMinima(minma, d.GetDistance( h.GetBitsAsFloats01(5, 15) + xOffset,
                                                                        h.GetBitsAsFloats01(5, 20) + yOffset,
                                                                        h.GetBitsAsFloats01(5, 25) + zOffset));
                    }
                  


                }
            }

            return default(F).Evaluate(d.Finalize3D(minma));

        }
    }

    

    static float4x2 UpdateVoronoiMinima(float4x2 minima, float4 distance)
    {
        bool4 newMinimum = distance < minima.c0;
        minima.c1 = select( select(minima.c1,distance,distance<minima.c1), minima.c0, newMinimum);
        minima.c0 = select(minima.c0, distance, newMinimum);

        return minima;
    }
}

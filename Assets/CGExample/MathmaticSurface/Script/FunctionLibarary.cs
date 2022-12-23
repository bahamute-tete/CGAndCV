using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{

    const float pi = 3.1415926f;

    public delegate Vector3 Function(float u, float v, float t);
    static Function[] functions = { Wave, MultiWave, Ripple, PerturbingSphere, Tours };
    public enum FunctionName { Wave, MultiWave, Ripple, PerturbingSphere, Tours };

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];
    }

    public static FunctionName GetNextFunction(FunctionName name)
    {
        if ((int)name < functions.Length - 1)
        {
            return name + 1;
        }
        else
        {
            return 0;
        }

    }

    public static FunctionName GetRandomFunction(FunctionName name)
    {
        var fName = (FunctionName)Random.Range(0, functions.Length);
        return (fName == name) ? 0 : fName;
    }

    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(pi * (u + v + t));
        p.z = v;
        return p;
    }

    public static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(pi * (u + 0.5f * t));
        p.y += 0.5f * Sin(2f * pi * (v + 0.8f* t));
        p.y += Sin(pi * (u + v + 0.25f * t));
        p.y *= 1f / 2.5f;
        p.z = v;
        return p;
    }

    public static Vector3 Ripple(float u, float v, float t)
    {
        Vector3 p;
        float d = Sqrt(u * u + v * v);
        p.x = u;
        p.y = Sin(pi * (6f * d - 2f*t)) / (1 + 10 * d);
        p.z = v;
        return p;
    }

    private static Vector3 Sphere(float u, float v, float r)
    {
        Vector3 p;
        p.x = r * Cos(0.5f * pi * v) * Sin(pi * u);
        p.y = r * Sin(0.5f * pi * v);
        p.z = r * Cos(0.5f * pi * v) * Cos(pi * u);
        return p;
    }

    public static Vector3 PerturbingSphere(float u, float v, float t)
    {
        Vector3 p;

        float r = 0.9f + 0.1f * Sin(pi * (8f * u + 12f * v + t));
        p = Sphere(u, v, r);
        return p;
    }

    public static Vector3 Tours(float u, float v, float t)
    {
        Vector3 p;

        //float r1=1f;
        //float r2=0.25f;
        float r1 = 0.7f + 0.1f * Sin(PI * (16f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));

        float r = r2 * Cos(pi * v) + r1;

        p.x = r * Sin(pi * u);
        p.y = r2 * Sin(pi * v);
        p.z = r * Cos(pi * u);

        return p;
    }

    public static Vector3 Morph(float u, float v, float t, Function from, Function to, float progress)
    {
        Vector3 res;
        res = Vector3.LerpUnclamped(from(u, v, t), to(u, v, t), SmoothStep(0, 1, progress));
        return res;
    }


    public static int FunctionCount => functions.Length;

}

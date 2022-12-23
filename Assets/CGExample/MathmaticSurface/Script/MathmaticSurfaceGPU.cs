using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;





public class MathmaticSurfaceGPU : MonoBehaviour
{
    

    [SerializeField,Range(10,1000)] int resolution = 10;
    
    [SerializeField] FunctionLibrary.FunctionName functionName;
    FunctionLibrary.FunctionName functionNameFrom;

    [SerializeField, Min(0f)] float functionDuration = 1.0f, transFunDuration = 1.0f;

    bool isTrans=false;

   
    float duration;
    FunctionLibrary.Function f;
    public enum TransMode {Cycle,Random }
    [SerializeField] TransMode mode;

    ComputeBuffer positionBuffer;
    [SerializeField] ComputeShader computeShader;
    static readonly int
        positionsID = Shader.PropertyToID("_Positions"),
        resolutionID = Shader.PropertyToID("_Resolution"),
        stepID = Shader.PropertyToID("_Step"),
        timeID = Shader.PropertyToID("_Ftime"),
        transitionProgessID = Shader.PropertyToID("_TransitionProgress");

    [SerializeField] Material material;
    [SerializeField] Mesh mesh;

    void OnEnable()
    {
        positionBuffer = new ComputeBuffer( resolution*resolution,3*4);//vector3 have 3 floats so we need 3*4 byte
        
    }

    private void OnDisable()
    {
        positionBuffer.Release();
        positionBuffer = null;
    }

    // Update is called once per frame
    void Update()
    {
        duration += Time.deltaTime;
        if (isTrans)
        {
            if (duration >= functionDuration)
            {
                duration -= functionDuration;
                isTrans = false;
            }
        }
        else
        {
            if (duration >= functionDuration)
            {
                duration -= functionDuration;
                isTrans = true;
                functionNameFrom = functionName;
                PickFuntion();
            }
        }


        UpdateFunctionOnGPU();



    }

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionID, resolution);
        computeShader.SetFloat(stepID, step);
        computeShader.SetFloat(timeID, Time.time);

        if (isTrans)
        {
            float t = Mathf.SmoothStep(0, 1f, duration / transFunDuration);
            computeShader.SetFloat(transitionProgessID, t);
        }


        var kernelIndex = (int)functionName + (int)(isTrans? functionNameFrom : functionName) * 5;

       // Debug.Log("kernelIndex===" + kernelIndex);
        computeShader.SetBuffer(kernelIndex, positionsID, positionBuffer);

        int groups = CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);


        material.SetBuffer(positionsID, positionBuffer);
        material.SetFloat(stepID, step);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f+2f/resolution));//bound =2,but cube have width & height so we should add this
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material,bounds,positionBuffer.count);

    }

    void PickFuntion()
    {
        if (mode == TransMode.Cycle)
        {
            functionName = FunctionLibrary.GetNextFunction(functionName);
        }
        else
        {
            functionName = FunctionLibrary.GetRandomFunction(functionName);
        }
    }

}

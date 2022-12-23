using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class JobSysTest : MonoBehaviour
{
    [SerializeField ,Range(100,10000)]
    int length = 10;
    [SerializeField] bool isJobSystem;
    float a = 10f;
    float b = 20f;

  

    struct Job1 : IJobFor
    {
        public float a;
        public float b;

        public NativeArray<float> res;

        public void Execute(int i)
        {
            res[i] = i + a + b;
            Debug.Log("res " + i +"=="+ res[i]);
           
        }
    }




    struct Job2 : IJobParallelFor
    {
        public float a;
        public float b;

        public NativeArray<float> res;

        public void Execute(int i)
        {
            res[i] = i + a + b;
            Debug.Log("res " + i + "==" + res[i]);

        }
    }

    [BurstCompile]
    struct Job3 : IJobParallelForTransform
    {
       
        public NativeArray<float3> cubeNewPos;
        public NativeArray<float3> cubeInitialPos;
        public NativeArray<float3> cubeDirect;
        public NativeArray<float3> cubeForwad;

        public float time;

        public void Execute(int i,TransformAccess transformAccess)
        {
            cubeForwad[i] =normalize( mul(quaternion.RotateZ(0.2f * PI), cubeDirect[i]));
           
            cubeNewPos[i] = cubeInitialPos[i] + cubeDirect[i] * Mathf.Sin (time);

            transformAccess.position = cubeNewPos[i];
            transformAccess.rotation = quaternion.LookRotation( cubeForwad[i],float3(0,1,0));
        }
    }


    [SerializeField]GameObject prefab;
    GameObject[] cubes;

    NativeArray<float3> jcubeInitialPos;
    NativeArray<float3> jcubeNewPos ;
    NativeArray<float3> jcubeDirect ;
    NativeArray<float3> jcubeForwad ;


    float3[] cubeInitialPos ;
    float3[] cubeNewPos ;
    float3[] cubeDirect;
    float3[] cubeForwad ;


    TransformAccessArray _Trasn;

    // Start is called before the first frame update
    void Start()
    {

        cubes = new GameObject[length];
        isJobSystem = true;
       // _Trasn = new TransformAccessArray(length);
        TransformAccessArray.Allocate(length, length, out _Trasn);
        jcubeInitialPos = new NativeArray<float3>(length, Allocator.Persistent);
        jcubeNewPos = new NativeArray<float3>(length, Allocator.Persistent);
        jcubeDirect = new NativeArray<float3>(length, Allocator.Persistent);
        jcubeForwad = new NativeArray<float3>(length, Allocator.Persistent);


        cubeInitialPos = new float3[length];
        cubeNewPos = new float3[length];
        cubeDirect = new float3[length];
        cubeForwad = new float3[length];

        for (int i = 0; i < length; i++)
        {
            cubes[i] = Instantiate<GameObject>(prefab);
            cubes[i].transform.SetParent( this.transform);
            _Trasn.Add(cubes[i].transform);
        }


    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.timeSinceLevelLoad * 0.1f;

        if (isJobSystem)
        {
            #region jobsystem

          

            Job3 job3 = new Job3();
            job3.cubeDirect = jcubeDirect;
            job3.cubeInitialPos = jcubeInitialPos;
            job3.cubeNewPos = jcubeNewPos;
            job3.cubeForwad = jcubeForwad;
            job3.time = time;

            for (int i = 0; i < length; i++)
            {
                jcubeInitialPos[i] = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                jcubeDirect[i] = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            }

            JobHandle jobHandle3 = job3.Schedule(_Trasn);

            jobHandle3.Complete();

            #endregion
        }
        else
        {
            #region regular
           

            for (int i = 0; i < length; i++)
            {
                cubeInitialPos[i] = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                cubeDirect[i] = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

                cubeForwad[i] = normalize(mul(quaternion.RotateZ(0.2f * PI), cubeDirect[i]));

                cubeNewPos[i] = cubeInitialPos[i] + cubeDirect[i] * Mathf.Sin(time);

                cubes[i].transform.localPosition = cubeNewPos[i];
                cubes[i].transform.forward = cubeForwad[i];
            }

            #endregion
        }





    }

    private void OnDestroy()
    {
        if (isJobSystem)
        {
            jcubeInitialPos.Dispose();
            jcubeNewPos.Dispose();
            jcubeDirect.Dispose();
            jcubeForwad.Dispose();
            _Trasn.Dispose();
        }
        else
        {
            cubeInitialPos =null;
            cubeNewPos = null;
            cubeDirect = null;
            cubeForwad = null;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class HashVisualizationWithDomain : MonoBehaviour
{
    [SerializeField, Range(1, 100)] int resolution = 16;

    [SerializeField] int seed;

    [SerializeField, Range(-2f, 2f)] float verticalOffset = 1f;

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor
    {
        [WriteOnly]
        public NativeArray<uint> hashes;
        //[ReadOnly]
        //public NativeArray<float3> positions;


        public int resolution;
        public float invResolution;
        public SmallXXHash hash;

        public float3x4 domainTRS;

        public void Execute(int i)
        {

            float vf = floor(invResolution * i + 0.00001f);
            float uf = invResolution * (i - resolution * vf + 0.5f) - 0.5f;
            vf = invResolution * (vf + 0.5f) - 0.5f;

            
            float3 p = mul(domainTRS, float4(uf, 0, vf, 1f));



            //float3 p = mul(domainTRS, float4(positions[i], 1f));

            int u = (int)floor(p.x);
            int v = (int)floor(p.y);
            int w = (int)floor(p.z);


            //hashes[i] = (uint)(frac(v*u*0.381f)*256f);

            hashes[i] = hash.Eat(u).Eat(v).Eat(w);
           
            
        }
    }

    static int hashesID = Shader.PropertyToID("_Hashes"),
                configID = Shader.PropertyToID("_Config");
                //positionsID = Shader.PropertyToID("_Positions");

    [SerializeField] Mesh instanceMesh;
    [SerializeField] Material material;


    NativeArray<uint> hashes;
    //NativeArray<float3> positions;
    ComputeBuffer hashesBuffer;
    //ComputeBuffe positionsBuffer;
    MaterialPropertyBlock propertyBlock;

    [SerializeField]
    SpaceTRS domain = new SpaceTRS
    {
        scale = 8f
    };

    



    private void OnEnable()
    {
        int length = resolution * resolution;
        hashes = new NativeArray<uint>(length, Allocator.Persistent);
        //positions = new NativeArray<float3>(length, Allocator.Persistent);
        hashesBuffer = new ComputeBuffer(length, 4);
        //positionsBuffer = new ComputeBuffer(length, 3*4);

        //JobHandle handle = Shapes.Job.ScheduleParallel(positions, resolution,transform.localToWorldMatrix ,default);

        new HashJob
        {
            hashes = hashes,
            // positions = positions,
            resolution = resolution,
            invResolution = 1.0f / resolution,
            hash = SmallXXHash.Seed(seed),
            domainTRS = domain.Matrix

        }.ScheduleParallel(hashes.Length, resolution, default).Complete();

        hashesBuffer.SetData(hashes);
       // positionsBuffer.SetData(positions);

        propertyBlock = new MaterialPropertyBlock();

        propertyBlock.SetBuffer(hashesID,hashesBuffer);
       // propertyBlock.SetBuffer(positionsID, positionsBuffer);

        propertyBlock.SetVector(configID, new Vector4(resolution, 1.0f / resolution,verticalOffset/resolution));
       

    }

    private void OnDisable()
    {
        hashes.Dispose();
        hashesBuffer.Release();
        //positions.Dispose();
        //positionsBuffer.Release();
        hashesBuffer = null;
        //positionsBuffer = null;
    }

    private void OnValidate()
    {
        if (hashesBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, new Bounds(Vector3.zero, Vector3.one), hashes.Length, propertyBlock);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSTest2 : MonoBehaviour
{
    [SerializeField] ComputeShader cs;
    [SerializeField] Material csMat;
    [SerializeField] GameObject instanceObj;
    [SerializeField] int resolution;
    [SerializeField] Mesh mesh;

    ComputeBuffer particalBuffer;
    int kernalID;

    float step = 0.05f;
    public struct ParticleData
    {
        public Vector3 pos;
        public Color color;
    };

    // Start is called before the first frame update
    void Start()
    {
        particalBuffer = new ComputeBuffer(resolution * resolution, 28);

        ParticleData[] data = new ParticleData[resolution * resolution];

        particalBuffer.SetData(data);

        kernalID = cs.FindKernel("CSMain");
    }

    // Update is called once per frame
    void Update()
    {

        step = 2f / resolution; //radius =1
        cs.SetInt("_count", resolution);
        cs.SetFloat("_step", step);

        cs.SetBuffer(kernalID, "ParticleBuffer", particalBuffer);
        cs.SetFloat("_time", Time.time);

        cs.Dispatch(kernalID, resolution / 10, resolution / 10, 1);

        csMat.SetBuffer("_particleDataBuffer", particalBuffer);
        csMat.SetFloat("_step", step);
        


        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, csMat, bounds, particalBuffer.count);
       
    }

    private void OnRenderObject()
    {
        //csMat.SetPass(0);
        //Graphics.DrawProceduralNow(MeshTopology.Points, resolution * resolution);
        
    }

    private void OnDestroy()
    {
        particalBuffer.Release();
        particalBuffer = null;
    }
}

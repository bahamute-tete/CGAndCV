using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class ComputeShaderTest5 : MonoBehaviour
{

    public ComputeShader cs;
    public Material material;
    public Mesh mesh;

    ComputeBuffer cBuffer;

    public int resolution;

    float step;

    public struct Particel
    {
       public Vector3 pos;
       public Color color;
    }

    Particel[] particels;


    private void OnDisable()
    {
        cBuffer.Release();
        cBuffer = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        particels = new Particel[resolution * resolution];

        for (int i = 0; i < particels.Length; i++)
        {
            //particels[i].pos = new Vector3(Random.Range(-1f,1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            particels[i].color = Random.ColorHSV();
        }

        int posSize = sizeof(float)*3;
        int colorSize = sizeof(float)*4;
        cBuffer = new ComputeBuffer(particels.Length, posSize + colorSize);









    }

    // Update is called once per frame
    void Update()
    {
        step = 2f / resolution;
        cBuffer.SetData(particels);

        cs.SetBuffer(0, "particels", cBuffer);
        cs.SetFloat("step", step);
        cs.SetInt("resolution", resolution);

        int groups = Mathf.CeilToInt(resolution / 8f);
       // cs.Dispatch(0, resolution / 8, resolution / 8, 1);

        cs.Dispatch(0, groups, groups, 1);

        material.SetBuffer("particels", cBuffer);
        material.SetFloat("step", step);



        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, particels.Length);
       // Graphics.DrawMeshInstancedIndirect(mesh,0,material,bounds,cBuffer);

        
    }
}

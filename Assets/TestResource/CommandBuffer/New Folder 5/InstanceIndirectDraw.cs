using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;

public class InstanceIndirectDraw : MonoBehaviour
{

    public Mesh mesh;
    public Material material;
    public int count=100;
    private int cachedCount=-1;

    private ComputeBuffer cBufferPositionColor;
    private ComputeBuffer cBufferArgs;


    private CommandBuffer cs;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

   

    MaterialPropertyBlock pb;
    

    struct myBuffers
    {
       public  Vector4 position;
       public  Color color;

    };

    myBuffers[] buffers;

    private void OnEnable()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        pb = new MaterialPropertyBlock();

        cs = new CommandBuffer() { name = "InstanceMesh" };
        cBufferArgs = new ComputeBuffer(1, sizeof(uint) * args.Length, ComputeBufferType.IndirectArguments);
        BufferUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        if (cachedCount != count)
        {
            BufferUpdate();
        }


        Bounds bound = new Bounds(Vector3.zero, Vector3.one * 50f);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bound, cBufferArgs,0, pb);
        //cs.DrawMeshInstancedIndirect(mesh, 0, material, -1, cBufferArgs);


    }

    private void OnDisable()
    {
        if (cBufferArgs != null)
            cBufferArgs.Release();
        cBufferArgs = null;

        if (cBufferPositionColor != null)
            cBufferPositionColor.Release();
        cBufferPositionColor = null;
    }

    void BufferUpdate()
    {
        //set postionBuffer

        if (cBufferPositionColor != null)
            cBufferPositionColor.Release();

        buffers = new myBuffers[count];
        cBufferPositionColor = new ComputeBuffer(count, sizeof(float) * 8);



        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0, 2.0f * Mathf.PI);
            float radius = Random.Range(10f, 50f);
            float yOffset = Random.Range(-1f, 1f);
            float size = Random.Range(0.5f, 1f);

            Vector4 pos = new Vector4(radius * Mathf.Cos(angle), yOffset, radius * Mathf.Sin(angle), size);
            buffers[i].position = pos;

            buffers[i].color = Random.ColorHSV();
        }


        cBufferPositionColor.SetData(buffers);
        material.SetBuffer("MyBuffers", cBufferPositionColor);

        //set argsBuffer
        uint index = mesh.GetIndexCount(0);
        args[0] = index;
        args[1] = (uint)count;
        cBufferArgs.SetData(args);


        cachedCount = count;
    }
}

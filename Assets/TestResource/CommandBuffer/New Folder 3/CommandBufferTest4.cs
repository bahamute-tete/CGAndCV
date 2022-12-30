using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class CommandBufferTest4 : MonoBehaviour
{

    CommandBuffer cmd;

    public Transform tr;

    public Mesh mesh;

    public int _Count;

    public Material mat;


    Matrix4x4[] matrix4X4s;
    MaterialPropertyBlock pb;

    public bool isCommandBuffer = true;

    Vector4[] colors;
    // Start is called before the first frame update
    void Start()
    {
        cmd = new CommandBuffer() { name = "MeshDraw" };
        pb = new MaterialPropertyBlock();

        colors = new Vector4[_Count];



        matrix4X4s = new Matrix4x4[_Count];

        for (int i = 0; i < _Count; i++)
        {
            Vector3 pos = Random.onUnitSphere * 5f;
            tr.position = pos;
            pb.SetColor("_Color", Random.ColorHSV());
            matrix4X4s[i] = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f));

            colors[i] = new Vector4(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1f);
        }

        pb.SetVectorArray("_Color", colors);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCommandBuffer)
        {
            Graphics.DrawMeshInstanced(mesh, 0, mat, matrix4X4s, _Count,pb);
        }
        else
        {
            if (cmd != null)
            {
                cmd.Clear();
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cmd);
            }

            cmd.DrawMeshInstanced(mesh, 0, mat, -1, matrix4X4s, _Count, pb);
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmd);
        }

    }
}

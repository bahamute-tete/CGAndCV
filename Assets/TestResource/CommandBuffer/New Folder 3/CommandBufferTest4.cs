using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


public class CommandBufferTest4 : MonoBehaviour
{

    CommandBuffer cmd;

    public Transform tr;

    public Mesh mesh;

    public int _Count;

    public Material mat;


    Matrix4x4[] matrix4X4s;
    MaterialPropertyBlock pb;

    Vector3[] orginalPos;
    Quaternion[] qs;
    Vector3[] dirs;

    bool isExpand = true;

    public bool isCommandBuffer = true;

    Vector4[] colors;

    
    float d=0;
    // Start is called before the first frame update
    void Start()
    {
        cmd = new CommandBuffer() { name = "MeshDraw" };
        pb = new MaterialPropertyBlock();

        colors = new Vector4[_Count];
        orginalPos = new Vector3[_Count];
        qs = new Quaternion[_Count];
        dirs = new Vector3[_Count];


        matrix4X4s = new Matrix4x4[_Count];

        for (int i = 0; i < _Count; i++)
        {
            Vector3 pos = Random.onUnitSphere * 1f;
            orginalPos[i] = pos;
            //tr.position = pos;
            //pb.SetColor("_Color", Random.ColorHSV());


            Quaternion q= Quaternion.FromToRotation(Vector3.forward,pos.normalized);
            qs[i] = q;


            dirs[i] = pos.normalized;
            //matrix4X4s[i] = Matrix4x4.TRS(pos, q, new Vector3(0.1f, 0.1f, 0.1f));

            //colors[i] = new Vector4(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1f);
        }

        pb.SetVectorArray("_Color", colors);
    }

    // Update is called once per frame
    void Update()
    {

        d += Time.deltaTime;

        d = Mathf.Clamp(d, 0f, 5f);

        if (d == 5f)
        {
            d = 5f;
        }
        

      

        //Debug.Log("d ==" + d);

        for (int i = 0; i < _Count; i++)
        {
           Vector3 pos = orginalPos[i]+ dirs[i] * d;

            matrix4X4s[i] = Matrix4x4.TRS(pos, qs[i], new Vector3(0.1f, 0.1f, 0.1f));

            //colors[i] = new Vector4(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1f);
        }

        if (!isCommandBuffer)
        {
            Graphics.DrawMeshInstanced(mesh, 0, mat, matrix4X4s, _Count);
        }
        else
        {
            if (cmd != null)
            {
                cmd.Clear();
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterSkybox, cmd);
            }

            cmd.DrawMeshInstanced(mesh, 0, mat, 0, matrix4X4s, _Count);
            Camera.main.AddCommandBuffer(CameraEvent.AfterSkybox, cmd);
        }

    }
}

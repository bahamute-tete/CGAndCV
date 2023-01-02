using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static Noise;


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

    public bool isCommandBuffer = true;

    Vector4[] colors;


    public float duration;
    float value = 0;


    [SerializeField]
    bool autoReverse = false;
    
    public bool Reversed { get; set; }
    public bool AutoReversed
    {
        get => autoReverse;
        set => autoReverse = value;
    }
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

            colors[i] = new Vector4(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1f);
        }

        pb.SetVectorArray("_Color", colors);
    }

    // Update is called once per frame
    void Update()
    {

        float delta = Time.deltaTime / duration;
        if (Reversed)
        {
            value -= delta;
            if (value <= 0f)
            {
                if (autoReverse)
                {
                    value = Mathf.Min(1f, -value);
                    Reversed = false;
                }
                else
                {
                    value = 0f;
                   
                }
            }
        }
        else
        {
            value += delta;
            if (value >= 1f)
            {
                if (autoReverse)
                {
                    value = Mathf.Max(0f, 2f - value);
                    Reversed = true;
                }
                else
                {
                    value = 1f;
                }
            }
        }




        //Debug.Log("d ==" + d);

        for (int i = 0; i < _Count; i++)
        {
           Vector3 pos = orginalPos[i]+ dirs[i] * value;

            matrix4X4s[i] = Matrix4x4.TRS(pos, qs[i], new Vector3(0.1f, 0.1f, 0.1f));

            //colors[i] = new Vector4(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1f);
        }

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

            cmd.DrawMeshInstanced(mesh, 0, mat, -1, matrix4X4s, _Count);
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmd);
        }

    }
}

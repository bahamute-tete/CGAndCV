using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;

public class InstanceTest2 : MonoBehaviour
{
    [SerializeField] Mesh instanceMesh;
    [SerializeField] Material instanceMat;
    [SerializeField] int instanceCount;
    Matrix4x4[] matrix4X4s;

    CommandBuffer m_buffer;
    // Start is called before the first frame update
    void Start()
    {
  
        matrix4X4s = new Matrix4x4[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = Random.insideUnitSphere*10f;
            Quaternion q = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            Vector3 scale = new Vector3(Random.Range(1f, 2f), Random.Range(1f, 2f), Random.Range(1f, 2f));
            Matrix4x4 m_rts = Matrix4x4.TRS(pos, q, scale);
            matrix4X4s[i] = m_rts;

        }


        //if (m_buffer != null)
        //{
        //    Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_buffer);
        //    m_buffer.Release();
        //}

        //m_buffer = new CommandBuffer();


    }

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMeshInstanced(instanceMesh, 0, instanceMat, matrix4X4s, instanceCount);


        //Graphics.D(instanceMesh, 0, instanceMat, matrix4X4s, instanceCount);
    }
}

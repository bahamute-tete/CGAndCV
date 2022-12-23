using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SphereTest : MonoBehaviour
{
    Mesh mesh;
    [SerializeField]List<Vector3> pos = new List<Vector3>();
    Mesh spherePlane;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        int[] triangleIndex = mesh.triangles;

        spherePlane = new Mesh();

        Vector3 ro = new Vector3(0, -1, 0);
        Vector3 N = -Vector3.up;

        Matrix4x4 M_o2w = new Matrix4x4();
        
        M_o2w = gameObject.transform.localToWorldMatrix;

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
         
            Vector3 p = mesh.vertices[i];
            Vector4 hp = new Vector4(p.x, p.y, p.z, 1.0f);
            Matrix4x4 mp = new Matrix4x4();
            mp.SetColumn(0, hp);
            mp.SetColumn(3, new Vector4(0, 0, 0, 1));
            Vector3 tp = (M_o2w * mp).GetColumn(0) / (M_o2w * mp).GetColumn(0).w;


            float t = 0;
            Vector3 V ;
            if (Vector3.Magnitude(tp - ro) < 0.0001f)
            {
                V = new Vector3( 0.999f,0,0); 
            }
            else
            {
                V = Vector3.Normalize(tp - ro);
              
            }
            t = -Vector3.Dot(N, ro) / Vector3.Dot(N, V);

            Vector3 ip = ro + V * t;

            //if (Vector3.Dot(N, V) < 0.01f && Vector3.Dot(N, V) > 0)
            //    ip = Vector3.zero;

            pos.Add(ip);
        }

        mesh.SetVertices(pos.ToArray());
        mesh.RecalculateNormals();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

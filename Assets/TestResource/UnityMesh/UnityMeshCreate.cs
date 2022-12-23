using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class UnityMeshCreate : MonoBehaviour
{
    public int xsize;
    public int ysize;

    private Mesh mesh;

    private Vector3[] vertices;

    // Start is called before the first frame update



    void Start()
    {
        GreatePannel();
    }


    // Update is called once per frame
    void Update()
    {
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    float d = Vector3.Magnitude(vertices[i]);
        //    vertices[i].z = 0.05f * d * Mathf.Cos(1.2f * (d - Time.timeSinceLevelLoad * 4));
        //}


        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    vertices[i].z =math.pow(vertices[i].x, 2) *0.2f - 0*math.pow(vertices[i].y, 2) * 0.2f;
        //}


        mesh.vertices = vertices;
        mesh.RecalculateNormals(); 

    }


    void GreatePannel()
    {
        vertices = new Vector3[(xsize + 1) * (ysize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        for (int i = 0, y = 0; y <= ysize; y++)
        {
            for (int x = 0; x <= xsize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                uv[i] = new Vector2((float)x / xsize, (float)y / ysize);
                tangents[i] = new Vector4(1, 0, 0, -1);
            }
        }

        mesh = new Mesh();
        mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        mesh.name = "Procedural Grid";
        mesh.vertices = vertices;

        int[] triangles = new int[xsize * ysize * 6];

        for (int i = 0, j = 0, x = 0, y = 0; y < ysize; y++, j++)
        {
            for (x = 0; x < xsize; x++, i += 6, j++)
            {
                triangles[i] = j;
                triangles[i + 1] = j + xsize + 1;
                triangles[i + 2] = j + 1;
                triangles[i + 3] = j + 1;
                triangles[i + 4] = j + xsize + 1;
                triangles[i + 5] = j + xsize + 2;
            }
        }



        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.uv = uv;
        mesh.tangents = tangents;
    }
}

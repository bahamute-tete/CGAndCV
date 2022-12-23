using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnityProcedularSphere : MonoBehaviour
{

    public int gridSize;
    int  xSize, ySize, zSize;
    public float radius = 1f;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Color32[] cubeUV;
    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        mesh = GetComponent<MeshFilter>().mesh = new Mesh();
        mesh.name = "Procedular Sphere";

        xSize = ySize = zSize = gridSize;


        CreateVertices();
        CreateTriangles();
        CreatColliders();
        
    }

    private void CreatColliders()
    {
        gameObject.AddComponent<SphereCollider>();
    }

   

    private void CreateVertices()
    {

        
        int cornerVertices = 8; //8coner

        //12edges four edges have same directions,each direction have (x-1)+(y-1)+(z-1)
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        //6faces 2faces have same directions,each face have(x-1)*(y-1)+(x-1)*(z-1)+(y-1)*(z-1)
        int faceVeertices = (
                            (xSize - 1) * (ySize - 1) +
                            (xSize - 1) * (zSize - 1) +
                            (ySize - 1) * (zSize - 1)) * 2;

        vertices = new Vector3[cornerVertices + edgeVertices + faceVeertices];
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];
        //Debug.Log($"verticesCount ={vertices.Length}");

        int v = 0;
        for (int  y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                SetVertex(v++, x, y, 0);

            }

            for (int z = 1; z <= zSize; z++)
            {
                SetVertex(v++, xSize, y, z);

            }

            for (int x = xSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, zSize);

            }

            for (int z = zSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);

            }
        }

        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, ySize, z);
            }
        }

        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, 0, z);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors32 = cubeUV;
    }
    private void SetVertex(int i, int x, int y, int z)
    {
        Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;

        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;

        Vector3 s;

        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - y2 / 2f - x2 / 2f + y2 * x2 / 3f);

        normals[i] = s;
        vertices[i] = normals[i]*radius;

        //use 2channels as uv .for example uv= cubeUV[i].xz=float2(1,10),just fit on grid 
        cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }


    private void CreateTriangles()
    {
        //two pairs  
        int[] trianglesZ = new int[(xSize * ySize) * 6*2];
        int[] trianglesX = new int[(ySize * zSize) * 6*2];
        int[] trianglesY = new int[(xSize * zSize) * 6*2];

        //the number of triangles is simply equal to that of the six faces combined
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
        int[] triangles = new int[quads * 6];

        //next row offset = a ring vertices(x+z)*2
        int ring = (xSize + zSize) * 2;
        int tZ = 0, tX=0 , tY=0, v = 0;

        //Draw face except top and down
        for (int y = 0; y < ySize; y++, v++)
        {
            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (int q = 0; q < zSize; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }

            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (int q = 0; q < zSize-1; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            //last quad need draw alone
            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }

        tY = CreateTopFace(trianglesY, tY, ring);
        tY = CreateBottomFace(trianglesY, tY, ring);

        mesh.subMeshCount = 3;
        mesh.SetTriangles(trianglesZ, 0);
        mesh.SetTriangles(trianglesX, 1);
        mesh.SetTriangles(trianglesY, 2);

        //mesh.triangles = triangles;
    }
    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        int v = ring * ySize;

        //first Row
        for (int x = 0; x < xSize - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }

        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

        //next row
        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            //first part
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
            //mid part
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }
            //last part
            t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
        }


        //last row
        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);

        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }

        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

        return t;
    }
    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        int v = 1;
        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);//CW Dirtection

        //first Row
        for (int x = 1; x < xSize - 1; x++, v++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }

        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        //next row
        int vMin = ring - 2;
        vMid -= xSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            //first part
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
            //mid part
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            //last part
            t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }


        //last row
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);

        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }

        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

        return t;
    }
    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        //  v01...v11
        //    .     .
        //    .     .
        //    .     .
        //  v00....v10
        //CW direction
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }



    private void OnDrawGizmos()
    {
        //if (vertices == null)
        //    return;

        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    Gizmos.color = Color.black;
        //    Gizmos.DrawSphere(vertices[i], 0.1f);
        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawRay(vertices[i], normals[i]);
        //}
        //Gizmos.matrix = Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.localScale);
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCube(gameObject.transform.position, new Vector3(xSize, ySize, zSize));


    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.Mathf;

public class CameraFrustum : MonoBehaviour
{

    [SerializeField] GameObject cubePrefab;
    [SerializeField] int X, Y, Z;

    [SerializeField] new Camera camera;

    Vector3[] nearPlaneConners = new Vector3[4];
    Vector3[] farPlaneConners = new Vector3[4];

    [SerializeField] Plane plane = new Plane(new Vector3(0, 1, 0), 0);

     Vector3[] points = new Vector3[4];

     float xmin, xmax, zmin, zmax;


     bool[] rayOnPlane = new bool[4];

    [SerializeField] Vector3 minPoint, maxPoint;
    float xsize, zsize;


    [SerializeField]List<GameObject> cubes = new List<GameObject>();
    GameObject content;

    int[,] marks;
    [SerializeField]GameObject[,] cubsArray;

    int[,] SE1 = new int[3, 3] { {1,1,1},
                                 {1,1,1},
                                 {1,1,1}};


    // Start is called before the first frame update
    void Start()
    {
        content = new GameObject("Content");
        ProjectionOnPlane();
        CreateCubesMatrix(X, Z);

    }

    void Update()
    {
        //Canculate min Rec which bound to  Camera frustum projection on plane
        GetProjectionPlane(camera, plane, out minPoint, out maxPoint);
        UpdateCubesInFrustumProjectionPlane(minPoint, maxPoint);
        ProjectionFrustumCull(cubes, minPoint, maxPoint); 
    }

    private void OnDrawGizmos()
    {

        var cameraPos = camera.transform.position;

        Gizmos.color = Color.yellow;
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearPlaneConners);
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farPlaneConners);
        float rayLength = Vector3.Distance(farPlaneConners[0], nearPlaneConners[0]);

        DrawFrustum(nearPlaneConners, farPlaneConners);

        for (int i = 0; i < 4; i++)
        {
            Vector3 worldSpaceNearCorner = camera.transform.TransformPoint(nearPlaneConners[i]);
            Vector3 worldSpaceFarCorner = camera.transform.TransformPoint(nearPlaneConners[i]);

            Vector3 dir = Vector3.Normalize(worldSpaceNearCorner - cameraPos);

            Vector4 S = new Vector4(dir.x, dir.y, dir.z, 0);
            Vector4 V = new Vector4(worldSpaceNearCorner.x, worldSpaceNearCorner.y, worldSpaceNearCorner.z, 1);
            Vector4 L = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

            float t = -Vector4.Dot(L, V) / Vector4.Dot(L, S);


            if (t > 0 && Vector3.Distance(worldSpaceFarCorner, worldSpaceNearCorner) <= rayLength)
            {
                rayOnPlane[i] = true;

                Vector3 point = worldSpaceNearCorner + dir * t;

                Gizmos.DrawLine(worldSpaceNearCorner, point);

                points[i] = point;
            }
            else
            {
                rayOnPlane[i] = false;
            }
        }

        if (!Array.Exists(rayOnPlane, a => a.Equals(false)))
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < 3)
                    Gizmos.DrawLine(points[i], points[i + 1]);
                else
                    Gizmos.DrawLine(points[i], points[0]);
            }

            xmin = points.Min(t => t.x);
            xmax = points.Max(t => t.x);
            zmin = points.Min(t => t.z);
            zmax = points.Max(t => t.z);

            Vector3 P0 = new Vector3(xmin, 0, zmin);
            Vector3 P1 = new Vector3(xmin, 0, zmax);
            Vector3 P2 = new Vector3(xmax, 0, zmax);
            Vector3 P3 = new Vector3(xmax, 0, zmin);


            Gizmos.color = Color.red;
            Gizmos.DrawLine(P0, P1);
            Gizmos.DrawLine(P1, P2);
            Gizmos.DrawLine(P2, P3);
            Gizmos.DrawLine(P3, P0);
        }


        //var orderX = points.OrderBy(t => t.x).ToList();
        //xmin = orderX.First().x;
        //xmax = orderX.Last().x;

        //var  orderY= points.OrderBy(t => t.y).ToList();
        //ymin = orderX.First().y;
        //ymax = orderX.Last().y;
    }

    private void DrawFrustum(Vector3[] nearPlaneConners,Vector3[] farPlaneConners)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < 3)
            {
                Gizmos.DrawLine(camera.transform.TransformPoint(nearPlaneConners[i]), camera.transform.TransformPoint(nearPlaneConners[i + 1]));
                Gizmos.DrawLine(camera.transform.TransformPoint(farPlaneConners[i]), camera.transform.TransformPoint(farPlaneConners[i + 1]));
            }
            else
            {
                Gizmos.DrawLine(camera.transform.TransformPoint(nearPlaneConners[i]), camera.transform.TransformPoint(nearPlaneConners[0]));
                Gizmos.DrawLine(camera.transform.TransformPoint(farPlaneConners[i]), camera.transform.TransformPoint(farPlaneConners[0]));
            }

            Gizmos.DrawLine(camera.transform.TransformPoint(nearPlaneConners[i]), camera.transform.TransformPoint(farPlaneConners[i]));
        }
    }

    private void ProjectionOnPlane()
    {
       
        GetProjectionPlane(camera, plane, out minPoint, out maxPoint);

        xsize = cubePrefab.transform.localScale.x;
        zsize = cubePrefab.transform.localScale.z;

        for (float i = minPoint.x; i < maxPoint.x; i += xsize)
        {
            for (float j = minPoint.z; j < maxPoint.z; j += zsize)
            {
                GameObject temp = Instantiate(cubePrefab, new Vector3(i, 0, j), Quaternion.identity, content.transform);
                cubes.Add(temp);

            }
        }
    }
    private void GetProjectionPlane(Camera camera,Plane plane ,out Vector3 minPoint,out Vector3 maxPoint)
    {
        minPoint = maxPoint = default;

        float e = camera.nearClipPlane;
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), e, Camera.MonoOrStereoscopicEye.Mono,  nearPlaneConners);

        var cameraPos = camera.transform.position;

        for (int i = 0; i < 4; i++)
        {
            Vector3 worldSpaceCorner = camera.transform.TransformPoint(nearPlaneConners[i]);

            Vector3 dir = Vector3.Normalize(worldSpaceCorner - cameraPos);

            Vector4 S = new Vector4(dir.x, dir.y, dir.z, 0);
            Vector4 V = new Vector4(worldSpaceCorner.x, worldSpaceCorner.y, worldSpaceCorner.z, 1);
            Vector4 L = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

            float t = -Vector4.Dot(L, V) / Vector4.Dot(L, S);

            if (t > 0)
            {
                rayOnPlane[i] = true;

                Vector3 point = worldSpaceCorner + dir * t;

                points[i] = point;
            }
            else
            {
                rayOnPlane[i] = false;
            }
        }


        if (!Array.Exists(rayOnPlane,p=> p.Equals(false)))
        {
            //int count=0;
            //Array.ForEach(rayOnPlane, (p) => { if (p.Equals(true)) { count++; } });

            //Debug.Log($"Count ={count}");

            xmin = points.Min(t => t.x);
            xmax = points.Max(t => t.x);
            zmin = points.Min(t => t.z);
            zmax = points.Max(t => t.z);

            Vector3 P0 = new Vector3(xmin, 0, zmin);
            
            Vector3 P2 = new Vector3(xmax, 0, zmax);

            minPoint = P0;
            maxPoint = P2;
        }

    }
    private bool IsPointInFrustumProjectionPlane(Vector3 point, Vector3[] polygonPoints)
    {

        List<Vector3> rays = new List<Vector3>();
        foreach (var vertex in polygonPoints)
        {
            Vector3 vec = Vector3.Normalize(vertex - point);
            rays.Add(vec);
        }

        for (int i = 0; i < rays.Count; i++)
        {

        }

        Vector3 dir0 =Vector3.Normalize(polygonPoints[0] - point);
        Vector3 dir1 =Vector3.Normalize(polygonPoints[1] - point);
        Vector3 dir2 =Vector3.Normalize(polygonPoints[2] - point);
        Vector3 dir3 =Vector3.Normalize(polygonPoints[3] - point);


        Vector3 x01 = Vector3.Cross(dir0, dir1);
        Vector3 x12 = Vector3.Cross(dir1, dir2);
        Vector3 x23 = Vector3.Cross(dir2, dir3);
        Vector3 x30 = Vector3.Cross(dir3, dir0);

        if (x01.y > 0 && x12.y > 0 && x23.y > 0 && x30.y > 0)
            return true;
        else
            return false;


    }





    void UpdateCubesInFrustumProjectionPlane(Vector3 minPoint,Vector3 maxPoint)
    {
        
        
        cubes.Clear();

        for (int i = 0; i < content.transform.childCount; i++)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }

        for (float i = minPoint.x; i < maxPoint.x; i += xsize)
        {
            for (float j = minPoint.z; j < maxPoint.z; j += zsize)
            {
                GameObject temp = Instantiate(cubePrefab, new Vector3(i, 0, j), Quaternion.identity, content.transform);
                cubes.Add(temp);
            }
        }

    }        
    void ProjectionFrustumCull(List<GameObject> gameObjects,Vector3 minPoint,Vector3 maxPoint)
    {



        int x = CeilToInt(maxPoint.x - minPoint.x);
        int z = CeilToInt(maxPoint.z - minPoint.z);

        //Debug.Log($"x= {x},z={z}");

        //Plane[] Ls = GeometryUtility.CalculateFrustumPlanes(camera);//[0] =left、[1] = right、[2] = down、[3] = up、[4] = near、[5] = forward
        int[,] marks = new int[x, z];


        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                marks[i, j] = 0;
            }
        }


        for (int i = 0; i < gameObjects.Count; i++)
        {
            //bool inside = GeometryUtility.TestPlanesAABB(Ls, gameObjects[i].GetComponentInChildren<MeshRenderer>().bounds);
            bool inside = IsPointInFrustumProjectionPlane(gameObjects[i].transform.position, points);
            if (inside)
            {
               int  iy = i % z;
               int  ix = (i - iy) / z;
               marks[ix, iy] = 1;
            }
        }

        int[,] remarks = ImMorphology(marks,x,z, SE1);



        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                if (remarks[i, j] == 1)
                {
                    cubes[i * z + j].GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                }
                else
                {
                    cubes[i * z + j].GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                }
            }
        }

    }
    void FrustumCullProjection()
    {
        GetProjectionPlane(camera, plane, out minPoint, out maxPoint);


        cubes.Clear();

        for (int i = 0; i < content.transform.childCount; i++)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }


        for (float i = minPoint.x; i < maxPoint.x; i += xsize)
        {
            for (float j = minPoint.z; j < maxPoint.z; j += zsize)
            {
                GameObject temp = Instantiate(cubePrefab, new Vector3(i, 0, j), Quaternion.identity, content.transform);
                cubes.Add(temp);
            }
        }


        for (int i = 0; i < cubes.Count; i++)
        {
            if (IsPointInFrustumProjectionPlane(cubes[i].transform.position, points))
            {
                cubes[i].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            }

        }

    }
    void CreateCubesMatrix(int x ,int y ,int z)
    {

        for (int i = -x; i < x; i++)
        {
            for (int j = -y; j < y; j++)
            {
                for (int k = -z; k < z; k++)
                {
                    GameObject temp = Instantiate(cubePrefab, new Vector3(i, j, k), Quaternion.identity,content.transform);
                    cubes.Add(temp);
                }

            }
        }
    }
    int[,] CreateCubesMatrix(int x, int z)
    {
        // each cube has a mark equal 0;
        int M = 2 * x + 1;
        int N = 2 * z + 1;
        int [,]marks = new int[M, N];

        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                marks[i, j] = 0;
            }
        }

        //Instantiate Cube
        for (int i = -x; i <=x; i++)
        {
                for (int k = -z; k <=z; k++)
                {
                    GameObject temp = Instantiate(cubePrefab, new Vector3(i, 1, k), Quaternion.identity, content.transform);
                    cubes.Add(temp);
                }

         }

        return marks;
    }


    int[,] OR(int[,] SE, int[,] T)
    {
        int r = SE.GetUpperBound(0) + 1;
        int c = SE.GetUpperBound(1) + 1;
        int[,] id = new int[r, c];

        if (T.Length != SE.Length)
        {
            Debug.LogError("T(image stencil mask) must have the same length as SE");
        }
        else
        {

            for (int s = 0; s < r; s++)
            {
                for (int t = 0; t < c; t++)
                {
                    id[s, t] = SE[s, t] | T[s, t];
                }
            }
        }
        return id;
    }
    int[,] Dilation(int[,] marks, int[,] SE)
    {
        int r = SE.GetUpperBound(0) + 1;
        int c = SE.GetUpperBound(1) + 1;
        int M = marks.GetUpperBound(0) + 1;
        int N = marks.GetUpperBound(1) + 1;
        int midr = (r - 1) / 2;
        int midc = (c - 1) / 2;

        int[,] refSE = new int[r, c];

       

        for (int s = 0; s < r; s++)
        {
            for (int t = 0; t < c; t++)
            {
                refSE[s, t] = SE[r - 1 - s, c - 1 - t];
            }
        }

        int[,] outputMark = new int[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                int[,] nhood = new int[r, c];
                int[,] id = new int[r, c];

                if (marks[m,n] == 1)
                {
                    if (m - midr >= 0 && n - midc >= 0 && m+midr <M && n+midc<N)
                    {
                        for (int s = -midr; s <= midr; s++)
                        {
                            for (int t = -midc; t <= midc; t++)
                            {
                                nhood[s + midr, t + midc] = marks[m + s, n + t];

                                id = OR(refSE, nhood);
                            }
                        }

                        for (int s = -midr; s <= midr; s++)
                        {
                            for (int t = -midc; t <= midc; t++)
                            {
                                if (id[s + midr, t + midc] == 1)
                                {
                                    outputMark[m + s, n + t] = 1;
                                }

                            }
                        }

                    }
                }
            }
        }
        return outputMark;
    }
    int[,] ImMorphology(int[,] marks, int M ,int N ,int[,] SE)
    {
        int r = SE.GetUpperBound(0) + 1;
        int c = SE.GetUpperBound(1) + 1;
        int midr = (r - 1) / 2;
        int midc = (c - 1) / 2;

        int[,] resMarks = new int[M, N];
        resMarks = Dilation(marks, SE);
        return resMarks;
    }
}




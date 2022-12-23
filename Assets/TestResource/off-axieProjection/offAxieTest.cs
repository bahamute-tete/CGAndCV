using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer),typeof(Camera))]
public class offAxieTest : MonoBehaviour
{
    [SerializeField] Material testMat;
    [SerializeField] GameObject viewPoint;
    Mesh testMesh;

    Vector3[] vertices;

    [SerializeField]Rect rect;
    Camera cam;


    Vector3 pOnNear;

    private void Awake()
    {

        cam = GetComponent<Camera>();
        testMesh = GetComponent<MeshFilter>().mesh = new Mesh();
        testMesh.name = "TestNearPlane";

        GetComponent<MeshRenderer>().material = testMat;

        vertices = new Vector3[4];

        rect =CreateBoundary();

        cam.ResetProjectionMatrix();
        cam.ResetCullingMatrix();

    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 viewPointPos = cam.transform.TransformPoint(viewPoint.transform.position);
        Vector3 fwd = cam.transform.TransformVector(-viewPoint.transform.forward);
        Plane plane = new Plane(fwd, viewPointPos);
        float near = plane.ClosestPointOnPlane(Vector3.zero).magnitude;



        float l = viewPointPos.x -0.730f;
        float r = viewPointPos.x + 0.73f;
        float t = viewPointPos.y +0.35f;
        float b = viewPointPos.y -0.35f;

        //Scale NearPlane
        float scale_factor = 0.01f / near;
        near *= scale_factor;
        l *= scale_factor;
        r *= scale_factor;
        t *= scale_factor;
        b *= scale_factor;

        Matrix4x4 m = Matrix4x4.Frustum(l, r, b, t, near, cam.farClipPlane);
        cam.projectionMatrix = m;
        cam.cullingMatrix = m * cam.worldToCameraMatrix;

       
    }

    private void LimitedCameraMove(Camera cam, Rect rect)
    {
        
        float x = Mathf.Clamp(cam.transform.position.x ,rect.min.x, rect.max.x);
        float y =Mathf.Clamp(cam.transform.position.y, rect.min.y, rect.max.y);
        cam.transform.position = new Vector3(x, y, cam.transform.position.z);

    }

    private void OnDrawGizmos()
    {
        cam = GetComponent<Camera>();

        vertices = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, vertices);

        Vector3[] farvertices = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farvertices);

        Vector3[] bounds = new Vector3[4];
        Vector3[] boundsfar = new Vector3[4];


        Vector3[] wp = new Vector3[4];
       

        for (int i = 0; i < vertices.Length; i++)
        {
            wp[i] = cam.transform.TransformPoint(vertices[i]);
        }


        for (int i = 0; i < wp.Length; i++)
        {
            Gizmos.DrawLine(wp[i], wp[(i + 1) % wp.Length]);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(cam.transform.position, cam.nearClipPlane *cam.transform.forward);




        Gizmos.color = Color.cyan;
        for (int i = 0; i < vertices.Length; i++)
        {
            bounds[i] = gameObject.transform.parent.transform.TransformPoint(vertices[i]);
            boundsfar[i] = gameObject.transform.parent.transform.TransformPoint(farvertices[i]);

        }
        for (int i = 0; i < bounds.Length; i++)
        {
            Gizmos.DrawLine(bounds[i], bounds[(i + 1) % bounds.Length]);
            Gizmos.DrawLine(boundsfar[i], boundsfar[(i + 1) % bounds.Length]);
            Gizmos.DrawLine(bounds[i], boundsfar[i]);
        }


       




    }


    private Rect CreateBoundary()
    {
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, vertices);

        Vector3[] boudary = new Vector3[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            boudary[i] = gameObject.transform.parent.transform.TransformPoint(vertices[i]);
        }

        return new Rect(boudary[0].x, boudary[0].y, boudary[2].x - boudary[0].x, boudary[2].y - boudary[0].y);
    }

    private void CreateTestPlane()
    {
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, vertices);


        testMesh.vertices = vertices;
        testMesh.triangles = new int[6] { 0, 1, 3, 3, 1, 2 };
        testMesh.RecalculateNormals();
        Vector2 uv00 = new Vector2(0f, 0f);
        Vector2 uv01 = new Vector2(0f, 1f);
        Vector2 uv10 = new Vector2(1f, 1f);
        Vector2 uv11 = new Vector2(1f, 0f);
        testMesh.uv = new Vector2[4] { uv00, uv01, uv10, uv11 };
    }

    void GyroModifyCamera()
    {
        transform.rotation = GyroToUnity(Input.gyro.attitude);
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

}

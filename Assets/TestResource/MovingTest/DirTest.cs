using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Reflection;

public class DirTest : MonoBehaviour
{
    

     Vector3 planeNormal = new Vector3(0, 1.0f, 0);
     float planeDis = 0f;


    public float planeGizmoX = 5.0f;
    public float planeGizemoZ = 5.0f;

    public Vector3[] gizmoPlaneVertex = new Vector3[4];
    private Vector3 intersectionPoint = new Vector3(0, 0, 0);

    private Vector3 planeNormlaPoint = new Vector3(0, 0, 0);
    private Vector3 pCenter = new Vector3(0, 0, 0);

    public Vector3 rotXYZ = new Vector3(0, 0, 0.0f);
    public Vector3 tranXYZ = new Vector3(0, 0, 0);


    [SerializeField]
    private Color gizmoColor;
    [SerializeField]
    private Material PlaneGizmoMat;
    private List<GameObject> lrList = new List<GameObject>();
   
    private LineRenderer lr0 = new LineRenderer();
    private LineRenderer lr1 = new LineRenderer();
    private LineRenderer lr2 = new LineRenderer();
    private LineRenderer lr3 = new LineRenderer();
    private LineRenderer lr4 = new LineRenderer();

    private LineRenderer lr5 = new LineRenderer();

    Vector3 pNormal;


    public Vector3 velocityDir = Vector3.one;

    private void Start()
    {
        lrList.Clear();
        for (int i = 0; i < 6; i++)
        {
            GameObject olr = new GameObject("LR" + i);
            lrList.Add(olr);
        }

      
        lr0 = lrList[0].AddComponent<LineRenderer>();
        lr1 = lrList[1].AddComponent<LineRenderer>();
        lr2 = lrList[2].AddComponent<LineRenderer>();
        lr3 = lrList[3].AddComponent<LineRenderer>();
        lr4 = lrList[4].AddComponent<LineRenderer>();

        lr5 = lrList[5].AddComponent<LineRenderer>();

    }


    private void Update()
    {
        DrawPlaneGizmo();

       
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(pCenter, 0.5f);
        //平面法向量和 速度方向做叉乘 
        Vector3 nv = Vector3.Cross(pNormal, velocityDir.normalized);
        //Debug.Log($"nv={nv}");

       //显示速度方向
        Gizmos.color = Color.red;
        Vector3 restDis = velocityDir.normalized * 1f;
        Vector3 p2 = pCenter - restDis;

        Gizmos.DrawLine(pCenter, p2);
        Gizmos.DrawLine(pCenter, pCenter + restDis);

        Gizmos.color = Color.cyan;
        Vector3 p3 = p2 - Vector3.Dot((p2 - pCenter), pNormal) * pNormal;
        Gizmos.DrawWireSphere(p3, 0.1f);
        Gizmos.DrawLine(p2, p3);
        
        //Debug.Log($"velocityDir={velocityDir}");

        //在用平面法向量和 刚才第一步得到的向量 在做一次叉乘 就得到想要的方向来 
        Gizmos.color = Color.yellow;
        Vector3 pp = Vector3.Cross(pNormal, nv).normalized;
        Gizmos.DrawLine(pCenter, pCenter + pp);
        //Debug.Log($"dir={pCenter+Vector3.Cross(pNormal, nv).normalized}");


    }


    Vector4 TransformPlane(Vector3 T_xyz, Vector3 R_xyz, Vector3 normal, float D)
    {
        Vector4 planeHomogeneous = new Vector4(normal.x, normal.y, normal.z, D);
        Quaternion q = Quaternion.Euler(R_xyz.x, R_xyz.y, R_xyz.z);
        Matrix4x4 Rot = Matrix4x4.Rotate(q);
        Matrix4x4 Trans = Matrix4x4.Translate(T_xyz);

        Matrix4x4 customTrans = new Matrix4x4();
        customTrans.SetColumn(0, new Vector4(T_xyz.x, T_xyz.y, T_xyz.z, 0));

        Matrix4x4 InverRot = Rot.inverse;

        Matrix4x4 MT = InverRot * customTrans;

        Matrix4x4 IF = new Matrix4x4();
        IF.SetColumn(0, InverRot.GetColumn(0));
        IF.SetColumn(1, InverRot.GetColumn(1));
        IF.SetColumn(2, InverRot.GetColumn(2));
        IF.SetColumn(3, -MT.GetColumn(0));
        IF.SetRow(3, new Vector4(0, 0, 0, 1));


        Matrix4x4 ITF = Matrix4x4.Transpose(IF);

        Vector4 newP = ITF * planeHomogeneous;

        return newP;
    }

    Vector3[] RotateVertex(Vector3 T_xyz, Vector3 R_xyz, Vector3[] vector3s)
    {
        Vector4 v1;
        Vector4 v2;
        Vector4 v3;
        Vector4 v4;

        Vector3 scale = new Vector3(1, 1, 1);

        Quaternion q = Quaternion.Euler(R_xyz.x, R_xyz.y, R_xyz.z);

        Matrix4x4 Rot = Matrix4x4.Rotate(q);
        Matrix4x4 Tanslate = Matrix4x4.Translate(T_xyz);

        Matrix4x4 TRS = Matrix4x4.TRS(T_xyz, q, scale);

        Matrix4x4 vetexGRP = new Matrix4x4();
        vetexGRP.SetColumn(0, vector3s[0]);
        vetexGRP.SetColumn(1, vector3s[1]);
        vetexGRP.SetColumn(2, vector3s[2]);
        vetexGRP.SetColumn(3, vector3s[3]);

        Matrix4x4 vT = Tanslate * Rot * vetexGRP;

        v1 = vT.GetColumn(0);
        v2 = vT.GetColumn(1);
        v3 = vT.GetColumn(2);
        v4 = vT.GetColumn(3);

        Vector3[] vertexGRP = new Vector3[4];
        vertexGRP[0] = v1;
        vertexGRP[1] = v2;
        vertexGRP[2] = v3;
        vertexGRP[3] = v4;

        return vertexGRP;

    }


    Vector3[] FourConner(float D, float width, float heigh, Vector3 tran)
    {
        Vector3 orgin = new Vector3(0, 0, 0);
        Vector3 d = (D < 0) ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        pCenter = orgin - tran + d * Mathf.Abs(D);
        //pCenter.x = pCenter.x + tran.x;
        //pCenter.z = pCenter.z + tran.z;



        Vector3 xdir = new Vector3(width, 0, 0);
        Vector3 zdir = new Vector3(0, 0, heigh);

        Vector3 bottomLeftDir = Vector3.Normalize(-xdir - zdir);
        Vector3 bottomRightDir = Vector3.Normalize(xdir - zdir);
        Vector3 UpRightDir = Vector3.Normalize(xdir + zdir);
        Vector3 UpLeftDir = Vector3.Normalize(-xdir + zdir);
        float t = Mathf.Sqrt(Mathf.Pow(width / 2.0f, 2) + Mathf.Pow(heigh / 2.0f, 2));

        Vector3 p0 = pCenter + bottomLeftDir * t;
        Vector3 p1 = pCenter + bottomRightDir * t;
        Vector3 p2 = pCenter + UpRightDir * t;
        Vector3 p3 = pCenter + UpLeftDir * t;

        Vector3[] vertexPlaneGizmo = new Vector3[4];
        vertexPlaneGizmo[0] = p0;
        vertexPlaneGizmo[1] = p1;
        vertexPlaneGizmo[2] = p2;
        vertexPlaneGizmo[3] = p3;
        return vertexPlaneGizmo;
    }

    void SetLine(Vector3 a, Vector3 b, LineRenderer lr, Color color, float lineWidth, Material mat)
    {

        lr.material = mat;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        mat.color = color;
    }

    void DrawPlaneGizmo()
    {
        Vector3[] vector3s = new Vector3[4];

        vector3s = FourConner(planeDis, planeGizmoX, planeGizemoZ, tranXYZ);

        gizmoPlaneVertex = RotateVertex(tranXYZ, rotXYZ, vector3s);

        SetLine(gizmoPlaneVertex[0], gizmoPlaneVertex[1], lr0, gizmoColor, 0.01f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[1], gizmoPlaneVertex[2], lr1, gizmoColor, 0.01f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[2], gizmoPlaneVertex[3], lr2, gizmoColor, 0.01f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[3], gizmoPlaneVertex[0], lr3, gizmoColor, 0.01f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[0], gizmoPlaneVertex[2], lr4, gizmoColor, 0.01f, PlaneGizmoMat);

        Vector4 plane = TransformPlane(tranXYZ, rotXYZ, Vector3.Normalize(planeNormal), planeDis);
         pNormal = Vector3.Normalize(new Vector3(plane.x, plane.y, plane.z));

        Debug.Log($"pNormal=={pNormal}");
       
        SetLine(pCenter, pCenter+ pNormal*1f, lr5, Color.green, 0.01f, PlaneGizmoMat);

    }
}

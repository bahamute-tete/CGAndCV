using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneIntersection : MonoBehaviour
{
    public GameObject tragetModel;
    
    public Vector3 R_xyz = new Vector3(0, 0, 0);

    [SerializeField]
    private GameObject Prefab;
    private GameObject[] pointsObj = new GameObject[8];
    private GameObject pointObjContent;

    private Vector4 planeNear = new Vector4(0, 0, 0, 0);

    private Vector4 planeFar = new Vector4(0, 0, 0, 0);
 
    private Vector4 planeLeft= new Vector4(0, 0, 0, 0);

    private Vector4 planeRight = new Vector4(0, 0, 0, 0);

    private Vector4 planeBottom = new Vector4(0, 0, 0, 0);

    private Vector4 planeTop = new Vector4(0, 0, 0, 0);
    [SerializeField]
    private float e = 1.0f;//focus  
    [SerializeField]
    private float n = 0;//near
    [SerializeField]
    private float f = 0;//far
    [SerializeField]
    private float a = 0;//aspect

    private Vector3 cameraPos;
    private Quaternion cameraRotation;
    [SerializeField]
    private GameObject projectionCamera;
    private float projectionCameraOF;
    [SerializeField]
    private GameObject Drone;
    private float droneOffsetZ = 0.0f;
    private Vector3 originDronePos;




    public Vector3 offset = new Vector3(0, 0, 0);

    Vector3[] CameraFustumPoints = new Vector3[8];
    float[] pointDis = new float[12];
    int[] pointDisID = new int[12];



    GameObject LineRenderContent;
    GameObject CameraFustumLr;
    private List<GameObject> fustumlrList = new List<GameObject>();

    [SerializeField]
    private Color lineColor;
    [SerializeField]
    private Material lineMat;
    public float lineWidth = 0.025f;
    /// <summary>
    /// ProjectionPoint
    /// </summary>
    public Vector3[] cubeVertex = new Vector3[8];
     private Vector3[] tempVertex = new Vector3[8];
    private GameObject[] PpointsObj = new GameObject[8];
    private GameObject PpointObjContent;
    Vector3[] PcubeVertex = new Vector3[8];
    [SerializeField]
    private Color lineColor2;
    [SerializeField]
    private Material lineMat2;
    [SerializeField]
    private Color lineColor3;
    [SerializeField]
    private Material lineMat3;
    GameObject ProjectionLrGRP;
    GameObject OriginLrGRP;
    private List<GameObject> ProjectionlrList = new List<GameObject>();
    private List<GameObject> OriginlrList = new List<GameObject>();


    private bool bPerspective = true;
    private bool bOthrographic =false;




    #region
    /// <summary>
    /// UI
    /// </summary>
    /// 
    [SerializeField]
    Slider rx;
    [SerializeField]
    Slider ry;
    [SerializeField]
    Slider rz;
    [SerializeField]
    Slider tx;
    [SerializeField]
    Slider ty;
    [SerializeField]
    Slider tz;
    [SerializeField]
    Button resetBtn;
    [SerializeField]
    Button randomBtn;


    [SerializeField]
    Slider offset_Z;

    [SerializeField]
    Toggle perspective;
    [SerializeField]
    Toggle othrographic;
    #endregion



    void UISet()
    {

        rx.value = R_xyz.x;
        rx.onValueChanged.AddListener(delegate {
            R_xyz.x = rx.value;
        });

        ry.value = R_xyz.y;
        ry.onValueChanged.AddListener(delegate {
            R_xyz.y = ry.value;
        });

        rz.value = R_xyz.z;
        rz.onValueChanged.AddListener(delegate {
            R_xyz.z = rz.value;
        });


        tx.value = offset.x;
        tx.onValueChanged.AddListener(delegate {
            offset.x = tx.value;
        });

        ty.value = offset.y;
        ty.onValueChanged.AddListener(delegate {
            offset.y = ty.value;
        });

        tz.value = offset.z;
        tz.onValueChanged.AddListener(delegate {
            offset.z = tz.value;
        });

        tempVertex = cubeVertex;
        randomBtn.onClick.AddListener(delegate {
            

            for (int i = 0; i < cubeVertex.Length; i++)
            {   
                
                cubeVertex[i].x = tempVertex[i].x +Random.Range(-0.2f, 0.2f);
                cubeVertex[i].y = tempVertex[i].y+Random.Range(-0.2f, 0.2f);
                cubeVertex[i].z = tempVertex[i].z+Random.Range(-0.2f, 0.2f);
            }
           
        });

        n = 1.0f;
        offset_Z.value = droneOffsetZ;
        offset_Z.onValueChanged.AddListener(delegate {
            droneOffsetZ = offset_Z.value;
        });



        resetBtn.onClick.AddListener(delegate {
            R_xyz = new Vector3(0, 0, 0);
            offset = new Vector3(0, 0, 2.0f);
            rx.value = R_xyz.x;
            ry.value = R_xyz.y;
            rz.value = R_xyz.z;

            tx.value = offset.x;
            ty.value = offset.y;
            tz.value = offset.z;

            cubeVertex[0] = new Vector3(1, 1, 1);
            cubeVertex[1] = new Vector3(1, 1, -1);
            cubeVertex[2] = new Vector3(-1, 1, -1);
            cubeVertex[3] = new Vector3(-1, 1, 1);
            cubeVertex[4] = new Vector3(1, -1, 1);
            cubeVertex[5] = new Vector3(1, -1, -1);
            cubeVertex[6] = new Vector3(-1, -1, -1);
            cubeVertex[7] = new Vector3(-1, -1, 1);


            droneOffsetZ =1.38f;
            offset_Z.value = droneOffsetZ;

        });


        bPerspective = true;
        bOthrographic = false;
        perspective.onValueChanged.AddListener(delegate
        {
            bPerspective = true;
            bOthrographic = false;
        });

        othrographic.onValueChanged.AddListener(delegate
        {
            bOthrographic = true;
            bPerspective = false;
        });

    }
    // Start is called before the first frame update
    void Start()
    {
        droneOffsetZ = 1.38f;
        offset = new Vector3(0, 0, 2.0f);

        UISet();


        pointObjContent = new GameObject("PointContent");
        LineRenderContent = new GameObject("LineRenderContent");

        CameraFustumLr = new GameObject("CameraFustumLr");
        CameraFustumLr.transform.SetParent(LineRenderContent.transform);

        fustumlrList.Clear();
        

        for (int i = 0; i < 12; i++)
        {
            GameObject temp = new GameObject("LR" + i);
            temp.transform.SetParent(CameraFustumLr.transform);
            temp.AddComponent<LineRenderer>();
            fustumlrList.Add(temp);
        }

        for (int i = 0; i < pointsObj.Length; i++)
        {
            GameObject temp = Instantiate(Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            temp.transform.SetParent(pointObjContent.transform);
            pointsObj[i] = temp;
        }

        //Projection
        PpointObjContent = new GameObject("ProjectionPoints");

        ProjectionLrGRP = new GameObject("ProjectionlrList");
        ProjectionLrGRP.transform.SetParent(LineRenderContent.transform);

        OriginLrGRP = new GameObject("OriginlrList");
        OriginLrGRP.transform.SetParent(LineRenderContent.transform);

        ProjectionlrList.Clear();
        OriginlrList.Clear();

        for (int i = 0; i < 12; i++)
        {
            GameObject temp = new GameObject("LR" + i);
            temp.transform.SetParent(ProjectionLrGRP.transform);
            temp.AddComponent<LineRenderer>();
           
            ProjectionlrList.Add(temp);

            GameObject temp2 = new GameObject("LR_Origin" + i);
            temp2.transform.SetParent(OriginLrGRP.transform);
            temp2.layer = 8;
            temp2.AddComponent<LineRenderer>();
            OriginlrList.Add(temp2);

        }

        for (int i = 0; i < PpointsObj.Length; i++)
        {
            GameObject temp = Instantiate(Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            temp.layer = 8;
            temp.transform.SetParent(PpointObjContent.transform);
            PpointsObj[i] = temp;
        }

        originDronePos = Drone.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

      
        cameraPos = tragetModel.transform.position;
        cameraRotation = tragetModel.transform.rotation;


        a = 1.0f / projectionCamera.GetComponent<Camera>().aspect;
        e = a / Mathf.Tan(projectionCamera.GetComponent<Camera>().fieldOfView * 0.5f * Mathf.Deg2Rad);
        n = projectionCamera.GetComponent<Camera>().nearClipPlane;
        f = projectionCamera.GetComponent<Camera>().farClipPlane;


        //CameraFrustum
        CameraFustumPoints = GetCameraFrustumPoint(n, f,e,a, cameraRotation, cameraPos);
        for (int i = 0; i < CameraFustumPoints.Length; i++)
        {
            pointsObj[i].transform.position = CameraFustumPoints[i];
        }
        
        for (int i = 0; i < 4; i++)
        {
            
            SetLine(CameraFustumPoints[i], CameraFustumPoints[(i + 1) % 4], fustumlrList[i].GetComponent<LineRenderer>(), lineColor, lineWidth, lineMat);
      
            SetLine(CameraFustumPoints[i+4], CameraFustumPoints[((i + 1) % 4)+4], fustumlrList[i+4].GetComponent<LineRenderer>(), lineColor, lineWidth, lineMat);
       
            SetLine(CameraFustumPoints[i], CameraFustumPoints[i + 4], fustumlrList[i + 8].GetComponent<LineRenderer>(), lineColor, lineWidth, lineMat);

            pointDis[i] = Vector3.Magnitude(CameraFustumPoints[i] - CameraFustumPoints[(i + 1) % 4]);
            pointDis[i + 4] = Vector3.Magnitude(CameraFustumPoints[i + 4] - CameraFustumPoints[((i + 1) % 4) + 4]);
            pointDis[i + 8] = Vector3.Magnitude(CameraFustumPoints[i] - CameraFustumPoints[i + 4]);

            pointDisID[i] = Shader.PropertyToID("_LineWidth");
            pointDisID[i + 4] = Shader.PropertyToID("_LineWidth");
            pointDisID[i + 8] = Shader.PropertyToID("_LineWidth");

            fustumlrList[i].GetComponent<LineRenderer>().material.SetFloat(pointDisID[i], pointDis[i]);
            fustumlrList[i + 4].GetComponent<LineRenderer>().material.SetFloat(pointDisID[i + 4], pointDis[i + 4]);
            fustumlrList[i + 8].GetComponent<LineRenderer>().material.SetFloat(pointDisID[i + 8], pointDis[i + 8]);

        }




        //Projection

        Drone.transform.position = originDronePos +new Vector3(0,0, droneOffsetZ);

        Vector3[] cubeR = new Vector3[cubeVertex.Length];
        cubeR = RotateVertex(R_xyz, cubeVertex);
        Vector3[] cubeTrans = new Vector3[cubeVertex.Length];

        for (int i = 0; i < cubeVertex.Length; i++)
        {
            cubeTrans[i] = cubeR[i] + offset;
        }

        for (int i = 0; i < cubeTrans.Length; i++)
        {
            PpointsObj[i].transform.position = cubeTrans[i];
        }

        PcubeVertex = Projection(cubeTrans, e);


        if (bPerspective)
        {
            projectionCamera.GetComponent<Camera>().orthographic = false;
        }

        if (bOthrographic)
        {
            projectionCamera.GetComponent<Camera>().orthographic = true;
        }
        for (int i = 0; i < 4; i++)
        {

            SetLine(cubeTrans[i], cubeTrans[(i + 1) % 4], OriginlrList[i].GetComponent<LineRenderer>(), lineColor2, lineWidth, lineMat2);

            SetLine(cubeTrans[i + 4], cubeTrans[((i + 1) % 4) + 4], OriginlrList[i + 4].GetComponent<LineRenderer>(), lineColor2, lineWidth, lineMat2);

            SetLine(cubeTrans[i], cubeTrans[i + 4], OriginlrList[i + 8].GetComponent<LineRenderer>(), lineColor2, lineWidth, lineMat2);

            /////////////////////////////////////////////////////////////////////////////////
            SetLine(PcubeVertex[i], PcubeVertex[(i + 1) % 4], ProjectionlrList[i].GetComponent<LineRenderer>(), lineColor3, lineWidth, lineMat3);

            SetLine(PcubeVertex[i + 4], PcubeVertex[((i + 1) % 4) + 4], ProjectionlrList[i + 4].GetComponent<LineRenderer>(), lineColor3, lineWidth, lineMat3);

            SetLine(PcubeVertex[i], PcubeVertex[i + 4], ProjectionlrList[i + 8].GetComponent<LineRenderer>(), lineColor3, lineWidth, lineMat3);

        }


       

    }

    Vector3[] GetCameraFrustumPoint(float n,float f,float e,float a,Quaternion q,Vector3 t)
    {
      

        Vector4 planeNear = new Vector4(0, 0, -1.0f, n);
        Vector4 planeFar = new Vector4(0, 0, 1.0f, -f);
        Vector4 planeLeft = new Vector4(e / Mathf.Sqrt(e * e + 1), 0, -1 / Mathf.Sqrt(e * e + 1), 0);
        Vector4 planeRight = new Vector4(-e / Mathf.Sqrt(e * e + 1), 0, -1 / Mathf.Sqrt(e * e + 1), 0);
        Vector4 planeBottom = new Vector4(0, e / Mathf.Sqrt(e * e + a * a), -a / Mathf.Sqrt(e * e + a * a), 0);
        Vector4 planeTop = new Vector4(0, -e / Mathf.Sqrt(e * e + a * a), -a / Mathf.Sqrt(e * e + a * a), 0);

        Vector3 T_xyz = new Vector3(0, 0, 0);

        Vector4 nplaneNear = TransformPlane(T_xyz,q, planeNear);
        Vector4 nplaneFar = TransformPlane(T_xyz, q, planeFar);
        Vector4 nplaneLeft = TransformPlane(T_xyz, q, planeLeft);
        Vector4 nplaneRight = TransformPlane(T_xyz, q, planeRight);
        Vector4 nplaneBottom = TransformPlane(T_xyz, q, planeBottom);
        Vector4 nplaneTop = TransformPlane(T_xyz, q, planeTop);

        Vector3[] intersections = new Vector3[8];

        intersections[0] = ThreePlaneIntersection(nplaneNear, nplaneBottom, nplaneLeft)+ t;
        intersections[1] = ThreePlaneIntersection(nplaneNear, nplaneBottom, nplaneRight)+ t;
        intersections[2] = ThreePlaneIntersection(nplaneNear, nplaneTop, nplaneRight)+ t;
        intersections[3] = ThreePlaneIntersection(nplaneNear, nplaneTop, nplaneLeft)+ t;

        intersections[4] = ThreePlaneIntersection(nplaneFar, nplaneBottom, nplaneLeft)+ t;
        intersections[5] = ThreePlaneIntersection(nplaneFar, nplaneBottom, nplaneRight)+ t;
        intersections[6] = ThreePlaneIntersection(nplaneFar, nplaneTop, nplaneRight)+ t;
        intersections[7] = ThreePlaneIntersection(nplaneFar, nplaneTop, nplaneLeft)+ t;


        return intersections;

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

    void TwoPlaneIntersection(Vector4 p0, Vector4 p1,float t,LineRenderer lr,Color color,Material material)
    {
        Vector3 p0Normal = Vector3.Normalize(new Vector3(p0.x, p0.y, p0.z));
        Vector3 p1Normal = Vector3.Normalize(new Vector3(p1.x, p1.y, p1.z));

        Vector3 V = Vector3.Normalize(Vector3.Cross(p0Normal, p1Normal));

        Matrix4x4 M = new Matrix4x4();
        M.SetRow(0, p0Normal);
        M.SetRow(1, p1Normal);
        M.SetRow(2, V);
        M.SetRow(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 D = new Matrix4x4();

        D.SetColumn(0, new Vector4(-p0.w,-p1.w, 0 ,1.0f));

        Matrix4x4 Q = M.inverse * D;

        Vector3 q = Q.GetColumn(0);

        Vector3 endPoint = q + V * t;

        

    }

    Vector3 ThreePlaneIntersection(Vector4 p0, Vector4 p1, Vector4 p2)
    {
        Vector3 p0Normal = new Vector3(p0.x, p0.y, p0.z);
        Vector3 p1Normal = new Vector3(p1.x, p1.y, p1.z);
        Vector3 p2Normal = new Vector3(p2.x, p2.y, p2.z);

        Matrix4x4 M = new Matrix4x4();
        M.SetRow(0, p0Normal);
        M.SetRow(1, p1Normal);
        M.SetRow(2, p2Normal);
        M.SetRow(3, new Vector4 (0,0,0,1));

        Matrix4x4 D = new Matrix4x4();

        D.SetColumn(0, new Vector4(-p0.w, -p1.w, -p2.w,1.0f));

        Matrix4x4 Q = new Matrix4x4();
        
        Q = M.inverse * D;
   
        Vector3 q = Q.GetColumn(0);
       
        return q;
    }

    Vector4 TransformPlane(Vector3 T_xyz, Quaternion q, Vector4 L)
    {
        
        Matrix4x4 Rot = Matrix4x4.Rotate(q);
        Matrix4x4 Trans = Matrix4x4.Translate(T_xyz);

        Matrix4x4 customTrans = new Matrix4x4();
        customTrans.SetColumn(0, new Vector4(T_xyz.x, T_xyz.y, T_xyz.z, 0));

        Matrix4x4 InverRot = Rot.inverse;

        Matrix4x4 M_Inver_T = InverRot* customTrans;

        Matrix4x4 IF = new Matrix4x4();
        IF.SetColumn(0, InverRot.GetColumn(0));
        IF.SetColumn(1, InverRot.GetColumn(1));
        IF.SetColumn(2, InverRot.GetColumn(2));
        IF.SetColumn(3, -M_Inver_T.GetColumn(0));
        IF.SetRow(3, new Vector4(0, 0, 0, 1));
      
        Matrix4x4 ITF = Matrix4x4.Transpose(IF);

        Vector4 newP = ITF * L;     
        return newP;
    }

    Vector3[] Projection(Vector3[] points,float d)
    {
        Vector3[] Ppoints = new Vector3[points.Length];

        Matrix4x4 M_Cam_R = new Matrix4x4();
        Matrix4x4 M_Cam_T = new Matrix4x4();
        Matrix4x4 M_Cam = new Matrix4x4();
        Vector3 w = Vector3.Normalize(projectionCamera.GetComponent<Camera>().transform.forward);//froward
        Vector3 t = new Vector3(0, 1, 0);
        Vector3 u =Vector3.Normalize( Vector3.Cross(t, w));//right
        Vector3 v = Vector3.Normalize(Vector3.Cross(w, -u));//up
       

        M_Cam.SetColumn(0, new Vector4(u.x, u.y, u.z, 0  ));
        M_Cam.SetColumn(1, new Vector4(v.x, v.y, v.z, 0  ));
        M_Cam.SetColumn(2, new Vector4(w.x, w.y, w.z, 0  ));
        M_Cam.SetColumn(3, new Vector4(projectionCamera.GetComponent<Camera>().transform.position.x, projectionCamera.GetComponent<Camera>().transform.position.y, projectionCamera.GetComponent<Camera>().transform.position.z, 1));

        Matrix4x4 M_View = new Matrix4x4();
        M_View = M_Cam.inverse;
        //M_View = projectionCamera.GetComponent<Camera>().worldToCameraMatrix;


        Matrix4x4 M_Projection = new Matrix4x4();
        M_Projection.SetColumn(0, new Vector4(1, 0, 0, 0       ));
        M_Projection.SetColumn(1, new Vector4(0, 1, 0, 0       ));
        M_Projection.SetColumn(2, new Vector4(0, 0, 0, -1.0f/d  ));
        M_Projection.SetColumn(3, new Vector4(0, 0, 0, 1       ));

        Matrix4x4 op = new Matrix4x4();
        op.SetColumn(0, new Vector4(1, 0, 0, 0));
        op.SetColumn(1, new Vector4(0, 1, 0, 0));
        op.SetColumn(2, new Vector4(0, 0, 0,0));
        op.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 PerspectiveCanonic = new Matrix4x4();
        PerspectiveCanonic.SetRow(0, new Vector4(e, 0, 0, 0));
        PerspectiveCanonic.SetRow(1, new Vector4(0, e / a, 0, 0));
        PerspectiveCanonic.SetRow(2, new Vector4(0, 0, -(f + n) / (f - n), -2.0f * f * n / (f - n)));
        PerspectiveCanonic.SetRow(3, new Vector4(0, 0, -1.0f, 0));
        //PerspectiveCanonic = projectionCamera.GetComponent<Camera>().projectionMatrix;

        Matrix4x4 OrthographicCanonic = new Matrix4x4();
        OrthographicCanonic.SetRow(0, new Vector4(e, 0, 0, 0));
        OrthographicCanonic.SetRow(1, new Vector4(0, e/a, 0, 0));
        OrthographicCanonic.SetRow(2, new Vector4(0, 0,-2.0f/(f-n),-(n+f)/(f-n)));
        OrthographicCanonic.SetRow(3, new Vector4(0, 0,0, 1));
       


        Matrix4x4 M_vp = new Matrix4x4();
        M_vp.SetColumn(0, new Vector4((Screen.width)/2.0f, 0, 0, Screen.width*0.5f-0.5f));
        M_vp.SetColumn(1, new Vector4(0, (Screen.height) / 2.0f, 0, Screen.height* 0.5f - 0.5f));
        M_vp.SetColumn(2, new Vector4(0, 0, 1,0));
        M_vp.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 M_vp2 = new Matrix4x4();
        M_vp2.SetColumn(0, new Vector4(-e/n,  0,   0,    0));
        M_vp2.SetColumn(1, new Vector4(0,  e*a/n, 0,    0));
        M_vp2.SetColumn(2, new Vector4(0,    0,   1.0f, 0));
        M_vp2.SetColumn(3, new Vector4(0,    0,   0,    1));

        Matrix4x4 M_vp3 = new Matrix4x4();
        M_vp3.SetColumn(0, new Vector4(e, 0, 0, 0));
        M_vp3.SetColumn(1, new Vector4(0, -e*a, 0, 0));
        M_vp3.SetColumn(2, new Vector4(0, 0, 1.0f, 0));
        M_vp3.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4[] Phs = new Matrix4x4[points.Length];
        for (int i = 0; i < Phs.Length; i++)
        {
            Phs[i].SetColumn(0,new Vector4(points[i].x, points[i].y, points[i].z,1.0f));

            Matrix4x4 temp1 = new Matrix4x4();

            if (bPerspective)
            {
                 temp1 =  M_vp2* op * PerspectiveCanonic* M_View*Phs[i]; 
            }

            if (bOthrographic)
            {
                temp1 = M_vp3 *  op* OrthographicCanonic * M_View * Phs[i];
            }

            Ppoints[i] = (temp1.GetColumn(0) / temp1.GetColumn(0).w);

        }
        return Ppoints;
    }

    Vector3[] RotateVertex(Vector3 R_xyz, Vector3[] vector3s)
    {

        Vector3[] vertexGRP = new Vector3[vector3s.Length];

        

        Quaternion q = Quaternion.Euler(R_xyz.x, R_xyz.y, R_xyz.z);

        Matrix4x4 R = Matrix4x4.Rotate(q);

        Matrix4x4[] Phs = new Matrix4x4[vector3s.Length];
        for (int i = 0; i < Phs.Length; i++)
        {
            Phs[i].SetColumn(0, new Vector4(vector3s[i].x, vector3s[i].y, vector3s[i].z, 1.0f));

            Matrix4x4 temp = R * Phs[i];

            vertexGRP[i] = temp.GetColumn(0);
        }

        return vertexGRP;

    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class RayIntersection : MonoBehaviour
{
    #region Triangle 
    [Header("Triangle")]
    public Vector3[] trianglePos = new Vector3[3];
    private LineRenderer triangelLr = new LineRenderer();
    [SerializeField]
    private Material lineMat;
    [SerializeField]
    private float lineWidth;
    [SerializeField]
    private Color lineColor = new Vector4(1, 1, 1, 1);
    struct Triangel
    {
        public Vector3[] vertexs;
    }
    Triangel tesTriangle;
    GameObject gameObject1;
    #endregion

    #region Ray 
    [Header("Ray")]
    [SerializeField]
    GameObject rayCaster;
    private Vector3 rayCasterOriDir;
    private Vector3 ro;
    [SerializeField] private Vector3 rd = new Vector3(0,0,1);
    [SerializeField] private float dis = 10.0f;
    public Vector3 Ray_xyz = new Vector3(0, 0, 1);
    private LineRenderer rayLr = new LineRenderer();
    [SerializeField]
    private Material lineMat2;
    [SerializeField]
    private float lineWidth2;
    [SerializeField]
    private Color lineColor2 = new Vector4(1, 1, 1, 1);
    struct RayTraceRay
    {
        public Vector3 ro;
        public Vector3 rd;
        public float dis;
    }
    RayTraceRay rayTrace;
    Vector3 IntersectionPoint;
    #endregion

    #region Cube 
    GameObject LineRenderContent;
    [Header("Cube")]
    public Vector3[] cubeVertex = new Vector3[8];
    public Vector3 R_xyz = new Vector3(0, 0, 0);
    public Vector3 S_xyz = new Vector3(0, 0, 0);
    public Vector3 offset = new Vector3(0, 0, 0);
    GameObject OriginLrGRP;
    private List<GameObject> OriginlrList = new List<GameObject>();
    [SerializeField]
    private Material lineMat3;
    [SerializeField]
    private Color lineColor3 = new Vector4(1, 1, 1, 1);
    GameObject gameObject2;
    struct Cube
    {
        public Vector3[] vertexs;
    }
    Cube testCube;
    public Vector3[] cubeInObj = new Vector3[8];
    private float an=0;
    #endregion

    #region Cylinder 
    [Header("Cylinder")]
    public float cr = 1.0f;
    public float ch = 2.0f;
    public int segment = 16;
    public Vector3 cylinderOffsets = new Vector3(0, 0, 0);
    private Vector3 center = new Vector3(0, 0, 0);
    LineRenderer topCapLr = new LineRenderer();
    LineRenderer bottomCapLr = new LineRenderer();
    LineRenderer colLr = new LineRenderer();
    [SerializeField] private Material lineMat4;
    [SerializeField]  private Color lineColor4 = new Vector4(1, 1, 1, 1);
    struct Cylinder
    {
        public float cylinderR;
        public float cylinderS;
        public float cylinderH;
        public Vector3 center;
        public int cylinderSegment;
        
    }
    Cylinder testCylinder;
    #endregion

    #region CartesionCoordinates 
    [Header("CartesionCoordinates")]
    [SerializeField] int MaxNumber;
    [SerializeField] float Width;
    [SerializeField] Color x=Color.red;
    [SerializeField] Color y=Color.green;
    [SerializeField] Color z=Color.blue;
    GameObject CartesianAixeLrGRPX;
    private List<GameObject> CartesianAixeX = new List<GameObject>();
    GameObject CartesianAixeLrGRPY;
    private List<GameObject> CartesianAixeY = new List<GameObject>();
    GameObject CartesianAixeLrGRPZ;
    private List<GameObject> CartesianAixeZ = new List<GameObject>();
    [SerializeField]
    GameObject arrowObj;
    #endregion

    #region UI 
    [Header("UI")]
    [SerializeField] private Slider xr;
    [SerializeField] private Slider yr;
     private Slider zr;
    [SerializeField] private Text intersectionPoint;
    [SerializeField] private Button resetBtn;
    [SerializeField] private Toggle Rotation;
    #endregion

    private List<float> tGRP = new List<float>();

    void UISet()
    {
        xr.value = Ray_xyz.x;
        xr.onValueChanged.AddListener(delegate {
            Ray_xyz.x = xr.value;
        });

        yr.value = Ray_xyz.y;
        yr.onValueChanged.AddListener(delegate {
            Ray_xyz.y = yr.value;
        });


        resetBtn.onClick.AddListener(delegate {
            Ray_xyz = new Vector3(0, 0, 0);
            xr.value = Ray_xyz.x;
            yr.value = Ray_xyz.y;

        });



    }

    // Start is called before the first frame update
    void Start()
    {
        UISet();
        LineRenderContent = new GameObject("LineRenderContent");

        GameObject emptyObj = new GameObject("Content");
        triangelLr = emptyObj.AddComponent<LineRenderer>();
        triangelLr.positionCount = trianglePos.Length + 1;
        TriangleSet();


        GameObject emptyObj1 = new GameObject("Ray");
        rayLr = emptyObj1.AddComponent<LineRenderer>();
        rayLr.positionCount = 2;
        rayCasterOriDir = rayCaster.transform.forward;

        gameObject1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObject1.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        gameObject1.GetComponent<MeshRenderer>().material.shader =Shader.Find("Unlit/Color");
        gameObject1.GetComponent<MeshRenderer>().sharedMaterial.color = lineColor;

        CubeInitial();

        GameObject tcapLrC;
        GameObject bcapLrC;
        GameObject colLrC;

        bcapLrC = new GameObject("BCapLrContent");
        bottomCapLr = bcapLrC.AddComponent<LineRenderer>();
        tcapLrC = new GameObject("TCapLrContent");
        topCapLr = tcapLrC.AddComponent<LineRenderer>();
        colLrC = new GameObject("ColLrContent");
        colLr = colLrC.AddComponent<LineRenderer>();

        //DrawCylinder(center,cr,ch,16,lineMat,0.05f,lineColor);

        DrawCartesianCoordinates(MaxNumber, Width, x, y, z);

    }

   
    // Update is called once per frame
    void Update()
    {
        if (Rotation.isOn)
        {
            an++;
            if (an < -360) an += 360;
            if (an > 360) an -= 360;
            R_xyz = new Vector3(an, an, an);
          


        }
        else
        {
            R_xyz = new Vector3(0, 0, 0);
        }

        CylinderSet();
        RayTranceRaySet();
        DrawTriangel(tesTriangle);
        DrawCube();
        DrawCylinder(testCylinder, lineMat4, lineWidth, lineColor4);
        Intersection();
    }

    #region Method
    
    void CylinderSet()
    {
        testCylinder.center = center + cylinderOffsets;
        testCylinder.cylinderH = ch;
        testCylinder.cylinderR = cr;
        testCylinder.cylinderS = cr;
        testCylinder.cylinderSegment = segment;
    }
    void TriangleSet()
    {
        tesTriangle.vertexs = trianglePos;
    }
    void RayTranceRaySet()
    {
        Quaternion q = Quaternion.Euler(Ray_xyz);
       // rayCaster.transform.forward = q * rayCasterOriDir;
        rayTrace.dis = dis;
        rayTrace.ro = rayCaster.transform.position;
        rayTrace.rd = q*rayCaster.transform.forward;
    }
    void CubeInitial()
    {
        OriginlrList.Clear();
        OriginLrGRP = new GameObject("OriginlrList");
        OriginLrGRP.transform.SetParent(LineRenderContent.transform);

        for (int i = 0; i < 12; i++)
        {

            GameObject temp2 = new GameObject("LR_Origin" + i);
            temp2.transform.SetParent(OriginLrGRP.transform);
            temp2.layer = 8;
            temp2.AddComponent<LineRenderer>();
            OriginlrList.Add(temp2);

        }
        testCube.vertexs = cubeVertex;

    }


    void SetLine(Vector3 a, Vector3 b, LineRenderer lr, Color color, float lineWidth, Material mat)
    {

        lr.material = mat;
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        mat.color = color;

    }
    void SetLine(Vector3 a, Vector3 b, Vector3 c, LineRenderer lr, Color color, float lineWidth, Material mat)
    {

        lr.material = mat;
        lr.positionCount = 3;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.SetPosition(2, c);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        mat.color = color;

    }


    void DrawCartesianCoordinates(int size, float aixeWidth, Color x, Color y, Color z)
    {
        float lineWidth = aixeWidth;

        CartesianAixeLrGRPX = new GameObject("CartesianAixeLrGRPX");
        CartesianAixeLrGRPX.transform.SetParent(LineRenderContent.transform);

        CartesianAixeLrGRPY = new GameObject("CartesianAixeLrGRPY");
        CartesianAixeLrGRPY.transform.SetParent(LineRenderContent.transform);

        CartesianAixeLrGRPZ = new GameObject("CartesianAixeLrGRPZ");
        CartesianAixeLrGRPZ.transform.SetParent(LineRenderContent.transform);

        for (int i = 0; i <= size; i++)
        {
            GameObject temp = new GameObject("CartesionAixeX" + i);
            temp.transform.SetParent(CartesianAixeLrGRPX.transform);
            temp.AddComponent<LineRenderer>();
            CartesianAixeX.Add(temp);

            GameObject temp1 = new GameObject("CartesionAixeY" + i);
            temp1.transform.SetParent(CartesianAixeLrGRPY.transform);
            temp1.AddComponent<LineRenderer>();
            CartesianAixeY.Add(temp1);

            GameObject temp2 = new GameObject("CartesionAixeZ" + i);
            temp2.transform.SetParent(CartesianAixeLrGRPZ.transform);
            temp2.AddComponent<LineRenderer>();
            CartesianAixeZ.Add(temp2);
        }


        Vector3 xdir = Vector3.zero;
        Vector3 ydir = Vector3.zero;
        Vector3 zdir = Vector3.zero;

        Vector3 xstart = Vector3.zero;
        Vector3 ystart = Vector3.zero;
        Vector3 zstart = Vector3.zero;

        for (int i = 0; i <= size; i++)
        {
            xdir += Vector3.zero + new Vector3(1, 0, 0);
            Vector3 xmark = xdir + new Vector3(0, 1, 0) * 0.3f;

            ydir += Vector3.zero + new Vector3(0, 1, 0);
            Vector3 ymark = ydir + new Vector3(1, 0, 0) * 0.3f;

            zdir += Vector3.zero + new Vector3(0, 0, 1);
            Vector3 zmark = zdir + new Vector3(0, 1, 0) * 0.3f;

            if (i != size)
            {
                SetLine(xstart, xdir, xmark, CartesianAixeX[i].GetComponent<LineRenderer>(), x, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                xstart = xdir;

                SetLine(ystart, ydir, ymark, CartesianAixeY[i].GetComponent<LineRenderer>(), y, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                ystart = ydir;

                SetLine(zstart, zdir, zmark, CartesianAixeZ[i].GetComponent<LineRenderer>(), z, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                zstart = zdir;
            }
            else
            {
                SetLine(xdir, Vector3.zero, CartesianAixeX[i].GetComponent<LineRenderer>(), x, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                xstart = xdir;
                Instantiate(arrowObj, xstart, Quaternion.Euler(0, 0, 270));

                SetLine(ydir, Vector3.zero, CartesianAixeY[i].GetComponent<LineRenderer>(), y, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                ystart = ydir;
                Instantiate(arrowObj, ystart, Quaternion.Euler(0, 0, 0));

                SetLine(zdir, Vector3.zero, CartesianAixeZ[i].GetComponent<LineRenderer>(), z, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                zstart = zdir;
                Instantiate(arrowObj, zstart, Quaternion.Euler(90, 0, 0));
            }

        }
    }
    void DrawCube()
    {
        Vector3[] cubeR = new Vector3[cubeVertex.Length];
        cubeR = RotateVertex(R_xyz, S_xyz, cubeVertex);
        Vector3[] cubeTrans = new Vector3[cubeVertex.Length];

        for (int i = 0; i < cubeVertex.Length; i++)
        {
            cubeTrans[i] = cubeR[i] + offset;
        }


        for (int i = 0; i < 4; i++)
        {

            SetLine(cubeTrans[i], cubeTrans[(i + 1) % 4], OriginlrList[i].GetComponent<LineRenderer>(), lineColor3, lineWidth, lineMat3);

            SetLine(cubeTrans[i + 4], cubeTrans[((i + 1) % 4) + 4], OriginlrList[i + 4].GetComponent<LineRenderer>(), lineColor3, lineWidth, lineMat3);

            SetLine(cubeTrans[i], cubeTrans[i + 4], OriginlrList[i + 8].GetComponent<LineRenderer>(), lineColor3, lineWidth, lineMat3);


        }
    }
    void DrawTriangel(Triangel t)
    {
        triangelLr.SetPosition(0, t.vertexs[0]);
        triangelLr.SetPosition(1, t.vertexs[1]);
        triangelLr.SetPosition(2, t.vertexs[2]);
        triangelLr.SetPosition(3, t.vertexs[0]);
        triangelLr.material = lineMat;
        triangelLr.startWidth = lineWidth;
        triangelLr.endWidth = lineWidth;
        triangelLr.numCapVertices = 4;
        triangelLr.numCornerVertices = 4;
        lineMat.color = lineColor;
    }
    void DrawRay(RayTraceRay r,Color c)
    {

        r.rd = Vector3.Normalize(r.rd);
        Vector3 s = r.ro +r.rd*r.dis;

        rayLr.SetPosition(0, r.ro);
        rayLr.SetPosition(1, s);

        rayLr.material = lineMat2;
        rayLr.startWidth = lineWidth2;
        rayLr.endWidth = lineWidth2;
        rayLr.numCapVertices = 4;
        rayLr.numCornerVertices = 4;
        lineMat2.color = c;


       
    }
    void DrawCylinder(Cylinder cy, Material mat, float lineWidth, Color lineColor)
    {

        List<Vector3> topCapCircles = new List<Vector3>();
        List<Vector3> bottomCapCircles = new List<Vector3>();
        List<Vector3> colvertex = new List<Vector3>();

        Vector3 cird = new Vector3(1, 0, 0);
        topCapCircles.Clear(); bottomCapCircles.Clear(); colvertex.Clear();

        segment = Mathf.Max(3, Mathf.Min(cy.cylinderSegment, 32));

        float dtheta = 360.0f / cy.cylinderSegment;


        for (int i = 0; i < cy.cylinderSegment; i++)
        {
            Quaternion q = Quaternion.Euler(0, i * dtheta, 0);
            Vector3 temp0 = cy.center + (q * cird) * cy.cylinderR;
            Vector3 temp1 = temp0 - new Vector3(0, cy.cylinderH / 2, 0);
            Vector3 temp2 = temp0 + new Vector3(0, cy.cylinderH / 2, 0);

            topCapCircles.Add(temp1);
            bottomCapCircles.Add(temp2);
            colvertex.Add(temp1);
            colvertex.Add(temp2);
        }

        topCapLr.positionCount = bottomCapLr.positionCount = cy.cylinderSegment + 1;


        topCapCircles.Add(topCapCircles[0]);
        topCapLr.SetPositions(topCapCircles.ToArray());
        bottomCapCircles.Add(bottomCapCircles[0]);
        bottomCapLr.SetPositions(bottomCapCircles.ToArray());


        colLr.positionCount = cy.cylinderSegment * 2;
        colLr.SetPositions(colvertex.ToArray());
        colLr.numCornerVertices = 1;


        topCapLr.material = bottomCapLr.material = colLr.material = mat;
        topCapLr.startWidth = bottomCapLr.startWidth = lineWidth;
        topCapLr.endWidth = bottomCapLr.endWidth = lineWidth;

        colLr.startWidth = colLr.endWidth = lineWidth * 0.35f;
        mat.color = lineColor;

    }


    bool TriangelIntersect(RayTraceRay ray, Triangel t ,out float ti)
    {
       
        Matrix4x4 M = new Matrix4x4();
        Vector4 v0 = new Vector4(t.vertexs[0].x - t.vertexs[1].x, t.vertexs[0].x - t.vertexs[2].x, ray.rd.x, 0);
        Vector4 v1 = new Vector4(t.vertexs[0].y - t.vertexs[1].y, t.vertexs[0].y - t.vertexs[2].y, ray.rd.y, 0);
        Vector4 v2 = new Vector4(t.vertexs[0].z - t.vertexs[1].z, t.vertexs[0].z - t.vertexs[2].z, ray.rd.z, 0);
        Vector4 v3 = new Vector4( 0, 0, 0, 1);
        M.SetRow(0, v0);
        M.SetRow(1, v1);
        M.SetRow(2, v2);
        M.SetRow(3, v3);

        Matrix4x4 beta = new Matrix4x4();
        Vector4 b0 = new Vector4(t.vertexs[0].x -ray.ro.x, t.vertexs[0].x - t.vertexs[2].x, ray.rd.x, 0);
        Vector4 b1 = new Vector4(t.vertexs[0].y -ray.ro.y, t.vertexs[0].y - t.vertexs[2].y, ray.rd.y, 0);
        Vector4 b2 = new Vector4(t.vertexs[0].z -ray.ro.z, t.vertexs[0].z - t.vertexs[2].z, ray.rd.z, 0);
        Vector4 b3 = new Vector4(0, 0, 0, 1);
        beta.SetRow(0, b0);
        beta.SetRow(1, b1);
        beta.SetRow(2, b2);
        beta.SetRow(3, b3);

        Matrix4x4 gama = new Matrix4x4();
        Vector4 g0 = new Vector4(t.vertexs[0].x - t.vertexs[1].x, t.vertexs[0].x - ray.ro.x, ray.rd.x, 0);
        Vector4 g1 = new Vector4(t.vertexs[0].y - t.vertexs[1].y, t.vertexs[0].y - ray.ro.y, ray.rd.y, 0);
        Vector4 g2 = new Vector4(t.vertexs[0].z - t.vertexs[1].z, t.vertexs[0].z - ray.ro.z, ray.rd.z, 0);
        Vector4 g3 = new Vector4(0, 0, 0, 1);
        gama.SetRow(0, g0);
        gama.SetRow(1, g1);
        gama.SetRow(2, g2);
        gama.SetRow(3, g3);

        Matrix4x4 tt = new Matrix4x4();
        Vector4 tt0 = new Vector4(t.vertexs[0].x - t.vertexs[1].x, t.vertexs[0].x - t.vertexs[2].x, t.vertexs[0].x - ray.ro.x, 0);
        Vector4 tt1 = new Vector4(t.vertexs[0].y - t.vertexs[1].y, t.vertexs[0].y - t.vertexs[2].y, t.vertexs[0].y - ray.ro.y, 0);
        Vector4 tt2 = new Vector4(t.vertexs[0].z - t.vertexs[1].z, t.vertexs[0].z - t.vertexs[2].z, t.vertexs[0].z - ray.ro.z, 0);
        Vector4 tt3 = new Vector4(0, 0, 0, 1);
        tt.SetRow(0, tt0);
        tt.SetRow(1, tt1);
        tt.SetRow(2, tt2);
        tt.SetRow(3, tt3);

        float b = 0, r = 0; 
        ti = 0;

        ti = tt.determinant / M.determinant;
        if (ti < 0 || ti > dis)
        {
            ti = 0;
            return false;
        }
           

        r = gama.determinant / M.determinant;
        if (r < 0 || r > 1)
        {
            ti = 0;
            return false;
        }
          

        b = beta.determinant / M.determinant;
        if (b < 0 || b > 1 - r)
        {
            ti = 0;
            return false;
        }

        return true;   

    }
    bool CubeIntersect(RayTraceRay r, Cube c, Vector3 offset, out float t)
    {
        t = 0;
        //Transform Cube and Ray in ObjectSpace
        ////////////////////////////////////////////////////////////////
        Vector3[] cubeR = new Vector3[c.vertexs.Length];
        cubeR = RotateVertex(R_xyz, S_xyz, cubeVertex);

        Vector3[] cubeTrans = new Vector3[c.vertexs.Length];

        for (int i = 0; i < c.vertexs.Length; i++)
        {
            cubeTrans[i] = cubeR[i] + offset;
        }

        Vector3 u = Vector3.Normalize(cubeTrans[0] - cubeTrans[3]);
        Vector3 w = Vector3.Normalize(cubeTrans[0] - cubeTrans[1]);
        Vector3 v = Vector3.Normalize(cubeTrans[0] - cubeTrans[4]);

        Matrix4x4 M_obj = new Matrix4x4();
        M_obj.SetColumn(0, u);
        M_obj.SetColumn(1, v);
        M_obj.SetColumn(2, w);
        M_obj.SetColumn(3, new Vector4(offset.x, offset.y, offset.z, 1));

        cubeInObj = new Vector3[8];
        Matrix4x4[] Phs = new Matrix4x4[c.vertexs.Length];
        for (int i = 0; i < Phs.Length; i++)
        {
            Phs[i].SetColumn(0, new Vector4(cubeTrans[i].x, cubeTrans[i].y, cubeTrans[i].z, 1.0f));

            Matrix4x4 temp1 = new Matrix4x4();

            temp1 = M_obj.inverse * Phs[i];

            cubeInObj[i] = (temp1.GetColumn(0) / temp1.GetColumn(0).w);
        }

        Vector3 roo = Vector3.zero;
        Vector3 rdo = Vector3.zero;


        Matrix4x4 lightInMatix = new Matrix4x4();
        lightInMatix.SetColumn(0, new Vector4(rayCaster.transform.position.x, rayCaster.transform.position.y, rayCaster.transform.position.z, 1.0f));
        Matrix4x4 temp2 = new Matrix4x4();
        temp2 = M_obj.inverse * lightInMatix;
        roo = (temp2.GetColumn(0) / temp2.GetColumn(0).w);

        Quaternion q = Quaternion.Euler(R_xyz.x, R_xyz.y, R_xyz.z);
        Matrix4x4 R = Matrix4x4.Rotate(q);
        Matrix4x4 lightrdInMatix = new Matrix4x4();
        
        lightrdInMatix.SetColumn(0, new Vector4(r.rd.x,r.rd.y, r.rd.z, 1.0f));
        Matrix4x4 temp3 = new Matrix4x4();
        temp3 = R.inverse * lightrdInMatix;
        rdo = (temp3.GetColumn(0) / temp3.GetColumn(0).w);
        /////////////////////////////////////////////////////////

        #region wordSpace
        //float sizeX = Vector3.Magnitude(c.vertexs[0] - c.vertexs[3]);
        //float sizeY = Vector3.Magnitude(c.vertexs[0] - c.vertexs[4]);
        //float sizeZ = Vector3.Magnitude(c.vertexs[0] - c.vertexs[1]);
        //Vector3 size = new Vector3(sizeX, sizeY, sizeZ) * 0.5f;
        //Debug.Log("size==" + size);

        //Vector3 centerPos = cubeTrans[0] - size;
        //float leftPlane = centerPos.x - size.x;
        //float rightPlane = centerPos.x + size.x;

        //float topPlane = centerPos.y + size.y;
        //float bottomPlane = centerPos.y - size.y;

        //float farPlane = centerPos.z + size.z;
        //float nearPlane = centerPos.z - size.z;

        //float tx = 0;
        //float ty = 0;
        //float tz = 0;


        //if (r.ro.x > rightPlane || r.ro.x < leftPlane)
        //{
        //    tx = (r.rd.x > 0) ? (leftPlane - r.ro.x) / r.rd.x : (rightPlane - r.ro.x) / r.rd.x;
        //}
        //else
        //{
        //    tx = 0;
        //}

        //if (r.ro.y > topPlane || r.ro.y < bottomPlane)
        //{
        //    ty = (r.rd.y > 0) ? (bottomPlane - r.ro.y) / r.rd.y : (topPlane - r.ro.y) / r.rd.y;
        //}
        //else
        //{
        //    ty = 0;
        //}

        //if (r.ro.z > farPlane || r.ro.z < nearPlane)
        //{
        //    tz = (r.rd.z > 0) ? (nearPlane - r.ro.z) / r.rd.z : (farPlane - r.ro.z) / r.rd.z;
        //}
        //else
        //{
        //    tz = 0;
        //}

        //Vector3 endpos;
        //if (tx > 0)
        //{
        //    endpos = r.ro + r.rd * tx;
        //    if (endpos.y > bottomPlane && endpos.y < topPlane && endpos.z > nearPlane && endpos.z < farPlane)
        //    {
        //        t = tx;
        //        return true;
        //    }
        //}
        //if (ty > 0)
        //{
        //    endpos = r.ro + r.rd * ty;
        //    if (endpos.x > leftPlane && endpos.x < rightPlane && endpos.z > nearPlane && endpos.z < farPlane)
        //    {
        //        t = ty;
        //        return true;
        //    }

        //}
        //if (tz > 0)
        //{
        //    endpos = r.ro + r.rd * tz;
        //    if (endpos.x > leftPlane && endpos.x < rightPlane && endpos.y > bottomPlane && endpos.y < topPlane)
        //    {
        //        t = tz;
        //        return true;
        //    }

        //}
        #endregion

        #region ObjectSpace
        float sizeXO = Vector3.Magnitude(cubeInObj[0] - cubeInObj[3]);
        float sizeYO = Vector3.Magnitude(cubeInObj[0] - cubeInObj[4]);
        float sizeZO= Vector3.Magnitude(cubeInObj[0] - cubeInObj[1]);
        Vector3 sizeO = new Vector3(sizeXO, sizeYO, sizeZO) * 0.5f;

        float leftPlaneO = - sizeO.x;
        float rightPlaneO =  sizeO.x;

        float topPlaneO =   sizeO.y;
        float bottomPlaneO =  -sizeO.y;

        float farPlaneO =  sizeO.z;
        float nearPlaneO = -sizeO.z;

        float txo = 0;
        float tyo = 0;
        float tzo = 0;


        if (roo.x > rightPlaneO || roo.x < leftPlaneO)
        {
            txo = (rdo.x > 0) ? (leftPlaneO - roo.x) / rdo.x : (rightPlaneO - roo.x) / rdo.x;
        }
        else
        {
            txo = 0;
        }

        if (roo.y > topPlaneO || roo.y < bottomPlaneO)
        {
            tyo = (rdo.y > 0) ? (bottomPlaneO - roo.y) / rdo.y : (topPlaneO - roo.y) / rdo.y;
        }
        else
        {
            tyo = 0;
        }

        if (roo.z > farPlaneO || roo.z < nearPlaneO)
        {
            tzo = (rdo.z > 0) ? (nearPlaneO - roo.z) / rdo.z : (farPlaneO - roo.z) / rdo.z;
        }
        else
        {
            tzo = 0;
        }

        Vector3 endposo;
        if (txo > 0)
        {
            endposo = roo + rdo * txo;
            if (endposo.y > bottomPlaneO && endposo.y < topPlaneO && endposo.z > nearPlaneO && endposo.z < farPlaneO)
            {
                t = txo;
                return true;
            }
        }
        if (tyo > 0)
        {
            endposo = roo + rdo * tyo;
            if (endposo.x > leftPlaneO && endposo.x < rightPlaneO && endposo.z > nearPlaneO && endposo.z < farPlaneO)
            {
                t = tyo;
                return true;
            }

        }
        if (tzo > 0)
        {
            endposo = roo + rdo * tzo;
            if (endposo.x > leftPlaneO && endposo.x < rightPlaneO && endposo.y > bottomPlaneO && endposo.y < topPlaneO)
            {
                t = tzo;
                return true;
            }

        }
        #endregion
        return false;
    }
    bool CylinderInterset(RayTraceRay r, Cylinder cylinder, Vector3 offset, out float t) //m=r/s
    {

        float m = cylinder.cylinderR / cylinder.cylinderS;
        float radiu = cylinder.cylinderR;
        float a = r.rd.x * r.rd.x + m * m * r.rd.z * r.rd.z;
        float b = 2 * (r.ro.x * r.rd.x + m * m * r.ro.z * r.rd.z- offset.x*r.rd.x-offset.z*r.rd.z);
        float c =r.ro.x*r.ro.x+ m * m * r.ro.z * r.ro.z - radiu * radiu -2*(r.ro.x*offset.x+r.ro.z*offset.z)+offset.x*offset.x +offset.z*offset.z;

        float d2 = b * b - 4 * a * c;
        t = 0;
        float tx = (d2 > 0) ? (-b - Mathf.Sqrt(d2)) / (2 * a) : 0;
      
        Vector3 p = r.ro + r.rd * tx;
        float dyp = Mathf.Abs(p.y - cylinder.center.y);
        if (dyp < cylinder.cylinderH/2)
        {
            t = tx;
            return true;
        }

        return false;
       
    }


    void BestProjectionPlane(Vector3 pos)
    {
        if (Mathf.Abs(pos.z) > Mathf.Abs(pos.x) && Mathf.Abs(pos.z) > Mathf.Abs(pos.y))
            Debug.Log("BestPlane is XY");
        else if (Mathf.Abs(pos.y) > Mathf.Abs(pos.x))
            Debug.Log("BestPlane is XZ");
        else
            Debug.Log("BestPlane is ZY");
    }


    Vector3[] RotateVertex(Vector3 R_xyz,Vector3 S_xyz, Vector3[] vector3s)
    {

        Vector3[] vertexGRP = new Vector3[vector3s.Length];

        Quaternion q = Quaternion.Euler(R_xyz.x, R_xyz.y, R_xyz.z);

        Matrix4x4 R = Matrix4x4.Rotate(q);
        Matrix4x4 S = Matrix4x4.Scale(S_xyz);

        Matrix4x4[] Phs = new Matrix4x4[vector3s.Length];
        for (int i = 0; i < Phs.Length; i++)
        {
            Phs[i].SetColumn(0, new Vector4(vector3s[i].x, vector3s[i].y, vector3s[i].z, 1.0f));

            Matrix4x4 temp = R * S* Phs[i];

            vertexGRP[i] = temp.GetColumn(0);
        }
        return vertexGRP;
    }


    void Intersection()
    {

        tGRP.Clear();
        float tt = 0, ct = 0, cyt = 0, t=0;
        bool bt, bc, bcy;
        Color rayColor =lineColor2;

        bt = TriangelIntersect(rayTrace, tesTriangle, out tt);
        bc = CubeIntersect(rayTrace, testCube, offset, out ct);
        bcy = CylinderInterset(rayTrace, testCylinder, cylinderOffsets, out cyt);

        //Debug.Log("tt==" + tt);
        //Debug.Log("ct==" + ct);
        //Debug.Log("cyt==" + cyt);


        tGRP.Add(tt); tGRP.Add(ct); tGRP.Add(cyt);

        tGRP.Sort((x, y) => x.CompareTo(y));

        for (int i = 0; i < tGRP.Count; i++)
        {
            if (tGRP[i] != 0)
            {
                t = tGRP[i];
                break;
            }
        }


        if (t != 0)
        {
            if (t == tt)
            {
                rayTrace.dis = tt;
                rayColor = lineColor;
                gameObject1.GetComponent<MeshRenderer>().sharedMaterial.color = lineColor;
            }

            if (t == ct)
            {

                rayTrace.dis = ct;
                rayColor = lineColor3;
                gameObject1.GetComponent<MeshRenderer>().sharedMaterial.color = lineColor3;
            }

            if (t == cyt)
            {
                rayColor = lineColor4;
                rayTrace.dis = cyt;
                gameObject1.GetComponent<MeshRenderer>().sharedMaterial.color = lineColor4;
            }

            intersectionPoint.enabled = true;
            Vector2 scrPropos = RectTransformUtility.WorldToScreenPoint(Camera.main, IntersectionPoint);
            intersectionPoint.rectTransform.position = scrPropos + new Vector2(0, 35);
            intersectionPoint.text = IntersectionPoint.ToString();
        }
        else
        {
            intersectionPoint.enabled = false;
            rayColor = lineColor2;
            rayTrace.dis = dis;
        }
     
        IntersectionPoint = rayTrace.ro + rayTrace.rd * t;
        DrawRay(rayTrace, rayColor);
        gameObject1.transform.position = IntersectionPoint;

        
    }

   

    #endregion

}

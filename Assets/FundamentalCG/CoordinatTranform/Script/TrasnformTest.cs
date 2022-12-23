using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;







public class TrasnformTest : MonoBehaviour
{

    [Header("Cube")]
    GameObject LineRenderContent;
    public Vector3[] cubeVertex = new Vector3[8];
    public Vector3 R_xyz = new Vector3(0, 0, 0);
    public Vector3 offset = new Vector3(0, 0, 0);
    GameObject OriginLrGRP;
    GameObject TransLrGRP;
    private List<GameObject> OriginlrList = new List<GameObject>();
    private List<GameObject> TranslrList = new List<GameObject>();
    [SerializeField] private float lineWidth;
    [SerializeField] private Material lineMat3;
    [SerializeField] private Color lineColor3 = new Vector4(1, 1, 1, 1);


    [SerializeField] private Material lineMat4;
    [SerializeField] private Color lineColor4 = new Vector4(1, 1, 1, 1);

    GameObject gameObject2;
    struct Cube
    {
        public Vector3[] vertexs;
    }
    Cube testCube;
    public Vector3[] cubeInObj = new Vector3[8];


    [Header("Ray")]
    [SerializeField] GameObject rayCaster;
    Vector3 rayCasterOriDir;
    private Vector3 ro;
    [SerializeField] private Vector3 rd = new Vector3(0, 0, 1);
    [SerializeField] private float dis = 10.0f;
    public Vector3 Ray_xyz = new Vector3(0, 0, 1);
    private LineRenderer rayLr = new LineRenderer();
    private LineRenderer rayLrObj = new LineRenderer();
    [SerializeField] private Material lineMat2;
    [SerializeField]private float lineWidth2;
    [SerializeField] private Color lineColor2 = new Vector4(1, 1, 1, 1);
    struct RayTraceRay
    {
        public Vector3 ro;
        public Vector3 rd;
        public float dis;
    }
    RayTraceRay rayTrace;
    RayTraceRay rayInObj;
    [SerializeField] GameObject lightInObj;



    [Header("CartesionCoordinates")]
    [SerializeField] int MaxNumber;
    [SerializeField] float Width;
    [SerializeField] Color x = Color.red;
    [SerializeField] Color y = Color.green;
    [SerializeField] Color z = Color.blue;
    #region CartesionCoordinates 
    GameObject CartesianAixeLrGRPX;
    private List<GameObject> CartesianAixeX = new List<GameObject>();
    GameObject CartesianAixeLrGRPY;
    private List<GameObject> CartesianAixeY = new List<GameObject>();
    GameObject CartesianAixeLrGRPZ;
    private List<GameObject> CartesianAixeZ = new List<GameObject>();
    [SerializeField]
    GameObject arrowObj;
    #endregion


    [Header("UI")]
    [SerializeField]
    Slider cubeRX;
    [SerializeField]
    Slider cubeRY;
    [SerializeField]
    Slider cubeRZ;
    [SerializeField]
    Slider cubeTX;
    [SerializeField]
    Slider cubeTY;
    [SerializeField]
    Slider cubeTZ;

    [SerializeField]
    Slider RayDirX;
    [SerializeField]
    Slider RayDirY;


    [SerializeField]
    Button resetBtn;

    [SerializeField]
    Text oc;
    [SerializeField]
    Text wc;

    void UISet()
    {
        cubeRX.value = R_xyz.x;
        cubeRX.onValueChanged.AddListener(delegate {
            R_xyz.x = cubeRX.value;
        });

        cubeRY.value = R_xyz.y;
        cubeRY.onValueChanged.AddListener(delegate {
            R_xyz.y = cubeRY.value;
        });

        cubeRZ.value = R_xyz.z;
        cubeRZ.onValueChanged.AddListener(delegate {
            R_xyz.z = cubeRZ.value;
        });

        cubeTX.value = offset.x;
        cubeTX.onValueChanged.AddListener(delegate {
            offset.x = cubeTX.value;
        });

        cubeTY.value = offset.y;
        cubeTY.onValueChanged.AddListener(delegate {
            offset.y = cubeTY.value;
        });

        cubeTZ.value = offset.z;
        cubeTZ.onValueChanged.AddListener(delegate {
            offset.z = cubeTZ.value;
        });


        RayDirX.value = Ray_xyz.x;
        RayDirX.onValueChanged.AddListener(delegate {
            Ray_xyz.x = RayDirX.value;
        });

        RayDirY.value = Ray_xyz.y;
        RayDirY.onValueChanged.AddListener(delegate {
            Ray_xyz.y = RayDirY.value;
        });



        resetBtn.onClick.AddListener(delegate {


            Ray_xyz = new Vector3(0, 0, 0f);
            R_xyz = new Vector3(0, 0, 0.0f);
            offset = new Vector3(3.5f, 0, 0);


            cubeRX.value = R_xyz.x;
            cubeRY.value = R_xyz.y;
            cubeRZ.value = R_xyz.z;
            cubeTX.value = offset.x;
            cubeTY.value = offset.y;
            cubeTZ.value = offset.z;

            RayDirX.value = Ray_xyz.x;
            RayDirY.value = Ray_xyz.y;

        });
    }

    // Start is called before the first frame update
    void Start()
    {
        UISet();
        LineRenderContent = new GameObject("LRRenderContent");
        CubeInitial();

        GameObject emptyObj1 = new GameObject("Ray");
        GameObject emptyObj2 = new GameObject("RayObj");

        rayLr = emptyObj1.AddComponent<LineRenderer>();
        rayLr.positionCount = 2;

        rayLrObj = emptyObj2.AddComponent<LineRenderer>();
        rayLrObj.positionCount = 2;

        rayCasterOriDir = rayCaster.transform.forward;
        RayTranceRaySet();
       
        DrawCartesianCoordinates(MaxNumber, Width, x, y, z);
    }

    // Update is called once per frame
    void Update()
    {


        RayTranceRaySet();
        DrawCube();
        
        DrawRay(rayTrace, lineColor2);

        Vector2 scrPropos1 = RectTransformUtility.WorldToScreenPoint(Camera.main, cubeVertex[3]+offset);
        wc.rectTransform.position = scrPropos1 + new Vector2(0, 60);

        Vector2 scrPropos2 = RectTransformUtility.WorldToScreenPoint(Camera.main, cubeInObj[3]);
        oc.rectTransform.position = scrPropos2 + new Vector2(0, 60);

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

    void CubeInitial()
    {
        OriginlrList.Clear();
        OriginLrGRP = new GameObject("OriginlrList");
        OriginLrGRP.transform.SetParent(LineRenderContent.transform);

        TranslrList.Clear();
        TransLrGRP = new GameObject("TranslrList");
        TransLrGRP.transform.SetParent(LineRenderContent.transform);

        for (int i = 0; i < 12; i++)
        {

            GameObject temp2 = new GameObject("LR_Origin" + i);
            temp2.transform.SetParent(OriginLrGRP.transform);
            temp2.AddComponent<LineRenderer>();
            OriginlrList.Add(temp2);

            GameObject temp3 = new GameObject("LR_Trans" + i);
            temp3.transform.SetParent(TransLrGRP.transform);
            temp3.AddComponent<LineRenderer>();
            TranslrList.Add(temp3);

        }
        testCube.vertexs = cubeVertex;

    }

    void DrawCube()
    {
        Vector3[] cubeR = new Vector3[cubeVertex.Length];
        cubeR = RotateVertex(R_xyz, cubeVertex);
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


        Vector3 u = Vector3.Normalize(cubeTrans[0] - cubeTrans[3]);
        Vector3 w = Vector3.Normalize(cubeTrans[0] - cubeTrans[1]);
        Vector3 v = Vector3.Normalize(cubeTrans[0] - cubeTrans[4]);

        Matrix4x4 M_obj = new Matrix4x4();
        M_obj.SetColumn(0, u);
        M_obj.SetColumn(1, v);
        M_obj.SetColumn(2, w);
        M_obj.SetColumn(3, new Vector4(offset.x, offset.y, offset.z, 1));

      

        Matrix4x4[] Phs = new Matrix4x4[cubeVertex.Length];
        for (int i = 0; i < Phs.Length; i++)
        {
            Phs[i].SetColumn(0, new Vector4(cubeTrans[i].x, cubeTrans[i].y, cubeTrans[i].z, 1.0f));

            Matrix4x4 temp1 = new Matrix4x4();

            temp1 = M_obj.inverse * Phs[i];

            cubeInObj[i] = (temp1.GetColumn(0) / temp1.GetColumn(0).w);

        }

        for (int i = 0; i < 4; i++)
        {
            SetLine(cubeInObj[i], cubeInObj[(i + 1) % 4], TranslrList[i].GetComponent<LineRenderer>(), lineColor4, lineWidth, lineMat4);

            SetLine(cubeInObj[i + 4], cubeInObj[((i + 1) % 4) + 4], TranslrList[i + 4].GetComponent<LineRenderer>(), lineColor4, lineWidth, lineMat4);

            SetLine(cubeInObj[i], cubeInObj[i + 4], TranslrList[i + 8].GetComponent<LineRenderer>(), lineColor4, lineWidth, lineMat4);
        }

        Vector3 lightPosInCubeObj = Vector3.zero;
        Vector3 lightRdInCubeObj = Vector3.zero;

        Matrix4x4 lightInMatix = new Matrix4x4();
        lightInMatix.SetColumn(0, new Vector4(rayCaster.transform.position.x, rayCaster.transform.position.y, rayCaster.transform.position.z,1.0f));
        Matrix4x4 temp2 = new Matrix4x4();
        temp2 = M_obj.inverse * lightInMatix;
        lightPosInCubeObj = (temp2.GetColumn(0) / temp2.GetColumn(0).w);



        Quaternion q = Quaternion.Euler(R_xyz.x, R_xyz.y, R_xyz.z);
        Matrix4x4 R = Matrix4x4.Rotate(q);
        Matrix4x4 lightrdInMatix = new Matrix4x4();

        lightrdInMatix.SetColumn(0, new Vector4(rayCaster.transform.forward.x, rayCaster.transform.forward.y, rayCaster.transform.forward.z, 1.0f));
        Matrix4x4 temp3 = new Matrix4x4();
        temp3 = R.inverse * lightrdInMatix;
        lightRdInCubeObj = (temp3.GetColumn(0) / temp3.GetColumn(0).w);


       

        lightInObj.transform.position = lightPosInCubeObj;
        lightInObj.transform.forward = lightRdInCubeObj;




        Vector3 end = lightPosInCubeObj + Vector3.Normalize(lightRdInCubeObj) * dis;

        rayLrObj.SetPosition(0, lightPosInCubeObj);
        rayLrObj.SetPosition(1, end);

        rayLrObj.material = lineMat4;
        rayLrObj.startWidth = lineWidth2;
        rayLrObj.endWidth = lineWidth2;
        rayLrObj.numCapVertices = 4;
        rayLrObj.numCornerVertices = 4;
        lineMat4.color = lineColor4;

    }

    void RayTranceRaySet()
    {
        Quaternion q = Quaternion.Euler(Ray_xyz);
        rayCaster.transform.forward = q * rayCasterOriDir;

        rayTrace.dis = dis;
        rayTrace.ro = rayCaster.transform.position;
        rayTrace.rd = rayCaster.transform.forward;
        
    }
    void DrawRay(RayTraceRay r, Color c)
    {

        r.rd = Vector3.Normalize(r.rd);
        Vector3 s = r.ro + r.rd * r.dis;

        rayLr.SetPosition(0, r.ro);
        rayLr.SetPosition(1, s);

        rayLr.material = lineMat2;
        rayLr.startWidth = lineWidth2;
        rayLr.endWidth = lineWidth2;
        rayLr.numCapVertices = 4;
        rayLr.numCornerVertices = 4;
        lineMat2.color = c;



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
}

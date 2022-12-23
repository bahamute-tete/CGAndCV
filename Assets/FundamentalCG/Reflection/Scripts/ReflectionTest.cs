using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReflectionTest : MonoBehaviour
{
    [SerializeField]
    private GameObject lightPrefab;
    private GameObject reflectionPointSph;
    private GameObject refractionPointSph;
    [SerializeField]
    private GameObject arrowPrefab;

    private Vector3 ro = new Vector3(2.0f, 1.0f, 0.0f);
    private Vector3 rd = new Vector3(0, -1, 0);

   
    public Vector3 RayAngle = new Vector3(0, 0, -55);
   


    private GameObject roSph;

    private List<GameObject> lrList = new List<GameObject>();
    private LineRenderer lr0 = new LineRenderer();
    private LineRenderer lr1 = new LineRenderer();
    private LineRenderer lr2 = new LineRenderer();
    private LineRenderer lr3 = new LineRenderer();
    private LineRenderer lr4 = new LineRenderer();
    private LineRenderer lr5 = new LineRenderer();
    private LineRenderer lr6 = new LineRenderer();
    private LineRenderer lr7 = new LineRenderer();
    private LineRenderer lr8 = new LineRenderer();


    [SerializeField]
    private Color gizmoColor;
    [SerializeField]
    private Color rayColor;
    [SerializeField]
    private Color noIntersectionrColor;
    [SerializeField]
    private Color reflecColor;
    [SerializeField]
    private Color refracColor;
    [SerializeField]
    private Material RayMat;
    [SerializeField]
    private Material PlaneGizmoMat;


    [SerializeField]
    private Vector3 planeNormal = new Vector3(0, 1.0f, 0);
    public float planeDis = -2.0f;
    private Vector3 pc = new Vector3(0, 0, 0);
    private float plane_t = 0;
    public float planeGizmoX = 5.0f;
    public float planeGizemoZ = 5.0f;
    public Vector3 rotXYZ = new Vector3(0, 0,0.0f);
    public Vector3 tranXYZ = new Vector3(0, 0, 0);
    public Vector3[] gizmoPlaneVertex = new Vector3[4];
    private Vector3 intersectionPoint = new Vector3(0, 0, 0);
    private Vector3 reflectPoint = new Vector3(0, 0, 0);
    private Vector3 refractionPoint = new Vector3(0, 0, 0);
    private Vector3 planeNormlaPoint = new Vector3(0, 0, 0);
    private Vector3 pCenter = new Vector3(0, 0, 0);
    bool isInside = false;

    public float IOR_in = 1.003f;
    public float IOR_out = 1.333f;



    [SerializeField]
    private Material AnimRay;
    [SerializeField]
    private Material AnimRay_Reflection;
    [SerializeField]
    private Material AnimRay_Refraction;
    int P1ID = 0;
    int P2ID = 0;

    /// <summary>
    /// UIInfo
    /// </summary>
    [SerializeField]
    Text textd1;
    [SerializeField]
    Text textd2;
    [SerializeField]
    Text textd3;
    [SerializeField]
    Text textd4;
    [SerializeField]
    Text textd5;
    [SerializeField]
    Text tIORin;
    [SerializeField]
    Text tIORout;
    [SerializeField]
    Text normaltext;
    [SerializeField]
    Button resetBtn;
    /// <summary>
    /// PlaneParameter
    /// </summary>
    [SerializeField]
    Slider PlaneDirX;
    [SerializeField]
    Slider PlaneDirY;
    [SerializeField]
    Slider PlaneDirZ;
    [SerializeField]
    Slider PlaneWidth;
    [SerializeField]
    Slider PlaneHeigh;
    [SerializeField]
    Slider PlaneTranX;
    [SerializeField]
    Slider PlaneTranY;
    [SerializeField]
    Slider PlaneTranZ;
    /// <summary>
    /// RayParameter
    /// </summary>
    [SerializeField]
    Slider RayDirX;
    [SerializeField]
    Slider RayDirY;
    [SerializeField]
    Slider RayDirZ;

    [SerializeField]
    Dropdown IORin;
    [SerializeField]
    Dropdown IORout;
    [SerializeField]
    Slider Multiplier;
    float multi = 1.0f;

    private void Awake()
    {
        roSph = Instantiate(lightPrefab, ro, Quaternion.identity);
        P1ID = Shader.PropertyToID("_LineWidth");
        P2ID = Shader.PropertyToID("_LineWidth");
       
    }


    // Start is called before the first frame update
    void Start()
    {
        lrList.Clear();
        for (int i = 0; i < 9; i++)
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
        lr6 = lrList[6].AddComponent<LineRenderer>();
        lr7 = lrList[7].AddComponent<LineRenderer>();
        lr8 = lrList[8].AddComponent<LineRenderer>();

        InitialLine();

        IOR_in = 1.003f;
        IOR_out = 2.417f;


        #region
        PlaneDirX.value = rotXYZ.x;
        PlaneDirX.onValueChanged.AddListener(delegate {
            rotXYZ.x = PlaneDirX.value;
        });

        PlaneDirY.value = rotXYZ.y;
        PlaneDirY.onValueChanged.AddListener(delegate {
            rotXYZ.y = PlaneDirY.value;
        });

        PlaneDirZ.value = rotXYZ.z;
        PlaneDirZ.onValueChanged.AddListener(delegate {
            rotXYZ.z = PlaneDirZ.value;
        });

        PlaneTranX.value = tranXYZ.x;
        PlaneTranX.onValueChanged.AddListener(delegate {
            tranXYZ.x = PlaneTranX.value;
        });

        PlaneTranY.value = planeDis;
        PlaneTranY.onValueChanged.AddListener(delegate {
            planeDis = PlaneTranY.value;
        });

        PlaneTranZ.value = tranXYZ.z;
        PlaneTranZ.onValueChanged.AddListener(delegate {
            tranXYZ.z = PlaneTranZ.value;
        });

        PlaneWidth.value = planeGizmoX;
        PlaneWidth.onValueChanged.AddListener(delegate {
            planeGizmoX = PlaneWidth.value;
        });

        PlaneHeigh.value = planeGizemoZ;
        PlaneHeigh.onValueChanged.AddListener(delegate {
            planeGizemoZ = PlaneHeigh.value;
        });

        RayDirX.value = RayAngle.x; 
        RayDirX.onValueChanged.AddListener(delegate {
            RayAngle.x = RayDirX.value;
        });

        RayDirY.value = RayAngle.y;
        RayDirY.onValueChanged.AddListener(delegate {
            RayAngle.y = RayDirY.value;
        });

        RayDirZ.value = RayAngle.z;
        RayDirZ.onValueChanged.AddListener(delegate {
            RayAngle.z = RayDirZ.value;
        });

        Dropdown.OptionData optionData0 = new Dropdown.OptionData();
        Dropdown.OptionData optionData1 = new Dropdown.OptionData();
        Dropdown.OptionData optionData2 = new Dropdown.OptionData();
        Dropdown.OptionData optionData3 = new Dropdown.OptionData();
        Dropdown.OptionData optionData4 = new Dropdown.OptionData();

        optionData0.text = "Air";
        optionData1.text = "Water";
        optionData2.text = "Glass";
        optionData3.text = "Ruby";
        optionData4.text = "Diamonds";

        IORin.options.Add(optionData0);
        IORin.options.Add(optionData1);
        IORin.options.Add(optionData2);
        IORin.options.Add(optionData3);
        IORin.options.Add(optionData4);

        IORout.options.Add(optionData0);
        IORout.options.Add(optionData1);
        IORout.options.Add(optionData2);
        IORout.options.Add(optionData3);
        IORout.options.Add(optionData4);

        IORin.value  =  0;
        IORout.value = 4;

        IORin.onValueChanged.AddListener(delegate
        {
            switch (IORin.value)
            {
                case 0:
                    IOR_in = 1.003f;
                    break;

                case 1:
                    IOR_in = 1.333f;
                    break;

                case 2:
                    IOR_in = 1.500f;
                    break;

                case 3:
                    IOR_in = 1.770f;
                    break;

                case 4:
                    IOR_in = 2.417f;
                    break;
            }

        });

        IORout.onValueChanged.AddListener(delegate
        {
            switch (IORout.value)
            {
                case 0:
                    IOR_out = 1.003f;
                    break;

                case 1:
                    IOR_out = 1.333f;
                    break;

                case 2:
                    IOR_out = 1.500f;
                    break;

                case 3:
                    IOR_out = 1.770f;
                    break;

                case 4:
                    IOR_out = 2.417f;
                    break;
            }

        });


        Multiplier.value = multi;
        Multiplier.onValueChanged.AddListener(delegate {
            multi = Multiplier.value;
        });



        resetBtn.onClick.AddListener(delegate {

            roSph.transform.position = new Vector3(2.0f, 1.0f, 0);
            RayAngle = new Vector3(0, 0, -55.0f);
            rotXYZ = new Vector3(0, 0, 0.0f);
            tranXYZ = new Vector3(0, 0, 0);

            planeDis = -1.0f;
            planeGizmoX = 5.0f;
            planeGizemoZ = 5.0f;
            multi = 1.0f;
            IORin.value = 0;
            IORout.value = 4;
            PlaneDirX.value = rotXYZ.x;
            PlaneDirY.value = rotXYZ.y;
            PlaneDirZ.value = rotXYZ.z;
            PlaneTranX.value = tranXYZ.x;
            PlaneTranX.value = tranXYZ.x;
            PlaneTranY.value = planeDis;
            PlaneTranZ.value = tranXYZ.z;
            PlaneWidth.value = planeGizmoX;
            PlaneHeigh.value = planeGizemoZ;
            RayDirX.value = RayAngle.x;
            RayDirY.value = RayAngle.y;
            RayDirZ.value = RayAngle.z;


        });
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        
        DrawPlaneGizmo();
        DrawRay(RayAngle);



        float d1 = Vector3.Magnitude(ro - intersectionPoint);
        float d2 = Vector3.Magnitude(reflectPoint - intersectionPoint);
        P1ID = Shader.PropertyToID("_LineWidth");
        lr0.material.SetFloat(P1ID, d1);
        lr6.material.SetFloat(P2ID, d2);

        UIControl();
    }

    void UIControl()
    {
        if (!isInside)
        {
            textd2.gameObject.SetActive(false);
            textd3.gameObject.SetActive(false);
            textd4.gameObject.SetActive(false);
            textd5.gameObject.SetActive(false);
            normaltext.gameObject.SetActive(false);
        }
        else
        {
            textd2.gameObject.SetActive(true);
            textd3.gameObject.SetActive(true);
            textd4.gameObject.SetActive(true);
            textd5.gameObject.SetActive(true);
            normaltext.gameObject.SetActive(true);
        }

        Vector2 scrPropos = RectTransformUtility.WorldToScreenPoint(Camera.main, ro);
        textd1.rectTransform.position = scrPropos + new Vector2(0, 60);

        Vector2 scrPropos1 = RectTransformUtility.WorldToScreenPoint(Camera.main, reflectPoint);
        textd2.rectTransform.position = scrPropos1 + new Vector2(0, 60);

        Vector2 scrPropos2 = RectTransformUtility.WorldToScreenPoint(Camera.main, intersectionPoint);
        textd3.rectTransform.position = scrPropos2 + new Vector2(0, -30);

        Vector2 scrPropos3 = RectTransformUtility.WorldToScreenPoint(Camera.main, intersectionPoint);
        textd4.rectTransform.position = scrPropos3 + new Vector2(0, -80);
        textd4.text = "P : " + intersectionPoint.ToString();

        Vector2 scrPropos4 = RectTransformUtility.WorldToScreenPoint(Camera.main, refractionPoint);
        textd5.rectTransform.position = scrPropos4 + new Vector2(0, -40);

        tIORin.text = IOR_in.ToString();
        tIORout.text = IOR_out.ToString();

        Vector2 scrPropos5 = RectTransformUtility.WorldToScreenPoint(Camera.main, planeNormlaPoint);
        normaltext.rectTransform.position = scrPropos5 + new Vector2(0, 40);
        


    }

    void InitialLine()
    {
        RayAngle.x = Mathf.Clamp(RayAngle.x, -60, 60);
        RayAngle.y = Mathf.Clamp(RayAngle.y, -60, 60);
        RayAngle.z = Mathf.Clamp(RayAngle.z, -60, 60);
        Quaternion q = new Quaternion();
        q = Quaternion.Euler(RayAngle);

        Vector3 lineDir = q * rd;
        plane_t = IntersctionPlane(planeNormal, planeDis, ro, lineDir);
        intersectionPoint = ro + lineDir * plane_t;
        reflectionPointSph = Instantiate(arrowPrefab, intersectionPoint, Quaternion.identity);
        refractionPointSph = Instantiate(arrowPrefab, intersectionPoint, Quaternion.identity);
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

    void DrawRay(Vector3 RayAngle)
    {
        RayAngle.x = Mathf.Clamp(RayAngle.x, -60, 60);
        RayAngle.y = Mathf.Clamp(RayAngle.y, -60, 60);
        RayAngle.z = Mathf.Clamp(RayAngle.z, -60, 60);
        Quaternion q = new Quaternion();
        q = Quaternion.Euler(RayAngle);

        ro = roSph.transform.position;
        Vector3 lineDir = q * rd;

        Vector4 plane = TransformPlane(tranXYZ, rotXYZ, Vector3.Normalize(planeNormal), planeDis);
        Vector3 pNormal = Vector3.Normalize(new Vector3(plane.x, plane.y, plane.z));

        plane_t = IntersctionPlane(pNormal, plane.w, ro, lineDir);

        intersectionPoint = ro + lineDir * plane_t;
        planeNormlaPoint = intersectionPoint + pNormal * 1.0f;

         isInside = InsidePlane(gizmoPlaneVertex[0], gizmoPlaneVertex[1], gizmoPlaneVertex[2], gizmoPlaneVertex[3], intersectionPoint);

        Color colorRay;
       

        if (isInside)
        {
            colorRay = rayColor;
            planeNormlaPoint = intersectionPoint + pNormal * 1.0f;//Plane Normal
            reflectPoint = ReflectionRay(ro, intersectionPoint, pNormal);
            refractionPoint =RefractionRay(ro, intersectionPoint, pNormal, IOR_in, IOR_out* multi);
        }
        else
        {
            colorRay = noIntersectionrColor;
            
            intersectionPoint = ro + lineDir * (plane_t + 25.0f);
            reflectPoint = intersectionPoint;
            planeNormlaPoint = intersectionPoint;
            refractionPoint = intersectionPoint;
        }

        SetLine(ro, intersectionPoint, lr0, colorRay, 0.05f, AnimRay);//Ray
        SetLine(intersectionPoint, reflectPoint, lr6, reflecColor, 0.05f, AnimRay_Reflection);//Refletion Ray
        SetLine(intersectionPoint, refractionPoint, lr8, refracColor, 0.05f, AnimRay_Refraction);//Refraction Ray


        SetLine(intersectionPoint, planeNormlaPoint, lr7, gizmoColor, 0.05f, PlaneGizmoMat);//PlaneNormal

        reflectionPointSph.transform.position = reflectPoint;
        reflectionPointSph.transform.up = Vector3.Normalize( reflectPoint - intersectionPoint);
        refractionPointSph.transform.position = refractionPoint;
        refractionPointSph.transform.up = Vector3.Normalize(refractionPoint - intersectionPoint);

    }

    float IntersctionPlane(Vector3 normal, float D, Vector3 ro, Vector3 rd)
    {
        float intersection_t = 0;
        float side = Mathf.Sign(Vector3.Dot(normal, ro - pCenter));
        float t = -(Vector3.Dot(normal, ro) - D) / Vector3.Dot(normal, rd);
        intersection_t = (side > 0) ? t : 15.0f;
        return intersection_t;
    }

    void DrawPlaneGizmo()
    {
        Vector3[] vector3s = new Vector3[4];

        vector3s = FourConner(planeDis, planeGizmoX, planeGizemoZ, tranXYZ);
      
        gizmoPlaneVertex = RotateVertex(tranXYZ,rotXYZ, vector3s);

        SetLine(gizmoPlaneVertex[0], gizmoPlaneVertex[1], lr1, gizmoColor, 0.03f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[1], gizmoPlaneVertex[2], lr2, gizmoColor, 0.03f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[2], gizmoPlaneVertex[3], lr3, gizmoColor, 0.03f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[3], gizmoPlaneVertex[0], lr4, gizmoColor, 0.03f, PlaneGizmoMat);
        SetLine(gizmoPlaneVertex[0], gizmoPlaneVertex[2], lr5, gizmoColor, 0.03f, PlaneGizmoMat);
       
    }

    bool InsidePlane(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 p)
    {
        bool isInside;
        Vector3 ba = b - a;
        Vector3 cb = c - b;
        Vector3 dc = d - c;
        Vector3 ad = a - d;

        Vector3 pa = p - a;
        Vector3 pb = p - b;
        Vector3 pc = p - c;
        Vector3 pd = p - d;

        float pba = Mathf.Sign(Vector3.Cross(ba, pa).y);
        float pcb = Mathf.Sign(Vector3.Cross(cb, pb).y);
        float pdc = Mathf.Sign(Vector3.Cross(dc, pc).y);
        float pda = Mathf.Sign(Vector3.Cross(ad, pd).y);
        if (pba < 0 && pcb < 0 && pdc < 0 && pda < 0)
            isInside = true;
        else
            isInside = false;

        return isInside;
    }

    Vector3[] FourConner(float D, float width, float heigh,Vector3 tran)
    {
        Vector3 orgin = new Vector3(0, 0, 0);
        Vector3 d = (D < 0) ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        pCenter = orgin-tran + d * Mathf.Abs(D);
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

        Matrix4x4 vT = Tanslate* Rot * vetexGRP;

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

    Vector3 Reflection(Vector3 L, Vector3 N)
    {
        Vector3 R = new Vector3(0, 0, 0);
        R = Vector3.Normalize(2 * (Vector3.Dot(N, L)) * N - L);
        return R;
    }
    Vector3 ReflectionRay(Vector3 LightOrigin, Vector3 intersectionPoint, Vector3 planeNormal)
    {
        Vector3 L = LightOrigin - intersectionPoint;
        Vector3 N = Vector3.Normalize(planeNormal);
        Vector3 R = Reflection(L, N);
        Vector3 refRayPoint = intersectionPoint + R * 4.0f;

        return refRayPoint;

    }

    Vector3 Refraction(Vector3 L, Vector3 N, float n1, float n2)
    {
        Vector3 T = new Vector3();

        float IOR = n1 / n2;
        float IOR2 = Mathf.Pow(n1 / n2, 2);
        float NL2 = Mathf.Pow(Vector3.Dot(N, L), 2);
        float D = 1 - IOR2 * (1 - NL2);
        if (D >= 0)
            T = (IOR * (Vector3.Dot(N, L)) - Mathf.Sqrt(D)) * N - IOR * L;
        else
            T = Reflection(L, N);
        return Vector3.Normalize(T);
    }
    Vector3 RefractionRay(Vector3 LightOrigin, Vector3 intersectionPoint, Vector3 planeNormal, float n1, float n2)
    {
        Vector3 L = LightOrigin - intersectionPoint;
        Vector3 N = Vector3.Normalize(planeNormal);
        Vector3 T = Refraction(L, N,n1,n2);
        Vector3 refractionPoint = intersectionPoint + T * 4.0f;
        return refractionPoint;

    }
}

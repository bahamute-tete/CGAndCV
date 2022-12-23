using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineIn3D : MonoBehaviour
{

    
    [SerializeField]
    private GameObject pointProxy;
    [SerializeField]
    private GameObject pointProxy1;
    [SerializeField]
    private GameObject pointProxy2;
    [SerializeField]
    private Color colorLine1;
    [SerializeField]
    private Color colorLine2;
    [SerializeField]
    private Color colorLine3;
    [SerializeField]
    private Color colorLine4;
    [SerializeField]
    private Material lineMat0;

    [SerializeField]
    private Material dotLineMat0;
    [SerializeField]
    private Material dotLineMat1;
    [SerializeField]
    private Material dotLineMat2;
    [SerializeField]
    private Material dotLineMat3;

    [SerializeField]
    private Image carImage;



    private Vector3 pointPos1 = new Vector3(-2.3f, 0, 0);   
    private Vector3 pointPos2 = new Vector3(5, 2.87f, 4);
    private Vector3 pointPos = new Vector3(1, 2, 0);
    private Vector3 pointPro = new Vector3(0, 0, 0);
    private Vector3 pro;



    private GameObject pointSph1;
    private GameObject pointSph2;
    private GameObject pointSph3;
    private GameObject pointSph4;
    private GameObject pointSph5;
    private GameObject pointSph6;

    private GameObject pointSph7;
    private GameObject pointSph8;

    private List<GameObject> lrList = new List<GameObject>();

    private LineRenderer lr0 = new LineRenderer();
    private LineRenderer lr1 = new LineRenderer();
    private LineRenderer lr2 = new LineRenderer();
    private LineRenderer lr3 = new LineRenderer();

    private LineRenderer lr4 = new LineRenderer();
    private LineRenderer lr5 = new LineRenderer();
    private LineRenderer lr6 = new LineRenderer();

    private LineRenderer lr7 = new LineRenderer();

    private Vector3 ro = new Vector3(2.31f, -1.57f, -0.7f);
    private Vector3 rd = new Vector3(1, 0, 0);


    private float thetaY =128.0f;
    private float thetaZ = 128.0f;


    [SerializeField]
    [Range(0.1f, 8)]
    private float t = 0.1f;
    private GameObject roSph;

    private float d1=0.0f, d2=0.0f;

    [SerializeField]
    Text textd1;
    [SerializeField]
    Text textd2;
    [SerializeField]
    Text textd3;
    [SerializeField]
    Text T1;
    [SerializeField]
    Text T2;
    [SerializeField]
    Text D1;
    [SerializeField]
    Text D2;
    [SerializeField]
    Text D3;

    [SerializeField]
    Slider l2rdY;
    [SerializeField]
    Slider l2length;

    [SerializeField]
    Button resetBtn;


    int P1ID = 0;
    int P2ID = 0;
    int P3ID = 0;
    int P4ID = 0;
    int P5ID = 0;

    // Start is called before the first frame update
    void SetLine(Vector3 a, Vector3 b,LineRenderer lr,Color color,float lineWidth,Material mat)
    {
      
        lr.material = mat;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        mat.color = color;
       

    }

    Vector3 ProjP2L( Vector3 a,Vector3 b,Vector3 p,out float d)
    {
        d = 0;
        Vector3 ba = b - a;
        Vector3 pa = p - a;
        //Vector3 pro = a+ Mathf.Clamp(( Vector3.Dot(pa, ba) /Vector3.Dot(ba,ba)),0,1)*ba;
        Vector3 pro = a + (Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba)) * ba;
        Vector3 h = pa - pro;
        d = Vector3.Magnitude(h);
        return pro;
    }

    Vector2 DistanceIn3D( Vector3 pointPos1,Vector3 pointPos2,Vector3 ro,Vector3 rd)
    {
        Vector3 S1 = pointPos1;
        Vector3 S2 = ro;
        Vector3 ba = pointPos2 - pointPos1;
        Vector3 V1 = ba / Vector3.Magnitude(ba);
        Vector3 V2 = rd;

        float x1 = Vector3.Dot(S2 - S1, V1);
        float x2 = Vector3.Dot(S2 - S1, V2);

        float a = -Vector3.Dot(V2, V2);
        float b = Vector3.Dot(V1, V2);
        float c = -Vector3.Dot(V1, V2);
        float d = Vector3.Dot(V1, V1);

        float k = Mathf.Pow(Vector3.Dot(V1, V2), 2) - Vector3.Dot(V1, V1) * Vector3.Dot(V2, V2);

        float t1 = (a * x1 + b * x2)*(1/k);
        float t2 = (c * x1 + d * x2)*(1/k);

        return new Vector2(t1, t2);
    }

    private void Awake()
    {
        pointSph1 = Instantiate(pointProxy, pointPos1, Quaternion.identity);
        pointSph2 = Instantiate(pointProxy, pointPos2, Quaternion.identity);
        pointSph3 = Instantiate(pointProxy, pointPos, Quaternion.identity);
        roSph = Instantiate(pointProxy, ro, Quaternion.identity);
    }
    void Start()
    {

        lrList.Clear();
        for (int i = 0; i < 7; i++)
        {
            GameObject olr = new GameObject("LR" + i);
            lrList.Add(olr);
        }

        float cy = Mathf.Cos(thetaY * Mathf.Deg2Rad);
        float sy = Mathf.Sin(thetaY * Mathf.Deg2Rad);
        rd = new Vector3(cy, rd.y, sy);
        Vector3 endPoint = ro + rd * t;

        Vector3 pro = ProjP2L(pointPos1, pointPos2, pointPos,out d1);
        Vector3 pro2 = ProjP2L(ro, endPoint, pointPos, out d2);

       
       
        pointSph4 = Instantiate(pointProxy2, pro, Quaternion.identity);//pro
        pointSph5 = Instantiate(pointProxy2, pro2, Quaternion.identity);//pro2
        pointSph6 = Instantiate(pointProxy1, endPoint, Quaternion.identity);
        pointSph7 = Instantiate(pointProxy2, new Vector3(0, 0, 0), Quaternion.identity);//t1
        pointSph8 = Instantiate(pointProxy2, new Vector3(0, 0, 0), Quaternion.identity);//t2




        lr0 = lrList[0].AddComponent<LineRenderer>();
        //lr1 = pointSph2.AddComponent<LineRenderer>();
        lr2 = lrList[1].AddComponent<LineRenderer>();
        lr3 = lrList[2].AddComponent<LineRenderer>();
        lr4 = lrList[3].AddComponent<LineRenderer>();
        lr5 = lrList[4].AddComponent<LineRenderer>();
        lr6 = lrList[5].AddComponent<LineRenderer>();
        lr7 = lrList[6].AddComponent<LineRenderer>();



        l2rdY.value = 128.0f;
        l2rdY.onValueChanged.AddListener(delegate {
            thetaY = l2rdY.value;
        });


        l2length.value = 5.0f;
        l2length.onValueChanged.AddListener(delegate {
            t = l2length.value;
        });

        resetBtn.onClick.AddListener(delegate {

            pointSph1.transform.position = new Vector3(-2.3f, 0, 0);
            pointSph2.transform.position = new Vector3(5, 2.87f, 4);
            pointSph3.transform.position = new Vector3(1, 2, 0);
            roSph.transform.position = new Vector3(2.31f, -1.57f, -0.7f);
            rd = new Vector3(1, 0, 0);
            l2rdY.value = 128.0f;
            l2length.value = 5.0f;

        });



    }

    // Update is called once per frame
    void Update()
    {

        DrawLineSegment();
        Vector2 ImagePos = RectTransformUtility.WorldToScreenPoint(Camera.main, pointPos);
        carImage.rectTransform.position = ImagePos;
    }

    void DrawLineSegment()
    {
        Vector2 scrPpos = RectTransformUtility.WorldToScreenPoint(Camera.main, pointPos);
        textd3.rectTransform.position = scrPpos + new Vector2(0, 70);


        pro = ProjP2L(pointPos1, pointPos2, pointPos, out d1);
        Vector2 scrPropos = RectTransformUtility.WorldToScreenPoint(Camera.main, pro);
        textd1.rectTransform.position = scrPropos + new Vector2(0, -70);
        //textd1.text = "Dis1 = " + d1.ToString();
        pointPos1 = pointSph1.transform.position;
        pointPos2 = pointSph2.transform.position;
        pointPos = pointSph3.transform.position;
        pointSph4.transform.position = pro;


        SetLine(pointPos1, pointPos2, lr0, colorLine1, 0.1f,lineMat0);
        //SetLine(pointPos1, pointPos, lr1, Color.gray, 0.01f);
        SetLine(pro, pointPos, lr2, colorLine2, 0.05f, dotLineMat0);
        SetLine(pointPos1, pro, lr3, colorLine4, 0.02f, dotLineMat1);



        float cy = Mathf.Cos(thetaY * Mathf.Deg2Rad);
        float sy = Mathf.Sin(thetaY * Mathf.Deg2Rad);

        rd = new Vector3(cy, rd.y, sy);

        ro = roSph.transform.position;
        Vector3 endPoint = ro + rd * t;
        Vector3 pro2 = ProjP2L(ro, endPoint, pointPos, out d2);
        Vector2 scrPropos2 = RectTransformUtility.WorldToScreenPoint(Camera.main, pro2);
        textd2.rectTransform.position = scrPropos2 + new Vector2(0, -70);
        //textd2.text = "Dis2 = " + d2.ToString();
        pointSph5.transform.position = pro2;
        pointSph6.transform.position = endPoint;

        SetLine(ro, endPoint, lr4, colorLine1, 0.1f, lineMat0);
        SetLine(pro2, pointPos, lr5, colorLine2, 0.05f, dotLineMat1);
        SetLine(pro2, endPoint, lr6, colorLine4, 0.02f, dotLineMat3);


        Vector2 distancT = DistanceIn3D(pointPos1, pointPos2, ro, rd);
        Vector3 ba = pointPos2 - pointPos1;
        Vector3 V1 = ba / Vector3.Magnitude(ba);
        Vector3 p7 = pointPos1 + V1 * distancT.x;
        Vector3 p8 = ro + rd * distancT.y;
        pointSph7.transform.position = p7;
        pointSph8.transform.position = p8;
        SetLine(p7, p8, lr7, colorLine3, 0.05f, dotLineMat2);
        Vector2 scrT1pos = RectTransformUtility.WorldToScreenPoint(Camera.main, p7);
        T1.rectTransform.position = scrT1pos + new Vector2(0, 70);
        Vector2 scrT2pos = RectTransformUtility.WorldToScreenPoint(Camera.main, p8);
        T2.rectTransform.position = scrT2pos + new Vector2(0, -70);


        float distanceP1 = Vector3.Distance(pro, pointPos1);
        float distanceP2 = Vector3.Distance(pro2, endPoint);

        P1ID= Shader.PropertyToID("_LineWidth");
        P2ID = Shader.PropertyToID("_LineWidth");
        P3ID = Shader.PropertyToID("_LineWidth");
        P4ID = Shader.PropertyToID("_LineWidth");
        P5ID = Shader.PropertyToID("_LineWidth");

        lr2.material.SetFloat(P1ID, d1);
        lr5.material.SetFloat(P2ID, d2);
        lr7.material.SetFloat(P3ID, Vector3.Magnitude(p8 - p7));
       
        lr3.material.SetFloat(P4ID, distanceP1);
        lr6.material.SetFloat(P5ID, distanceP2);
        

        textd1.text = "P1: "+pro.ToString("0.00");
        textd2.text = "P2: "+ pro2.ToString("0.00");
        textd3.text = "P: "+ pointPos.ToString("0.00");
        T1.text = "t1: " + distancT.x.ToString("0.00");
        T2.text = "t2: " + distancT.y.ToString("0.00");
        D1.text = "(P->P1):        " + d1.ToString("0.00");
        D2.text = "(P->P2):        " + d2.ToString("0.00");
        D3.text = "(T1->T2):       " + Vector3.Magnitude(p8-p7).ToString("0.00");
        
        

    }
}

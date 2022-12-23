using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

public class DoublePendulum : MonoBehaviour
{
    [Header("DoublePendulum")]
    [Range(1,10)]
    [SerializeField] float r1 =2.0f;
    [Range(1, 10)]
    [SerializeField] float r2 =2.0f;
    [Range(1, 40)]
    [SerializeField] float m1 = 10.0f;
    [Range(1, 40)]
    [SerializeField] float m2 =12.0f;
    private const float pi = 3.1415926f;
    [Range(0, 360)]
    [SerializeField]
    private float startAngle1;
    private float a1;
    [Range(0, 360)]
    [SerializeField]
    private float startAngle2;
    private float a2;
    float av1 = 0; float av2 = 0;//Velocity
    float aa1 = 0.0001f; float aa2 = 0.0001f;//Acceleration
    [Range(0.0f,0.01f)]
    [SerializeField]
    float lossEnerge = 0.01f;
    private float g = 9.8f;
    private Vector3 v1;
    private Vector3 v2;
    public bool simulate = false;


    [Header("Render")]
    [SerializeField] GameObject ballPrefab;
    [SerializeField] Vector3 startPos = Vector3.zero;
    [SerializeField] private Material lineMat;
    [SerializeField] private float lineWidth;
    [SerializeField] private Color lineColor = new Vector4(1, 1, 1, 1);
    private LineRenderer lr1 = new LineRenderer();
    private LineRenderer lr2 = new LineRenderer();
    private GameObject ball1;
    private GameObject ball2;

    [Header("UI")]
    [SerializeField] Slider lslidr01;
    [SerializeField] Slider lslidr02;
    [SerializeField] Slider mslidr01;
    [SerializeField] Slider mslidr02;
    [SerializeField] Slider aslidr01;
    [SerializeField] Slider aslidr02;
    [SerializeField] Slider resistanceSlider;
    [SerializeField] Button sim;
    [SerializeField] Button reset;
    [SerializeField] Text ma;
    [SerializeField] Text mb;
    [SerializeField] Text la;
    [SerializeField] Text lb;
    [SerializeField] Text theta;
    [SerializeField] Text phi;
    [SerializeField] Text va;
    [SerializeField] Text vb;
    [SerializeField] Text aa;
    [SerializeField] Text ab;
    [SerializeField] Text res;


    private void UISet()
    {
        lslidr01.value = r1;
        lslidr01.onValueChanged.AddListener(delegate {
            r1 = lslidr01.value;
        });

        lslidr02.value =r2;
        lslidr02.onValueChanged.AddListener(delegate {
           r2 = lslidr02.value;
        });

        mslidr01.value = m1;
        mslidr01.onValueChanged.AddListener(delegate {
            m1 = mslidr01.value;
        });

        mslidr02.value = m2;
        mslidr02.onValueChanged.AddListener(delegate {
            m2= mslidr02.value;
        });

        aslidr01.value =startAngle1;
        aslidr01.onValueChanged.AddListener(delegate {
            startAngle1 = aslidr01.value;
        });

        aslidr02.value = startAngle2;
        aslidr02.onValueChanged.AddListener(delegate {
            startAngle2 = aslidr02.value;
        });

        resistanceSlider.value = lossEnerge;
        resistanceSlider.onValueChanged.AddListener(delegate {
            lossEnerge = resistanceSlider.value;
        });

        sim.onClick.AddListener(delegate {

            simulate = true;
        });

        reset.onClick.AddListener(delegate {

            simulate = false;
            r1 = 2.0f;
            r2 = 2.0f;
            m1 = 10.0f;
            m2 = 12.0f;
            a1 = startAngle1 * Mathf.Deg2Rad;
            a2 = startAngle2 * Mathf.Deg2Rad;
            av1 = 0; av2 = 0;
            aa1 = 0; aa2 = 0;
            lossEnerge =0.0f;


            lslidr01.value = r1;
            lslidr02.value = r2;
            mslidr01.value = m1;
            mslidr02.value = m2;
            aslidr01.value = startAngle1;
            aslidr02.value = startAngle2;
            resistanceSlider.value = lossEnerge;
        });
    }

    private void UIInfor()
    {
        ma.text = m1.ToString("#.###");
        mb.text = m2.ToString("#.###");
        la.text = r1.ToString("#.###");
        lb.text = r2.ToString("#.###");

        float angletheta = ClampAngle(a1 * Mathf.Rad2Deg,-360,360);
        float anglePhi = ClampAngle(a2 * Mathf.Rad2Deg,-360,360);

        theta.text = angletheta.ToString("#.##");
        phi.text = anglePhi.ToString("#.##");


        va.text = av1.ToString("#.###");
        vb.text = av2.ToString("#.###");
        aa.text = aa1.ToString("#.###");
        ab.text = aa2.ToString("#.###");
        res.text = lossEnerge.ToString("#.####");
    }

    // Start is called before the first frame update
    void Start()
    {
        UISet();

        GameObject lrObj1 = new GameObject("lr1");
        lr1 = lrObj1.AddComponent<LineRenderer>();
        ball1 = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);


        GameObject lrObj2 = new GameObject("lr2");
        lr2 = lrObj2.AddComponent<LineRenderer>();
        ball2 = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);

        a1 = startAngle1 * Mathf.Deg2Rad;
        a2 = startAngle2 * Mathf.Deg2Rad;

        SimulationDoublePendulum( out v1, out v2);
        SetLine(startPos, v1, lr1, lineColor, lineWidth, lineMat);
        SetLine(v1, v2, lr2, lineColor, lineWidth, lineMat);

        ball1.transform.position = v1;
        ball2.transform.position = v2;

        ball1.transform.localScale = new float3(m1 / 30.0f); 
        ball2.transform.localScale = new float3(m2 / 30.0f);

    }

    // Update is called once per frame
    void Update()
    {
        


        if (simulate)
        {
            ball1.GetComponent<TrailRenderer>().enabled = false;
            ball2.GetComponent<TrailRenderer>().enabled = true;
            //ball2.GetComponent<TrailRenderer>().startWidth =ball2.transform.localScale.x/4 ;

            g = 9.8f;
            SimulationDoublePendulum(out v1, out v2);
        }
        else
        {
            ball1.GetComponent<TrailRenderer>().enabled = false;
            ball2.GetComponent<TrailRenderer>().enabled = false;
            a1 = startAngle1 * Mathf.Deg2Rad;
            a2 = startAngle2 * Mathf.Deg2Rad;
            av1 = 0; av2 = 0;
            aa1 = 0; aa2 = 0;
            g = 0;
            SimulationDoublePendulum(out v1, out v2);

        }
        
        SetLine(startPos, v1, lr1, lineColor, lineWidth, lineMat);
        SetLine(v1, v2, lr2, lineColor, lineWidth, lineMat);

        ball1.transform.localScale = new float3(m1 / 30.0f);
        ball2.transform.localScale = new float3(m2 / 30.0f);

        ball1.transform.position = v1;
        ball2.transform.position = v2;

        UIInfor();

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

    void SimulationDoublePendulum( out Vector3 v1,out Vector3 v2)
    {
        v1 = Vector3.zero;
        v2 = Vector3.zero;

        float pp1 = -g * (2 * m1 + m2) * Mathf.Sin(a1);
        float pp2 = -m2 * g * Mathf.Sin(a1 - 2 * a2);
        float pp3 = -2 * Mathf.Sin(a1 - a2) * m2;
        float pp4 = av2 * av2 * r2 + av1 * av1 * r1 * Mathf.Cos(a1 - a2);
        float den = r1 * (2 * m1 + m2 - m2 * Mathf.Cos(2 * a1 - 2 * a2));
        aa1 = (pp1 + pp2 + pp3 * pp4) / den;

        float mm1 = 2 * Mathf.Sin(a1 - a2);
        float mm2 = av1 * av1 * r1 * (m1 + m2);
        float mm3 = g * (m1 + m2) * Mathf.Cos(a1);
        float mm4 = av2 * av2 * r2 * m2 * Mathf.Cos(a1 - a2);
        float dd = r2 * (2 * m1 + m2 - m2 * Mathf.Cos(2 * a1 - 2 * a2));
        aa2 = (mm1 * (mm2 + mm3 + mm4)) / dd;


        aa1 = (Mathf.Abs(aa1) - Mathf.Epsilon < 0) ? 0 : aa1;
        aa2 = (Mathf.Abs(aa2) - Mathf.Epsilon < 0) ? 0 : aa2;

        av1 += aa1 * Time.fixedDeltaTime;
        av2 += aa2 * Time.fixedDeltaTime;

        av1 *= 1.0f - lossEnerge;
        av2 *= 1.0f - lossEnerge;

        a1  += av1 * Time.fixedDeltaTime;
        a2  += av2 * Time.fixedDeltaTime;



        float x1 = startPos.x + r1 * Mathf.Sin(a1);
        float y1 = startPos.y - r1 * Mathf.Cos(a1);
         v1 = new float3(x1, y1, 0);

        float x2 = x1 + r2 * Mathf.Sin(a2);
        float y2 = y1 - r2 * Mathf.Cos(a2);
         v2 = new float3(x2, y2, 0);

    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}

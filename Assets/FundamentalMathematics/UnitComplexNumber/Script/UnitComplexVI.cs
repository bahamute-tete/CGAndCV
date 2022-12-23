using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitComplexVI : MonoBehaviour
{
    const float pi = 3.1415f;
    Camera camera;
    GameObject imaginAixe;
    LineRenderer lri;
    GameObject realAixe;
    LineRenderer lrr;
    [SerializeField] Color AixeColor;
    

    GameObject circleO;
    LineRenderer lrc;
    List<Vector3> circle = new List<Vector3>();
    [SerializeField] Color gizmoCircleColor;

    GameObject pointGRP;
    [SerializeField] int mappingCount;

    List<GameObject> mappingPoint = new List<GameObject>();
    List<Vector4> circleMapping = new List<Vector4>();

    List<GameObject> intersectPointO = new List<GameObject>();
    List<Vector3> inteserctPos = new List<Vector3>();

    [SerializeField] GameObject pointMapping;//Point On Y aixe
    [SerializeField] GameObject pointCircle;//Point On circle;
    [SerializeField] GameObject handlePoint;
    [SerializeField] GameObject handlePoint_Mapping;
    [SerializeField]private Color rayColor;
    List<LineRenderer> lrGRP = new List<LineRenderer>();


    Vector3 startAn = Vector3.zero;
    Vector3 curretnAn;
    Vector3 offsetAn;
    private Vector3 currentHandleScrPos;
    Vector3 wInpute;
    bool isHit = false;
    [SerializeField] GameObject backgroundPanel;


    GameObject TriggerContent;
    List<GameObject> soundTriggerGRP = new List<GameObject>();
    [SerializeField] GameObject soundTrigger;

    #region ui
    [SerializeField] TextMeshProUGUI xText;
    [SerializeField] TextMeshProUGUI realText;
    [SerializeField] TextMeshProUGUI imgText;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        offsetAn = Vector3.zero;


        LineRenderSet();
        DrawCircle();
        DrawMappingPoint(mappingCount);
        DrawRay(rayColor);
       

    }

    // Update is called once per frame
    void Update()
    {
        lri.material.color = lrr.material.color = AixeColor;
        lrc.material.color = gizmoCircleColor;

        RotationControl();

    }

    void LineRenderSet()
    {
        pointGRP = new GameObject("PointsGRP");

        imaginAixe = new GameObject("imaginAixe");
        realAixe = new GameObject("realAixe");
        circleO = new GameObject("circle");

        lri = imaginAixe.AddComponent<LineRenderer>();
        lrr = realAixe.AddComponent<LineRenderer>();
        lrc = circleO.AddComponent<LineRenderer>();

        lri.positionCount = lrr.positionCount;
        lri.material = lrr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lri.material.SetFloat("_Mode", 2);
        lri.material.SetFloat("_ColorMode", 0);
        lri.material.color = lrr.material.color = AixeColor;
        lri.startWidth = lrr.startWidth = lri.endWidth = lrr.endWidth = 0.015f;
        lri.SetPosition(0, new Vector3(0, -2, 0)); lrr.SetPosition(0, new Vector3(-2, 0, 0));
        lri.SetPosition(1, new Vector3(0, 2, 0)); lrr.SetPosition(1, new Vector3(2, 0, 0));
    }
    void DrawCircle()
    {
        for (int i = 0; i <= 80; i++)
        {
            float h = (float)i / 80;
            float an = 2 * pi * h;
            float x = Mathf.Cos(an);
            float y = Mathf.Sin(an);
            circle.Add(new Vector3(x, y));
        }

        lrc.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lrc.material.SetFloat("_Mode", 2);
        lrc.material.SetFloat("_ColorMode", 0);
        lrc.material.color = gizmoCircleColor;
        lrc.startWidth = lrc.endWidth = 0.015f;
        lrc.positionCount = circle.Count;
        lrc.SetPositions(circle.ToArray());

    }
    void DrawMappingPoint(int Num)
    {
        if (Num != 0)
        {
            for (int i = 0; i <= Num; i++)
            {
                float h = (float)i / Num;
                float an = 2 * pi * h;
                float x = Mathf.Cos(an);
                float y = Mathf.Sin(an);
               

                GameObject temp = Instantiate(pointMapping, pointGRP.transform);
               

                //if theta>0.5pi && theta<1.5*pi then at the left of Y aixe eigher at right
                float righ = ( an < 0.5f * pi || an>1.5*pi) ? 1 : -1;
                float ymapping = (x != -1) ? y / (x + 1) : 0;

                temp.transform.position = new Vector4(0, ymapping, -0.01f);
                mappingPoint.Add(temp);
                circleMapping.Add(new Vector4(0, ymapping, -0.01f, righ));
            }

            TriggerContent = new GameObject("TriggerContent");
            float da = 2 * pi / Num;
            for (int i = 0; i <= Num; i++)
            {
                float x = Mathf.Cos(da * i);
                float y = Mathf.Sin(da * i);
                GameObject temp = Instantiate(soundTrigger, TriggerContent.transform);

                temp.transform.position = new Vector4(x, y, -0.01f);
                soundTriggerGRP.Add(temp);
            }
        }

       
    }
    void DrawRay(Color color )
    {
        Vector3 origin = new Vector3(-1, 0, -0.01f);
        GameObject rayContent = new GameObject("RayContent");
        
        for (int i = 0; i < circleMapping.Count; i++)
        {
            GameObject rayO = new GameObject("Ray"+i);
            rayO.transform.SetParent(rayContent.transform);
            LineRenderer lrray = rayO.AddComponent<LineRenderer>();
            lrray.material = new Material(Shader.Find("Particles/Standard Unlit"));
            lrray.material.SetFloat("_Mode", 2);
            lrray.material.SetFloat("_ColorMode", 0);
            lrray.material.color = color;
            lrray.startWidth = lrray.endWidth = 0.01f;
            lrGRP.Add(lrray);

            Vector3 cPos = new Vector3(circleMapping[i].x, circleMapping[i].y, circleMapping[i].z);
            Vector3 dir = Vector3.Normalize(cPos - origin);
            Vector3 intersectPoint = origin + dir * SphIntersect(origin, dir);

            GameObject tempC = Instantiate(pointCircle, pointGRP.transform);
            tempC.transform.position = intersectPoint;
            inteserctPos.Add(intersectPoint);
            intersectPointO.Add(tempC);

            lrray.SetPosition(0, origin);

            if (circleMapping[i].w==1)
                lrray.SetPosition(1, intersectPoint);
            else
                lrray.SetPosition(1, cPos);

        }
    }
    float SphIntersect(Vector3 ro, Vector3 rd)
    {

        Vector3 oc = ro ;
        float b = Vector3.Dot(oc, rd);
        float c = Vector3.Dot(oc, oc) - 1;
        float d = b * b - c;

        if (d < 0) return 0;
        d = Mathf.Sqrt(d);
        return -b + d;
    }
    void RotationControl()
    {
        Quaternion q = Quaternion.Euler(startAn);

        handlePoint.transform.position = intersectPointO[0].transform.position;
        handlePoint_Mapping.transform.position = mappingPoint[0].transform.position;
        handlePoint.transform.rotation = Quaternion.identity;
        handlePoint_Mapping.transform.rotation = Quaternion.identity;
        backgroundPanel.transform.right = q * Vector3.right;

        realText.text = intersectPointO[0].transform.position.x.ToString("#.####");
        imgText.text = intersectPointO[0].transform.position.y.ToString("#.####");

        for (int i = 0; i < intersectPointO.Count; i++)
        {
            intersectPointO[i].transform.position = q * inteserctPos[i];


            Vector3 v = intersectPointO[i].transform.position - new Vector3(-1, 0, -0.01f);
            float slope = v.y / v.x;

            mappingPoint[i].transform.position = new Vector3(0, slope, -0.01f);

            lrGRP[i].material.color = rayColor;
            lrGRP[i].SetPosition(0, new Vector3(-1, 0, -0.01f));
            float rightUp = (slope < 1 && slope > 0) ? 1 : 0;
            float rightDown = (slope < 0 && slope > -1) ? 1 : 0;

            if (rightUp == 1 || rightDown == 1)
                lrGRP[i].SetPosition(1, intersectPointO[i].transform.position);
            else
                lrGRP[i].SetPosition(1, mappingPoint[i].transform.position);
        }
       

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                isHit = true;
                currentHandleScrPos = handlePoint.transform.position;

            }
        }

        if (Input.GetMouseButton(0))
        {
            if (isHit)
            {
                float N = camera.nearClipPlane;
                float depth = (handlePoint.transform.position - Camera.main.transform.position).z;
                Vector3 ro = ray.origin;
                wInpute = new Vector3(ro.x * depth / N, ro.y * depth / N, currentHandleScrPos.z);
                Vector3 inputeDir = Vector3.Normalize(wInpute);

                float an = Vector3.Dot(inputeDir, currentHandleScrPos);// handlePoint on unit circle so no need normalize
                float sign = (Vector3.Cross(inputeDir, currentHandleScrPos).z > 0) ? -1.0f : 1.0f;

                offsetAn.z = sign * Mathf.Acos(an) * Mathf.Rad2Deg;
                startAn = curretnAn + offsetAn;
               
                xText.text =(sign* Mathf.Acos(an)).ToString("#.####");
               

            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isHit = false;
            curretnAn += offsetAn;
        }
    }
}

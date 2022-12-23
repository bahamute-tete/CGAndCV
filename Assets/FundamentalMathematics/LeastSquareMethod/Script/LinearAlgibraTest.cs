using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LinearAlgibraTest : MonoBehaviour
{
    
    private LineRenderer lineLr0 = new LineRenderer();
    private LineRenderer lineLr1 = new LineRenderer();
    private LineRenderer lineLr2 = new LineRenderer();
    private LineRenderer lineLr3 = new LineRenderer();
    [Header("LineRender")]
    [SerializeField]private Material lineMat;
    [SerializeField] private Material lineMat1;
    [SerializeField] private Material lineMat2;
    [SerializeField] private Material lineMat3;
    [SerializeField] private float lineWidth;
    [SerializeField] private Color lineColor = new Vector4(1, 1, 1, 1);
    [SerializeField] private Color lineColor1 = new Vector4(1, 1, 1, 1);
    [SerializeField] private Color lineColor2 = new Vector4(1, 1, 1, 1);
    [SerializeField] private Color lineColor3 = new Vector4(1, 1, 1, 1);

    [Header("GramSchmidt_Orthogonalization")]
    [SerializeField]
    List<float3> float3s = new List<float3>();
    [SerializeField]
    List<Vector4> vector4s = new List<Vector4>();
    [SerializeField]
    float3[] res;
    [SerializeField]
    Vector4[] res4;

    [Header("QR Solve R")]
    [SerializeField]
    float4x2 A;
    [SerializeField]
    float4x2 Q;
    [SerializeField]
    float2x2 R;

    [Header("LeastSquaresMethod")]
    [SerializeField]
    List<Vector3> p = new List<Vector3>();
    [SerializeField] List<Vector3> inputPoint = new List<Vector3>();
    [SerializeField] List<float> weights = new List<float>();
    [SerializeField] GameObject prefabPoint;
    [SerializeField]List<GameObject> inputPointPrefab = new List<GameObject>();
    public enum FITTINGMOD {Line =0 ,Parabola =1, Polynomial_3_Degree =2,Guassain};
    public FITTINGMOD fittingMod =0;


    [Header("UI")]
    [SerializeField] private Button resetBtn;
    [SerializeField] private Text funtionText;
    [SerializeField] List<Text> markTexts = new List<Text>();
    [SerializeField] GameObject uiMarksContent;
    [SerializeField] List<GameObject> uiObjGRP = new List<GameObject>();
    [SerializeField] Dropdown fittingType;
    [SerializeField] GameObject weightObjPrefab;
    [SerializeField] GameObject uiWeightSlederContent;
    [SerializeField] List<GameObject> weightGRP = new List<GameObject>();
    [SerializeField] GameObject weightTable ;

    [Header("Grid")]
    [SerializeField] int Numcount;
    [SerializeField] Material axiexMat;
    [SerializeField] Color axiexColor;


    void UISet()
    {
        resetBtn.onClick.AddListener(delegate {
            inputPoint.Clear();
            weights.Clear();
            foreach (var o in inputPointPrefab)
            {
                GameObject.DestroyImmediate(o);
            }
            inputPointPrefab.Clear();

            lineLr0.positionCount = 0;
            lineLr1.positionCount = 0;
            lineLr2.positionCount = 0;
            lineLr3.positionCount = 0;


            funtionText.text = "";

            foreach (var o in uiObjGRP)
            {
                GameObject.DestroyImmediate(o);
            }
            uiObjGRP.Clear();

            foreach (var o in weightGRP)
            {
                GameObject.DestroyImmediate(o);
            }
            weightGRP.Clear();
        });

        Dropdown.OptionData optionData0 = new Dropdown.OptionData();
        Dropdown.OptionData optionData1 = new Dropdown.OptionData();
        Dropdown.OptionData optionData2 = new Dropdown.OptionData();
        Dropdown.OptionData optionData3 = new Dropdown.OptionData();

        optionData0.text = "Line";
        optionData1.text = "Parabola";
        optionData2.text = "Polynomial(3Degree)";
        optionData3.text = "Guassain";

        fittingType.options.Add(optionData0);
        fittingType.options.Add(optionData1);
        fittingType.options.Add(optionData2);
        fittingType.options.Add(optionData3);


        fittingType.value = (int)FITTINGMOD.Line;
        fittingType.onValueChanged.AddListener(delegate
        {
            switch (fittingType.value)
            {
                case (int)FITTINGMOD.Line:

                    fittingType.value =0;
                    break;

                case (int)FITTINGMOD.Parabola:
                    fittingType.value = 1;
                    break;

                case (int)FITTINGMOD.Polynomial_3_Degree:
                    fittingType.value = 2;
                    break;

                case (int)FITTINGMOD.Guassain:
                    fittingType.value = 3;
                    break;


            }
        });
        fittingType.transform.GetChild(0).GetComponent<Text>().text = optionData0.text;

        for (int i = 0; i < weightGRP.Count; i++)
        {

            weightGRP[i].transform.GetChild(1).GetComponent<Slider>().value = 1;
           
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        UISet();
        GameObject emptyObj1 = new GameObject("Line");
        lineLr0 = emptyObj1.AddComponent<LineRenderer>();
        

        GameObject emptyObj2 = new GameObject("Parabola");
        lineLr1 = emptyObj2.AddComponent<LineRenderer>();

        GameObject emptyObj3 = new GameObject("Polynomial");
        lineLr2 = emptyObj3.AddComponent<LineRenderer>();

        GameObject emptyObj4 = new GameObject("Gaussain");
        lineLr3 = emptyObj4.AddComponent<LineRenderer>();

        //float3s.Clear();

        res = new float3[float3s.Count];
        res4 = new Vector4[vector4s.Count];

        float2x4 M = new float2x4();
        M.c0 = new float2(1, 2);
        M.c1 = new float2(1, 5);
        M.c2 = new float2(1, 7);
        M.c3 = new float2(1, 8);


        inputPoint.Clear();
        inputPointPrefab.Clear();
        Camera.main.orthographic = true;
        uiObjGRP.Clear();
        DrawGrid();
    }

    // Update is called once per frame
    void Update()
    {
        res = GramSchmidtOrthogonalization(float3s);
        res4 = GramSchmidtOrthogonalization(vector4s);
        SolveR();

        

        Ray pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0)&& EventSystem.current.IsPointerOverGameObject()==false)
        {

            Vector3 tempPoint = new Vector3(pointerRay.GetPoint(1.0f).x, pointerRay.GetPoint(1.0f).y, 1.0f);
            float e = 1.0f;
            inputPoint.Add(tempPoint);
            weights.Add(e);
            GameObject tempObj = Instantiate(prefabPoint, inputPoint[inputPoint.Count-1], Quaternion.identity);
            inputPointPrefab.Add(tempObj);

            GameObject uiObj = new GameObject("ObjUI");
            uiObj.AddComponent<Text>();
            uiObj.AddComponent<ContentSizeFitter>();
            uiObj.transform.SetParent(uiMarksContent.transform);
            uiObjGRP.Add(uiObj);

            GameObject weightUI = Instantiate(weightObjPrefab, uiWeightSlederContent.transform);
            weightGRP.Add(weightUI);

        }

        if (inputPoint.Count != 0 && inputPointPrefab.Count != 0)
        {
            for (int i = 0; i < inputPointPrefab.Count; i++)
            {
                inputPointPrefab[i].transform.position = inputPoint[i];
            }
        }


        for (int i = 0; i < uiObjGRP.Count; i++)
        {
            Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, inputPointPrefab[i].transform.position);
            uiObjGRP[i].GetComponent<RectTransform>().position = pos + new Vector2(0, 45);
            uiObjGRP[i].GetComponent<Text>().font = funtionText.font;
            uiObjGRP[i].GetComponent<Text>().alignment = funtionText.alignment;
            uiObjGRP[i].GetComponent<Text>().fontSize = funtionText.fontSize-5;
            uiObjGRP[i].GetComponent<Text>().color = funtionText.color;
            Vector2 displayVec = new Vector2(inputPointPrefab[i].transform.position.x, inputPointPrefab[i].transform.position.y); 
            uiObjGRP[i].GetComponent<Text>().text ="P"+(i+1).ToString()+ ": "+displayVec. ToString(" #.#");
            uiObjGRP[i].GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            uiObjGRP[i].GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        }


        if (weightGRP.Count != 0)
        {
            for (int i = 0; i < weightGRP.Count; i++)
            {
                weightGRP[i].transform.GetChild(0).GetComponent<Text>().text = "w " + (i+1).ToString();
                weights[i] = weightGRP[i].transform.GetChild(1).GetComponent<Slider>().value;
                weightGRP[i].transform.GetChild(2).GetComponent<Text>().text = weights[i].ToString("#.#");
            }
        }
       

        inputPoint.Sort((v1, v2) => v1.x.CompareTo(v2.x));

        weightTable.SetActive(true);
        if (fittingType.value == 0)
        {
            LeastSquaresMethod_LineV2(inputPoint, weights);
            lineLr1.enabled = false;
            lineLr2.enabled = false;
            lineLr0.enabled = true;
            lineLr3.enabled = false;
        }
        else if (fittingType.value == 1)
        {
            LeastSquaresMethod_Polynomial2Degree(inputPoint, weights);
            lineLr1.enabled = true;
            lineLr2.enabled = false;
            lineLr0.enabled = false;
            lineLr3.enabled = false;
        }
        else if (fittingType.value == 2)
        {
            LeastSquaresMethod_Polynomial3Degree(inputPoint, weights);
            lineLr1.enabled = false;
            lineLr2.enabled = true;
            lineLr0.enabled = false;
            lineLr3.enabled = false;
        }
        else
        {
            LeastSquaresMethod_Gaussion(inputPoint, weights);
            lineLr1.enabled = false;
            lineLr2.enabled = false;
            lineLr0.enabled = false;
            lineLr3.enabled = true;
            weightTable.SetActive(false);
        }
       
       
    }


    /// <summary>
    /// GramSchmidtOrthogonalizatio
    /// </summary>
    float3[] GramSchmidtOrthogonalization(List<float3> vs3)
    {

        float3[] res = new float3[vs3.Count];

        res[0] = vs3[0];
        int n = vs3.Count;

        for (int i = 1; i < n; i++)
        {
            res[i] = vs3[i] - CauculateVector(vs3, res, i + 1);
        }

        return res;
    }

    float3 CauculateVector(List<float3> vec, float3[] res, int index)
    {
        float3 vn = 0;

        for (int i = 1; i < index; i++)
        {
            vn += math.dot(vec[index - 1], res[i - 1]) / math.dot(res[i - 1], res[i - 1]) * res[i - 1];
        }

        return vn;
    }

    Vector4[] GramSchmidtOrthogonalization(List<Vector4> vs4)
    {

        Vector4[] res = new Vector4[vs4.Count];

        res[0] = vs4[0];
        int n = vs4.Count;

        for (int i = 1; i < n; i++)
        {
            res[i] = vs4[i] - CauculateVector(vs4, res,  i +1);
        }

        return res;
    }

    Vector4 CauculateVector(List<Vector4> vec, Vector4[] res, int index )
    {
        Vector4 vn = Vector4.zero;

        for (int i = 1; i < index; i++)
        {
            vn += Vector4.Dot(vec[index-1], res[index-i-1]) / Vector4.Dot(res[index - i - 1], res[index - i - 1]) * res[index - i - 1];

        }

        return vn;
    }

    /// <summary>
    /// QRSolution For R
    /// </summary>
    void SolveR()
    {
       float2x4  QT = math.transpose(Q) ;
       R = math.mul(QT, A);
    }

    /// <summary>
    /// LeastSquaresMethod
    /// </summary>
    void LeastSquaresMethod_Line(List<Vector3> v3s)
    {
        float avergeX = 0;
        float avergeY = 0;
        float sumXX = 0;
        float sumXY = 0;
        float sumX = 0;
        float sumY = 0;

        float m = 0;
        float b = 0;

        float n = v3s.Count;

        for (int i = 0; i < n; i++)
        {
            sumX += v3s[i].x;
            sumY += v3s[i].y;

            sumXX += v3s[i].x * v3s[i].x;
            sumXY += v3s[i].x * v3s[i].y;
        }
        avergeX = sumX / n;
        avergeY = sumY / n;

        m= (n * sumXY - sumX * sumY) / (n*sumXX -sumX*sumX);
        b = avergeY - m * avergeX;

        //Debug.Log("m===" + m);
        //Debug.Log("b===" + b);
        Debug.Log("LineFunctionV1: y= " + m + "x +" + b);

        float x1 = v3s[0].x;
        float y1 = m * x1 + b;
        float x2 = v3s[v3s.Count - 1].x;
        float y2 = m * x2 + b;

        Vector3 p1 = new Vector3(x1, y1, v3s[0].z);
        Vector3 p2 = new Vector3(x2, y2, v3s[v3s.Count - 1].z);
        SetLine(p1, p2, lineLr0, lineColor, 0.01f, lineMat);
    }

    void LeastSquaresMethod_LineV2(List<Vector3> v3s,List<float> w)
    {
       
        float c00 = 0;
        float c01 = 0;
        float c10 = 0;
        float c11 = 0;

        float d00 = 0;
        float d10 = 0;

        float m = 0;
        float b = 0;

        List<Vector3> pr = new List<Vector3>();
        pr.Clear();

       
        if (v3s.Count != 0 && w.Count ==v3s.Count)
        {
            for (int i = 0; i < v3s.Count; i++)
            {
                float3 temp = new float3(1, v3s[i].x, 0)*w[i];
                pr.Add(temp);
            }
               
        }

        float n = pr.Count;

        float2 gama =0;

        if (n != 0)
        {
            for (int i = 0; i < n; i++)
            {
                c00 +=  pr[i].x;
                c01 +=  pr[i].x * pr[i].y;
                c10 +=  pr[i].y * pr[i].x;
                c11 +=  pr[i].y * pr[i].y;

                d00 += pr[i].x * (v3s[i].y * w[i]);
                d10 += pr[i].y * (v3s[i].y * w[i]);
            }

            float2x2 beta = new float2x2();
            beta.c0 = new float2(c00, c01);
            beta.c1 = new float2(c10, c11);

            gama = new float2(d00, d10);


            if (math.determinant(beta) != 0)
            {
                b = math.mul(math.inverse(beta), gama).x;
                m = math.mul(math.inverse(beta), gama).y;
            }
            else
            { Debug.Log("No Inverse"); }

            //Debug.Log("LineFunctionV2 : y= " + m + "x +" + b);

            float x1 = v3s[0].x;
            float y1 = m * x1 + b;
            float x2 = v3s[v3s.Count - 1].x;
            float y2 = m * x2 + b;

            float3 p0 = new Vector3(x1, y1, v3s[0].z);
            float3 p1 = new Vector3(x2, y2, v3s[v3s.Count - 1].z);
            lineLr0.positionCount = 2;
            SetLine(p0, p1, lineLr0, lineColor, lineWidth, lineMat);

            //Vector2 scrPropos = RectTransformUtility.WorldToScreenPoint(Camera.main, p1);
            //funtionText.rectTransform.position = scrPropos + new Vector2(0, 35);
            funtionText.text = "F(X) = " + b.ToString("#.##") + " + "+m.ToString("#.##") + " X " ;

        }

    }

    void LeastSquaresMethod_Polynomial2Degree(List<Vector3> v3s, List<float> w)
    {

        float c00 = 0; float c01 = 0; float c02 = 0;
        float c10 = 0; float c11 = 0; float c12 = 0;
        float c20 = 0; float c21 = 0; float c22 = 0;

        float d00 = 0; float d10 = 0; float d20 = 0;

        float beta0 = 0;
        float beta1 = 0;
        float beta2 = 0;
        float3 cofficient = new float3(beta0,beta1,beta2);

        List<Vector3> pr = new List<Vector3>();
        pr.Clear();


        if (v3s.Count != 0)
        {

            for (int i = 0; i < v3s.Count; i++)
            {
                float3 temp = new float3(1, v3s[i].x, math.pow(v3s[i].x,2)) * w[i];
                pr.Add(temp);
            }
        }

        float n = pr.Count;
       
        if (n != 0)
        {
            for (int i = 0; i < n; i++)
            {
                c00 += pr[i].x;
              
                c01 += pr[i].x * pr[i].y;
                c02 += pr[i].x * pr[i].z;

                c10 += pr[i].y * pr[i].x;
                c11 += pr[i].y * pr[i].y;
                c12 += pr[i].y * pr[i].z;

                c20 += pr[i].z * pr[i].x;
                c21 += pr[i].z * pr[i].y;
                c22 += pr[i].z * pr[i].z;

                d00 += pr[i].x * v3s[i].y*w[i];
                d10 += pr[i].y * v3s[i].y*w[i];
                d20 += pr[i].z * v3s[i].y*w[i];
            }

            float3x3 M = new float3x3();
            M.c0 = new float3(c00, c01, c02);
            M.c1 = new float3(c10, c11, c12);
            M.c2 = new float3(c20, c21, c22);
            
            float3 gama = new float3(d00, d10, d20);

            if (math.determinant(M) != 0)
            {
                cofficient = math.mul(math.inverse(M), gama);
                //Debug.Log(cofficient);
            }
            else
            { Debug.Log("No Inverse"); }

            beta0 = cofficient.x;
            beta1 = cofficient.y;
            beta2 = cofficient.z;
            //Debug.Log("Parabola : y= " + beta0 + " + " + beta1 + "x +" + beta2 + " x*x ");

            List<Vector3> drawPoint = new List<Vector3>();
            drawPoint.Clear();
            int num = 0;

            for (float t = v3s[0].x; t < v3s[v3s.Count - 1].x; t += (v3s[v3s.Count - 1].x - v3s[0].x) / 100)
            {
                float y = beta0 + beta1 * t + beta2 * t * t;
                drawPoint.Add(new Vector3(t, y, 1.0f));
                num++;
            }
          
            if(v3s.Count>2)
            {
                lineLr1.positionCount = num;
                lineLr1.SetPositions(drawPoint.ToArray());
                lineLr1.material = lineMat1;
                lineMat1.color = lineColor1;
                lineLr1.startWidth = lineWidth;
                lineLr1.endWidth = lineWidth;
            }

            funtionText.text = "F(X) = " + beta0.ToString("#.##")+" + "+ beta1.ToString("#.##") +"X"+ " + " + beta2.ToString("#.##")+"X^2";
        }

    }

    void LeastSquaresMethod_Polynomial3Degree(List<Vector3> v3s, List<float> w)
    {
        float4x4 MC = 0;
        float4 V = 0;
        float4 cofficient = 0;

        List<float4> pr = new List<float4>();
        pr.Clear();

        if (v3s.Count != 0)
        {

            for (int i = 0; i < v3s.Count; i++)
            {
                float4 temp = new float4(1, v3s[i].x, math.pow(v3s[i].x, 2), math.pow(v3s[i].x, 3)) * w[i];
                pr.Add(temp);
            }

        }

        float n = pr.Count;

        if (n != 0)
        {
            for (int i = 0; i < n; i++)
            {
                MC.c0.x += pr[i].x;
                MC.c0.y += pr[i].x * pr[i].y;
                MC.c0.z += pr[i].x * pr[i].z;
                MC.c0.w += pr[i].x * pr[i].w;

                MC.c1.x += pr[i].y * pr[i].x;
                MC.c1.y += pr[i].y * pr[i].y;
                MC.c1.z += pr[i].y * pr[i].z;
                MC.c1.w += pr[i].y * pr[i].w;

                MC.c2.x += pr[i].z * pr[i].x;
                MC.c2.y += pr[i].z * pr[i].y;
                MC.c2.z += pr[i].z * pr[i].z;
                MC.c2.w += pr[i].z * pr[i].w;


                MC.c3.x += pr[i].w * pr[i].x;
                MC.c3.y += pr[i].w * pr[i].y;
                MC.c3.z += pr[i].w * pr[i].z;
                MC.c3.w += pr[i].w * pr[i].w;

                V.x += pr[i].x * v3s[i].y * w[i];
                V.y += pr[i].y * v3s[i].y * w[i];
                V.z += pr[i].z * v3s[i].y * w[i];
                V.w += pr[i].w * v3s[i].y * w[i];
            }

            if (math.determinant(MC) != 0)
            {
                cofficient = math.mul(math.inverse(MC), V);
                //Debug.Log(cofficient);
            }
            else
            { Debug.Log("No Inverse"); }

           // Debug.Log("Parabola : y= " + cofficient.x + " + " + cofficient.y + "x +" + cofficient.z + " x*x "+ cofficient.w+ "x*x*x");

            List<Vector3> drawPoint = new List<Vector3>();
            drawPoint.Clear();
            int num = 0;

            for (float t = v3s[0].x; t < v3s[v3s.Count - 1].x; t += (v3s[v3s.Count - 1].x - v3s[0].x) / 100)
            {
                float y = cofficient.x + cofficient.y * t + cofficient.z * t * t+cofficient.w*t*t*t;
                drawPoint.Add(new Vector3(t, y, 1.0f));
                num++;
            }

            if (v3s.Count > 3)
            {
                lineLr2.positionCount = num;
                lineLr2.SetPositions(drawPoint.ToArray());
                lineLr2.material = lineMat2;
                lineMat2.color = lineColor2;
                lineLr2.startWidth = lineWidth;
                lineLr2.endWidth = lineWidth;
            }

            funtionText.text = "F(X) = " + cofficient.x.ToString("#.##")+" + " + cofficient.y.ToString("#.##") + "X"+ " + " + cofficient.z.ToString("#.##") + "X^2"+" + "+cofficient.w.ToString("#.##")+"X^3";
        }

    }

    void LeastSquaresMethod_multi(List<Vector3> v3s)
    {

        float c00 = 0; float c01 = 0; float c02 = 0;
        float c10 = 0; float c11 = 0; float c12 = 0;
        float c20 = 0; float c21 = 0; float c22 = 0;

        float d00 = 0; float d10 = 0; float d20 = 0;

    
        float3 cofficient =0;

        List<Vector3> pr = new List<Vector3>();
        pr.Clear();


        if (v3s.Count != 0)
        {

            for (int i = 0; i < v3s.Count; i++)
            {
                float3 temp = new float3(1, v3s[i].x, v3s[i].y);
                pr.Add(temp);
            }
        }

        float n = pr.Count;

        if (n != 0)
        {
            for (int i = 0; i < n; i++)
            {
                c00 += pr[i].x;

                c01 += pr[i].x * pr[i].y;
                c02 += pr[i].x * pr[i].z;

                c10 += pr[i].y * pr[i].x;
                c11 += pr[i].y * pr[i].y;
                c12 += pr[i].y * pr[i].z;

                c20 += pr[i].z * pr[i].x;
                c21 += pr[i].z * pr[i].y;
                c22 += pr[i].z * pr[i].z;

                d00 += pr[i].x * v3s[i].z;
                d10 += pr[i].y * v3s[i].z;
                d20 += pr[i].z * v3s[i].z;
            }

            float3x3 M = new float3x3();
            M.c0 = new float3(c00, c01, c02);
            M.c1 = new float3(c10, c11, c12);
            M.c2 = new float3(c20, c21, c22);

            float3 gama = new float3(d00, d10, d20);

            if (math.determinant(M) != 0)
            {
                cofficient = math.mul(math.inverse(M), gama);
                //Debug.Log(cofficient);
            }
            else
            { Debug.Log("No Inverse"); }

            
         

            //for (int i  = 0; i  <v3s.Count;i++)
            //{
            //    float z = cofficient.x + cofficient.y * v3s[i].x + cofficient.z * v3s[i].y;

            //    drawPoint.Add()
            //}


            float x1 = v3s[0].x;
            float y1 = v3s[0].y;
            float z1 =cofficient.x+ cofficient.y * x1 + cofficient.z* y1;

            float x2 = v3s[v3s.Count - 1].x;
            float y2 = v3s[v3s.Count - 1].y;
            float z2 = cofficient.x+ cofficient.y * x2 + cofficient.z * y2;

            float3 p0 = new Vector3(x1, y1, z1);
            float3 p1 = new Vector3(x2, y2, z2);
            lineLr0.positionCount = 2;
            SetLine(p0, p1, lineLr0, lineColor, lineWidth, lineMat);

            //funtionText.text = "F(X) = " + beta0.ToString("#.##") + " + " + beta1.ToString("#.##") + "X" + " + " + beta2.ToString("#.##") + "Power(X,2)";
        }

    }

    void LeastSquaresMethod_Gaussion(List<Vector3> v3s, List<float> w)
    {

        float c00 = 0; float c01 = 0; float c02 = 0;
        float c10 = 0; float c11 = 0; float c12 = 0;
        float c20 = 0; float c21 = 0; float c22 = 0;

        float d00 = 0; float d10 = 0; float d20 = 0;

        float beta0 = 0;
        float beta1 = 0;
        float beta2 = 0;
        float3 cofficient = new float3(beta0, beta1, beta2);

        List<Vector3> pr = new List<Vector3>();
        pr.Clear();


        if (v3s.Count != 0)
        {

            for (int i = 0; i < v3s.Count; i++)
            {
                float3 temp = new float3(1, v3s[i].x, math.pow(v3s[i].x, 2));
                pr.Add(temp);
            }
        }

        float n = pr.Count;

        if (n != 0)
        {
            for (int i = 0; i < n; i++)
            {
                c00 += pr[i].x;

                c01 += pr[i].x * pr[i].y;
                c02 += pr[i].x * pr[i].z;

                c10 += pr[i].y * pr[i].x;
                c11 += pr[i].y * pr[i].y;
                c12 += pr[i].y * pr[i].z;

                c20 += pr[i].z * pr[i].x;
                c21 += pr[i].z * pr[i].y;
                c22 += pr[i].z * pr[i].z;

                d00 += pr[i].x * Mathf.Log(v3s[i].y);
                d10 += pr[i].y * Mathf.Log(v3s[i].y);
                d20 += pr[i].z * Mathf.Log(v3s[i].y);
            }

            float3x3 M = new float3x3();
            M.c0 = new float3(c00, c01, c02);
            M.c1 = new float3(c10, c11, c12);
            M.c2 = new float3(c20, c21, c22);

            float3 gama = new float3(d00, d10, d20);

            if (math.determinant(M) != 0)
            {
                cofficient = math.mul(math.inverse(M), gama);
                //Debug.Log(cofficient);
            }
            else
            { Debug.Log("No Inverse"); }

            beta0 = cofficient.x;
            beta1 = cofficient.y;
            beta2 = cofficient.z;
            //Debug.Log("Parabola : y= " + beta0 + " + " + beta1 + "x +" + beta2 + " x*x ");

            float S = -1.0f / beta2;
            float X = beta1 * S / 2.0f;
            float Y =Mathf.Exp( beta0 + X*X/ S);


            List<Vector3> drawPoint = new List<Vector3>();
            drawPoint.Clear();
            int num = 0;

            for (float t = v3s[0].x; t < v3s[v3s.Count - 1].x; t += (v3s[v3s.Count - 1].x - v3s[0].x) / 100)
            {
                float p = -Mathf.Pow(t - X, 2) / S;
                float y = Y * Mathf.Exp(p);
                drawPoint.Add(new Vector3(t, y, 1.0f));
                num++;
            }

            if (v3s.Count > 3)
            {
                lineLr3.positionCount = num;
                lineLr3.SetPositions(drawPoint.ToArray());
                lineLr3.material = lineMat3;
                lineMat3.color = lineColor3;
                lineLr3.startWidth = lineWidth;
                lineLr3.endWidth = lineWidth;
            }

            funtionText.text = "F(X) = " + Y.ToString("#.##") + " *  EXP(  "  +   "( X" + " - " + X.ToString("#.##") + ")^2 " + "/ " + S.ToString("#.##")+")";

        }

    }


    /// <summary>
    /// DrawLine
    /// </summary>
    void SetLine(float3 a, float3 b, LineRenderer lr, Color color, float lineWidth, Material mat)
    {

        lr.material = mat;
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        mat.color = color;

    }

    void DrawGrid()
    {
        GameObject axiexGRP = new GameObject("axiexGRP");

        for (int i = -Numcount; i < Numcount; i++)
        {
            Vector3 actop = new Vector3(i+18, Numcount+10,2.0f);
            Vector3 acbottom = new Vector3(i+18, -Numcount+10,2.0f);

            GameObject content = new GameObject("axlrObj");
            content.transform.SetParent(axiexGRP.transform);
            LineRenderer lr = content.AddComponent<LineRenderer>();
            SetLine(actop, acbottom, lr, axiexColor, 0.01f, axiexMat);
        }

        for (int i = -Numcount; i < Numcount; i++)
        {
            Vector3 acleft = new Vector3(-Numcount+18, i+10,2.0f);
            Vector3 acright = new Vector3(Numcount+18, i+10,2.0f);

            GameObject content = new GameObject("axlrObj");
            content.transform.SetParent(axiexGRP.transform);
            LineRenderer lr = content.AddComponent<LineRenderer>();
            SetLine(acleft, acright, lr, axiexColor, 0.01f, axiexMat);
        }
    }


}

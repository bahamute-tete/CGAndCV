using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VectorFiled : MonoBehaviour
{

    [SerializeField] GameObject prefab;

    float countTime = 0.07f;
    [SerializeField] int  MaxIterationNum = 40;
    int Count=0;
    [Header("EigenValue")]
    [SerializeField] Vector2 attractor;


    [Header("Dynamical system")]
    [SerializeField] Vector2 v1;
    [SerializeField] Vector2 v2;

     Vector3[] initialParticles;
    [SerializeField] int particlesNum;
    GameObject[] particleInstance;

    [Header("VectorFiled")]
    [SerializeField] Material vectorFieldMat;
    [SerializeField] Color vectorFieldColor;
    [SerializeField] GameObject arrow;
    [Range(3,7)]
    [SerializeField] int Numcount;

    [SerializeField] Material axiexMat;
    [SerializeField] Color axiexColor;
    GameObject CC ;
    GameObject arrowGRP;
    Vector3 targetPos;
    Vector3 vc;




    [SerializeField]
    private Material lineMat;
    [SerializeField]
    private float lineWidth;
    [SerializeField]
    private Color lineColor = new Vector4(1, 1, 1, 1);

    [Header("UI")]
    [SerializeField] Text col1x;
    [SerializeField] Text col1y;
    [SerializeField] Text col2x;
    [SerializeField] Text col2y;
    [SerializeField] Text ItertationNum;
    [SerializeField] Slider scol1x;
    [SerializeField] Slider scol1y;
    [SerializeField] Slider scol2x;
    [SerializeField] Slider scol2y;
    [SerializeField] Button resetMatrix;
    [SerializeField]
    Dropdown MatrixSets;


    /// <summary>
    /// v1=0.8,-0.2 v2 =0.5,1.0
    /// 
    /// v1=1.16 ,0 v2=0,0.8
    /// 
    /// v1=0.8 ,0 v2=0,0.64
    /// 
    /// v1=1.44 ,0 v2=0,1.2
    /// </summary>
    // Start is called before the first frame update
    void UISet()
    {
        scol1x.value = v1.x;
        scol1x.onValueChanged.AddListener(delegate {
            v1.x = scol1x.value;
        });

        scol1y.value = v1.y;
        scol1y.onValueChanged.AddListener(delegate {
            v1.y = scol1y.value;
        });

        scol2x.value = v2.x;
        scol2x.onValueChanged.AddListener(delegate {
            v2.x = scol2x.value;
        });


        scol2y.value = v2.y;
        scol2y.onValueChanged.AddListener(delegate {
            v2.y = scol2y.value;
        });

       

        col1x.text = v1.x.ToString();
        col1y.text = v1.y.ToString();
        col2x.text = v2.x.ToString();
        col2y.text = v2.y.ToString();


        Dropdown.OptionData optionData0 = new Dropdown.OptionData();
        Dropdown.OptionData optionData1 = new Dropdown.OptionData();
        Dropdown.OptionData optionData2 = new Dropdown.OptionData();
        Dropdown.OptionData optionData3 = new Dropdown.OptionData();
        Dropdown.OptionData optionData4 = new Dropdown.OptionData();

        optionData0.text = "VectorField_0";
        optionData1.text = "VectorField_1";
        optionData2.text = "VectorField_2";
        optionData3.text = "VectorField_3";
        optionData4.text = "VectorField_4";


        MatrixSets.options.Add(optionData0);
        MatrixSets.options.Add(optionData1);
        MatrixSets.options.Add(optionData2);
        MatrixSets.options.Add(optionData3);
        MatrixSets.options.Add(optionData4);

        MatrixSets.value = 0;
        MatrixSets.onValueChanged.AddListener(delegate
        {
            switch (MatrixSets.value)
            {
                case 0:
                    v1.x = 0.8f; v1.y = -0.2f; ; v2.x = 0.5f; v2.y = 1.0f;

                    break;

                case 1:
                    v1.x =1.16f; v1.y = 0; ; v2.x = 0; v2.y = 0.8f;
                    break;

                case 2:
                    v1.x = 0.8f; v1.y = 0; ; v2.x = 0; v2.y = 0.64f;
                    break;

                case 3:
                    v1.x = 1.44f; v1.y =0; ; v2.x =0; v2.y = 1.2f;
                    break;
                case 4:
                    v1.x = 0.95f; v1.y = 0.05f; v2.x = 0.03f; v2.y = 0.97f;
                    break;
            }

            scol1x.value = v1.x;
            scol1y.value = v1.y;
            scol2x.value = v2.x;
            scol2y.value = v2.y;

        });

        resetMatrix.onClick.AddListener(delegate {

            v1.x = 0.8f; v1.y = -0.2f; ; v2.x = 0.5f; v2.y = 1.0f;
            scol1x.value = v1.x;
            scol1y.value = v1.y;
            scol2x.value = v2.x;
            scol2y.value = v2.y;
            MatrixSets.value = 0;
        });
        MatrixSets.transform.GetChild(0).GetComponent<Text>().text = optionData0.text;
    }

    void Start()
    {         
        UISet();
        GameObject SphereGRP = new GameObject("SphereGRP");
        particleInstance = new GameObject[particlesNum];
        initialParticles = new Vector3[particlesNum];

        for (int i = 0; i < particlesNum; i++)
        {
            Vector2 v2 = CircleUniformDistribution_Rejection(3.0f);
            initialParticles[i] = v2;

            GameObject temp = Instantiate(prefab, v2, Quaternion.identity);
            temp.transform.SetParent(SphereGRP.transform);
            particleInstance[i] = temp;

        }

        DrawGrid();
        DrawVectorField();
    }

    void TexDis()
    {
        col1x.text = v1.x.ToString();
        col1y.text = v1.y.ToString();
        col2x.text = v2.x.ToString();
        col2y.text = v2.y.ToString();
        ItertationNum.text = Count.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        countTime -= Time.deltaTime;
        if (countTime < 0)
        {
            Count++;
            countTime = 0.07f;
        }

       if (Count > MaxIterationNum)
            Count = 0;

        for (int i = 0; i < particlesNum; i++)
        {
            Vector3[] temp = new Vector3[Count];
            temp = IterationFun(v1, v2, initialParticles[i], Count);
            for (int j = 0; j < Count; j++)
            {
                particleInstance[i].transform.position = temp[j];
                if (particleInstance[i].transform.position == temp[0] && particleInstance[i].transform.position == temp[Count-1])
                {
                    particleInstance[i].GetComponent<TrailRenderer>().enabled = false;
                    particleInstance[i].GetComponent<TrailRenderer>().startWidth = 0.0f;
                    particleInstance[i].GetComponent<TrailRenderer>().endWidth = 0.0f;
                    particleInstance[i].GetComponent<TrailRenderer>().emitting = false;
                }
                else
                {
                    particleInstance[i].GetComponent<TrailRenderer>().enabled = true;
                    particleInstance[i].GetComponent<TrailRenderer>().startWidth = 0.05f;
                    particleInstance[i].GetComponent<TrailRenderer>().endWidth = 0.0f;
                    particleInstance[i].GetComponent<TrailRenderer>().emitting = true;
                }
            }
        }


        for (int i = -Numcount; i <= Numcount; i++)
        {
            for (int j = -Numcount; j <= Numcount; j++)
            {
                #region vectorFiled
                vc = new Vector3(i, j, 0);
                Vector3 nextVc = new Vector2(v1.x * vc.x + v2.x * vc.y, v1.y * vc.x + v2.y * vc.y);
                Vector3 dir = Vector3.Normalize(nextVc - vc);
                float length =Vector3.Magnitude(nextVc - vc)*0.5f;
               
                targetPos = vc + dir * length;

                int a = i + Numcount;
                int b = j + Numcount;

                int c = a + b + Numcount * 2 * a;
                CC.transform.GetChild(c).GetComponent<LineRenderer>().SetPosition(0, vc);
                CC.transform.GetChild(c).GetComponent<LineRenderer>().SetPosition(1, targetPos);
                vectorFieldMat.color = vectorFieldColor;
                #endregion
                arrowGRP.transform.GetChild(c).transform.position = targetPos;
                arrowGRP.transform.GetChild(c).transform.up = dir;
            }
        }


        foreach (var o in particleInstance)
        {
            if (Vector3.Magnitude(o.transform.position) > 10000)
            {
                o.transform.position = new Vector3(10000, 10000, 10000);
            }
        }
        TexDis();
    }

    Vector3[] IterationFun(Vector2 initialState,int n,Vector2 atrractor)
    {
        Vector2 priewV = Vector2.zero;
        Vector2 currentV = new Vector2(initialState.x, initialState.y);
        Vector3[] vs = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            priewV = currentV;

            currentV = currentV.x * atrractor.x * new Vector2(1, 0) + currentV.y * atrractor.y * new Vector2(0, 1);
            vs[i] = currentV;
        }

        return vs;
    }

    void DrawLine(Vector3[] pos, float width, Material material, Color color,LineRenderer lr)
    {

        lr.positionCount = pos.Length;
        lr.SetPositions(pos);
        lr.material = material;
        lr.material.color = color;
        lr.startWidth = width;
        lr.endWidth = width;
    }


    Vector3[] IterationFun(Vector2 v1,Vector2 v2,Vector2 initialState, int n)
    {
        Vector2 priewV = Vector2.zero;
        Vector2 currentV = new Vector2(initialState.x, initialState.y);
        Vector3[] vs = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            priewV = currentV;

            currentV.x = v1.x * currentV.x + v2.x * currentV.y;
            currentV.y = v1.y * currentV.x + v2.y * currentV.y;

            vs[i] = currentV;
        }

        return vs;
    }

    Vector2 CircleUniformDistribution_Rejection(float r)
    {
        float x = 0, y = 0;


        x = (-1.0f + 2 * Random.Range(0f, 1.0f)) * r;
        y = (-1.0f + 2 * Random.Range(0f, 1.0f)) * r;

        if (x * x + y * y > r * r)
        {
            x = 0;
            y = 0;
        }
        return new Vector2(x, y);
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

    void DrawGrid()
    {
        GameObject axiexGRP = new GameObject("axiexGRP");
        for (int i = -Numcount; i < Numcount; i++)
        {
            Vector3 actop = new Vector3(i, Numcount);
            Vector3 acbottom = new Vector3(i, -Numcount);

            GameObject content = new GameObject("axlrObj");
            content.transform.SetParent(axiexGRP.transform);
            LineRenderer lr = content.AddComponent<LineRenderer>();
            SetLine(actop, acbottom, lr, axiexColor, 0.01f, axiexMat);
        }

        for (int i = -Numcount; i < Numcount; i++)
        {
            Vector3 acleft = new Vector3(-Numcount, i);
            Vector3 acright = new Vector3(Numcount, i);

            GameObject content = new GameObject("axlrObj");
            content.transform.SetParent(axiexGRP.transform);
            LineRenderer lr = content.AddComponent<LineRenderer>();
            SetLine(acleft, acright, lr, axiexColor, 0.01f, axiexMat);
        }
    }
    void DrawVectorField()
    {
        CC = new GameObject("CC");
        arrowGRP = new GameObject("ArrowGRP");
        for (int i = -Numcount; i <= Numcount; i++)
        {
            for (int j = -Numcount; j <= Numcount; j++)
            {
                #region vectorFiled
                vc = new Vector3(i, j, 0);
                Vector3 nextVc = new Vector2(v1.x * vc.x + v2.x * vc.y, v1.y * vc.x + v2.y * vc.y);
                Vector3 dir = Vector3.Normalize(nextVc - vc);
                float length = Vector3.Magnitude(nextVc - vc)*0.5f;


                targetPos = vc + dir * length;

                GameObject content = new GameObject("Content");
                content.transform.SetParent(CC.transform);

                LineRenderer lr = content.AddComponent<LineRenderer>();
                lr.SetPosition(0, vc);
                lr.SetPosition(1, targetPos);
                lr.startWidth = 0.02f;
                lr.endWidth = 0.02f;
                lr.material = vectorFieldMat;
                vectorFieldMat.color = vectorFieldColor;
                GameObject arrowObj = Instantiate(arrow, targetPos, Quaternion.identity);
                arrowObj.transform.SetParent(arrowGRP.transform);
                arrowObj.transform.up = dir;
                if (arrowObj.transform.position == Vector3.zero)
                {
                    arrowObj.SetActive(false);
                }
                #endregion

            }
        }
    }

    float Remap(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewtonIterationMethod : MonoBehaviour
{
    #region CartesionCoordinates 
    [Header("CartesionCoordinates")]
    [SerializeField] int MaxNumber;
    [SerializeField] float Width;
    [SerializeField] Color x = Color.red;
    [SerializeField] Color y = Color.green;
    [SerializeField] Color z = Color.blue;
    [SerializeField]
    GameObject arrowObj;
    #endregion

    [SerializeField]private List<Vector3> funP = new List<Vector3>();
    private List<Vector3> tangent_P = new List<Vector3>();
    float dt = 0.1f;
    
    [SerializeField] float startX;
    [SerializeField] float Num;
    private float originX;
    GameObject functionLineLrContent;
    GameObject tangentLineLrContent;
    GameObject PerpendiculaLineContet;
 
    [SerializeField] GameObject pointPrefab;
    GameObject realRoot;
    GameObject pointOnFx;
    GameObject pointForPrependicular;
    GameObject nextPoint;
    [SerializeField] private Material lineMat;
    [SerializeField] private Material dotlineMat;
    int P1ID = 0;
    [SerializeField]private float lineWidth;
    [SerializeField]private Color lineColor = new Vector4(1, 1, 1, 1);
    [SerializeField] private Color lineColor2 = new Vector4(1, 1, 1, 1);
    [SerializeField] private Color lineColor3 = new Vector4(1, 1, 1, 1);
    int depth = 0;
    [SerializeField] int iterationCount;
    float root;
    float root0;


    List<GameObject> tangentGRP = new List<GameObject>();
    List<GameObject> perpendicularGRP = new List<GameObject>();
    List<GameObject> pointGRP = new List<GameObject>();

    #region UI 
    [Header("UI")]
    [SerializeField]private Slider originValueSlid;
    [SerializeField] private Button caculate;
    bool bCaculate = false;

    [SerializeField]Dropdown funtionType;
    public enum FunctionType { Normal =0 , Squart, Reciprocal };
    [SerializeField] FunctionType ft;
    int originType = 0;
    [SerializeField] InputField inputeNum;
    float preNum;
    private bool bchildrenUI =false;
    [SerializeField] LayoutGroup ly0;
    [SerializeField] LayoutGroup ly1;
    [SerializeField]Text funtext;
    private string sfuntext = "";
    [SerializeField] Text realroot;
    [SerializeField] Text root1;
    [SerializeField] Text root2;
    [SerializeField] Text tItertationCount;
    [SerializeField] Text indicatorRoot;
    [SerializeField] Text indecatorNext;
    #endregion

    void UISet()
    {
        originValueSlid.value = startX;
        originValueSlid.maxValue = MaxNumber*0.5f;
        originValueSlid.minValue = -MaxNumber*0.5f;
        originValueSlid.onValueChanged.AddListener(delegate {
            startX = originValueSlid.value;
        });

        bCaculate = false;
        caculate.onClick.AddListener(delegate {
            bCaculate = true;
        });


        Dropdown.OptionData optionData0 = new Dropdown.OptionData();
        Dropdown.OptionData optionData1 = new Dropdown.OptionData();
        Dropdown.OptionData optionData2 = new Dropdown.OptionData();


        optionData0.text = "Arbitrary Funtion Root";
        optionData1.text = "Square Root"; 
        optionData2.text = "Reciprocal";

        funtionType.options.Add(optionData0);
        funtionType.options.Add(optionData1);
        funtionType.options.Add(optionData2);

        funtionType.value = 0;
        funtionType.onValueChanged.AddListener(delegate
        {
            switch (funtionType.value)
            {
                case 0:
                     ft =FunctionType.Normal;
                    inputeNum.gameObject.SetActive(false);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ly0.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ly1.GetComponent<RectTransform>());
                    break;

                case 1:
                    ft = FunctionType.Squart;
                    inputeNum.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ly0.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ly1.GetComponent<RectTransform>());
                    break;

                case 2:
                    ft = FunctionType.Reciprocal;
                    inputeNum.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ly0.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ly1.GetComponent<RectTransform>());
                    break;
            }

        });
        funtionType.transform.GetChild(0).GetComponent<Text>().text = optionData0.text;

        inputeNum.text = Num.ToString();
        inputeNum.gameObject.SetActive(false);
        preNum = Num;
        inputeNum.contentType = InputField.ContentType.DecimalNumber;
        inputeNum.onEndEdit.AddListener(delegate(string text)
        {
            Num =Mathf.Abs( float.Parse(text));
        });

       

    }

    void InitialState()
    {
        depth = 0;
        root = 0;
        
        originX = startX;
        
        tangentGRP.Clear();
        perpendicularGRP.Clear();
        pointGRP.Clear();
        tangent_P.Clear();

        pointOnFx = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);
        pointForPrependicular = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);
        nextPoint = Instantiate(pointPrefab, new Vector3(-1000,0,0), Quaternion.identity);
        nextPoint.GetComponent<Renderer>().material.color = Color.red;

        pointGRP.Add(pointOnFx); pointGRP.Add(pointForPrependicular); pointGRP.Add(nextPoint);



        TangentLine(startX, tangent_P);
        tangentLineLrContent.GetComponent<LineRenderer>().positionCount = tangent_P.Count;
        tangentLineLrContent.GetComponent<LineRenderer>().SetPositions(tangent_P.ToArray());


        DrawPerpendicular(startX, PerpendiculaLineContet.GetComponent<LineRenderer>());

        pointOnFx.transform.position = new Vector3(startX, Fun(startX, Num, out sfuntext), 0);
        pointForPrependicular.transform.position = new Vector3(startX, 0, 0);

       
        //Debug.Log("root0 =" + root0);
    }
    // Start is called before the first frame update
    void Start()
    {
        UISet();
        DrawCartesianCoordinates(MaxNumber, Width, x, y, z);
        originType = (int)ft;
        funP.Clear();
        root0 = 0;


        functionLineLrContent = new GameObject("Fun");
        LineRenderer lr = functionLineLrContent.AddComponent<LineRenderer>();
        lr.startWidth = lr.endWidth = lineWidth;
        lr.material = lineMat;
        lineMat.color = lineColor;

        tangentLineLrContent = new GameObject("TangentLine0");
        LineRenderer lr1 = tangentLineLrContent.AddComponent<LineRenderer>();
        lr1.startWidth = lr1.endWidth = lineWidth;
        lr1.material = lineMat;
        lr1.material.color = lineColor2;
        

        PerpendiculaLineContet = new GameObject("PerpendiculaLineContet0");
        LineRenderer perpendiculalr = PerpendiculaLineContet.AddComponent<LineRenderer>();
        perpendiculalr.positionCount = 2;
       
        InitialState();

        if (ft == FunctionType.Reciprocal)
        {
            startX = Random.Range(0.001f, 2.0f / Num - 0.001f);
        }
        else
        {
            startX = Mathf.Clamp(startX, -MaxNumber * 0.5f, MaxNumber * 0.5f);
        }
        iterationCount = 1;
        float nextp = Solve(startX);
        //nextPoint = Instantiate(pointPrefab, new Vector3(nextp, 0, 0), Quaternion.identity);
        Vector2 ImagePos1 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(nextp, 0, 0));
        indecatorNext.rectTransform.position = ImagePos1 + new Vector2(0, 70);

        for (float x = -MaxNumber * 0.5f; x <= 0.5f * MaxNumber; x += dt)
        {
            float y = Fun(x, Num, out sfuntext);
            funP.Add(new Vector2(x, y));
        }
        iterationCount = 20;
        root0 = Solve(startX);

        
        //Debug.Log("root0==" + root0);

        realRoot = Instantiate(pointPrefab, new Vector3(root0, 0, 0), Quaternion.identity);
        realRoot.GetComponent<Renderer>().material.color = Color.green;
        Vector2 ImagePos2 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(root0, 0, 0));
        indicatorRoot.rectTransform.position = ImagePos2 + new Vector2(0, 70);

        lr.positionCount = funP.Count;
        lr.SetPositions(funP.ToArray());
        iterationCount = 0;
      

        realroot.text = "root =" + root0.ToString();
        root1.text = "current X = " + startX.ToString();
        root2.text = "next X = " + nextp.ToString();
        funtext.text = sfuntext;
        tItertationCount.text = "ItertationCount = " + iterationCount.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        float x0 = startX;
        if(ft != FunctionType.Reciprocal)
        startX = Mathf.Clamp(startX, -MaxNumber * 0.5f, MaxNumber * 0.5f);

        

        if ((int)ft != originType)
        {
            
            funP.Clear();
            foreach (var o in tangentGRP)
            {
                GameObject.DestroyImmediate(o);
            }
            foreach (var o in perpendicularGRP)
            {
                GameObject.DestroyImmediate(o);
            }
            foreach (var o in pointGRP)
            {
                GameObject.DestroyImmediate(o);
            }


            if (ft == FunctionType.Reciprocal)
            {
                startX = Random.Range(0.001f, 2.0f / Num - 0.001f);
                originValueSlid.minValue = 0.001f;
                originValueSlid.maxValue = 2.0f / Num - 0.001f;
            }
            else
            {
                originValueSlid.minValue = -MaxNumber * 0.5f;
                originValueSlid.maxValue = MaxNumber * 0.5f;
            }

            for (float x = -MaxNumber * 0.5f; x <= 0.5f * MaxNumber; x += dt)
            {
                float y = Fun(x, Num, out sfuntext);
                funP.Add(new Vector2(x, y));
            }

            depth = 0;
            iterationCount = 1;
            float nextp = Solve(startX);

            iterationCount = 20;
            root0 = Solve(startX);

            functionLineLrContent.GetComponent<LineRenderer>().positionCount = funP.Count;
            functionLineLrContent.GetComponent<LineRenderer>().SetPositions(funP.ToArray());
            realRoot.transform.position = new Vector3(root0, 0, 0);

            InitialState();
            originType = (int)ft;
            iterationCount = 0;
            x0 = startX;
            funtext.text = sfuntext;

            Vector2 ImagePos1 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(nextp, 0, 0));
            indecatorNext.rectTransform.position = ImagePos1 + new Vector2(0, 70);

            Vector2 ImagePos2 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(root0, 0, 0));
            indicatorRoot.rectTransform.position = ImagePos2 + new Vector2(0, 70);

            realroot.text = "root =" + root0.ToString();
            root1.text = "current X = " + x0.ToString();
            root2.text = "next X = " + nextp.ToString();
            tItertationCount.text = "ItertationCount = " + iterationCount.ToString();
        }
        if (originX != startX )
        {

            foreach (var o in tangentGRP)
            {
                GameObject.DestroyImmediate(o);
            }
            foreach (var o in perpendicularGRP)
            {
                GameObject.DestroyImmediate(o);
            }
            foreach (var o in pointGRP)
            {
                GameObject.DestroyImmediate(o);
            }

            InitialState();
            iterationCount = 1;
            float nextp = Solve(startX);
            iterationCount = 0;
            x0 = startX;
            Vector2 ImagePos1 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(nextp, 0, 0));
            indecatorNext.rectTransform.position = ImagePos1 + new Vector2(0, 70);

            realroot.text = "root =" + root0.ToString();
            root1.text = "current X = " + x0.ToString();
            root2.text = "next X = " + nextp.ToString();
            tItertationCount.text = "ItertationCount = " + iterationCount.ToString();

        }
        if (ft == FunctionType.Reciprocal || ft == FunctionType.Squart)
        {
            if (Num != preNum)
            {
                funP.Clear();
                foreach (var o in tangentGRP)
                {
                    GameObject.DestroyImmediate(o);
                }
                foreach (var o in perpendicularGRP)
                {
                    GameObject.DestroyImmediate(o);
                }
                foreach (var o in pointGRP)
                {
                    GameObject.DestroyImmediate(o);
                }


                if (ft == FunctionType.Reciprocal)
                {
                    startX = Random.Range(0.001f, 2.0f / Num - 0.001f);
                    originValueSlid.minValue = 0.001f;
                    originValueSlid.maxValue = 2.0f / Num - 0.001f;
                }
                else
                {
                    originValueSlid.minValue = -MaxNumber * 0.5f;
                    originValueSlid.maxValue = MaxNumber * 0.5f;
                }

                for (float x = -MaxNumber * 0.5f; x <= 0.5f * MaxNumber; x += dt)
                {
                    float y = Fun(x, Num,out sfuntext);
                    funP.Add(new Vector2(x, y));
                }



                depth = 0;

                iterationCount = 1;
                float nextp = Solve(startX);

                Vector2 ImagePos1 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(nextp, 0, 0));
                indecatorNext.rectTransform.position = ImagePos1 + new Vector2(0, 70);

                iterationCount = 20;
                root0 = Solve(startX);

                Vector2 ImagePos2 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(root0, 0, 0));
                indicatorRoot.rectTransform.position = ImagePos2 + new Vector2(0, 70);

                functionLineLrContent.GetComponent<LineRenderer>().positionCount = funP.Count;
                functionLineLrContent.GetComponent<LineRenderer>().SetPositions(funP.ToArray());
                realRoot.transform.position = new Vector3(root0, 0, 0);

                InitialState();
                iterationCount = 0;
                x0 = startX;
                preNum = Num;

                realroot.text = "root =" + root0.ToString();
                root1.text = "current X = " + x0.ToString();
                root2.text = "next X = " + nextp.ToString();
                tItertationCount.text = "ItertationCount = " + iterationCount.ToString();
            }
        }

        if (bCaculate)
        {
            depth = 0;
            iterationCount++;
            float nextP = Solve(x0);
            //Debug.Log("nextP =="+ nextP);
            nextPoint.transform.position = new Vector3(nextP, 0, -0.1f);
            nextPoint.GetComponent<Renderer>().material.color = Color.red;
            x0 = nextP;
            pointOnFx.transform.position = new Vector3(x0, Fun(x0, Num, out sfuntext), 0);

            GameObject tangentLineLrContent1 = new GameObject("TangentLine1");
            LineRenderer lr1 = tangentLineLrContent1.AddComponent<LineRenderer>();
            lr1.startWidth = lr1.endWidth = lineWidth;
            lr1.material = lineMat;
            lr1.material.color = lineColor2;
            List<Vector3> tangent_P1 = new List<Vector3>();
            TangentLine(nextP, tangent_P1);
            tangentLineLrContent1.GetComponent<LineRenderer>().positionCount = tangent_P1.Count;
            tangentLineLrContent1.GetComponent<LineRenderer>().SetPositions(tangent_P1.ToArray());
            tangentGRP.Add(tangentLineLrContent1);

            GameObject PerpendiculaLineContet1 = new GameObject("PerpendiculaLineContet1");
            LineRenderer perpendiculalr1 = PerpendiculaLineContet1.AddComponent<LineRenderer>();
            perpendiculalr1.material.color = lineColor2;
            perpendiculalr1.positionCount = 2;
            DrawPerpendicular(nextP, perpendiculalr1);
            GameObject pointForPrependicular1 = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);
            pointForPrependicular1.transform.position = new Vector3(x0, 0, 0);
            perpendicularGRP.Add(PerpendiculaLineContet1);
            pointGRP.Add(pointForPrependicular1);

            bCaculate = false;

            Vector2 ImagePos1 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(nextP, 0, 0));
            indecatorNext.rectTransform.position = ImagePos1 + new Vector2(0, 70);

            Vector2 ImagePos2 = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(root0, 0, 0));
            indicatorRoot.rectTransform.position = ImagePos2 + new Vector2(0, 70);

            realroot.text ="root ="+ root0.ToString();
            root1.text ="current X = "+ x0.ToString();
            root2.text ="next X = "+ nextP.ToString();
            tItertationCount.text = "ItertationCount = "+ iterationCount.ToString();
        }
    }

    float Fun(float x,float r,out string funtext)
    {
        funtext = "";
        switch ((int)ft)
        {
            case 0:
                funtext ="f(x)=-(x*x*x)/5+(6/10)*(x*x)-(1/10)*x+2";
                return -0.2f * x * x * x + 0.6f * x * x - 0.1f * x + 2;
               

            case 1:
                funtext = "f(x) = x * x = " + r.ToString();
                return x * x - r;


            case 2:
                funtext = "f(x) = 1 / x = " + r.ToString();
                if (x < 0.001f && x > -0.001f)
                    break;
                else
                    return 1.0f / x - r;
                
        }
        return 0;
    }
    float DiffFun(float x)
    {
        switch ((int)ft)
        {
            case 0:
              
                return -0.6f * x * x + 1.2f * x - 0.1f;


            case 1:
               
                return 2*x;


            case 2:
               
                if (x < 0.001f && x > -0.001f)
                    break;
                else
                    return -(1.0f / x) * (1.0f / x);

        }
        return 0;
    }
    float Solve(float x)
    {  
        if (depth < iterationCount)
        {
            depth++;
            if (ft==FunctionType.Reciprocal)
            {
                root = (x < 0.001f && x > -0.001f) ? x - Fun(x, Num, out sfuntext) / DiffFun(x) : 0;
            }
            root = x - Fun(x, Num, out sfuntext) / DiffFun(x);
            root = Solve(root);

        }
        return root;
    }

    float SquareFun(float x ,float r)
    {
        return  x*x-r ;
    }
    float DSquareFun(float x)
    {
        return 2*x;
    }
    float SolveSqrt(float x,float r)
    {
        if (depth < iterationCount)
        {
            depth++;
            root = x - SquareFun(x,r) / DSquareFun(x);
            root = Solve(root);
        }
        return root;
    }

    float Reciprocal(float x, float r)
    {
        float nextp;

        return nextp = x*(2 - r * x);//0< x < 2.0f/r 
    }
    float Sqrt(float x, float r)
    {
        float nextp;
        nextp =0.5f* x * (3 - r * x * x);//0< x < sqrt(3.0/r) ==> 1.0/sqrt(r)
        return r * nextp;
    }


    void TangentLine(float px,List<Vector3> res )
    {
        float sy = DiffFun(px);
        float fx = Fun(px, Num, out sfuntext);
        res.Clear();
        for (float x = -0.5f * MaxNumber; x <= 0.5f*MaxNumber; x += dt)
        {
            float dy = fx + sy * (x - px);
            res.Add(new Vector3(x, dy));
        }
    }
    void DrawPerpendicular(float px, LineRenderer lr)
    {
       
        float y = Fun(px, Num, out sfuntext);
        SetLine(new Vector3(px, y), new Vector3(px, 0), lr, lineColor3, lineWidth, dotlineMat);
        float d1 = Vector3.Distance(new Vector3(px, y), new Vector3(px, 0));
        P1ID = Shader.PropertyToID("_LineWidth");
        lr.material.SetFloat(P1ID, d1);
    }

    void DrawCartesianCoordinates(int size, float aixeWidth, Color x, Color y, Color z)
    {
        float lineWidth = aixeWidth;
        GameObject LineRenderContent = new GameObject("CartesianAixe");


        GameObject CartesianAixeLrGRPX = new GameObject("CartesianAixeLrGRPX");
        CartesianAixeLrGRPX.transform.SetParent(LineRenderContent.transform);

        GameObject CartesianAixeLrGRPY = new GameObject("CartesianAixeLrGRPY");
        CartesianAixeLrGRPY.transform.SetParent(LineRenderContent.transform);

        GameObject CartesianAixeLrGRPZ = new GameObject("CartesianAixeLrGRPZ");
        CartesianAixeLrGRPZ.transform.SetParent(LineRenderContent.transform);

        List<GameObject> CartesianAixeX = new List<GameObject>();
   
        List<GameObject> CartesianAixeY = new List<GameObject>();
   
        List<GameObject> CartesianAixeZ = new List<GameObject>();

        for (int i =0; i <= size; i++)
        {
            GameObject temp = new GameObject("CartesionAixeX"+ i );
            temp.transform.SetParent(CartesianAixeLrGRPX.transform);
            temp.AddComponent<LineRenderer>();
            CartesianAixeX.Add(temp);

            GameObject temp1 = new GameObject("CartesionAixeY"+ i );
            temp1.transform.SetParent(CartesianAixeLrGRPY.transform);
            temp1.AddComponent<LineRenderer>();
            CartesianAixeY.Add(temp1);

            GameObject temp2 = new GameObject("CartesionAixeZ"+ i );
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
            xdir +=new Vector3(0,0,0) + new Vector3(1, 0, 0);
          
            Vector3 xmark = xdir + new Vector3(0, 1, 0) * 0.3f;

            ydir += new Vector3(0, 0, 0) + new Vector3(0, 1, 0);
            Vector3 ymark = ydir + new Vector3(1, 0, 0) * 0.3f;

            zdir += new Vector3(0, 0, 0) + new Vector3(0, 0, 1);
            Vector3 zmark = zdir + new Vector3(0, 1, 0) * 0.3f;

            Vector3 offsetX = new Vector3(0.5f * size, 0, 0);
            Vector3 offsetY = new Vector3(0,0.5f * size, 0);
            Vector3 offsetZ = new Vector3(0,0,0.5f * size);

            if (i != size)
            {
                SetLine(xstart- offsetX, xdir- offsetX, xmark- offsetX, CartesianAixeX[i].GetComponent<LineRenderer>(), x, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                xstart = xdir;

                SetLine(ystart- offsetY, ydir- offsetY, ymark- offsetY, CartesianAixeY[i].GetComponent<LineRenderer>(), y, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                ystart = ydir;

                SetLine(zstart- offsetZ, zdir- offsetZ, zmark- offsetZ, CartesianAixeZ[i].GetComponent<LineRenderer>(), z, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                zstart = zdir;
            }
            else
            {
                SetLine(xdir- offsetX, Vector3.zero, CartesianAixeX[i].GetComponent<LineRenderer>(), x, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                xstart = xdir;
                Instantiate(arrowObj, xstart- offsetX, Quaternion.Euler(0, 0, 270));

                SetLine(ydir- offsetY, Vector3.zero, CartesianAixeY[i].GetComponent<LineRenderer>(), y, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                ystart = ydir;
                Instantiate(arrowObj, ystart- offsetY, Quaternion.Euler(0, 0, 0));

                SetLine(zdir- offsetZ, Vector3.zero, CartesianAixeZ[i].GetComponent<LineRenderer>(), z, lineWidth, new Material(Shader.Find("Particles/Standard Unlit")));
                zstart = zdir;
                Instantiate(arrowObj, zstart- offsetZ, Quaternion.Euler(90, 0, 0));
            }
        }
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
}

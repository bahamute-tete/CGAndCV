using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using UnityEngine.UI;
using TMPro;
using static CustomMathTool.MathematicTool;

//https://docs.microsoft.com/en-us/dotnet/api/system.string.substring?view=net-6.0#system-string-substring(system-int32-system-int32)
public class TaylorSeries : MonoBehaviour
{

    public delegate float TaylorExpansion(float x, int k);
    public enum FunctionName { TExp = 0,Exp, TSin, Sin, TCos,Cos}
    public enum FuntionType {Exp =0,Sin,Cos};
    [SerializeField] FuntionType type = FuntionType.Exp;
    FunctionName fn = FunctionName.Exp;
    FunctionName fn2 = FunctionName.Exp;
    TaylorExpansion[] funcs = { ExpTaylor, SinTaylor, CosTaylor };

    [SerializeField,Range(1,20)]int k = 4;
    [SerializeField, Min(0.01f)] float lineWidth = 0.01f;
    [SerializeField] Color taylorFun = Color.white;
    [SerializeField] Color originFun = Color.white;
    [SerializeField] Rect rect = new Rect(-1, -1, 3, 3);
    System.Text.ASCIIEncoding aSCII = new System.Text.ASCIIEncoding();

   //[SerializeField] List<string> fstr = new List<string>();

    CustomMathTool.MathematicTool mathematicTool;
    public CoordinateSet coodSet;

    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI text;
    //[SerializeField] Color32 color32 = new Color32(255, 255, 134, 255);

    LineRenderer lrTaylor;
    LineRenderer lr;

    private void OnEnable()
    {
        coodSet.OnChange += updateFun;
    }

    private void OnDisable()
    {
        coodSet.OnChange -= updateFun;
    }

    private void Awake()
    {
        
    }

    void Start()
    {
        fn = FunctionName.TExp;
        fn2 = FunctionName.Exp;
        slider.value = k;
        dropdown.value = 0;
        mathematicTool =this.gameObject.AddComponent<CustomMathTool.MathematicTool>();
        GameObject funcObjTaylor = new GameObject("Function_Taylor");
        lrTaylor = funcObjTaylor.AddComponent<LineRenderer>();
       

        GameObject funcObj = new GameObject("Function");
        lr = funcObj.AddComponent<LineRenderer>();

        ImFunc(lrTaylor , coodSet.invokeRect, - 2 * PI, 2 * PI, k, 0.02f, fn, lineWidth, taylorFun);
        ImFunc(lr , coodSet.invokeRect, - 2 * PI, 2 * PI, k, 0.02f, fn2, lineWidth, originFun);

        


        dropdown.onValueChanged.AddListener(delegate
        {
            type = (FuntionType)dropdown.value;

            switch (type)
            {
                case FuntionType.Exp:
                    fn = FunctionName.TExp;
                    fn2 = FunctionName.Exp;
                    break;

                case FuntionType.Sin:
                    fn = FunctionName.TSin;
                    fn2 = FunctionName.Sin;
                    break;

                case FuntionType.Cos:
                    fn = FunctionName.TCos;
                    fn2 = FunctionName.Cos;
                    break;
            }
            ImFunc(lrTaylor , coodSet.invokeRect, - 2 * PI, 2 * PI, k, 0.02f, fn, lineWidth, taylorFun);
            ImFunc(lr , coodSet.invokeRect, - 2 * PI, 2 * PI, k, 0.02f, fn2, lineWidth, originFun);
        });

        slider.onValueChanged.AddListener(delegate
        {
            k = (int)slider.value;
            text.text = "K = " + k;
            ImFunc(lrTaylor, coodSet.invokeRect, -2 * PI, 2 * PI, k, 0.02f, fn, lineWidth, taylorFun);
            ImFunc(lr, coodSet.invokeRect, -2 * PI, 2 * PI, k, 0.02f, fn2, lineWidth, originFun);
        });
    }





    TaylorExpansion Getfunc(FunctionName fn) => funcs[(int)fn];

    static float ExpTaylor(float x, int k)
    {
        float res = 0;

        for (int i = 0; i < k; i++)
        {
            res += Power(x, i) / Factorial(i);
        }

        return res;
    }
    static float SinTaylor(float x, int k)
    {
        float res = 0;

        for (int i = 0; i < k; i++)
        {
            res += Power(-1, i) *Power(x, 2 * i + 1) / Factorial(2 * i + 1);
        }

        return res;
    }
    static float CosTaylor(float x, int k)
    {
        float res = 0;

        for (int i = 0; i < k; i++)
        {
            res += Power(-1, i) * Power(x, 2 * i) / Factorial(2 * i);
        }

        return res;
    }
    void ImFunc(LineRenderer lr, Rect rect, float min, float max,int k,float step ,FunctionName fn,float lineWidth, Color color)
    {
       
        List<Vector3> res = new List<Vector3>();
        for (float x = min ; x <= max; x += step)
        {
            float y = 0;
            float m = 0;
            switch (fn)
            {
                case FunctionName.TExp:
                    y = ExpTaylor(x, k);
                    break;

                case FunctionName.TSin:
                    y = SinTaylor(x, k);
                    break;

                case FunctionName.TCos:
                    y = CosTaylor(x, k);
                    break;

                case FunctionName.Exp:
                    y = Exp(x);
                    break;

                case FunctionName.Sin:
                    y = Sin(x);
                    break;

                case FunctionName.Cos:
                    y = Cos(x);
                    break;

            }
            res.Add(new Vector3(x, y, -.1f));
        }

        List<Vector3> pos = new List<Vector3>();
        foreach (var v in res)
        {
            if (v.y <= rect.yMax && v.y >= rect.yMin && v.x<= rect.xMax && v.x>=rect.xMin)
                pos.Add(v);
        }

        mathematicTool.LineRenderSet(lr, lineWidth, color, pos.Count);

        lr.SetPositions(pos.ToArray());
    }

    float ClampRange(float x, float min, float max) => (x >= max) ? max : (x <= min) ? min : x;

    #region strTest
    //string s = "f(x) =   x^2+ 3*x +(3*a+1) + 4";

    //string[] subs = s.Split('=');
    //foreach (var c in subs)
    //{
    //    if (!c.Equals(""))
    //        fstr.Add(c);
    //}
    //byte[] encodes = aSCII.GetBytes(fstr[1]);
    //List<byte> nc = new List<byte>();
    //foreach (var e in encodes)
    //{
    //    if (e != 32)
    //        nc.Add(e);
    //}
    //string decode = aSCII.GetString(nc.ToArray());
    //Debug.Log("decode =" + decode);
    #endregion

    void updateFun()
    {
        ImFunc(lrTaylor, coodSet.invokeRect, -2 * PI, 2 * PI, k, 0.02f, fn, lineWidth, taylorFun);
        ImFunc(lr, coodSet.invokeRect, -2 * PI, 2 * PI, k, 0.02f, fn2, lineWidth, originFun);
    }

}

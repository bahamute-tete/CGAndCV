using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FourierTransform : MonoBehaviour
{
    private const float pi = 3.1415f;
    Camera camera;
   [SerializeField] Rect cameraRec;
    

    [SerializeField] GameObject massCenterSymbol;
    [Range(0.1f,5.00f)]
    [SerializeField] float cyclesFrequency;
    [Range(1,20)]
    [SerializeField]int Num=3;
    int currentN;
    [SerializeField] GameObject pOnInpute;

    GameObject inputeSignalContent;
    LineRenderer lr0;

    GameObject cyclesSignalContent;
    LineRenderer lr1;

    GameObject outputCoordinateContentX;
    LineRenderer lr2;
    GameObject outputCoordinateContentY;
    LineRenderer lr3;

    List<Vector3> cyclesSignal = new List<Vector3>();
    List<Vector3> inputeSignal = new List<Vector3>();
    List<Vector3> outputSignalX = new List<Vector3>();
    List<Vector3> outputSignalY = new List<Vector3>();
    float curentF;

    float ftx =0;//fourier Transform x
    float fty =0;//fourier Transform y

    Vector3 inputeCurveOffset = new Vector3(1.0f, 2.0f);
    Vector3 cyclesCurevOffset = new Vector3(-2.0f, -1.0f);
    Vector3 outputCurevOffsetX = new Vector3(-2.0f, 1.0f);
    Vector3 outputCurevOffsetY = new Vector3(-2.0f, 1.0f);


    [SerializeField] Vector3 inputeOffset_Screen = new Vector3(0.1f, 0.66f);
    [SerializeField] Vector3 cyclesOffset_Screen = new Vector3(0.26f, 0.295f);
    [SerializeField] Vector3 outputOffsetX_Screen = new Vector3(0.55f, 0.4f);
    [SerializeField] Vector3 outputOffsetY_Screen = new Vector3(0.55f, 0.25f);

    private Vector3[] outConers = new Vector3[4];


    [SerializeField] Slider slider_cyclesFrequency;
    [SerializeField] TextMeshProUGUI proText;

    [SerializeField] Slider slider_SeriersD;
    [SerializeField] TextMeshProUGUI degreeNum;

    [SerializeField] Toggle seriersToggle;
    bool currentS;

    private void Awake()
    {
        slider_cyclesFrequency.value = 0;
        slider_cyclesFrequency.minValue = 0.01f;
        slider_cyclesFrequency.maxValue = 5.0f;
        proText.text = cyclesFrequency.ToString();

        slider_cyclesFrequency.onValueChanged.AddListener(delegate
        {
            cyclesFrequency = slider_cyclesFrequency.value;
            proText.text = cyclesFrequency.ToString();
        });


        slider_SeriersD.value = 1;
        slider_SeriersD.minValue = 1.0f;
        slider_SeriersD.maxValue = 10.0f;
        degreeNum.text = Num.ToString();

        slider_SeriersD.onValueChanged.AddListener(delegate
        {
            Num =(int) slider_SeriersD.value;
            degreeNum.text = Num.ToString();
        });

        seriersToggle.isOn = false;


        camera = Camera.main;
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.transform.position.z, Camera.MonoOrStereoscopicEye.Mono, outConers);
        for (int i = 0; i < outConers.Length; i++)
        {
            outConers[i].z -= camera.transform.position.z;
            outConers[i].x += camera.transform.position.x;
            outConers[i].y += camera.transform.position.y;
        }

        cameraRec = new Rect(outConers[2].x, outConers[2].y, outConers[1].x- outConers[2].x, outConers[0].y- outConers[1].y);

        inputeCurveOffset = new Vector3(cameraRec.x+ inputeOffset_Screen .x* cameraRec.width, cameraRec.y + inputeOffset_Screen .y* cameraRec.height, outConers[0].z);

        cyclesCurevOffset = new Vector3(cameraRec.x + cyclesOffset_Screen.x * cameraRec.width, cameraRec.y + cyclesOffset_Screen.y * cameraRec.height, outConers[0].z);

        outputCurevOffsetX = new Vector3(cameraRec.x + outputOffsetX_Screen.x * cameraRec.width, cameraRec.y + outputOffsetX_Screen.y * cameraRec.height, outConers[0].z);

        outputCurevOffsetY = new Vector3(cameraRec.x + outputOffsetY_Screen.x * cameraRec.width, cameraRec.y + outputOffsetY_Screen.y * cameraRec.height, outConers[0].z);

    }
    // Start is called before the first frame update
    void Start()
    {
       
        LineRenderSetUp();
        curentF = cyclesFrequency;
        currentN = Num;
        currentS = seriersToggle.isOn;

        for (float t = 0; t < 5.0f; t += 0.01f)
        {
            float an = -2.0f * pi * cyclesFrequency * t;

            Vector3 ex = new Vector2(Mathf.Cos(an), Mathf.Sin(an));
            Vector3 gt = FunctionWithInpute(t);
            ftx += (gt.y * ex).x;
            fty += (gt.y * ex).y;
            inputeSignal.Add(gt+ inputeCurveOffset);
            cyclesSignal.Add((gt.y * ex)+cyclesCurevOffset);
        }

        massCenterSymbol.transform.position = new Vector3(ftx / cyclesSignal.Count, fty / cyclesSignal.Count, 0)+ cyclesCurevOffset;

        pOnInpute.transform.position = inputeSignal[0];

        lr0.positionCount = inputeSignal.Count;
        lr1.positionCount = cyclesSignal.Count;


        lr0.SetPositions(inputeSignal.ToArray());
        lr1.SetPositions(cyclesSignal.ToArray());




    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(outConers[0], outConers[1]);
        Gizmos.DrawLine(outConers[1], outConers[2]);
        Gizmos.DrawLine(outConers[2], outConers[3]);
        Gizmos.DrawLine(outConers[3], outConers[0]);

    }

    // Update is called once per frame
    void Update()
    {

        if (seriersToggle.isOn == false)
        {
            slider_SeriersD.gameObject.SetActive(false);
        }
        else
        {
            slider_SeriersD.gameObject.SetActive(true);
        }


        pOnInpute.transform.position =FunctionWithInpute((Time.timeSinceLevelLoad*0.5f) % 5)+ inputeCurveOffset;

        if (cyclesFrequency != curentF || currentN != Num || currentS != seriersToggle.isOn)
        {
            outputSignalX.Clear();
            outputSignalY.Clear();
            inputeSignal.Clear();
            cyclesSignal.Clear();

            for (float t = 0; t < 5.0f; t += 0.01f)
            {
                float an = -2.0f * pi * cyclesFrequency * t;

                Vector3 ex = new Vector2(Mathf.Cos(an), Mathf.Sin(an));
                Vector3 gt = FunctionWithInpute(t);
                ftx += (gt.y * ex).x;
                fty += (gt.y * ex).y;
                inputeSignal.Add(gt + inputeCurveOffset);
                cyclesSignal.Add((gt.y * ex) + cyclesCurevOffset);
            }

            lr0.positionCount = inputeSignal.Count;
            lr1.positionCount = cyclesSignal.Count;


            lr0.SetPositions(inputeSignal.ToArray());
            lr1.SetPositions(cyclesSignal.ToArray());


            for (float j = 0; j < cyclesFrequency; j += 0.01f)
            {
                cyclesSignal.Clear();
                ftx = 0;
                fty = 0;

                for (float t = 0; t < 5.0f; t += 0.01f)
                {
                    float an = -2.0f * pi * j * t;

                    Vector3 ex = new Vector2(Mathf.Cos(an), Mathf.Sin(an));
                    Vector3 gt = FunctionWithInpute(t);
                   
                    cyclesSignal.Add((gt.y * ex)+ cyclesCurevOffset);

                    ftx += (gt.y * ex).x;
                    fty += (gt.y * ex).y;
                }
                massCenterSymbol.transform.position = new Vector3(ftx / cyclesSignal.Count, fty / cyclesSignal.Count, 0)+ cyclesCurevOffset;

                Vector3 massCenter_X = new Vector3(j, ftx / cyclesSignal.Count);
                Vector3 massCenter_Y = new Vector3(j, fty / cyclesSignal.Count);

                lr1.positionCount = cyclesSignal.Count;
                lr1.SetPositions(cyclesSignal.ToArray());


                outputSignalX.Add(massCenter_X + outputCurevOffsetX);
                outputSignalY.Add(massCenter_Y + outputCurevOffsetY);
            }

            lr2.positionCount = outputSignalX.Count;
            lr2.SetPositions(outputSignalX.ToArray());

            lr3.positionCount = outputSignalY.Count;
            lr3.SetPositions(outputSignalY.ToArray());

            curentF = cyclesFrequency;
            currentN = Num;
            currentS = seriersToggle.isOn;
        }

    }

    Vector2 Cmul(Vector2 a, Vector2 b)
    {

      return new  Vector2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
        
    }

    void LineRenderSetUp()
    {
        inputeSignalContent = new GameObject("inputeSignalContent");
        lr0 = inputeSignalContent.AddComponent<LineRenderer>();
        lr0.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr0.startWidth = lr0.endWidth = 0.015f;
        lr0.startColor = lr0.endColor = Color.yellow;

        cyclesSignalContent = new GameObject("cyclesSignalContent");
        lr1 = cyclesSignalContent.AddComponent<LineRenderer>();
        lr1.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr1.startWidth = lr1.endWidth = 0.015f;
        lr1.startColor = lr1.endColor = Color.cyan;

        outputCoordinateContentX = new GameObject("outputCoordinateContent");
        lr2 = outputCoordinateContentX.AddComponent<LineRenderer>();
        lr2.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr2.startWidth = lr2.endWidth = 0.015f;
        lr2.startColor = lr2.endColor = Color.red;

        outputCoordinateContentY = new GameObject("outputCoordinateContent");
        lr3 = outputCoordinateContentY.AddComponent<LineRenderer>();
        lr3.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr3.startWidth = lr3.endWidth = 0.015f;
        lr3.startColor = lr3.endColor = Color.green;
    }

    Vector3 FunctionWithInpute(float t )
    {
        Vector3 res;
        float y = 0;
        float lambda = cameraRec.width * 0.8f / 5.0f;

        if (seriersToggle.isOn == false)
        {
            Vector3 ft1 = new Vector3(t * lambda, Mathf.Cos((2.0f * 2 * pi) * t) + 1);
            Vector3 ft2 = new Vector3(t * lambda, Mathf.Cos((3 * 2 * pi) * t) + 1);
            Vector3 ft3 = new Vector3(t * lambda, Mathf.Sin((4.0f * 2 * pi) * t) + 1);
            res = new Vector3(t * lambda, (ft1.y + ft2.y + ft3.y) / 3.0f);

        }
        else
        {
            for (int n = 0; n < Num; n++)
            {
                int k = ((n + 1) % 2 == 0) ? -1 : 1;
                int c = k * (2 * n + 1);
                y += (Mathf.Cos((2 * n + 1) * pi * t) + 1) / c;
            }
            res = new Vector3(t * lambda, y * 4 / pi);
        }
        return res;
    }
}

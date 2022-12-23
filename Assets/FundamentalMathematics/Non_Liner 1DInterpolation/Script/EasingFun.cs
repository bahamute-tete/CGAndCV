using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EasingFun : MonoBehaviour
{
    [SerializeField] GameObject startPos0;
    [SerializeField] GameObject endPos0;
    [SerializeField] GameObject prefab0;
    GameObject wh01; GameObject wh02;


    [SerializeField] GameObject startPos1;
    [SerializeField] GameObject endPos1;
    [SerializeField] GameObject prefab1;
    GameObject wh11; GameObject wh12;

    [SerializeField] GameObject startPos2;
    [SerializeField] GameObject endPos2;
    [SerializeField] GameObject prefab2;
    GameObject wh21; GameObject wh22;


    float t;
    float _timeScale;
    float ct;

    [SerializeField] Button button;
    bool bPlay = false;

    public Action action;

    public enum EaseType { Linear = 0, SmoothStart2, SmoothStart3, SmoothStop2, SmoothStop3, BellCurve, BezierCubic, ArcStart3, ArcStop3,OutBounce, OutElastic }

    int type1_1; int type1_2;
    [SerializeField] EaseType easeType1_1 = 0;
    [SerializeField] EaseType easeType1_2 = 0;
    [Range(0, 1.0f)]
    [SerializeField] float blend0;



    int type2_1; int type2_2;
    [SerializeField] EaseType easeType2_1 = 0;
    [SerializeField] EaseType easeType2_2 = 0;
    [Range(0, 1.0f)]
    [SerializeField] float blend1;


    int type3_1; int type3_2;
    [SerializeField] EaseType easeType3_1 = 0;
    [SerializeField] EaseType easeType3_2 = 0;
    [Range(0, 1.0f)]
    [SerializeField] float blend2;

    [SerializeField] Vector2 bezierBCPoint0;
    [SerializeField] Material m1;
    [SerializeField] Material m2;
    [SerializeField] Material m3;


    float distance;
    Mesh wheelMesh;
    float wheelDis;
    const float pi = 3.14f;
    [SerializeField] float numCircle;
    Vector3 startAngle;


    public delegate float EasingFunction(float t, EaseType easeType1, EaseType easeType2, float blend);
    EasingFunction easingFunction;


    #region UI
    [SerializeField] Dropdown EaseAType1;
    [SerializeField] Dropdown EaseAType2;
    [SerializeField] Slider EaseABlend;

    [SerializeField] Dropdown EaseBType1;
    [SerializeField] Dropdown EaseBType2;
    [SerializeField] Slider EaseBBlend;

    [SerializeField] Dropdown EaseCType1;
    [SerializeField] Dropdown EaseCType2;
    [SerializeField] Slider EaseCBlend;

    [SerializeField] Slider TimeScale;
    #endregion


    void UISet()
    {
        easeType1_1 = easeType1_2 = 0;
        easeType2_1 = easeType2_2 = (EaseType)1;
        easeType3_1 = easeType3_2 = (EaseType)4;
        blend0 = blend1 = blend2 = 0;

        DropDownSet();

        SliderSet();
    }

    private void SliderSet()
    {
        EaseABlend.value = 0;
        EaseABlend.maxValue = 1.0f;
        EaseABlend.minValue = 0;
        EaseABlend.onValueChanged.AddListener(delegate
        {
            blend0 = EaseABlend.value;
        });

        EaseBBlend.value = 0;
        EaseBBlend.maxValue = 1.0f;
        EaseBBlend.minValue = 0;
        EaseBBlend.onValueChanged.AddListener(delegate
        {
            blend1 = EaseBBlend.value;
        });

        EaseCBlend.value = 0;
        EaseCBlend.maxValue = 1.0f;
        EaseCBlend.minValue = 0;
        EaseCBlend.onValueChanged.AddListener(delegate
        {
            blend2 = EaseCBlend.value;
        });

        _timeScale = 0.5f;
        TimeScale.value = 0.5f;
        TimeScale.maxValue = 1.0f;
        TimeScale.minValue = 0.1f;
        TimeScale.onValueChanged.AddListener(delegate
        {
            _timeScale = TimeScale.value;
        });
    }

    private void DropDownSet()
    {
        #region Setting
        Dropdown.OptionData optionData0 = new Dropdown.OptionData();
        Dropdown.OptionData optionData1 = new Dropdown.OptionData();
        Dropdown.OptionData optionData2 = new Dropdown.OptionData();
        Dropdown.OptionData optionData3 = new Dropdown.OptionData();
        Dropdown.OptionData optionData4 = new Dropdown.OptionData();
        Dropdown.OptionData optionData5 = new Dropdown.OptionData();
        Dropdown.OptionData optionData6 = new Dropdown.OptionData();
        Dropdown.OptionData optionData7 = new Dropdown.OptionData();
        Dropdown.OptionData optionData8 = new Dropdown.OptionData();
        Dropdown.OptionData optionData9 = new Dropdown.OptionData();
        Dropdown.OptionData optionData10 = new Dropdown.OptionData();

        optionData0.text = "Linear";
        optionData1.text = "SmoothStart2";
        optionData2.text = "SmoothStart3";
        optionData3.text = "SmoothStop2";
        optionData4.text = "SmoothStop3";
        optionData5.text = "BellCurve";
        optionData6.text = "BezierCubic";
        optionData7.text = "ArcStart3";
        optionData8.text = "ArcStop3";
        optionData9.text = "EaseOutBounce";
        optionData10.text = "EaseOutElastic";

        EaseAType1.options.Add(optionData0);
        EaseAType1.options.Add(optionData1);
        EaseAType1.options.Add(optionData2);
        EaseAType1.options.Add(optionData3);
        EaseAType1.options.Add(optionData4);
        EaseAType1.options.Add(optionData5);
        EaseAType1.options.Add(optionData6);
        EaseAType1.options.Add(optionData7);
        EaseAType1.options.Add(optionData8);
        EaseAType1.options.Add(optionData9);
        EaseAType1.options.Add(optionData10);

        EaseAType2.options.Add(optionData0);
        EaseAType2.options.Add(optionData1);
        EaseAType2.options.Add(optionData2);
        EaseAType2.options.Add(optionData3);
        EaseAType2.options.Add(optionData4);
        EaseAType2.options.Add(optionData5);
        EaseAType2.options.Add(optionData6);
        EaseAType2.options.Add(optionData7);
        EaseAType2.options.Add(optionData8);
        EaseAType2.options.Add(optionData9);
        EaseAType2.options.Add(optionData10);

        EaseBType1.options.Add(optionData0);
        EaseBType1.options.Add(optionData1);
        EaseBType1.options.Add(optionData2);
        EaseBType1.options.Add(optionData3);
        EaseBType1.options.Add(optionData4);
        EaseBType1.options.Add(optionData5);
        EaseBType1.options.Add(optionData6);
        EaseBType1.options.Add(optionData7);
        EaseBType1.options.Add(optionData8);
        EaseBType1.options.Add(optionData9);
        EaseBType1.options.Add(optionData10);

        EaseBType2.options.Add(optionData0);
        EaseBType2.options.Add(optionData1);
        EaseBType2.options.Add(optionData2);
        EaseBType2.options.Add(optionData3);
        EaseBType2.options.Add(optionData4);
        EaseBType2.options.Add(optionData5);
        EaseBType2.options.Add(optionData6);
        EaseBType2.options.Add(optionData7);
        EaseBType2.options.Add(optionData8);
        EaseBType2.options.Add(optionData9);
        EaseBType2.options.Add(optionData10);

        EaseCType1.options.Add(optionData0);
        EaseCType1.options.Add(optionData1);
        EaseCType1.options.Add(optionData2);
        EaseCType1.options.Add(optionData3);
        EaseCType1.options.Add(optionData4);
        EaseCType1.options.Add(optionData5);
        EaseCType1.options.Add(optionData6);
        EaseCType1.options.Add(optionData7);
        EaseCType1.options.Add(optionData8);
        EaseCType1.options.Add(optionData9);
        EaseCType1.options.Add(optionData10);

        EaseCType2.options.Add(optionData0);
        EaseCType2.options.Add(optionData1);
        EaseCType2.options.Add(optionData2);
        EaseCType2.options.Add(optionData3);
        EaseCType2.options.Add(optionData4);
        EaseCType2.options.Add(optionData5);
        EaseCType2.options.Add(optionData6);
        EaseCType2.options.Add(optionData7);
        EaseCType2.options.Add(optionData8);
        EaseCType2.options.Add(optionData9);
        EaseCType2.options.Add(optionData10);
        #endregion

        EaseAType1.value = 0;
        EaseAType2.value = 0;
        EaseAType1.onValueChanged.AddListener(delegate
        {
            switch (EaseAType1.value)
            {
                case 0:
                    easeType1_1 = 0;
                    break;
                case 1:
                    easeType1_1 = (EaseType)1;
                    break;
                case 2:
                    easeType1_1 = (EaseType)2;
                    break;
                case 3:
                    easeType1_1 = (EaseType)3;
                    break;
                case 4:
                    easeType1_1 = (EaseType)4;
                    break;
                case 5:
                    easeType1_1 = (EaseType)5;
                    break;
                case 6:
                    easeType1_1 = (EaseType)6;
                    break;
                case 7:
                    easeType1_1 = (EaseType)7;
                    break;
                case 8:
                    easeType1_1 = (EaseType)8;
                    break;
                case 9:
                    easeType1_1 = (EaseType)9;
                    break;
                case 10:
                    easeType1_1 = (EaseType)10;
                    break;
            }

        });
        EaseAType2.onValueChanged.AddListener(delegate
        {
            switch (EaseAType2.value)
            {
                case 0:
                    easeType1_2 = 0;
                    break;
                case 1:
                    easeType1_2 = (EaseType)1;
                    break;
                case 2:
                    easeType1_2 = (EaseType)2;
                    break;
                case 3:
                    easeType1_2 = (EaseType)3;
                    break;
                case 4:
                    easeType1_2 = (EaseType)4;
                    break;
                case 5:
                    easeType1_2 = (EaseType)5;
                    break;
                case 6:
                    easeType1_2 = (EaseType)6;
                    break;
                case 7:
                    easeType1_2 = (EaseType)7;
                    break;
                case 8:
                    easeType1_2 = (EaseType)8;
                    break;
                case 9:
                    easeType1_2 = (EaseType)9;
                    break;
                case 10:
                    easeType1_2 = (EaseType)10;
                    break;

            }

        });
        EaseAType1.transform.GetChild(0).GetComponent<Text>().text = optionData0.text;
        EaseAType2.transform.GetChild(0).GetComponent<Text>().text = optionData0.text;

        EaseBType1.value = 1;
        EaseBType2.value = 1;
        EaseBType1.onValueChanged.AddListener(delegate
        {
            switch (EaseBType1.value)
            {
                case 0:
                    easeType2_1 = 0;
                    break;
                case 1:
                    easeType2_1 = (EaseType)1;
                    break;
                case 2:
                    easeType2_1 = (EaseType)2;
                    break;
                case 3:
                    easeType2_1 = (EaseType)3;
                    break;
                case 4:
                    easeType2_1 = (EaseType)4;
                    break;
                case 5:
                    easeType2_1 = (EaseType)5;
                    break;
                case 6:
                    easeType2_1 = (EaseType)6;
                    break;
                case 7:
                    easeType2_1 = (EaseType)7;
                    break;
                case 8:
                    easeType2_1 = (EaseType)8;
                    break;
                case 9:
                    easeType2_1 = (EaseType)9;
                    break;
                case 10:
                    easeType2_1 = (EaseType)10;
                    break;

            }

        });
        EaseBType2.onValueChanged.AddListener(delegate
        {
            switch (EaseBType2.value)
            {
                case 0:
                    easeType2_2 = 0;
                    break;
                case 1:
                    easeType2_2 = (EaseType)1;
                    break;
                case 2:
                    easeType2_2 = (EaseType)2;
                    break;
                case 3:
                    easeType2_2 = (EaseType)3;
                    break;
                case 4:
                    easeType2_2 = (EaseType)4;
                    break;
                case 5:
                    easeType2_2 = (EaseType)5;
                    break;
                case 6:
                    easeType2_2 = (EaseType)6;
                    break;
                case 7:
                    easeType2_2 = (EaseType)7;
                    break;
                case 8:
                    easeType2_2 = (EaseType)8;
                    break;
                case 9:
                    easeType2_2 = (EaseType)9;
                    break;
                case 10:
                    easeType2_2 = (EaseType)10;
                    break;
            }

        });
        EaseBType1.transform.GetChild(0).GetComponent<Text>().text = optionData1.text;
        EaseBType2.transform.GetChild(0).GetComponent<Text>().text = optionData1.text;

        EaseCType1.value = 4;
        EaseCType2.value = 4;
        EaseCType1.onValueChanged.AddListener(delegate
        {
            switch (EaseCType1.value)
            {
                case 0:
                    easeType3_1 = 0;
                    break;
                case 1:
                    easeType3_1 = (EaseType)1;
                    break;
                case 2:
                    easeType3_1 = (EaseType)2;
                    break;
                case 3:
                    easeType3_1 = (EaseType)3;
                    break;
                case 4:
                    easeType3_1 = (EaseType)4;
                    break;
                case 5:
                    easeType3_1 = (EaseType)5;
                    break;
                case 6:
                    easeType3_1 = (EaseType)6;
                    break;
                case 7:
                    easeType3_1 = (EaseType)7;
                    break;
                case 8:
                    easeType3_1 = (EaseType)8;
                    break;
                case 9:
                    easeType3_1 = (EaseType)9;
                    break;
                case 10:
                    easeType3_1 = (EaseType)10;
                    break;
            }

        });
        EaseCType2.onValueChanged.AddListener(delegate
        {
            switch (EaseCType2.value)
            {
                case 0:
                    easeType3_2 = 0;
                    break;
                case 1:
                    easeType3_2 = (EaseType)1;
                    break;
                case 2:
                    easeType3_2 = (EaseType)2;
                    break;
                case 3:
                    easeType3_2 = (EaseType)3;
                    break;
                case 4:
                    easeType3_2 = (EaseType)4;
                    break;
                case 5:
                    easeType3_2 = (EaseType)5;
                    break;
                case 6:
                    easeType3_2 = (EaseType)6;
                    break;
                case 7:
                    easeType3_2 = (EaseType)7;
                    break;
                case 8:
                    easeType3_2 = (EaseType)8;
                    break;
                case 9:
                    easeType3_2 = (EaseType)9;
                    break;
                case 10:
                    easeType3_2 = (EaseType)10;
                    break;
            }

        });
        EaseCType1.transform.GetChild(0).GetComponent<Text>().text = optionData4.text;
        EaseCType2.transform.GetChild(0).GetComponent<Text>().text = optionData4.text;
    }

    // Start is called before the first frame update
    void Start()
    {
        UISet();

        prefab0.transform.position = startPos0.transform.position;
        prefab1.transform.position = startPos1.transform.position;
        prefab2.transform.position = startPos2.transform.position;


        wh01 = prefab0.transform.GetChild(1).gameObject;
        wh02 = prefab0.transform.GetChild(2).gameObject;

        wh11 = prefab1.transform.GetChild(1).gameObject;
        wh12 = prefab1.transform.GetChild(2).gameObject;

        wh21 = prefab2.transform.GetChild(1).gameObject;
        wh22 = prefab2.transform.GetChild(2).gameObject;

        distance = Mathf.Abs(endPos0.transform.position.x - startPos0.transform.position.x);
        wheelMesh = wh01.GetComponent<MeshFilter>().mesh;
        float wheelR = Vector3.Magnitude(wheelMesh.vertices[0]);
        wheelDis = 2 * pi * wheelR;
        numCircle = distance / wheelDis;
        startAngle = gameObject.transform.rotation.eulerAngles;


        button.onClick.AddListener(delegate
        {
            bPlay = true;
            action();
        });

        action = delegate {
            prefab0.transform.position = startPos0.transform.position;
            prefab1.transform.position = startPos1.transform.position;
            prefab2.transform.position = startPos2.transform.position;
        };

        easingFunction += Easing;

        m1.SetInt("easingType1", type1_1);
        m1.SetInt("easingType2", type1_2);
        m1.SetFloat("B", bezierBCPoint0.x);
        m1.SetFloat("C", bezierBCPoint0.y);
        m1.SetFloat("_Mix", blend0);

        m2.SetInt("easingType1", type2_1);
        m2.SetInt("easingType2", type2_2);
        m2.SetFloat("B", bezierBCPoint0.x);
        m2.SetFloat("C", bezierBCPoint0.y);
        m1.SetFloat("_Mix", blend1);

        m3.SetInt("easingType1", type3_1);
        m3.SetInt("easingType2", type3_2);
        m3.SetFloat("B", bezierBCPoint0.x);
        m3.SetFloat("C", bezierBCPoint0.y);
        m1.SetFloat("_Mix", blend2);

        prefab0.transform.GetChild(0).GetComponent<MeshRenderer>().materials[1].color = m1.GetColor("_LineColor");
        prefab1.transform.GetChild(0).GetComponent<MeshRenderer>().materials[1].color = m2.GetColor("_LineColor");
        prefab2.transform.GetChild(0).GetComponent<MeshRenderer>().materials[1].color = m3.GetColor("_LineColor");
    }

    // Update is called once per frame
    void Update()
    {
        #region SetMat
        type1_1 = (int)easeType1_1;
        type1_2 = (int)easeType1_2;
        m1.SetInt("easingType1", type1_1);
        m1.SetInt("easingType2", type1_2);
        m1.SetFloat("B", bezierBCPoint0.x);
        m1.SetFloat("C", bezierBCPoint0.y);
        m1.SetFloat("_Mix", blend0);


        type2_1 = (int)easeType2_1;
        type2_2 = (int)easeType2_2;
        m2.SetInt("easingType1", type2_1);
        m2.SetInt("easingType2", type2_2);
        m2.SetFloat("B", bezierBCPoint0.x);
        m2.SetFloat("C", bezierBCPoint0.y);
        m2.SetFloat("_Mix", blend1);

        type3_1 = (int)easeType3_1;
        type3_2 = (int)easeType3_2;
        m3.SetInt("easingType1", type3_1);
        m3.SetInt("easingType2", type3_2);
        m3.SetFloat("B", bezierBCPoint0.x);
        m3.SetFloat("C", bezierBCPoint0.y);
        m3.SetFloat("_Mix", blend2);
        #endregion


        if (bPlay)
        {


            t += Time.deltaTime * _timeScale;

            ct = Mathf.Clamp01(t);



            float t0 = Easing(t, easeType1_1, easeType1_2, blend0);
            float t1 = Easing(t, easeType2_1, easeType2_2, blend1);
            float t2 = Easing(t, easeType3_1, easeType3_2, blend2);

            Vector3 offsetA0 = new Vector3(0, 180, 2 * pi * numCircle * Mathf.Rad2Deg * t0);
            Vector3 offsetA1 = new Vector3(0, 180, 2 * pi * numCircle * Mathf.Rad2Deg * t1);
            Vector3 offsetA2 = new Vector3(0, 180, 2 * pi * numCircle * Mathf.Rad2Deg * t2);

            m1.SetFloat("_T", t0);
            m2.SetFloat("_T", t1);
            m3.SetFloat("_T", t2);


            Vector3 pos0 = startPos0.transform.position * (1 - t0) + endPos0.transform.position * t0;
            prefab0.transform.position = pos0;

            wh01.transform.rotation = Quaternion.Euler(startAngle + offsetA0);
            wh02.transform.rotation = Quaternion.Euler(startAngle + offsetA0);


            Vector3 pos1 = startPos1.transform.position * (1 - t1) + endPos1.transform.position * t1;
            prefab1.transform.position = pos1;

            wh11.transform.rotation = Quaternion.Euler(startAngle + offsetA1);
            wh12.transform.rotation = Quaternion.Euler(startAngle + offsetA1);


            Vector3 pos2 = startPos2.transform.position * (1 - t2) + endPos2.transform.position * t2;
            prefab2.transform.position = pos2;

            wh21.transform.rotation = Quaternion.Euler(startAngle + offsetA2);
            wh22.transform.rotation = Quaternion.Euler(startAngle + offsetA2);


            if (ct == 1)
            {
                bPlay = false;
                t = 0;
                ct = 0;

            }

        }
    }

    float Remap01(float a, float b, float t)
    {
        return (t - a) / (b - a);
    }

    float Remap(float a, float b, float c, float d, float t)
    {
        float output = (t - a) / (b - a);
        output = Easing(output, EaseType.Linear, EaseType.SmoothStart2, 0);
        output *= (d - c);
        return output + c;
    }

    float Easing(float t, EaseType easeType1, EaseType easeType2, float blendWeight)
    {
        float res1 = 0;
        float res2 = 0;
        float res = 0;

        switch ((int)easeType1)
        {
            case 0:
                res1 = t;
                break;

            case 1:
                res1 = t * t;
                break;

            case 2:
                res1 = t * t * t;
                break;

            case 3:
                res1 = 1 - (1 - t) * (1 - t);
                break;

            case 4:
                res1 = 1 - (1 - t) * (1 - t) * (1 - t);
                break;

            case 5:
                res1 = 16.0f * (1.0f - t) * (1.0f - t) * t * t;
                break;

            case 6:
                res1 = BezierCubic(bezierBCPoint0.x, bezierBCPoint0.y, t);
                break;

            case 7:
                res1 = (t * t * (1 - t)) / 0.1481f;
                break;

            case 8:
                res1 = (t * (1 - t) * (1 - t)) / 0.1481f;
                break;

            case 9:
                res1 = EaseOutBounce(t);
                break;

            case 10:
                res1 = EaseOutElastic(t);
                break;

        }

        switch ((int)easeType2)
        {
            case 0:
                res2 = t;
                break;

            case 1:
                res2 = t * t;
                break;

            case 2:
                res2 = t * t * t;
                break;

            case 3:
                res2 = 1.0f - (1.0f - t) * (1.0f - t);
                break;

            case 4:
                res2 = 1.0f - (1.0f - t) * (1.0f - t) * (1.0f - t);
                break;

            case 5:
                res2 = 16.0f * (1.0f - t) * (1.0f - t) * t * t;
                break;

            case 6:
                res2 = BezierCubic(bezierBCPoint0.x, bezierBCPoint0.y, t);
                break;

            case 7:
                res2 = (t * t * (1 - t)) / 0.1481f;
                break;

            case 8:
                res2 = (t * (1 - t) * (1 - t)) / 0.1481f;
                break;

            case 9:
                res2 = EaseOutBounce(t);
                break;

            case 10:
                res2 = EaseOutElastic(t);
                break;
        }


        if ((int)easeType1 == (int)easeType2 || blendWeight == 0)
        {
            res = res1;
        }
        else
        {
            res = res1 * (1 - blendWeight) + res2 * blendWeight;
        }

        //res = Mathf.Clamp01(res);

        return res;
    }

    float EvolvingEasing(float t, EaseType easeType1, EaseType easeType2, float blendWeight)
    {
        float res = 0;
        res = t * Easing(t, easeType1, easeType2, blendWeight);//Scale
        res = (1 - t) * Easing(t, easeType1, easeType2, blendWeight);//ReverseScale
        res = (t * (1 - t)) / 0.25f;//Arch
        res = (t * t * (1 - t)) / 0.1481f;//ArchSmoothStart
        res = (t * (1 - t) * (1 - t)) / 0.1481f * Easing(t, easeType1, easeType2, blendWeight);//ArchSmoothEnd
        res = Easing(t, easeType1, easeType2, blendWeight) * Easing(t, easeType1, easeType2, blendWeight);//bellCurve
        return res;
    }

    float BezierCubic(float B, float C, float t)
    {
        float res = 0;

        float s1 = (1 - t) * (1 - t) * (1 - t);
        float s2 = (1 - t) * (1 - t) * t;
        float s3 = t * t * (1 - t);
        float s4 = t * t * t;

        res = +3.0f * B * s2 + 3.0f * C * s3 + s4;
        return res;
    }

    float EaseOutBounce(float t)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (t < 1 / d1) {
            return n1 * t * t;
        } else if (t < 2 / d1) {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        } else if (t < 2.5f / d1) {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        } else {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }

    float EaseOutElastic(float t) 
    {
        float  a = (2.0f * Mathf.PI) / 3;

        return (t == 0)? 0: (t == 1) ? 1: Mathf.Pow(2, -10 * t) *Mathf.Sin((t* 10.0f - 0.75f) * a) + 1;
    }


}

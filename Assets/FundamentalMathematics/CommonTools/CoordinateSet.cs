using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static CustomMathTool.MathematicTool;
using System;

public class CoordinateSet : MonoBehaviour
{
    public delegate void RectChange();
    public event RectChange OnChange;

    [SerializeField] Button optionBtn;
    [SerializeField] TMP_InputField xmin, xmax, ymin, ymax;
    [SerializeField] Slider widthSlidr;
    [SerializeField] TextMeshProUGUI widthValue;
    [SerializeField] GameObject gridRec, gridPara;
    [SerializeField] GameObject colorToggleGRP;
    
    Toggle[] colorToggles;
    Color[] colors;


    [SerializeField, Min(0.01f)] float lineWidth = 0.01f;
    [SerializeField, Min(1f)] float unit = 1f;
    [SerializeField] Color lineColor = Color.white;
    Vector3 center = Vector3.zero;
    float xmin_v =-8, xmax_v=8, ymin_v= -5, ymax_v=5;
    Rect rect => new Rect(new Vector2(xmin_v, ymin_v), new Vector2(xmax_v - xmin_v, ymax_v - ymin_v));
    [HideInInspector] public Rect invokeRect = new Rect();
    Coordinate testCoord = new Coordinate(new Rect(-1, -1, 2, 2));
    GameObject coordinate;
    GameObject markUI;

    bool isUnnfold = false;

    private void Awake()
    {
       int colCount= colorToggleGRP.transform.childCount;
        colorToggles = new Toggle[colCount];
        colorToggles = colorToggleGRP.transform.GetComponentsInChildren<Toggle>();
        colors = new Color[colCount];
        for (int i = 0; i < colCount; i++)
        {
            colors[i] = colorToggles[i].GetComponentInChildren<Image>().color;
        }
    }
    void Start()
    {

        invokeRect = rect;
        gridRec.SetActive(isUnnfold);
        gridPara.SetActive(isUnnfold);
        widthValue.text = widthSlidr.value.ToString();
        testCoord.lineWidth = widthSlidr.value;
        testCoord.lineColor = colors[0];
        testCoord.rect = rect;
        coordinate = testCoord.Create();
        markUI =testCoord.MarkSet();

        xmin.text = rect.xMin.ToString();
        xmax.text = rect.xMax.ToString();
        ymin.text = rect.yMin.ToString();
        ymax.text = rect.yMax.ToString();


        optionBtn.onClick.AddListener(delegate
        {
            isUnnfold = !isUnnfold;
            gridRec.SetActive(isUnnfold);
            gridPara.SetActive(isUnnfold);

        });


        xmin.onEndEdit.AddListener(delegate
       {
           invokeRect = String2IntRect(xmin.text, xmax.text, ymin.text, ymax.text);
           testCoord.rect = invokeRect;
           testCoord.Update(coordinate);
           testCoord.MarkUpdate(markUI);
           OnChange();


       });

        xmax.onEndEdit.AddListener(delegate
        {
            invokeRect = String2IntRect(xmin.text, xmax.text, ymin.text, ymax.text);
            testCoord.rect = invokeRect;
            testCoord.Update(coordinate);
            testCoord.MarkUpdate(markUI);
            OnChange();

        });

        ymin.onEndEdit.AddListener(delegate
        {
            invokeRect = String2IntRect(xmin.text, xmax.text, ymin.text, ymax.text);
            testCoord.rect = invokeRect;
            testCoord.Update(coordinate);
            testCoord.MarkUpdate(markUI);
            OnChange();

        });

        ymax.onEndEdit.AddListener(delegate
        {
            invokeRect = String2IntRect(xmin.text, xmax.text, ymin.text, ymax.text);
            testCoord.rect = invokeRect;
            testCoord.Update(coordinate);
            testCoord.MarkUpdate(markUI);
            OnChange();

        });

        widthSlidr.onValueChanged.AddListener(delegate
        {
            testCoord.lineWidth = widthSlidr.value;
            widthValue.text = widthSlidr.value.ToString("#.##");
            testCoord.Update(coordinate);
            
        });

        foreach (var t in colorToggles)
        {
            t.onValueChanged.AddListener(delegate
            {
                testCoord.lineColor = colors[ Array.FindIndex(colorToggles, (x) => (x.isOn))];
                testCoord.Update(coordinate);
            });
        }
    }



    Rect String2IntRect(string xmin, string xmax, string ymin, string ymax)
    {
        int xmin_v = int.Parse(xmin);
        int xmax_v = int.Parse(xmax);
        int ymin_v = int.Parse(ymin);
        int ymax_v = int.Parse(ymax);
        Vector2 size = new Vector2(xmax_v - xmin_v, ymax_v - ymin_v);
        Vector2 pos = new Vector2(xmin_v, ymin_v);

        return new Rect(pos, size);
    }
}

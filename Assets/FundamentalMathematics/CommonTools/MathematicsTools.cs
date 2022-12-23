using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using UnityEngine.UI;
using TMPro; 

namespace CustomMathTool
{
    public  class MathematicTool : MonoBehaviour
    {
        public struct Coordinate
        {
            public float lineWidth;
            public Color lineColor;
            public float unit;
            public Vector3 center;
            public Rect rect;

            public Coordinate(Rect rect , float lineWidth = 0.03f, Color lineColor = default(Color), float unit = 1f, Vector3 center = default(Vector3))
            {
                this.rect = rect;
                this.lineWidth = lineWidth;
                this.lineColor = lineColor;
                this.unit = unit;
                this.center = center;
            }

            void LineRenderInitial(LineRenderer lr, float lineWidth, Color lineColor, int positionCount = 2, params Material[] mats)
            {

                if (mats.Length == 0)
                {
                    lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
                    lr.startColor = lr.endColor = lineColor;
                }
                else
                    lr.materials = mats;

                lr.startWidth = lr.endWidth = lineWidth;
                lr.positionCount = positionCount;

            }
            int[] AxisMark(int count)
            {
                int M = 2 * count + 1;

                int[] markPos = new int[M];

                for (int j = -count; j <= count; j++)
                {
                    markPos[j + count] = j;
                }
                return markPos;
            }

            public GameObject Create()
            {
                Color lineCol = lineColor;
                Vector3 centerPos = center;

                int M = (int)rect.size.x;
                int N = (int)rect.size.y;

                GameObject Container = new GameObject("Coordinate");

                #region Grid
                ////////////////////////////Grid
                GameObject grid = new GameObject("grid");
                grid.transform.SetParent(Container.transform);

                GameObject[] horLines = new GameObject[N+1];
                GameObject[] vertiLines = new GameObject[M+1];

                for (int i =(int) rect.yMin,b=0; i <=(int) rect.yMax; i++,b++)
                {
                    horLines[b] = new GameObject("hline_" + i);
                    LineRenderer hlr = horLines[b].AddComponent<LineRenderer>();
                    Color gridColor = (i % N == 0) ? lineCol : lineCol * 0.5f;
                    LineRenderInitial(hlr, lineWidth, gridColor, M+1);

                    Vector3[] hpos = new Vector3[M+1];

                    for (int j = (int)rect.xMin ,a=0; j <=(int)rect.xMax ; j++,a++)
                    {
                        hpos[a] = (new Vector3(j, i , 0) + centerPos) * unit;
                    }

                    hlr.SetPositions(hpos);
                    horLines[b].transform.SetParent(grid.transform);
                }

                for (int i = (int)rect.xMin,b=0; i <= (int)rect.xMax; i++,b++)
                {
                    vertiLines[b] = new GameObject("vline_" + i);
                    LineRenderer vlr = vertiLines[b].AddComponent<LineRenderer>();
                    Color gridColor = (i % M == 0) ? lineCol : lineCol * 0.5f;
                    LineRenderInitial(vlr, lineWidth, gridColor, N+1);

                    Vector3[] vpos = new Vector3[N+1];

                    for (int j = (int)rect.yMin,a=0; j <= (int)rect.yMax; j++,a++)
                    {
                        vpos[a] = (new Vector3(i , j, 0) + centerPos) * unit;
                    }

                    vlr.SetPositions(vpos);

                    vertiLines[b].transform.SetParent(grid.transform);
                }
                #endregion
                return Container;
            }
            public GameObject MarkSet()
            {
                GameObject canvas = new GameObject("UIMarkCanvas");
                Canvas canv = canvas.AddComponent<Canvas>();
                CanvasScaler cansca = canvas.AddComponent<CanvasScaler>();
                canv.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
                canv.renderMode = RenderMode.ScreenSpaceOverlay;
                cansca.uiScaleMode = (CanvasScaler.ScaleMode)1;
                cansca.referenceResolution = new Vector2(Screen.width, Screen.height);

                int horizonCount = (int)rect.size.x + 1;
                int verticalCount = (int)rect.size.y + 1;

                int[] xmarks = AxisMark(horizonCount);
                int[] ymarks = AxisMark(verticalCount);

                Vector3 offsetX = new Vector3(0, -0.2f, 0);
                Vector3 offsetY = new Vector3(-0.2f, 0, 0);

                for (int i = (int)rect.xMin, a = 0; i <= (int)rect.xMax; i++, a++)
                {
                    if (i != 0)
                    {
                        GameObject text = new GameObject("XMark_" + i);
                        text.transform.SetParent(canvas.transform);
                        ContentSizeFitter csf = text.AddComponent<ContentSizeFitter>();
                        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        TextMeshProUGUI tex = text.AddComponent<TextMeshProUGUI>();
                        Vector2 ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(i, 0, 0) + offsetX);
                        tex.rectTransform.position = ScreenPos;
                        tex.text = i.ToString();

                    }

                }

                for (int i = (int)rect.yMin, a = 0; i <= (int)rect.yMax; i++, a++)
                {

                    GameObject text = new GameObject("YMark_" + i);
                    text.transform.SetParent(canvas.transform);
                    ContentSizeFitter csf = text.AddComponent<ContentSizeFitter>();
                    csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    TextMeshProUGUI tex = text.AddComponent<TextMeshProUGUI>();

                    Vector2 ScreenPos = Vector2.zero;
                    if (i != 0)
                    {
                        ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(0, i, 0) + offsetY);
                    }
                    else
                    {
                        ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(0, i, 0) + offsetY + offsetX);
                    }

                    tex.rectTransform.position = ScreenPos;
                    tex.text = i.ToString();
                }
                return canvas;
            }

            public void Update(GameObject coordinate)
            {
                Color lineCol = lineColor;
                Vector3 centerPos = center;
                int M = (int)rect.size.x;
                int N = (int)rect.size.y;
                Transform grid = coordinate.transform.Find("grid");
                ///////////////////////////////////////////////
                for (int i = 0; i < grid.childCount; i++)
                {
                    Destroy(grid.GetChild(i).gameObject);
                }

                GameObject[] horLines = new GameObject[N + 1];
                GameObject[] vertiLines = new GameObject[M + 1];

                for (int i = (int)rect.yMin, b = 0; i <= (int)rect.yMax; i++, b++)
                {
                    horLines[b] = new GameObject("hline_" + i);
                    LineRenderer hlr = horLines[b].AddComponent<LineRenderer>();
                    Color gridColor = (i % N == 0) ? lineCol : lineCol * 0.5f;
                    LineRenderInitial(hlr, lineWidth, gridColor, M + 1);

                    Vector3[] hpos = new Vector3[M + 1];

                    for (int j = (int)rect.xMin, a = 0; j <= (int)rect.xMax; j++, a++)
                    {
                        hpos[a] = (new Vector3(j, i, 0) + centerPos) * unit;
                    }

                    hlr.SetPositions(hpos);
                    horLines[b].transform.SetParent(grid);
                }

                for (int i = (int)rect.xMin, b = 0; i <= (int)rect.xMax; i++, b++)
                {
                    vertiLines[b] = new GameObject("vline_" + i);
                    LineRenderer vlr = vertiLines[b].AddComponent<LineRenderer>();
                    Color gridColor = (i % M == 0) ? lineCol : lineCol * 0.5f;
                    LineRenderInitial(vlr, lineWidth, gridColor, N + 1);

                    Vector3[] vpos = new Vector3[N + 1];

                    for (int j = (int)rect.yMin, a = 0; j <= (int)rect.yMax; j++, a++)
                    {
                        vpos[a] = (new Vector3(i, j, 0) + centerPos) * unit;
                    }

                    vlr.SetPositions(vpos);

                    vertiLines[b].transform.SetParent(grid);
                }


            }
            public void MarkUpdate(GameObject canvas)
            {
  
            
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    Destroy(canvas.transform.GetChild(i).gameObject);
                }

                int horizonCount = (int)rect.size.x + 1;
                int verticalCount = (int)rect.size.y + 1;

                int[] xmarks = AxisMark(horizonCount);
                int[] ymarks = AxisMark(verticalCount);

                Vector3 offsetX = new Vector3(0, -0.2f, 0);
                Vector3 offsetY = new Vector3(-0.2f, 0, 0);

                for (int i = (int)rect.xMin, a = 0; i <= (int)rect.xMax; i++, a++)
                {
                    if (i != 0)
                    {
                        GameObject text = new GameObject("XMark_" + i);
                        text.transform.SetParent(canvas.transform);
                        ContentSizeFitter csf = text.AddComponent<ContentSizeFitter>();
                        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        TextMeshProUGUI tex = text.AddComponent<TextMeshProUGUI>();
                        Vector2 ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(i, 0, 0) + offsetX);
                        tex.rectTransform.position = ScreenPos;
                        tex.text = i.ToString();

                    }

                }

                for (int i = (int)rect.yMin, a = 0; i <= (int)rect.yMax; i++, a++)
                {

                    GameObject text = new GameObject("YMark_" + i);
                    text.transform.SetParent(canvas.transform);
                    ContentSizeFitter csf = text.AddComponent<ContentSizeFitter>();
                    csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    TextMeshProUGUI tex = text.AddComponent<TextMeshProUGUI>();

                    Vector2 ScreenPos = Vector2.zero;
                    if (i != 0)
                    {
                        ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(0, i, 0) + offsetY);
                    }
                    else
                    {
                        ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector3(0, i, 0) + offsetY + offsetX);
                    }

                    tex.rectTransform.position = ScreenPos;
                    tex.text = i.ToString();
                }


            }

        };

        public static float Factorial(int k)
        {
            float res = 0;
            if (k == 0)
                return 1;

            for (int i = 0; i < k; i++)
            {
                float s = 1f;
                for (int j = 1; j <= i + 1; j++)
                {
                    s *= j;
                }
                res = s;
            }

            return res;
        }

        public static float Power(float x, int k)
        {

            float powRes = x;

            if (k == 0)
                return 1;
            if (k == 1)
                return x;

            for (int i = 1; i < k; i++)
            {
                powRes *= x;
            }
            return powRes;
        }

        public void LineRenderSet(LineRenderer lr, float lineWidth, Color lineColor, int positionCount = 2, params Material[] mats)
        {

            if (mats.Length == 0)
            {
                lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
                lr.startColor = lr.endColor = lineColor;
            }
            else
                lr.materials = mats;

            lr.startWidth = lr.endWidth = lineWidth;
            lr.positionCount = positionCount;

        }
    }
}
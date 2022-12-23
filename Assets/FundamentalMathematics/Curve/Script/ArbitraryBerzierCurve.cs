using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArbitraryBerzierCurve : MonoBehaviour
{


    [SerializeField]
    private List<Vector3> controlPoints= new List<Vector3>();
   // [SerializeField]
    private List<Vector3> berzierPos = new List<Vector3>();

   // [SerializeField]
    private List<Vector3> currentCV = new List<Vector3>();

    private List<GameObject> berzierSph = new List<GameObject>();

   // [SerializeField]
    private List<GameObject> sphGRP = new List<GameObject>();
    private List<GameObject> cvObjGRP = new List<GameObject>();
    [SerializeField]
    private GameObject gameObj;
    public GameObject objCV;
    int berzierRank;
    [Range(10, 100)]
    public int sampleNUM=10;
    private int currentSN = 0;
    private bool isChange = false;
    private Vector3 berzerPoint = new Vector3();

    private LineRenderer lr;

    [SerializeField]
    private GameObject cvSph;
    private LineRenderer lr2;


    private Vector3 BernsteinPolynomial(int n,float t, List<Vector3> ControlPoints)
    {

        int n_factorial = 1;

        int k_factorial = 1;

        int coefficient = 1;

        Vector3 BerzierPos = new Vector3(0, 0);

        if (n >= 2)
        {
            for (int i = 0; i < n + 1; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    if (j == 0) n_factorial = 1;
                    else
                        n_factorial *= j;
                }

                if (i == 0) k_factorial = 1;
                else
                    k_factorial *= i;

                int nk_factorial = n - i;

                for (int j = 1; j < n - i; j++)
                {
                    nk_factorial *= n - i - j;
                }

                if (nk_factorial == 0) nk_factorial = 1;
                coefficient = n_factorial / (k_factorial * nk_factorial);

                //Debug.Log("coeffient=" + coefficient);
                // Debug.Log("Pow(t, i)=" + i);
                // Debug.Log("Pow(1 - t, n - i)=" + (n - i));
                float BernsteinC = Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i) * coefficient;

                BerzierPos += BernsteinC * ControlPoints[i];
            }

        }
        else
        {
            return new Vector3(0,0,0);
        }

        return BerzierPos;

    }

    void Start()
    {
        
        berzierPos.Clear();
        berzierSph.Clear();
        currentCV.Clear();
        sphGRP.Clear();
        cvObjGRP.Clear();
        isChange = false;
        currentSN = sampleNUM;

        
        lr = GetComponent<LineRenderer>();
        lr2 = cvSph.GetComponent<LineRenderer>();

        berzierRank = controlPoints.Count - 1;
        for (int t = 0; t < sampleNUM; t++)
        {
            float h = (float)t /(float) (sampleNUM - 1);
            berzerPoint = BernsteinPolynomial(berzierRank, h, controlPoints);
            berzierPos.Add(berzerPoint);
        }


        foreach (Vector2 bv in berzierPos)
        {
            GameObject obj = Instantiate(gameObj, bv, Quaternion.identity);
            obj.SetActive(false);
            sphGRP.Add(obj);
        }

        for (int i = 0; i < controlPoints.Count; i++)
        {
            currentCV.Add(controlPoints[i]);
            GameObject cv = Instantiate(objCV, controlPoints[i], Quaternion.identity);
            cvObjGRP.Add(cv);
        }

        lr.positionCount = berzierPos.Count;
        float lineWide = 0.1f;
        lr.startWidth = lineWide;
        lr.endWidth = lineWide;


        lr2.positionCount = controlPoints.Count;
        float lineWide2 = 0.1f;
        lr2.startWidth = lineWide2;
        lr2.endWidth = lineWide2;


        for (int i = 0; i < berzierPos.Count; i++)
        {
            lr.SetPosition(i, berzierPos[i]);
        }

        for (int i = 0; i < controlPoints.Count; i++)
        {
            lr2.SetPosition(i, controlPoints[i]);
        }


    }



    // Update is called once per frame
    void Update()
    {



        for (int i = 0; i < controlPoints.Count; i++)
        {
            if (controlPoints[i] != currentCV[i] )
            {
                isChange = true;
                UpdateCV();
            }

            currentCV[i] = controlPoints[i];
            
        }


   
    }

    private void UpdateCV()
    {
        berzierPos.Clear();


        for (int t = 0; t < sampleNUM; t++)
        {
            float h = (float)t /(float) (sampleNUM - 1);
            berzerPoint = BernsteinPolynomial(berzierRank, h, controlPoints);
            berzierPos.Add(berzerPoint);
        }

        for (int i = 0; i < berzierPos.Count; i++)
        {
            sphGRP[i].transform.position = berzierPos[i];
        }

        for (int i = 0; i < controlPoints.Count; i++)
        {
            currentCV.Add(controlPoints[i]);
            cvObjGRP[i].transform.position = currentCV[i];
        }

        lr.positionCount = berzierPos.Count;
        lr2.positionCount = controlPoints.Count;

        for (int i = 0; i < berzierPos.Count; i++)
        {
            lr.SetPosition(i, berzierPos[i]);
        }

        for (int i = 0; i < controlPoints.Count; i++)
        {
            lr2.SetPosition(i, controlPoints[i]);
        }

    }
}

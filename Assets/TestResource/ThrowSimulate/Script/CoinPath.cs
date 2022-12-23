using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPath : MonoBehaviour
{

    [SerializeField] bool showPath = false;
    [SerializeField] float coinLerpTime = 2.5f;

    [SerializeField] GameObject cvSphere;
    List<GameObject> cvSpheres = new List<GameObject>();
    GameObject cvContent;

    [SerializeField] int CVNum=4;
    [SerializeField] private List<Vector3> controlPoints = new List<Vector3>();
    private List<Vector3> currentCV = new List<Vector3>();
    int currentControlPointNum;



    [SerializeField, Range(10, 200)] public int sampleNUM = 10;
    private int currentSN = 0;

    private List<Vector3> berzierPos = new List<Vector3>();
    private LineRenderer lr;//berzier
    LineRenderer lrc;//cv

   
    private Vector3 berzerPoint = new Vector3();
    int berzierRank;
    int n;
    int c = 0;

    bool isSpawnPath = false;

    Vector3 startPos;
    [SerializeField ]Transform endPos;
    private bool isThrow;

    //private void OnEnable()
    //{
    //    PickUpCoin.OnAttractThrowCoin += Attract;
    //    PickUpCoin.OnyThrowCoin += Throw;
    //}

    //private void OnDisable()
    //{
    //    PickUpCoin.OnAttractThrowCoin -= Attract;
    //    PickUpCoin.OnyThrowCoin -= Throw;
    //}


    private void Awake()
    {
        LrSet();
        

    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < CVNum; i++)
        {
            GameObject tempCVS = Instantiate(cvSphere, startPos, Quaternion.identity, cvContent.transform);
            cvSpheres.Add(tempCVS);
        }

        SpawnCoinPath(startPos, endPos.position, showPath);
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawnPath)
        {
            startPos = transform.position;
            showPath = true;
            UpdateBezierPose(startPos, endPos.position, showPath);
        }

        if (!isSpawnPath && isThrow)
        {
            
        }

    }



    private void LrSet()
    {
        cvContent = new GameObject("CVContent");
        lrc = cvContent.AddComponent<LineRenderer>();
        lrc.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lrc.startColor = lrc.endColor = Color.cyan;
        lrc.startWidth = lrc.endWidth = 0.01f;


        lr = transform.gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr.startColor = lr.endColor = Color.yellow;
        lr.startWidth = lr.endWidth = 0.01f;
    }
    private Vector3 BernsteinPolynomial(int n, float t, List<Vector3> ControlPoints)
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


                float BernsteinC = Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i) * coefficient;

                BerzierPos += BernsteinC * ControlPoints[i];
            }

        }
        else
        {
            return new Vector3(0, 0, 0);
        }

        return BerzierPos;

    }
    void SpawnCoinPath( Vector3 startPos,Vector3 endPos, bool showPath)
    {
        berzierPos.Clear();

        currentSN = sampleNUM;
        currentControlPointNum = CVNum;
        berzierRank = CVNum - 1;

        float ds = Vector3.Magnitude(endPos - startPos) / berzierRank;
        Vector3 dir = Vector3.Normalize(endPos - startPos);
        Vector3 newdir = Vector3.Normalize(new Vector3(dir.x, 0.5f, 0.5f));

        Vector3[] controlP = new Vector3[CVNum];

        controlP[0] = startPos;
      
        controlP[1] = controlP[0]+ newdir * ds;
        controlP[2] = controlP[1] + newdir * ds;
        controlP[CVNum - 1] = endPos;

        //for (int i = 0; i < controlP.Length; i++)
        //{
        //    Debug.Log("controlP===" + controlP[i]);
        //}
       
        controlPoints.AddRange(controlP);
       

        for (int i = 0; i < controlPoints.Count; i++)
        {
            cvSpheres[i].transform.position = controlPoints[i];
        }


        for (int t = 0; t < sampleNUM; t++)
        {
            float h = (float)t / (float)(sampleNUM - 1);
            berzerPoint = BernsteinPolynomial(berzierRank, h, controlPoints);
            berzierPos.Add(berzerPoint);
        }




        if (showPath)
        {

            lr.positionCount = berzierPos.Count;
            lrc.positionCount = controlPoints.Count;


            for (int i = 0; i < controlPoints.Count; i++)
            {
                lrc.SetPosition(i, controlPoints[i]);
            }


            for (int i = 0; i < berzierPos.Count; i++)
            {
                lr.SetPosition(i, berzierPos[i]);
            }
        }

    }

    void Attract()
    {
        isSpawnPath = true;
        isThrow = false;
    }

    void Throw()
    {
        isSpawnPath = false;
        isThrow = true;
    }


    void UpdateBezierPose(Vector3 startPos, Vector3 endPos, bool showPath)
    {
        float ds = Vector3.Magnitude(endPos - startPos) / berzierRank;
        Vector3 dir = Vector3.Normalize(endPos - startPos);
        Vector3 newdir = Vector3.Normalize(new Vector3(dir.x, 0.5f,0.5f));

        Vector3[] controlP = new Vector3[CVNum];

        controlP[0] = startPos;
        controlP[1] = controlP[0] + newdir * ds;
        controlP[2] = controlP[1] + newdir * ds;
        controlP[CVNum - 1] = endPos;

        for (int i = 0; i < controlP.Length; i++)
        {
            Debug.Log("controlP===" + controlP[i]);
        }

        controlPoints.AddRange(controlP);


        //for (int i = 0; i < controlPoints.Count; i++)
        //{
        //    cvSpheres[i].transform.position = controlPoints[i];
        //}

        berzierPos.Clear();
        for (int t = 0; t < sampleNUM; t++)
        {
            float h = (float)t / (float)(sampleNUM - 1);
            berzerPoint = BernsteinPolynomial(berzierRank, h, controlPoints);
            berzierPos.Add(berzerPoint);
        }
    }
}

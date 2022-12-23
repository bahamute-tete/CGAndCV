using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayTest : MonoBehaviour
{


    private LineRenderer lr2 = new LineRenderer();


    [SerializeField] GameObject cube;
    [SerializeField] GameObject rayReflector;
    [SerializeField] List<GameObject> cubeGRP = new List<GameObject>();
    [SerializeField] List<Transform> rayReflectorGRP = new List<Transform>();
    List<Vector3> OriginalDirGRP = new List<Vector3>();
    [SerializeField] int xSize;
    [SerializeField] int zSize;
    Vector3 offset = Vector3.zero;
    [SerializeField] float gaps = 0.5f;

    #region Ray 
    [Header("Ray")]
    [SerializeField] GameObject rayCaster;
    private Vector3 rayCasterOriDir;
    private Vector3 ro = new Vector3(0, 0, 0);
    private Vector3 rd = new Vector3(0, 0, 1);
    private float dis = 0;
    private LineRenderer rayLr = new LineRenderer();
    [SerializeField] private Material rayMat;
    [SerializeField] private float rayWidth;
    [SerializeField] private Color rayColor = new Vector4(1, 1, 1, 1);
    [SerializeField] int depth = 0;
    [SerializeField] List<Vector3> raycastHitsPos = new List<Vector3>();
    [SerializeField] List<Vector3> reflecVector = new List<Vector3>();
    [Range(1, 10)]
    [SerializeField] float bounce;


    //public GameObject prefab;
    //[SerializeField]
    //private List<Vector3> hemispherePosGRP = new List<Vector3>();
    //[SerializeField]
    //private List<GameObject> hemisphereObj = new List<GameObject>();
    //private float sphw = 15.0f;

    //Vector3 pos = Vector3.zero;
    #endregion

    #region UI 
    [Header("UI")]
    [SerializeField] private Slider Bounce;
    [SerializeField] List<Slider> rotateControl = new List<Slider>();
    [SerializeField] GameObject rotateSliderPrefab;
    [SerializeField] GameObject rotateSliderContent;
    [SerializeField] private Button resetBtn;
    #endregion

    [SerializeField] GameObject testScence;
    int number = 0;

    void UISet()
    {
        Bounce.value = bounce;
        Bounce.onValueChanged.AddListener(delegate
        {
            bounce = Bounce.value;
        });
        resetBtn.onClick.AddListener(delegate {
            foreach (var r in rotateControl)
            {
                r.value = 0;
            }

        });
    }
    // Start is called before the first frame update
    void Start()
    {

        GameObject o2 = new GameObject("2");
        lr2 = o2.AddComponent<LineRenderer>();

        
        lr2.material = rayMat;
        rayMat.color = rayColor;
       
        lr2.numCornerVertices = 2;
        lr2.startWidth = lr2.endWidth = rayWidth;
        lr2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; 

        raycastHitsPos.Clear();
        reflecVector.Clear();

        GameObject cubeContent = new GameObject("Scence");
        offset = new Vector3(-xSize / 2, 0, -zSize / 2);
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float cubePosx = (gaps+ cube.transform.localScale.x) * x;
                float cubePosZ = (gaps + cube.transform.localScale.z) * z;
                GameObject temp = Instantiate(cube, new Vector3(cubePosx,-0.5f, cubePosZ)+offset, Quaternion.identity);
                temp.transform.SetParent(cubeContent.transform);
                cubeGRP.Add(temp);
            }
        }

         number = testScence.transform.childCount;

        for (int i = 0; i < number; i++)
        {
            GameObject rotateSlider = Instantiate(rotateSliderPrefab);
            rotateSlider.transform.SetParent(rotateSliderContent.transform,false);
            rotateControl.Add(rotateSlider.transform.GetChild(1).GetComponent<Slider>());

            rayReflectorGRP.Add(testScence.transform.GetChild(i));
            OriginalDirGRP.Add(testScence.transform.GetChild(i).forward);
        }

        UISet();
        #region randomPoints 
       // GameObject reflectorGRP = new GameObject("Reflector");
        //for (int i = 0; i < 2; i++)
        //{
        //    int randomPos = Random.Range(0, (xSize-2) * (zSize-2));
           
            
        //    Vector3 refPos = cubeGRP[randomPos].transform.position + new Vector3(0, 0.5f,0);
        //    GameObject temp = Instantiate(rayReflector, refPos, Quaternion.Euler(0,-180,0));
        //    temp.transform.SetParent(reflectorGRP.transform);
        //    rayReflectorGRP.Add(temp);
        //}
        #endregion

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 ro = rayCaster.transform.position;
        Vector3 rd = rayCaster.transform.forward;

        Ray viewRay = new Ray(ro, rd);

       
        depth = 0;
        
        raycastHitsPos.Clear();
        reflecVector.Clear();
        raycastHitsPos.Add(viewRay.origin);


        for (int i = 0; i < number; i++)
        {
                Vector3 an = new Vector3(0, rotateControl[i].value, 0);
                rayReflectorGRP[i].forward = Quaternion.Euler(an) * OriginalDirGRP[i];

        }


        if (Physics.Raycast(viewRay) == false)
        {
            lr2.positionCount = 2;
            lr2.SetPosition(0, raycastHitsPos[0]);
            lr2.SetPosition(1, raycastHitsPos[0] + viewRay.direction * 10);
        }
        else
        {
            RayReflection(viewRay);
            Vector3 endRay = raycastHitsPos[raycastHitsPos.Count - 1] + reflecVector[reflecVector.Count - 1] * 10;
            raycastHitsPos.Add(endRay);
            lr2.positionCount = raycastHitsPos.Count;
            lr2.SetPositions(raycastHitsPos.ToArray());
        }
    }

    void RayReflection(Ray r)
    {
        if (depth <(int) bounce)
        {
            if (Physics.Raycast(r, out RaycastHit raycastHit))
            {
                Vector3 refdir = Vector3.Normalize(Vector3.Reflect(r.direction, raycastHit.normal));
                Ray reflecRay = new Ray(raycastHit.point, refdir);

                depth++;

                raycastHitsPos.Add(raycastHit.point);
                reflecVector.Add(refdir);

                RayReflection(reflecRay);
            }
        }
    }

    Vector3 HemisphereUniformDistribution_Inverse(float r)
    {

        float e1 = Random.Range(0.0f, 1.0f);
        float e2 = Random.Range(0.0f, 1.0f);

        float x = Mathf.Cos(2 * Mathf.PI * e1) * Mathf.Sqrt(e2);
        float y = Mathf.Sqrt(1 - e2);
        float z = Mathf.Sin(2 * Mathf.PI * e1) * Mathf.Sqrt(e2);

        Vector3 v = new Vector3(r * x, r * y, r * z);
        return v;

    }

    Vector3 SquarUniformDistribution_Rejection(int xs,int ys)
    {
        int x = 0, y = 0;
        x =(-1 + 2 * Random.Range(-xs/2,xs/2)) ;
        y =(-1 + 2 * Random.Range(-ys/2, ys/2));
        return new Vector3(x,0, y);
    }
}


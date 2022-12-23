using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticlesTest : MonoBehaviour
{

    LineRenderer boundaryLr = new LineRenderer();
    GameObject boundaryContent;

    TestParticles testParticles;
    public int particlesCount = 1;
    public Rect boundary = new Rect(0, 0, 10, 10);
    public GameObject particlePrefa;
    [Range(0.5f,4.0f)]public float speed = 1.0f;
    [SerializeField] private bool QuadTree = true;
    [SerializeField] private bool isTrail = true;
    List<GameObject> lrContent = new List<GameObject>();
   
   
    GameObject content;
    LineRenderer rangelr = new LineRenderer();
    GameObject rangeObj;

   

    Rect range = new Rect(0,0, 5, 5);

    [SerializeField] Material lineMat;
    [SerializeField] Color lineColor;
    [SerializeField] Color lineColor1;
    [SerializeField] float lineWidth;
    [SerializeField] new GameObject camera;

    [SerializeField]
    Slider Velocity;
    [SerializeField]
    Slider PCounts;
    [SerializeField]
    Toggle quadTreeUse;
    [SerializeField]
    Toggle hasTrail;
    [SerializeField]
    Text count;


    void UISet()
    {
        speed = 1.0f;
        Velocity.value = speed;
        Velocity.onValueChanged.AddListener(delegate {
            speed = Velocity.value;
        });

        particlesCount = 100;
        PCounts.value = particlesCount;
        PCounts.onValueChanged.AddListener(delegate {
            particlesCount = (int)PCounts.value;
        });

        QuadTree = true;
        quadTreeUse.onValueChanged.AddListener(delegate {
            QuadTree = !QuadTree;
        });

        isTrail = true;
        hasTrail.onValueChanged.AddListener(delegate {
            isTrail = !isTrail;
        });
    }
    // Start is called before the first frame update
    void Start()
    {
        UISet();

        camera.transform.position = new Vector3(boundary.center.x, boundary.center.y, camera.transform.position.z);

         boundaryContent = new GameObject("Boundary");
        boundaryLr=boundaryContent.AddComponent<LineRenderer>();
        boundaryLr.positionCount = 5;

        boundaryLr.startWidth = boundaryLr.endWidth = 0.02f;
        boundaryLr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        boundaryLr.startColor = boundaryLr.endColor = Color.yellow;

        Vector3 v1 = new Vector3(boundary.x, boundary.y, 0);
        Vector3 v2 = new Vector3(boundary.x, boundary.y + boundary.height, 0);
        Vector3 v3 = new Vector3(boundary.x + boundary.width, boundary.y + boundary.height, 0);
        Vector3 v4 = new Vector3(boundary.x + boundary.width, boundary.y, 0);
        boundaryLr.SetPosition(0, v1);
        boundaryLr.SetPosition(1, v2);
        boundaryLr.SetPosition(2, v3);
        boundaryLr.SetPosition(3, v4);
        boundaryLr.SetPosition(4, v1);

        testParticles = new TestParticles(particlesCount, boundary, particlePrefa,lineMat, lineColor, lineWidth);

        //content = new GameObject("content");
        //rangeObj = new GameObject("range");
        //rangelr = rangeObj.AddComponent<LineRenderer>();
        //rangelr.positionCount = 5;
        //rangelr.startWidth = rangelr.endWidth = 0.02f;
        //rangelr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        //rangelr.startColor = rangelr.endColor = Color.green;

    }

    // Update is called once per frame
    void Update()
    {
        count.text = testParticles.particlesObj.Count.ToString();



        if (testParticles.particlesObj.Count != particlesCount)
        {
            testParticles.Clear();
            testParticles = new TestParticles(particlesCount, boundary, particlePrefa, lineMat, lineColor, lineWidth);
        }

        for (int i = 0; i < testParticles.particlesObj.Count; i++)
        {
            testParticles.particlesObj[i].GetComponent<MeshRenderer>().material.color =lineColor1;
            testParticles.particlesObj[i].GetComponent<LineRenderer>().enabled = true;
            if (!isTrail)
            {
                testParticles.particlesObj[i].GetComponent<TrailRenderer>().enabled = false;
            }
            else
            {
                testParticles.particlesObj[i].GetComponent<TrailRenderer>().enabled = true;
            }
       
        }

        if (QuadTree)
        {
            QuadTree quadTree = new QuadTree(boundary, 4, 0);
            quadTree.Clear();

            for (int i = 0; i < testParticles.particlesPos.Count; i++)
            {

                if (testParticles.particlesPos[i].x < boundary.x-0.1f || testParticles.particlesPos[i].x > boundary.x + boundary.width+0.1f || testParticles.particlesPos[i].y < boundary.y-0.1f || testParticles.particlesPos[i].y > boundary.y + boundary.height+0.1f)
                {
                    testParticles.particlesPos[i] = new Vector3(boundary.center.x, boundary.center.y, 0);
                    
                }

                if (boundary.Contains(testParticles.particlesPos[i]))
                {
                    quadTree.Insert(testParticles.particlesPos[i]);
                }
            }

            for (int i = 0; i < testParticles.particlesPos.Count; i++)
            {

                List<Vector3> found = new List<Vector3>();
                found.Clear();
                
                float recx = testParticles.particlesPos[i].x - 3 * testParticles.particlesObj[i].transform.localScale.x / 2;
                float recy = testParticles.particlesPos[i].y - 3 * testParticles.particlesObj[i].transform.localScale.y / 2 ;
                float recw =  6 * testParticles.particlesObj[i].transform.localScale.y / 2;
                float rech =  6 * testParticles.particlesObj[i].transform.localScale.y / 2;

                Rect range = new Rect(recx, recy, recw, rech);

                quadTree.QueryRange(range, found);


                Vector3 targetPos = testParticles.particlesPos[i];
                foreach (var p in found)
                {
               
                    float distance = Vector3.Distance(targetPos, p);
                    if (testParticles.particlesPos[i] != p && distance <= testParticles.particlesObj[i].transform.localScale.x)
                    {

                        float randomAngle = Random.Range(0, 2 * 3.1415926f);
                        Vector3 randomDir = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
                        testParticles.particlesDir[i] = randomDir;
                        testParticles.particlesObj[i].GetComponent<MeshRenderer>().material.color = lineColor;
                    }
                        testParticles.particlesObj[i].GetComponent<LineRenderer>().positionCount = found.Count;
                        testParticles.particlesObj[i].GetComponent<LineRenderer>().SetPositions(found.ToArray());
                }
            }

            
        }
        else
        {
            for (int i = 0; i < testParticles.particlesPos.Count; i++)
            {
                testParticles.particlesObj[i].GetComponent<LineRenderer>().enabled = false; 
                Vector3 targetPos = testParticles.particlesPos[i];
                foreach (var p in testParticles.particlesPos)
                {
                    float distance = Vector3.Distance(targetPos, p);
                    if (testParticles.particlesPos[i] != p && distance <= testParticles.particlesObj[i].transform.localScale.x)
                    {
                        float randomAngle = Random.Range(0, 2 * 3.1415926f);
                        Vector3 randomDir = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
                        testParticles.particlesDir[i] = randomDir;
                        testParticles.particlesObj[i].GetComponent<MeshRenderer>().material.color = lineColor;
                    }
                }
            }
        }

        testParticles.Update(speed);
    }  
}


public class TestParticles: Object
{
    public List<Vector3> particlesPos = new List<Vector3>();
    public List<GameObject> particlesObj = new List<GameObject>();
    public List<Vector3> particlesDir = new List<Vector3>();
    private List<GameObject> prefabContent = new List<GameObject>();

    private int particlesCount;
    private float randomAngle;
    private const float pi = 3.1415926f;
    private Rect boundary;

    private GameObject displayPrefab;
    private Material lineMat;
    private Color lineColor;
    private float lineWidth;


    public TestParticles(int particlesCount,Rect boundary,GameObject displayPrefab, Material lineMat, Color lineColor, float lineWidth)
    {
        this.particlesCount = particlesCount;
        this.boundary = boundary;
        this.displayPrefab = displayPrefab;
        this.lineMat = lineMat;
        this.lineColor = lineColor;
        this.lineWidth = lineWidth;

        particlesPos.Clear();
        particlesDir.Clear();
        particlesObj.Clear();
        prefabContent.Clear();

        randomAngle = 0;
        InitialParticles(boundary, displayPrefab);
    }

    private void InitialParticles(Rect boundary,GameObject displayPrefab)
    {
        GameObject Content = new GameObject("Content");
        prefabContent.Add(Content);

        float xmin = boundary.x + boundary.width/4;
        float xmax = xmin + boundary.width/2;
        float ymin = boundary.y+ boundary.height/4;
        float ymax = ymin + boundary.height/2;

        for (int i = 0; i < particlesCount; i++)
        {
            Vector3 p = new Vector3(Random.Range(xmin, xmax), Random.Range(ymin, ymax), 0);
            particlesPos.Add(p);

            randomAngle = Random.Range(0, 2 * pi);
            Vector3 randomDir = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
            Vector3 dir = Vector3.Normalize(randomDir);
            particlesDir.Add(dir);
          
            GameObject temp = Instantiate(displayPrefab, p, Quaternion.identity, Content.transform);
            temp.AddComponent<LineRenderer>();

            temp.GetComponent<LineRenderer>().numCornerVertices = 2;
            temp.GetComponent<LineRenderer>().alignment = LineAlignment.TransformZ;
            temp.GetComponent<LineRenderer>().material = lineMat;
            lineMat.color = lineColor;
            temp.GetComponent<LineRenderer>().startWidth = lineWidth;
            temp.GetComponent<LineRenderer>().endWidth = lineWidth;
            particlesObj.Add(temp);

        }
    }

    public void Update(float velocity)
    {

        for(int i = 0; i  < particlesPos.Count; i ++)
        {

            if (!boundary.Contains(particlesPos[i]))
            {


                Vector3 oPos = particlesDir[i];
                Vector3 nextPos = oPos + particlesDir[i] * Time.fixedDeltaTime;
               

                if (particlesPos[i].x <= boundary.x || particlesPos[i].x >= boundary.x + boundary.width)
                {
                   Mathf.Clamp(particlesPos[i].x,boundary.x,boundary.x+boundary.width);
                    particlesDir[i] = Reflection(oPos - nextPos, new Vector3(1, 0, 0));
                }
                else if (particlesPos[i].y <= boundary.y || particlesPos[i].y >= boundary.y + boundary.height)
                {
                    Mathf.Clamp(particlesPos[i].y, boundary.y, boundary.y + boundary.height);
                    particlesDir[i] = Reflection(oPos - nextPos, new Vector3(0, 1, 0));
                }

                particlesPos[i] += particlesDir[i] * velocity * Time.fixedDeltaTime;
                particlesObj[i].transform.SetPositionAndRotation(particlesPos[i], Quaternion.identity);
            }
          
                particlesPos[i] += particlesDir[i] * velocity * Time.fixedDeltaTime;
                particlesObj[i].transform.SetPositionAndRotation(particlesPos[i], Quaternion.identity);

        }
   
    }

    public void Clear()
    {
        particlesPos.Clear();
        
        particlesDir.Clear();
        foreach (var o in particlesObj)
        {
            GameObject.DestroyImmediate(o);
        }
        particlesObj.Clear();

        foreach (var o in prefabContent)
        {
            GameObject.DestroyImmediate(o);
        }
        prefabContent.Clear();
    }

   private Vector3 Reflection(Vector3 L, Vector3 N)
    {
        Vector3 R = new Vector3(0, 0, 0);
        L = Vector3.Normalize(L);
        N = Vector3.Normalize(N);
        R = Vector3.Normalize(2 * (Vector3.Dot(N, L)) * N - L);
        return  R;
    }

}

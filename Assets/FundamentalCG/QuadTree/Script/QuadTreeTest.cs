using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeTest : MonoBehaviour
{
    [SerializeField] GameObject CameraTrans;
    //QuadTree quadTree = new QuadTree(new Boundary(0, 0, 10.0f, 10.0f), 4);
    //Boundary range = new Boundary(2, 2, 2, 2);
    Rect boundary = new Rect();
    [SerializeField] int sx;
    [SerializeField] int sy;
    [SerializeField] int xwidth;
    [SerializeField] int yheight;
    QuadTree quadTree;
    Rect range = new Rect(2, 2, 2, 2);
    LineRenderer pointlr = new LineRenderer();


    [Range(1,500)]
    [SerializeField] int PointNumber;
    int startPointNumber;
    [SerializeField] List<Vector3> pointPos = new List<Vector3>();
    [SerializeField] GameObject quadPrefab;
    [SerializeField] List<GameObject> quadGRP = new List<GameObject>();
     List<Vector3> found = new List<Vector3>();
    [SerializeField] List<Vector3> res = new List<Vector3>();
    GameObject content;
    LineRenderer rangelr = new LineRenderer();
    GameObject rangeObj;

   [SerializeField] List<GameObject> lrContent = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        CameraTrans.transform.position =new Vector3((float)sx+ (float)xwidth / 2,(float)sy+ (float)yheight/2, CameraTrans.transform.position.z);

        boundary.x = sx; boundary.y = sy; boundary.width = xwidth; boundary.height = yheight;
        quadTree = new QuadTree(boundary, 4,0);

        pointPos.Clear();
        quadGRP.Clear();
        found.Clear();
        res.Clear();

        content = new GameObject("content");

        rangeObj = new GameObject("range");
        rangelr= rangeObj.AddComponent<LineRenderer>();

        rangelr.positionCount = 5;
       
        rangelr.startWidth = rangelr.endWidth = 0.02f;
        rangelr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        rangelr.startColor = rangelr.endColor = Color.green;


      
        for (int i = 0; i < PointNumber; i++)
        {
            Vector3 p = new Vector3(UnityEngine.Random.Range((float)sx,(float) xwidth), UnityEngine.Random.Range((float)sy,(float) yheight), 0);
            quadTree.Insert(p);
            pointPos.Add(p);

            GameObject temp = Instantiate(quadPrefab, p, Quaternion.identity, content.transform);
            quadGRP.Add(temp);
  
        }
        quadTree.Show(lrContent);

    }

    // Update is called once per frame
    void Update()
    {


        Vector3 v1 = new Vector3(range.x, range.y, 0);
        Vector3 v2 = new Vector3(range.x, range.y + range.height, 0);
        Vector3 v3 = new Vector3(range.x + range.width, range.y + range.height, 0);
        Vector3 v4 = new Vector3(range.x + range.width, range.y, 0);
        rangelr.SetPosition(0, v1);
        rangelr.SetPosition(1, v2);
        rangelr.SetPosition(2, v3);
        rangelr.SetPosition(3, v4);
        rangelr.SetPosition(4, v1);

       

        if (pointPos.Count != PointNumber)
        {

            
            foreach (var o in lrContent)
            {
                GameObject.DestroyImmediate(o);

            }
            lrContent.Clear();

            quadTree.Clear();

            pointPos.Clear();
            foreach (var o in quadGRP)
            {
                GameObject.DestroyImmediate(o);

            }
            quadGRP.Clear();



            for (int i = 0; i < PointNumber; i++)
            {
                Vector3 p = new Vector3(UnityEngine.Random.Range((float)sx, (float)xwidth), UnityEngine.Random.Range((float)sy, (float)yheight), 0);
                quadTree.Insert(p);
                pointPos.Add(p);

                GameObject temp = Instantiate(quadPrefab, p, Quaternion.identity, content.transform);
                quadGRP.Add(temp);


            }

            quadTree.Show(lrContent);
        }

       

        if (Input.GetMouseButton(0))
        {
            Vector3 wordPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            range.x = wordPos.x - range.width / 2;
            range.y = wordPos.y - range.height / 2;
        }
        if (Input.GetMouseButtonUp(0))
        {

            found.Clear();
            res.Clear();
            quadTree.QueryRange(range, found);
            for (int i = 0; i < found.Count; i++)
            {
                if (found[i] != Vector3.zero)
                    res.Add(found[i]);
            }

            foreach (var o in quadGRP)
            {
                o.transform.GetComponent<MeshRenderer>().material.color = Color.white;
                o.transform.localScale = new Vector3(0.05f, .05f, .05f);

                foreach (var p in res)
                {
                    if (o.transform.position == p)
                    {
                        o.transform.GetComponent<MeshRenderer>().material.color = Color.green;
                        o.transform.localScale = new Vector3(0.1f,.1f,.1f);
                    }    
                }
                
            }

           
        } 
    }
}

//public class Boundary
//{
//    public float x;
//    public float y;
//    public float width;
//    public float height;

//    public Boundary(float x, float y, float width, float height)
//    {
//        this.x = x;
//        this.y = y;
//        this.width = width;
//        this.height = height;
//    }

//    public bool Contains(Vector3 p)
//    {
//        return ( p.x >= this.x - this.width &&
//                 p.x <= this.x + this.width &&
//                 p.y >= this.y - this.height &&
//                 p.y <= this.y + this.height
//            );
        
//    }

//    public bool Intersection(Boundary range)
//    {
//        return (range.x-range.width >=this.x+this.width ||
//                range.x+range.width<=this.x -this.width ||
//                range.y-range.height>=this.y+this.height|| 
//                range.y+range.height<=this.y-this.height);
       
//    }

   
    
//}

//public class QuadTree
//{
//    //private Rect boundary;

//    private Boundary boundary;
//    private int capacity;
//   // private int depth=0;
//    //private QuadTree[] nodes;
//    private List<Vector3> points;
    

//    private int MAX_CAPACITY =4;
//    private int MAX_DEPTH=10;
//    private int diviedCount = 0;


//    private QuadTree northeast;
//    private QuadTree northwest;
//    private QuadTree southeast;
//    private QuadTree southwest;

//    private bool divided;

//    public QuadTree(Boundary boundary,int capacity)
//    {
//        this.boundary = boundary;
//        this.capacity = capacity;
//        this.points = new List<Vector3>();
//        this.divided = false;
//    // nodes = new QuadTree[4];

//}


//    //public void Clear()
//    //{
//    //    points.Clear();
//    //    for (int i = 0; i < nodes.Length; i++)
//    //    {
//    //        if (nodes[i] != null)
//    //        {
//    //            nodes[i].Clear();
//    //            nodes[i] = null; 
//    //        }
//    //    }
//    //}
//    //private void Splite()
//    //{
//    //    float subWidth = boundary.width / 2;
//    //    float subHeight = boundary.height / 2;
//    //    float xc = boundary.center.x;
//    //    float yc = boundary.center.y;


//    //    Rect ne = new Rect(xc, yc, subWidth, subHeight);
//    //    nodes[0] = new QuadTree(ne, depth+1);

//    //    Rect nw = new Rect(xc - subWidth,yc, subWidth, subHeight);
//    //    nodes[1] = new QuadTree(nw, depth + 1);

//    //    Rect se = new Rect(xc, yc - subHeight, subWidth, subHeight);
//    //    nodes[2] = new QuadTree(se, depth + 1);

//    //    Rect sw = new Rect(xc - subWidth, yc -subHeight, subWidth, subHeight);
//    //    nodes[3] = new QuadTree(sw, depth + 1);

//    //    this.divided = true;

//    //}
//    //private int  GetIndex(Vector3 p)
//    //{
//    //    int index = -1;


//    //    bool btop = p.y < this.boundary.y + this.boundary.height && p.y > this.boundary.y + this.boundary.height / 2;
//    //    bool bbottom = p.y < this.boundary.y + this.boundary.height/2 && p.y > this.boundary.y ;
//    //    bool bleft = p.x < this.boundary.x + this.boundary.width / 2 && p.x > this.boundary.x;
//    //    bool bright = p.x > this.boundary.x + this.boundary.width / 2 && p.x < this.boundary.x + this.boundary.width;

//    //    if (bleft)
//    //    {
//    //        if (bbottom)
//    //        {
//    //            index = 2;
//    //        }
//    //        else if (btop)
//    //        {
//    //            index = 1;
//    //        }
//    //    }
//    //    else if (bright)
//    //    {
//    //        if (bbottom)
//    //        {
//    //            index = 3;
//    //        }
//    //        else if (btop)
//    //        {
//    //            index = 0;
//    //        }
//    //    }

//    //    return index;

//    //}
//    //public void Insert(Vector3 p)
//    //{
//    //    if (nodes[0] != null)
//    //    {
//    //        int index = GetIndex(p);

//    //        if (index != -1)
//    //        {
//    //            nodes[index].Insert(p);

//    //            return;
//    //        }
//    //    }

//    //    points.Add(p);

//    //    if (points.Count > MAX_CAPACITY && depth < MAX_DEPTH)
//    //    {
//    //        if (nodes[0] == null)
//    //        {
//    //            Splite();
//    //        }



//    //        for (int i = 0; i < points.Count; i++)
//    //        {
//    //            int index = GetIndex(points[i]);
//    //            if (index != -1)
//    //            {

//    //                nodes[index].Insert(points[i]);
//    //            }

//    //        }
//    //    }
//    //}

//    private void Subdivid()
//    {
//        float subWidth = boundary.width / 2;
//        float subHeight = boundary.height / 2;
//        float xc = boundary.x + boundary.width / 2;
//        float yc = boundary.y + boundary.height / 2;


//        Boundary ne = new Boundary(xc, yc, subWidth, subHeight);
//        this.northeast = new QuadTree(ne, this.capacity);

//        Boundary nw = new Boundary(xc - subWidth, yc, subWidth, subHeight);
//        this.northwest = new QuadTree(nw, this.capacity);

//        Boundary se = new Boundary(xc, yc - subHeight, subWidth, subHeight);
//        this.southeast = new QuadTree(se, this.capacity);

//        Boundary sw = new Boundary(xc - subWidth, yc - subHeight, subWidth, subHeight);
//        this.southwest = new QuadTree(sw, this.capacity);

//        this.divided = true;
//        diviedCount++;
//    }


//    public void Insert2( Vector3 p)
//    {


//        if (!this.boundary.Contains(p))
//        {
//            return ;
//        }

//        if (this.points.Count < this.capacity)
//        {
//            this.points.Add(p);
//        }
//        else
//        {
//            if (!this.divided)
//            {
//                this.Subdivid();
//            }
//            this.northeast.Insert2(p);
//            this.northwest.Insert2(p);
//            this.southeast.Insert2(p);
//            this.southwest.Insert2(p);

//        }

//    }


//    public void  QueryRange( Boundary range ,List<Vector3> found)
//    {
        

//        if (!this.boundary.Intersection(range))
//        {
//             found.Add(Vector3.zero);
//        }
//        else
//        {
            
//            for (int i = 0; i < points.Count; i++)
//            {
//               /// Debug.Log(points[i]);
//                if (range.Contains(points[i]))
//                {
//                    Debug.Log("22222");
//                    found.Add(  points[i]);
//                    Debug.Log(found[i]);
//                }
                
//            }
//        }

//        if (this.divided)
//        {
//            northeast.QueryRange(range,found);
//            northwest.QueryRange(range,found);
//            southeast.QueryRange(range,found);
//            southwest.QueryRange(range,found);

//            Debug.Log("333333");
//        }

//    }

//    //private void Subdivid()
//    //{
//    //    Rect ne = new Rect(this.boundary.center.x, this.boundary.center.y,this.boundary.width / 2, this.boundary.height / 2);
//    //    this.northeast = new QuadTree(ne,this.capacity);

//    //    Rect nw = new Rect(this.boundary.center.x - this.boundary.width / 2, this.boundary.center.y, this.boundary.width / 2, this.boundary.height / 2);
//    //    this.northwest = new QuadTree(nw, this.capacity);

//    //    Rect se = new Rect(this.boundary.center.x, this.boundary.center.y - this.boundary.height / 2, this.boundary.width / 2, this.boundary.height / 2);
//    //    this.southeast = new QuadTree(se, this.capacity);

//    //    Rect sw = new Rect(this.boundary.center.x - this.boundary.width / 2, this.boundary.center.y - this.boundary.height / 2, this.boundary.width / 2, this.boundary.height / 2);
//    //    this.southwest = new QuadTree(sw, this.capacity);

//    //    this.divided = true;
//    //}

//    public void Show()
//    {
//        LineRenderer lr = new LineRenderer();
//        GameObject go = new GameObject("rec");
//        lr = go.AddComponent<LineRenderer>();

//        lr.positionCount = 5;
//        Vector3 v1 = new Vector3(this.boundary.x  , this.boundary.y , 0);
//        Vector3 v2 = new Vector3(this.boundary.x  , this.boundary.y + this.boundary.height , 0);
//        Vector3 v3 = new Vector3(this.boundary.x + this.boundary.width , this.boundary.y + this.boundary.height , 0);
//        Vector3 v4 = new Vector3(this.boundary.x + this.boundary.width , this.boundary.y , 0);
//        lr.SetPosition(0, v1);
//        lr.SetPosition(1, v2);
//        lr.SetPosition(2, v3);
//        lr.SetPosition(3, v4);
//        lr.SetPosition(4, v1);
//        lr.startWidth = lr.endWidth = 0.02f;
//        lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
//        lr.startColor = lr.endColor = Color.red;


//        if (this.divided)
//        {
//            this.northeast.Show();
//            this.northwest.Show();
//            this.southeast.Show();
//            this.southwest.Show();
//        }
//    }
//}


public class QuadTree
{

    private Rect boundary;
    private int capacity;
    private int depth=0;
   
    private List<Vector3> points;


    //private int MAX_CAPACITY = 4;
    //private int MAX_DEPTH = 10;

    private QuadTree[] nodes;

    private bool divided;

    public QuadTree(Rect boundary, int capacity,int depth)
    {
        this.boundary = boundary;
        this.capacity = capacity;
        this.depth = depth;
        this.divided = false;

        this.points = new List<Vector3>();

        nodes = new QuadTree[4];
    }


    public void Clear()
    {
        points.Clear();
       
        for (int i = 0; i < nodes.Length; i++)
        {
            if (this.divided)
            {
                nodes[i].Clear();
                nodes[i] = null;
                this.divided = false;
            }
        }

       
    }


    private void Subdivid()
    {
        float subWidth = boundary.width / 2;
        float subHeight = boundary.height / 2;
        float xc = boundary.x + boundary.width / 2;
        float yc = boundary.y + boundary.height / 2;


        Rect ne = new Rect(xc, yc, subWidth, subHeight);
        nodes[0] = new QuadTree(ne, capacity, depth + 1);

        Rect nw = new Rect(xc - subWidth, yc, subWidth, subHeight);
        nodes[1]= new QuadTree(nw, capacity, depth + 1);

        Rect se = new Rect(xc, yc - subHeight, subWidth, subHeight);
        nodes[2] = new QuadTree(se, capacity, depth + 1);

        
        Rect sw = new Rect(xc - subWidth, yc - subHeight, subWidth, subHeight);
        nodes[3] = new QuadTree(sw, capacity, depth + 1);

        this.divided = true;

    }


    public void Insert(Vector3 p)
    {
       
        if (!this.boundary.Contains(p))
        {
            return;
        }

        if (this.points.Count < this.capacity)
        {
            this.points.Add(p);
        }
        else
        {
            if (!this.divided)
            {
                this.Subdivid();
              
            }
          
            nodes[0].Insert(p);
            nodes[1].Insert(p);
            nodes[2].Insert(p);
            nodes[3].Insert(p);
        }



    }


    public void QueryRange(Rect range, List<Vector3> found)
    {
        if (!this.boundary.Overlaps(range))
        {
            //found.Add(Vector3.zero);
            return;
        }
        else
        {

            for (int i = 0; i < points.Count; i++)
            {
                if (range.Contains(points[i]))
                {
                    found.Add(points[i]);
                }

            }
        }

        if (this.divided)
        {
            
            nodes[0].QueryRange(range, found);
            nodes[1].QueryRange(range, found);
            nodes[2].QueryRange(range, found);
            nodes[3].QueryRange(range, found);
        }

    }


    public void Show(List<GameObject> lrCount)
    {

        LineRenderer lr = new LineRenderer();
        GameObject go = new GameObject("rec");
        lr = go.AddComponent<LineRenderer>();
        lrCount.Add(go);


        lr.positionCount = 5;
        Vector3 v1 = new Vector3(this.boundary.x, this.boundary.y, 0);
        Vector3 v2 = new Vector3(this.boundary.x, this.boundary.y + this.boundary.height, 0);
        Vector3 v3 = new Vector3(this.boundary.x + this.boundary.width, this.boundary.y + this.boundary.height, 0);
        Vector3 v4 = new Vector3(this.boundary.x + this.boundary.width, this.boundary.y, 0);
        lr.SetPosition(0, v1);
        lr.SetPosition(1, v2);
        lr.SetPosition(2, v3);
        lr.SetPosition(3, v4);
        lr.SetPosition(4, v1);
        lr.startWidth = lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr.startColor = lr.endColor = Color.red;

        if (this.divided )
        {
           
            nodes[0].Show(lrCount);
            nodes[1].Show(lrCount);
            nodes[2].Show(lrCount);
            nodes[3].Show(lrCount);
        }

       

       
    }
}

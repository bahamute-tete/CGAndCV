using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MuiltSegmentPatch : MonoBehaviour
{


    private Vector3[] vertices;

    [SerializeField] List<Vector3> drawPoint = new List<Vector3>();
    [SerializeField] List<Vector3> lineDir = new List<Vector3>();
    [SerializeField] float lineWidth = 1.0f;
    //[SerializeField] List<Vector3> linePos = new List<Vector3>();

    [SerializeField] List<Vector3> inputePoint = new List<Vector3>();
    Vector3[] originPos;


    [SerializeField] private GameObject sph;
    [SerializeField]private bool bplay=false;
    [SerializeField] float dt = 0;
    [SerializeField] float k = 1.0f;
    [SerializeField] float speed = 1.0f;

    [SerializeField] Material mat;
    // Start is called before the first frame update
    void Start()
    {

        //GreatePannel(drawPoint, lineWidth);
        inputePoint.Clear();

        for (float x = 0; x < 10; x += 0.01f)
        {
            float y = Mathf.Sin(x);
            //float y = 1.0f;
            inputePoint.Add(new Vector3(x, y, 0));
        }

        GreatePannel(inputePoint, lineWidth);
   



        originPos = new Vector3[gameObject.GetComponent<MeshFilter>().mesh.vertices.Length];

        for (int i = 0; i < gameObject.GetComponent<MeshFilter>().mesh.vertices.Length; i++)
        {
            originPos[i] = gameObject.GetComponent<MeshFilter>().mesh.vertices[i];
        }

    }

    // Update is called once per frame
    void Update()
    {


        //if (Input.GetMouseButtonDown(0))
        //{
        //    sph.GetComponent<Animator>().SetTrigger("play");
        //    bplay = true;
        //    inputePoint.Clear();
        //}

        //if (bplay)
        //{

        //    AnimatorStateInfo stateInfo = sph.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        //    if (stateInfo.normalizedTime > 0.99 && stateInfo.IsName("MovingTest"))
        //        bplay = false;

        //    dt += Time.fixedDeltaTime;
        //    if (dt > 0.5f)
        //    {

        //        inputePoint.Add(sph.transform.position);
        //        dt = 0;
        //    }
        //}
        //else if (inputePoint.Count != 0)
        //{
        //    GreatePannel(inputePoint, lineWidth);
        //}

        //if (Input.GetMouseButtonDown(0))
        //{
        //    for (float x = 0; x < 10; x += 0.1f)
        //    {
        //        float y = Mathf.Sin(x);
        //        inputePoint.Add(new Vector3(x, y, 0));
        //    }

        //}

        //if (inputePoint.Count > 0)
        //{
        //    GreatePannel(inputePoint, lineWidth);
        //}

        Vector3[] vInPatch = new Vector3[gameObject.GetComponent<MeshFilter>().mesh.vertices.Length];

        float t = Time.timeSinceLevelLoad * speed;

        for (int i = 0; i < originPos.Length; i++)
        {
            vInPatch[i].x = originPos[i].x;

            vInPatch[i].y = originPos[i].y * math.cos(k * originPos[i].x + t) - math.sin(k * originPos[i].x + t) * originPos[i].z;
            vInPatch[i].z = originPos[i].y * math.sin(k * originPos[i].x + t) + math.cos(k * originPos[i].x + t) * originPos[i].z;
        }

        gameObject.GetComponent<MeshFilter>().mesh.vertices = vInPatch;
    }


    void GreatePannel(List<Vector3> Points,float lineWidth)
    {
        List<Vector3> linePos = new List<Vector3>();
        List<Vector3> lineDir = new List<Vector3>();
        linePos.Clear();
        lineDir.Clear();


        Vector3 rd = Camera.main.transform.forward;

        for (int i = 0; i < Points.Count; i++)
        {
            if (i != Points.Count - 1)
            {
                Vector3 dir = Points[i + 1] - Points[i];
               
                
                //if (rd == Vector3.Normalize( dir) )

                Vector3 linerd =Vector3.Normalize(Vector3.Cross(rd, dir));

                lineDir.Add(linerd);
            }
        }
        lineDir.Add(lineDir[lineDir.Count - 1]);

        for (int i = 0; i < Points.Count; i++)
        {
            Vector3 bottomPoint = Points[i] - lineDir[i] * lineWidth * 0.5f;
            linePos.Add(bottomPoint);
        }

        for (int i = 0; i < Points.Count; i++)
        {
            Vector3 topPoint = Points[i] + lineDir[i] * lineWidth * 0.5f;
            linePos.Add(topPoint);
        }

        int xsize = Points.Count - 1;

        int[] triangles = new int[xsize * 6];


        for (int x = 0, i = 0, j = 0; x < xsize; x++, i += 6, j++)
        {
            triangles[i] = j;
            triangles[i + 1] = j + xsize + 1;
            triangles[i + 2] = j + 1;
            triangles[i + 3] = j + 1;
            triangles[i + 4] = j + xsize + 1;
            triangles[i + 5] = j + xsize + 2;
        }

        Mesh mesh = new Mesh();
        mesh = gameObject.GetComponent<MeshFilter>().mesh;

        mesh.SetVertices(linePos);
       
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        gameObject.GetComponent<MeshRenderer>().material = mat;
    }

}

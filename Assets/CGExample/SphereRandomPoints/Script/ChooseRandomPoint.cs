using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Burst;





public class ChooseRandomPoint : MonoBehaviour
{

    public GameObject prefab;
    private LineRenderer hemisphereLr = new LineRenderer();
    //[SerializeField]
    private List<Vector3> hemispherePosGRP = new List<Vector3>();
    private List<GameObject> hemisphereObj = new List<GameObject>();
    [SerializeField]
    private List<Vector3> targetPos = new List<Vector3>();

    private Vector3 rotateAngle = new Vector3(0, 0, 0);

    private float sphw = 3.0f;
    private int count = 25;
    float theta;
    float phi;
    Gradient gradient;
    GradientColorKey[] colorKeys;
    GradientAlphaKey[] alphaKeys;



    TransformAccessArray accessArray;


    [BurstCompile]
    struct PositionChangeJob : IJobParallelForTransform
    {
        
        public NativeArray<Vector3> positions;

        public float changeFrequence ;
        

        public float radius;
        public float deltaTheta, deltaPhi;

   



        public void Execute(int i, TransformAccess transform)
        {
            Vector3 n = Vector3.Normalize(positions[i]);

           
            float curretnPostheta = Mathf.Acos(positions[i].y / radius);
            float curretnPosphi = Mathf.Atan2(positions[i].z, positions[i].x);

            float thetaNew = curretnPostheta + deltaTheta;
            float phiNew = curretnPosphi + deltaPhi;


            float x = radius * Mathf.Sin(thetaNew) * Mathf.Cos(phiNew);
            float y = radius * Mathf.Cos(thetaNew);
            float z = radius * Mathf.Sin(thetaNew) * Mathf.Sin(phiNew);

            Vector3 fPos = new Vector3(x, y, z) + 0.5f * n * changeFrequence * Vector3.Magnitude(new Vector3(x, y, z));

            transform.position = fPos;


        }
    }


    // Start is called before the first frame update
    void Start()
    {
        hemispherePosGRP.Clear();
        hemisphereObj.Clear();
        targetPos.Clear();

        gradient = new Gradient();

        colorKeys = new GradientColorKey[3];
        colorKeys[0]=new GradientColorKey(Color.red,0.0f);
        colorKeys[1]=new GradientColorKey(Color.yellow, 0.58f);
        colorKeys[2]=new GradientColorKey(Color.cyan, 1.0f);

        alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0]= new GradientAlphaKey(1.0f,0.0f);
        alphaKeys[1]= new GradientAlphaKey(0.5f, 0.78f);
        alphaKeys[2]= new GradientAlphaKey(0.0f,1.0f);


        gradient.SetKeys(colorKeys, alphaKeys);

        GameObject GRP = new GameObject("GRP");

        for (int i = 0; i <5000; i++)
        {
            //Vector3 v3 = HemisphereUniformDistribution_Inverse(sphw);


            //Vector2 v2 = CircleUniformDistribution_Inverse(sphw);
            //GameObject temp=Instantiate(prefab, v2, Quaternion.identity);

            //Vector2 v2 = CircleUniformDistribution_rejection(sphw);
            //GameObject temp=Instantiate(prefab, v2, Quaternion.identity);


            Vector3 v3 = SphereUniformDistribution_Rejection(sphw);
           


            GameObject temp = Instantiate(prefab, v3, Quaternion.identity);

            temp.AddComponent<LineRenderer>();
            temp.GetComponent<LineRenderer>().material.shader = Shader.Find("Particles/Standard Unlit");
            temp.GetComponent<LineRenderer>().startWidth =0.008f;
            temp.GetComponent<LineRenderer>().endWidth = 0.01f;
            temp.GetComponent<LineRenderer>().colorGradient.mode = GradientMode.Blend;
            temp.GetComponent<LineRenderer>().colorGradient = gradient;

            temp.transform.SetParent(GRP.transform);
            hemispherePosGRP.Add(v3);
            hemisphereObj.Add(temp);

           
        }


        accessArray = new TransformAccessArray(hemisphereObj.Count);

        for (int i = 0; i < hemisphereObj.Count; i++)
        {
            accessArray.Add(hemisphereObj[i].transform);
        }

    }

    // Update is called once per frame
    void Update()
    {
        count--;

        float changeFrequence =Mathf.Cos(3.0f * Time.timeSinceLevelLoad) * 0.5f + 0.5f;

        theta = 40.0f* Mathf.Cos(1.0f*Time.timeSinceLevelLoad)*Mathf.Deg2Rad;
        phi = 10.0f * Mathf.Sin(2.0f*Time.timeSinceLevelLoad) * Mathf.Deg2Rad;

        float rotateWithY = 75.0f * Time.timeSinceLevelLoad % 360.0f;
        float rotateWithX = 45.0f * Time.timeSinceLevelLoad % 360.0f;
        float rotateWithZ = 55.0f * Time.timeSinceLevelLoad % 360.0f;

        rotateAngle = new Vector3(rotateWithX, rotateWithY, rotateWithZ);

        Vector3[] hemisphereVertexs = new Vector3[hemispherePosGRP.Count];
        hemisphereVertexs = RotateVertex(rotateAngle, hemispherePosGRP.ToArray());


        #region job
        var PostionDatas = new NativeArray<Vector3>(hemisphereVertexs.Length, Allocator.TempJob);

        for (int i = 0; i < hemisphereVertexs.Length; i++)
        {
            PostionDatas[i] = hemisphereVertexs[i];
        }

        var job = new PositionChangeJob
        {
            positions = PostionDatas,
            changeFrequence = changeFrequence,
            radius = sphw,
            deltaTheta = theta,
            deltaPhi = phi

        };

       


        var handle = job.Schedule(accessArray);

        handle.Complete();

        PostionDatas.Dispose();
        #endregion

        #region Not Job
        //for (int i = 0; i < hemispherePosGRP.Count; i++)
        //{
        //    Vector3 n =Vector3.Normalize(hemisphereVertexs[i] - Vector3.zero);

        //    float curretnPostheta = Mathf.Acos(hemisphereVertexs[i].y / sphw);
        //    float curretnPosphi = Mathf.Atan2(hemisphereVertexs[i].z, hemisphereVertexs[i].x);

        //    //float theta = Random.Range(-2.0f, 2.0f)*Mathf.Deg2Rad;
        //    //float phi = Random.Range(-3.0f, 3.0f)* Mathf.Deg2Rad;


        //    float thetaNew = curretnPostheta + theta;
        //    float phiNew = curretnPosphi + phi;


        //    float x = sphw * Mathf.Sin(thetaNew) * Mathf.Cos(phiNew);
        //    float y = sphw * Mathf.Cos(thetaNew);
        //    float z = sphw * Mathf.Sin(thetaNew) * Mathf.Sin(phiNew);



        //    Vector3 fPos = new Vector3(x, y, z) + 0.5f * n * changeFrequence * Vector3.Magnitude(new Vector3(x, y, z) - Vector3.zero);

        //    hemisphereObj[i].transform.position = fPos;

        //    hemisphereObj[i].GetComponent<LineRenderer>().SetPosition(0, fPos);
        //    hemisphereObj[i].GetComponent<LineRenderer>().SetPosition(1, Vector3.zero);



        //}

        #endregion

    }

    private void OnDisable()
    {
        accessArray.Dispose();
    }

    Vector3 HemisphereUniformDistribution_Inverse(float r)
    {

        float e1 = UnityEngine.Random.Range(0.0f, 1.0f);
        float e2 = UnityEngine.Random.Range(0.0f, 1.0f);

        float x = Mathf.Cos(2 * Mathf.PI * e1) * Mathf.Sqrt(e2);
        float y = Mathf.Sqrt(1 - e2);
        float z = Mathf.Sin(2 * Mathf.PI * e1) * Mathf.Sqrt(e2);
       
        Vector3 v = new Vector3(r*x,r*y,r*z);
        return v;
       
    }

    Vector3 SphereUniformDistribution_Rejection(float r)
    {

        float x = 0, y = 0,z= 0;
        float l = 0;

        x = (-1.0f + 2 * UnityEngine.Random.Range(0f, 1.0f)) * r;
        y = (-1.0f + 2 * UnityEngine.Random.Range(0f, 1.0f)) * r;
        z = (-1.0f + 2 * UnityEngine.Random.Range(0f, 1.0f)) * r;

        l = Mathf.Sqrt(x * x + y * y + z * z);

        if (l> r )
        {
            x = 0;
            y = 0;
            z = 0;
        }
        //Vector3 v = new Vector3( x/l, y/l, z/l);
        Vector3 v = new Vector3(x , y , z );
        return v;

    }


    Vector2 CircleUniformDistribution_Inverse(float r)
    {

        float e1 = UnityEngine.Random.Range(0f, 1.0f);
        float e2 = UnityEngine.Random.Range(0f, 1.0f);

        float theta = 2 * Mathf.PI * e1;
        float radiu = r* Mathf.Sqrt(e2);

        float x=0, y=0;
        if (theta < 0.5f * Mathf.PI || theta > 1.5f * Mathf.PI)
        {
            x = Mathf.Sqrt((radiu * radiu) / (1 + Mathf.Tan(theta) * Mathf.Tan(theta)));

        }
        else
        {
            x =- Mathf.Sqrt((radiu * radiu) / (1 + Mathf.Tan(theta) * Mathf.Tan(theta)));
        }

         y = x * Mathf.Tan(theta);

        return new Vector2(x,y);
    }

    Vector2 CircleUniformDistribution_Rejection(float r)
    {
        float x=0, y=0;


        x = (-1.0f + 2 * UnityEngine.Random.Range(0f, 1.0f))*r;
        y  = (-1.0f + 2 * UnityEngine.Random.Range(0f, 1.0f))*r;

        if (x * x + y * y > r * r)
        {
            x = 0;
            y = 0;
        }
        return new Vector2(x, y);
    }

    Vector3[] RotateVertex(Vector3 R_xyz, Vector3[] vector3s)
    {

        Vector3[] vertexGRP = new Vector3[vector3s.Length];



        Quaternion q = Quaternion.Euler(R_xyz.x, R_xyz.y, R_xyz.z);

        Matrix4x4 R = Matrix4x4.Rotate(q);

        Matrix4x4[] Phs = new Matrix4x4[vector3s.Length];
        for (int i = 0; i < Phs.Length; i++)
        {
            Phs[i].SetColumn(0, new Vector4(vector3s[i].x, vector3s[i].y, vector3s[i].z, 1.0f));

            Matrix4x4 temp = R * Phs[i];

            vertexGRP[i] = temp.GetColumn(0);
        }

        return vertexGRP;

    }

}

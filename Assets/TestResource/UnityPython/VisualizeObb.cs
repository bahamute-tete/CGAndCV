using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class ObbData
{

    public List<List<float>> RST = new List<List<float>>();
    public float[] min_max_R = new float[2];
    public float[] min_max_S = new float[2];
    public float[] min_max_T = new float[2];
}

[System.Serializable]
public class PointsDatas
{
    public int count;
    public List<List<float>> position = new List<List<float>>();
}
public class VisualizeObb : MonoBehaviour
{
    ObbData obb = new ObbData();

    PointsDatas pd = new PointsDatas();

    string jsData;

    string jsPData;


    //string path = "/Users/xuchengqi/Python/EigenvectorDatas.json";
    string path = "D:\\UnityProject\\GitHub\\CGAndCV\\Assets\\TestResource\\UnityPython\\eigenRes.json";
    //pointData
    string pathPoint = "D:\\UnityProject\\GitHub\\CGAndCV\\Assets\\TestResource\\UnityPython\\point_position.json";

    public Vector3 R, S, T;
    public Vector2 min_max_R, min_max_S, min_max_T;

    public List<Vector3> poins= new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
 

        using (StreamReader sr = File.OpenText(pathPoint))
        {
            jsPData = sr.ReadToEnd();
            sr.Close();
        }
        //Debug.Log(jsData);

      
        pd = JsonConvert.DeserializeObject<PointsDatas>(jsPData);

       

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            using (StreamReader sr = File.OpenText(path))
            {
                jsData = sr.ReadToEnd();
                sr.Close();
            }

            obb = JsonConvert.DeserializeObject<ObbData>(jsData);


            R = new Vector3(obb.RST[0][0], obb.RST[0][1], obb.RST[0][2]);
            S = new Vector3(obb.RST[1][0], obb.RST[1][1], obb.RST[1][2]);
            T = new Vector3(obb.RST[2][0], obb.RST[2][1], obb.RST[2][2]);

            min_max_R = new Vector2(obb.min_max_R[0], obb.min_max_R[1]);
            min_max_S = new Vector2(obb.min_max_S[0], obb.min_max_S[1]);
            min_max_T = new Vector2(obb.min_max_T[0], obb.min_max_T[1]);


            for (int i = 0; i < pd.position.Count; i++)
            {
                Vector3 pos = Vector3.zero;
                for (int j = 0; j < 3; j++)
                {
                    pos = new Vector3(pd.position[i][0], pd.position[i][1], pd.position[i][2]);
                }
                poins.Add(pos);
            }
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        for (int i = 0; i < poins.Count; i++)
        {
            Gizmos.DrawWireSphere(poins[i], 0.2f);
        }


        float a = (float)(0.5 * (min_max_R.x+ min_max_R.y));
        float b = (float)(0.5 * (min_max_S.y + min_max_S.x));
        float c = (float)(0.5 * (min_max_T.y + min_max_T.x));

        float ra = (float)(0.5 * (min_max_R.y - min_max_R.x));
        float rb = (float)(0.5 * (min_max_S.y - min_max_S.x));
        float rc = (float)(0.5 * (min_max_T.y - min_max_T.x));


        Vector3 Center = a * R + b * S + c * T;

        Vector3 Q = Center;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(Center, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(Q, Q + R *ra);
        Gizmos.DrawLine(Q, Q - R * ra);
       

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Q, Q + S *rb );
        Gizmos.DrawLine(Q, Q - S * rb);


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Q, Q + T * rc );
        Gizmos.DrawLine(Q, Q - T *rc);

       



    }
}

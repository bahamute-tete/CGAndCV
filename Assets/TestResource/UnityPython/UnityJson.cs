using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;



public class UnityJson : MonoBehaviour
{

    ObbData obb = new ObbData();

    string jsData;

    //string path = "/Users/xuchengqi/Python/EigenvectorDatas.json";
    string path = "D:\\Python\\eigenRes.json";

    


    public Vector3 R,S,T;
    public Vector2 min_max_R, min_max_S, min_max_T;

    private void Awake()
    {
        using (StreamReader sr = File.OpenText(path))
        {
            jsData = sr.ReadToEnd();
            sr.Close();
        }

        //Debug.Log(jsData);

        obb = JsonConvert.DeserializeObject<ObbData>(jsData);


        R = new Vector3(obb.RST[0][0], obb.RST[0][1], obb.RST[0][2]);
        S = new Vector3(obb.RST[1][0], obb.RST[1][1], obb.RST[1][2]);
        T = new Vector3(obb.RST[2][0], obb.RST[2][1], obb.RST[2][2]);

        min_max_R = new Vector2(obb.min_max_R[0], obb.min_max_R[1]);
        min_max_S = new Vector2(obb.min_max_S[0], obb.min_max_S[1]);
        min_max_T = new Vector2(obb.min_max_T[0], obb.min_max_T[1]);
    }

    // Start is called before the first frame update
    void Start()
    {

      

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}

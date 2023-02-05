using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class ObbData
{

    public float[] eigenValue = new float[3];
    public List<List<float>> eigenVector = new List<List<float>>();
}

public class UnityJson : MonoBehaviour
{

    ObbData obb = new ObbData();

    string jsData;

    string path = "/Users/xuchengqi/Python/EigenvectorDatas.json";

    public Vector3 eigenValues = default;
    public Vector3[] eigenVectors = new Vector3[3];
    // Start is called before the first frame update
    void Start()
    {

        using (StreamReader sr = File.OpenText(path))
        {
            jsData = sr.ReadToEnd();
            sr.Close();
        }

        //Debug.Log(jsData);

        obb=JsonConvert.DeserializeObject<ObbData>(jsData);


        eigenValues = new Vector3(obb.eigenValue[0], obb.eigenValue[1], obb.eigenValue[2]);

        for (int i = 0; i < eigenVectors.Length; i++)
        {
            eigenVectors[i] = new Vector3(obb.eigenVector[i][0], obb.eigenVector[i][1], obb.eigenVector[i][2]);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}

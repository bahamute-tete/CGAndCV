using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class pointsData
{
    public List<List<float>> position = new List<List<float>>();
}

public class RandomCreate : MonoBehaviour
{

    public Transform prefab;
    public int instances = 100;
    public float radius = 10f;

    string path = Application.streamingAssetsPath;

    [SerializeField]List<Vector3> position = new List<Vector3>();

  
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(path);

        for (int i = 0; i < instances; i++)
        {
            Transform t = Instantiate(prefab);
            t.localPosition = Random.insideUnitSphere * radius;
            position.Add(t.position);
            t.rotation = Random.rotation;
            t.localScale = new Vector3(Random.Range(0.2f, 0.5f),
                                       Random.Range(0.2f, 0.5f),
                                       Random.Range(0.2f, 0.5f));
            t.SetParent(transform);
        }


        pointsData pointsData = new pointsData();
        for (int i = 0; i < position.Count; i++)
        {
            List<float> pos = new List<float>
            {
                position[i].x,
                position[i].y,
                position[i].z
            };

            pointsData.position.Add(pos);

        }
      


        string jsonData =JsonConvert.SerializeObject(pointsData,Formatting.Indented);


        Debug.Log(jsonData);
        using (StreamWriter sw = new StreamWriter(File.Open(path,FileMode.Create)))
        {
            sw.Write(jsonData);
            sw.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

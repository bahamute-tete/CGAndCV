using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceTest : MonoBehaviour
{
    [SerializeField] GameObject instanceObj;
    [SerializeField] int count;
    List<GameObject> cubs = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        MaterialPropertyBlock pb = new MaterialPropertyBlock();

        MeshRenderer mr;
        for (int i = 0; i < count; i++)
        {
            GameObject temp = Instantiate(instanceObj, Random.insideUnitSphere * 3f, Quaternion.identity, transform);
            cubs.Add(temp);

        }

        foreach (var o in cubs)
        {
            float r = Random.Range(0f, 1f);
            float g = Random.Range(0f, 1f);
            float b = Random.Range(0f, 1f);

            pb.SetColor("_Color", new Color(r, g, b));
            pb.SetFloat("_Theta", Random.Range(-Mathf.PI*0.5f, Mathf.PI * 0.5f));
            mr = o.GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(pb);

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

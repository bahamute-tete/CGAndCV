using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoneInstance : MonoBehaviour
{
    [SerializeField] GameObject instanceObj;
    [SerializeField] int count;
    List<GameObject> cubs = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject temp = Instantiate(instanceObj, Random.insideUnitSphere * 3f, Quaternion.identity, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

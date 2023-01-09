using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstanceTest : MonoBehaviour
{
    public Transform prefab;
    public int instances = 5000;
    public float radius = 50f;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < instances; i++)
        {
            Transform t = Instantiate(prefab);
            t.localPosition = Random.insideUnitSphere* radius;
            t.SetParent(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

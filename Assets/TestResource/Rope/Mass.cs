using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mass : MonoBehaviour
{
    public float m = 0.1f;
    public Vector3 F;
    public Vector3 v;
    public bool isStaticPos = false;
    public float drag = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        v = F = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Simulate(float deltaTime)
    {
        if (isStaticPos)
        {
            F = Vector3.zero;
            return;

        }

        //air 
        F += -v.normalized * drag * Mathf.Pow(v.magnitude, 2);

        Vector3 a = F / m;
        a += Vector3.down * 9.81f;

        v += a * deltaTime;
        transform.position += v * deltaTime;
        F = Vector3.zero;
    }
}

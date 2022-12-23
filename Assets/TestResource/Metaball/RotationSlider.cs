using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationSlider : MonoBehaviour
{
    float z = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        z += Time.deltaTime*100f;
        Quaternion q = Quaternion.Euler(0, 0, z);
        transform.rotation = q;
    }
}

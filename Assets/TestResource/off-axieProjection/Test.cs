using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            Debug.Log($"MouseX ={mouseX},MouseY ={mouseY}");
        }
        
    }
}

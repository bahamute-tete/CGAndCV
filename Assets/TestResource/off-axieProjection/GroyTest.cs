using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroyTest : MonoBehaviour
{

    public Rect boundary;

    float x, y = 0; 

    Vector3 pos = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;
        

        x = Input.gyro.gravity.x;
        y = Input.gyro.gravity.y;
        x = y = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Quaternion q = Input.gyro.attitude;

            //transform.rotation = Quaternion.Slerp(transform.rotation, ChangeHandness(q), Time.deltaTime*5f);
            //transform.rotation = ChangeHandness(q);

            x = Input.gyro.gravity.x;
            y = Input.gyro.gravity.y;


            Vector3 dir = new Vector3(x, y, 0).normalized;


            transform.position += dir * Time.deltaTime * 2f;


            //if (Mathf.Abs(x) > 0.1f || Mathf.Abs(y) > 0.1f)
            //{
            //    //rb.AddForce(dir);
            //}


            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -0.4f, 0.4f),
                                                   Mathf.Clamp(transform.position.y, -0.4f, 0.4f),
                                                   transform.position.z);

        }
    }
}

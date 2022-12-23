using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnityGyroTest : MonoBehaviour
{
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Input.gyro.enabled = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Quaternion q = Input.gyro.attitude;

            //transform.rotation = Quaternion.Slerp(transform.rotation, ChangeHandness(q), Time.deltaTime*5f);
            //transform.rotation = ChangeHandness(q);

            float x = Input.gyro.gravity.x;
            float y = Input.gyro.gravity.y;



            

            if (Mathf.Abs(x) > 0.1f || Mathf.Abs(y) > 0.1f)
            {

                Vector3 dir = new Vector3(x, y, 0).normalized;


                //transform.position += dir * Time.deltaTime * 3f;

                rb.velocity = dir * 3f;

                //rb.AddForce(dir);


            }


            //transform.position = new Vector3(Mathf.Clamp(transform.position.x, -0.9f, 0.9f),
            //                                       Mathf.Clamp(transform.position.y, -2.5f, 2.5f),
            //                                       transform.position.z);

        }

    }

    Quaternion ChangeHandness(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}

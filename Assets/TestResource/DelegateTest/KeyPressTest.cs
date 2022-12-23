using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPressTest : MonoBehaviour
{


    public void ChangePos()
    {
        Vector3 cp = gameObject.transform.position;
        float r = Random.Range(-1f, 1f);

        transform.position = cp + new Vector3(r, r, r);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawXYZAixe : MonoBehaviour
{
    private LineRenderer lr = new LineRenderer();
    [SerializeField]
    GameObject target;
    [SerializeField]
    Material mat;

    // Start is called before the first frame update
    void Start()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.material = mat;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
    }

    // Update is called once per frame
    void Update()
    {

        lr.SetPosition(0, gameObject.transform.position);
        lr.SetPosition(1, target.transform.position);
        //lr.SetPosition(1, gameObject.transform.position);

    }
}

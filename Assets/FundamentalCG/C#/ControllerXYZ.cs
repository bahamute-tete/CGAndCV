using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerXYZ : MonoBehaviour
{
    [SerializeField]
    GameObject X ;
    [SerializeField]
    GameObject Y;
    [SerializeField]
    GameObject Z;

    public bool isSelected = false;
    // Start is called before the first frame update
    void Start()
    {
        X.SetActive(false);
        Y.SetActive(false);
        Z.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (isSelected)
        {
            X.SetActive(true);
            Y.SetActive(true);
            Z.SetActive(true);
        }
        else
        {
            X.SetActive(false);
            Y.SetActive(false);
            Z.SetActive(false);
        }
    }
}

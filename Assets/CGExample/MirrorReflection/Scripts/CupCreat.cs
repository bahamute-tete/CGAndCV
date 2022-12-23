using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupCreat : MonoBehaviour
{

    private int count = 60;
    
   [SerializeField] GameObject prefab ;
    // Start is called before the first frame update
    void Start()
    {
        float step =  360.0f/60.0f;
        for (int i = 0; i < count; i++)
        {
            
            GameObject temp = Instantiate(prefab, transform);
            temp.transform.transform.rotation = Quaternion.Euler(0, step*i, 0);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

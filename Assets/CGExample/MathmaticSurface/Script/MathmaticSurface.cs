using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;





public class MathmaticSurface : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField,Range(10,100)] int resolution = 10;
    float step;
    
    [SerializeField] FunctionLibrary.FunctionName functionName;
    FunctionLibrary.FunctionName functionNameFrom;

    [SerializeField, Min(0f)] float functionDuration = 1.0f, transFunDuration = 1.0f;

    bool isTrans=false;

    GameObject[] cubes;
    float duration;
    FunctionLibrary.Function f;
    public enum TransMode {Cycle,Random }
    [SerializeField] TransMode mode;


    // Start is called before the first frame update
    void Start()
    {
        cubes = new GameObject[ resolution * resolution ];
        step = 2f / resolution;
        for (int i = 0;i < cubes.Length; i++)
        {
            GameObject temp = Instantiate(prefab);
            temp.transform.localScale *=step;
            temp.transform.SetParent(this.transform);
            cubes[i] = temp;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        duration += Time.deltaTime;
        if (isTrans)
        {
            if (duration >= functionDuration)
            {
                duration -= functionDuration;
                isTrans = false;
            }
        }
        else
        {
            if (duration >= functionDuration)
            {
                duration -= functionDuration;
                isTrans = true;
                functionNameFrom = functionName;
                PickFuntion();
            }
        }

       

        if (isTrans)
        {
            UpdateWithMorph();
        }
        else
        {
            UpdateFunction();
        }
       
    }

    void UpdateFunction()
    {
        float time = Time.time;
        f = FunctionLibrary.GetFunction(functionName);
        for (int i = 0, x = 0, z = 0; i < cubes.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
            }

            
            float xcoord = (x + 0.5f) * step - 1;
            float zcoord = (z + 0.5f) * step - 1; ;

            Vector3 position = cubes[i].transform.localPosition;
            Vector3 newPos = f(xcoord, zcoord, time);
            cubes[i].transform.localPosition = newPos;
        }
    }

    void UpdateWithMorph()
    {
        FunctionLibrary.Function from = FunctionLibrary.GetFunction(functionNameFrom);

        FunctionLibrary.Function to = FunctionLibrary.GetFunction(functionName);

        float progress = duration / transFunDuration;

        float time = Time.time;

        for (int i = 0, x = 0, z = 0; i < cubes.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
            }

            float xcoord = (x + 0.5f) * step - 1;
            float zcoord = (z + 0.5f) * step - 1; ;

            Vector3 position = cubes[i].transform.localPosition;
            Vector3 newPos =FunctionLibrary.Morph(xcoord,zcoord,time,from,to,progress);
            cubes[i].transform.localPosition = newPos;
        }
    }

    void PickFuntion()
    {
        if (mode == TransMode.Cycle)
        {
            functionName = FunctionLibrary.GetNextFunction(functionName);
        }
        else
        {
            functionName = FunctionLibrary.GetRandomFunction(functionName);
        }
    }
}

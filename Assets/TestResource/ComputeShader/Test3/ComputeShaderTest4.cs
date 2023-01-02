using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ComputeShaderTest4 : MonoBehaviour
{

    private List<GameObject> objects;
    public int count=50;
    public int interation=1;

    private Cube[] data;

    public Mesh mesh;
    public Material material;


    public ComputeShader computeShader;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CreatCubes()
    {
        objects = new List<GameObject>();


        data= new Cube[count*count];

        for (int x= 0; x < count; x++)
        {
            for (int y = 0; y < count; y++)
            {
                CreatCubes(x, y);
            }
        }
    }

    private void CreatCubes(int x, int y)
    {
       GameObject cube = new GameObject("Cube"+ x*count +y*count,typeof(MeshFilter),typeof(MeshRenderer));

        cube.GetComponent<MeshFilter>().mesh = mesh;
        cube.GetComponent<MeshRenderer>().material = new Material(material);
        cube.transform.position = new Vector3(x, y, Random.Range(-0.1f,0.1f));

        Color color = Random.ColorHSV();
        cube.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        
        objects.Add(cube);

        Cube cubedata= new Cube();
        cubedata.position = cube.transform.position;
        cubedata.color = color;

        data[x * count + y] = cubedata;

    }

    public void OnRandomizeGPU()
    {
        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;

        ComputeBuffer cubeBuffer = new ComputeBuffer(data.Length, totalSize);
        
        cubeBuffer.SetData(data);

        computeShader.SetBuffer(0, "cubes", cubeBuffer);
        computeShader.SetFloat("resolution", data.Length);
        computeShader.SetInt("interation", interation);

        computeShader.Dispatch(0, data.Length / 10, 1, 1);

        cubeBuffer.GetData(data);
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            Cube cube = data[i];
            obj.transform.position = cube.position;
            obj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
        }

        cubeBuffer.Dispose();

    }

    public void OnRandomizeCPU()
    {
        for (int i = 0; i < interation; i++)
        {
            for (int j = 0; j < objects.Count; j++)
            {
                GameObject obj = objects[j];
                obj.transform.position = new Vector3(obj.transform.position.x,
                                                     obj.transform.position.y,
                                                     Random.Range(-0.1f, 0.1f));
                obj.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());

            }
        }
    }


    private void OnGUI()
    {
        if (objects == null)
        {
            if (GUI.Button(new Rect(100, 0, 100, 50), "Create"))
            {
                CreatCubes();
            }
        }
        else
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "RandomCPU"))
            {
                OnRandomizeCPU();
            }

            if (GUI.Button(new Rect(100, 0, 100, 50), "RandomGPU"))
            {
                OnRandomizeGPU();
            }
        }

      
    }
}

public struct Cube
{
    public Vector3 position;
    public Color color;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeTest : MonoBehaviour
{
    [SerializeField] Vector3[] originPos; 
    [SerializeField] Vector3[] bendPos;
    [Range(0.0f,0.15f)]
    [SerializeField] float t;
    Mesh mesh;
    MeshCollider mc;
    // Start is called before the first frame update
    void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        mc = gameObject.GetComponent<MeshCollider>();

        originPos = new Vector3[mesh.vertices.Length];
        bendPos = new Vector3[mesh.vertices.Length];

       

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            originPos[i] = mesh.vertices[i];
        }

    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < originPos.Length; i++)
        {
            float s = Mathf.Sin(originPos[i].x * t);
            float c = Mathf.Cos(originPos[i].x * t);


            float x = c * originPos[i].x - s * originPos[i].y;
            float y = s * originPos[i].x + c * originPos[i].y;

            Vector3 temp = new Vector3(x, y, originPos[i].z);
            bendPos[i] = temp;
        }

        mesh.vertices = bendPos;
        mesh.RecalculateNormals();




    }
}

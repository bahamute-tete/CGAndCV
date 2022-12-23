using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangleTest : MonoBehaviour
{
    Mesh mesh;
    [SerializeField]int[] triangles;
    [SerializeField] Vector3[] vertexs;

    [SerializeField] GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        triangles = new int[mesh.triangles.Length];
        triangles = mesh.triangles;

        vertexs = new Vector3[mesh.vertices.Length];
        vertexs = mesh.vertices;

        for (int i = 0; i < vertexs.Length; i++)
        {
            GameObject temp = Instantiate(prefab, vertexs[i], Quaternion.identity);
            temp.name = i.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnDrawGizmos()
    //{
    //    Debug.
    //}
}

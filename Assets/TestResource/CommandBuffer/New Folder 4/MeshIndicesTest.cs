using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshIndicesTest : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        uint index = mesh.GetIndexCount(0);

        Debug.Log($"MeshInciesCount=={index}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

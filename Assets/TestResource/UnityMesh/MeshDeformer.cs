
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;

    //Vertices move as the mesh is deformed. So we also have to store the velocity of each vertex
    Vector3[] vertexVelocities;

    public float springForce = 20f;
    public float damping = 5f;

    float uniformScale = 1f;

    public void AddDeformingForce(Vector3 point, float force)
    {
        Debug.DrawLine(Camera.main.transform.position, point);

        //change to localSpace
        point = transform.InverseTransformPoint(point);

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    private void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = displacedVertices[i] - point;
        //adjust Scale
        pointToVertex *= uniformScale;

        //Inverse-square law,one pluse guarantees full strength when distance is 0
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        // a = F/m --> v = at suggest m = 1
        float velocity = attenuatedForce * Time.deltaTime;
        //p1 =p0 + dt
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }



    // Start is called before the first frame update
    void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }

        vertexVelocities = new Vector3[originalVertices.Length];
    }


    // Update is called once per frame
    void Update()
    {
        uniformScale = transform.localScale.x;

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }

        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    private void UpdateVertex(int i )
    {
        Vector3 velocity = vertexVelocities[i];

        //make Spring system to pull back vertex
        Vector3 displancement = displacedVertices[i] - originalVertices[i];

        displancement *= uniformScale;
        velocity -= displancement * springForce * Time.deltaTime;

        // simple factor that decreases velocity over time
        velocity *= 1f - damping * Time.deltaTime;
        vertexVelocities[i] = velocity;

        displacedVertices[i] += velocity * (Time.deltaTime/uniformScale); 
    }
}

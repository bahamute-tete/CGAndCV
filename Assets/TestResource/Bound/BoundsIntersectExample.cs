using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsIntersectExample : MonoBehaviour
{
    public GameObject m_MyObject, m_NewObject, m_NewObject2;
    Collider m_Collider, m_Collider2;
    MeshFilter mf1, mf2;
    Mesh mesh1, mesh2;


    Vector3 worldCenter, worldCenter2;

    void Start()
    {
        
        //Check that the first GameObject exists in the Inspector and fetch the Collider
        if (m_MyObject != null)
        {
            m_Collider = m_MyObject.GetComponent<Collider>();
            mesh1 = m_MyObject.GetComponent<MeshFilter>().mesh;

        }

        //Check that the second GameObject exists in the Inspector and fetch the Collider
        if (m_NewObject != null)
        {
            m_Collider2 = m_NewObject.GetComponent<Collider>();
            

        }

        if (m_NewObject2 != null)
        {
            mesh2 = m_NewObject2.GetComponent<MeshFilter>().mesh;


        }
    }

    void Update()
    {
        //If the first GameObject's Bounds enters the second GameObject's Bounds, output the message
        //if (m_Collider.bounds.Intersects(m_Collider2.bounds))
        //{
        //    m_Collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        //}
        //else
        //{
        //    m_Collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        //}


        worldCenter = m_MyObject.transform.TransformPoint(mesh1.bounds.center);
        worldCenter2 = m_NewObject2.transform.TransformPoint(mesh2.bounds.center);
        var size0 = mesh1.bounds.extents*2f;
        var size1 = mesh2.bounds.extents*2f;
        Bounds b0 = new Bounds(worldCenter, size0);
        Bounds b1 = new Bounds(worldCenter2, size1);

        

        if (b0.Intersects(b1))
        {
            m_Collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        else
        {
            m_Collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        }

        //Matrix4x4 m2w = m_MyObject.transform.localToWorldMatrix;
    }

    //private void OnDrawGizmos()
    //{
    //    var  mesh1 = m_MyObject.GetComponent<MeshFilter>().sharedMesh;
    //    var  mesh2 = m_NewObject2.GetComponent<MeshFilter>().sharedMesh;

    //    var size0 = mesh1.bounds.extents * 2f;
    //    var size1 = mesh2.bounds.extents * 2f;

    //    Gizmos.matrix = Matrix4x4.TRS(m_MyObject.transform.localPosition, m_MyObject.transform.rotation, m_MyObject.transform.localScale);

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireCube(worldCenter, size0);

    //    //Gizmos.matrix = Matrix4x4.TRS(m_NewObject2.transform.localPosition, m_NewObject2.transform.rotation, m_NewObject2.transform.localScale);
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawWireCube(m_NewObject2.transform.TransformPoint(mesh1.bounds.center), size1);

    //}
}
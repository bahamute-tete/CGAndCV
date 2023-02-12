using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Collections.Generic;
using System.Linq;
using TMPro;

using UnityEngine;

public class OrientationBoundingBox : MonoBehaviour
{
    public int pointCount = 10;
    public GameObject prefab;

    List<Vector3> positions = new List<Vector3>();
    List<GameObject> points = new List<GameObject>();

    public List<Vector3> testPointSet = new List<Vector3>();
    [SerializeField] Vector3 center, extents;
    [SerializeField] Vector3 centerSph;
    [SerializeField] float radius;
    [SerializeField] Vector3[] obb = new Vector3[8];



    // Start is called before the first frame update
    void Start()
    {
        GameObject container = new GameObject("Container");
       

        points = CreatePointsSet(container, testPointSet);

        BoundingVolum.GetOrientationBoundingBox(testPointSet, out center,out extents,out obb);
        BoundingVolum.GetOrientationBoundingSphere(testPointSet, out centerSph, out radius);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, 0.1f);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(obb[0], obb[1]);
        Gizmos.DrawLine(obb[1], obb[3]);
        Gizmos.DrawLine(obb[3], obb[2]);
        Gizmos.DrawLine(obb[2], obb[0]);

        Gizmos.DrawLine(obb[4], obb[5]);
        Gizmos.DrawLine(obb[5], obb[7]);
        Gizmos.DrawLine(obb[7], obb[6]);
        Gizmos.DrawLine(obb[6], obb[4]);

        Gizmos.DrawLine(obb[4], obb[0]);
        Gizmos.DrawLine(obb[5], obb[1]);
        Gizmos.DrawLine(obb[7], obb[3]);
        Gizmos.DrawLine(obb[6], obb[2]);


        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centerSph, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerSph, radius);



    }


    List<Vector3> RandomPositions( int count) 
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
         Vector3 pos = new Vector3(  Random.Range(-5f, 5f),
                                     Random.Range(-5f, 5f),
                                     Random.Range(-5f, 5f));
            positions.Add(pos);
        }
        return positions;
    }

    List<GameObject> CreatePointsSet(GameObject container,List<Vector3>positions)
    {

        List<GameObject> points = new List<GameObject>();
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject o = Instantiate(prefab);
            o.transform.localPosition = positions[i];
            o.transform.localRotation = Random.rotation;
           // o.transform.localScale = Vector3.one * Random.Range(0.5f, 1);
            o.transform.SetParent(container.transform);
            points.Add(o);
        }
        return points;
    }

}

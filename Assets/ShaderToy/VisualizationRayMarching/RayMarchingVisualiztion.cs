using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class RayMarchingVisualiztion : MonoBehaviour
{

    public Mesh mesh;
    public GameObject raySource;
    public GameObject lightSource;
    private Vector3 rayPos;
    private Vector3 rayDir;


    public Vector3 sphereCenter=default;
    public float sphereRadius =1f;

    //public Vector3 sphereCenter2 = default;
    //public float sphereRadius2 = 1f;

    public float maxDistance =10f;
    public float surfaceDistance =0.01f;
    public int maxStep =10;
    public float shadowIntensity;


    public int xszie = 9;
    public int zszie = 9;
    List<float> distances = new List<float>();
    List<Vector3> planePos = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        rayPos = raySource.transform.position;
        rayDir = raySource.transform.forward;

        Gizmos.matrix = Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.localScale);

        Gizmos.color = Color.white;

        Gizmos.DrawWireMesh(mesh, 0, Vector3.zero, new Quaternion(-1, 0, 0, 1).normalized, new Vector3(1, 1, 1) * 10f);

        Gizmos.DrawWireSphere(rayPos, 0.05f);

        Plane L = new Plane(Vector3.up, Vector3.zero);
        float dis = (L.ClosestPointOnPlane(rayPos) - rayPos).magnitude;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(sphereCenter, sphereRadius);
        //Gizmos.DrawWireSphere(sphereCenter2, sphereRadius2);


        Gizmos.color = Color.yellow;
        float d = 0;
        for (int i = 0; i < 20; i++)
        {


            Vector3 p = rayPos + rayDir.normalized * d;


            float curd = GetDistance(p);

            Gizmos.DrawWireSphere(p, curd);
            Gizmos.DrawRay(p, rayDir.normalized * curd);

            Gizmos.color = Color.red;
            // Gizmos.DrawRay(p, (Vector3.Magnitude(sphereCenter-p)-sphereRadius)*(sphereCenter-p).normalized);
            Gizmos.color = Color.yellow;
            d += curd;


            if (d > maxDistance || Mathf.Abs(curd) < surfaceDistance)
            {
                //Debug.Log($"Iteration ={i}");
                Gizmos.DrawSphere(p, 0.03f);

                Vector3 normal = CalcNormal(p);
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(p, normal * 0.2f);


                Vector3 l = (lightSource.transform.position - p).normalized;
                float shadow = RayMarching(p + normal * surfaceDistance, l);

                if (shadow < Vector3.Magnitude(lightSource.transform.position - p))
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(p, l * shadow);
                }

                Gizmos.color = Color.green;
                Vector3 bitangent = GetBitangent(normal);
                Gizmos.DrawRay(p, bitangent * 0.2f);

                Gizmos.color = Color.red;
                Vector3 tangent = GetTangent(bitangent, normal);
                Gizmos.DrawRay(p, tangent * 0.2f);

                break;
            }
        }

        #region ShadowShow

        //Gizmos.color = Color.gray;
        //for (float x = 0; x < xszie; x += 0.4f)
        //{
        //    for (float z = 0; z < zszie; z += 0.4f)
        //    {

        //        Vector3 pos = new Vector3(x - xszie * 0.5f, 0, z - zszie * 0.5f);
        //        planePos.Add(pos);
        //        Gizmos.DrawWireCube(pos, new Vector3(0.2f, 0.01f, 0.2f));
        //    }
        //}

        //Gizmos.color = Color.black;
        //for (int i = 0; i < planePos.Count; i++)
        //{
        //    Vector3 l = (lightSource.transform.position - planePos[i]).normalized;
        //    float shadow = RayMarching(planePos[i] + Vector3.one * surfaceDistance, l);

        //    if (shadow < Vector3.Magnitude(lightSource.transform.position - planePos[i]))
        //    {
        //        //Gizmos.color = Color.magenta;
        //        //Gizmos.DrawRay(planePos[i], l * shadow);
        //        Gizmos.color = Color.black;
        //        Gizmos.DrawWireCube(planePos[i], new Vector3(0.2f, 0.01f, 0.2f));

        //    }
        //}
        #endregion



    }

    private Vector3 GetBitangent(Vector3 normal)
    {

        Vector3 bitangent = -Vector3.Cross(normal, new Vector3(0, 0, 1));
        return bitangent.normalized;
    }

    private Vector3 GetTangent(Vector3 bitangent, Vector3 normal)
    {

        Vector3 tangent = Vector3.Cross(normal, bitangent);
        return tangent.normalized;
    }

    float RayMarching(Vector3 p,Vector3 dir)
    {
        float d = 0;
        for (int i = 0; i <= maxStep; i++)
        {
            Vector3 pos = p + dir.normalized * d;
            //current depth;
            float curd = GetDistance(pos);
            d += curd;

            if (d > maxDistance || Mathf.Abs(curd) < surfaceDistance)
                break;
        }

        return d;
    }

    float GetDistance(Vector3 p)
    {
        float d = 0;
        float dsphere1 = SDFSphere(p, sphereCenter, sphereRadius);
        //float dsphere2 = SDFSphere(p, sphereCenter2, sphereRadius2);
        float dPlane = p.y;
        d =  Mathf.Min(dsphere1, dPlane);
        return d;
    }

    float SDFSphere(Vector3 p,Vector3 center,float radius)
    {
        float d = 0;
        d = Vector3.Magnitude(center - p) - radius;
        return d;
    }

    Vector3 GetNormal(Vector3 p)
    {
        Vector3 normal = Vector3.zero;
        float e = 0.0001f;
        
        normal = new Vector3( GetDistance(p + new Vector3(e, 0, 0)) - GetDistance(p - new Vector3(e, 0, 0)),
                              GetDistance(p + new Vector3(0, e, 0)) - GetDistance(p - new Vector3(0, e, 0)),
                              GetDistance(p + new Vector3(e, 0, 0)) - GetDistance(p - new Vector3(0, 0, e)));
        return normal.normalized;
    }

    //use this method
    Vector3 CalcNormal(Vector3 p) 
    {
        //Tetrahedron technique
        const float h = 0.0001f; // replace by an appropriate value
        //const vec2 k = vec2(1, -1);
        Vector3 k0 = new Vector3(1f, -1f, -1f);
        Vector3 k1 = new Vector3(-1f, -1f, 1f);
        Vector3 k2 = new Vector3(-1f, 1f, -1f);
        Vector3 k3 = new Vector3(1f, 1f, 1f);


        Vector3 n = k0 * GetDistance(p + k0 * h) +k1 * GetDistance(p + k1 * h) +k2 * GetDistance(p + k2 * h) + k3 * GetDistance(p + k3 * h);
        return n.normalized;
    }

    float Shading(Vector3 p,Vector3 lightPos)
    {

        Vector3 l = (lightPos -p).normalized;
        Vector3 n = CalcNormal(p);
        float albedo = Mathf.Clamp01(Vector3.Dot(n, l));

        float d = RayMarching(p + n * surfaceDistance, l);
        if (d < Vector3.Magnitude(lightPos - p))
            albedo = albedo * (1 - shadowIntensity);
        return albedo;

    }



}

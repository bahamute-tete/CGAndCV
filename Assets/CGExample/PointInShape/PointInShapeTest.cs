using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.ComponentModel;


public class PointInShapeTest : MonoBehaviour
{

    [SerializeField] List<Vector3> shapePoints = new List<Vector3>();
    [SerializeField] GameObject pointPrefab;
    GameObject content;
    List<GameObject> shapePointsProxy = new List<GameObject>();

    [SerializeField] Color lineColor = default;
    [SerializeField] float lineWidth = 0.1f;

    GameObject testPoint;
    LineRenderer lr;

    Color PointColor = Color.white;
    private void Awake() {

        LineRenderInitial(lineColor,lineWidth);
        content = new GameObject("Content");

    }

    private void LineRenderInitial(Color lineColor, float lineWidth)
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.enabled = false;
        lr.startColor = lr.endColor = lineColor;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }

    // Start is called before the first frame update
    void Start()
    {
        testPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        testPoint.transform.position = float3(0);
        testPoint.transform.rotation = Quaternion.identity;
        testPoint.transform.localScale = float3(0.5f);


        DrawShape(pointPrefab);
    }

    private void DrawShape(GameObject pointProxy)
    {
        lr.enabled = true;
        shapePoints.Add(shapePoints[0]);
        lr.positionCount = shapePoints.Count;
        lr.SetPositions(shapePoints.ToArray());

        foreach (var p in shapePoints)
        {
            shapePointsProxy.Add(Instantiate(pointProxy, p, Quaternion.identity, content.transform));
        }

    }

    // Update is called once per frame
    void Update()
    {
        UpdateShape(shapePointsProxy);
        PointColor = IsPointInShapeWindingNumber(testPoint.transform.position, shapePoints.ToArray()) ? Color.red : Color.gray;
        testPoint.GetComponent<MeshRenderer>().material.color = PointColor;
    }

    private void UpdateShape(List<GameObject> PointsProxy)
    {
        lr.positionCount = PointsProxy.Count;

        shapePoints.Clear();

        for (int i = 0; i < PointsProxy.Count; i++)
        {
            lr.SetPosition(i, PointsProxy[i].transform.position);
            shapePoints.Add(PointsProxy[i].transform.position);
        }

    }

    bool IsPointInShapeWindingNumber(Vector3 p, Vector3[] shapeVertex)
    {

        float windingNumber = 0;

        for (int i = 0; i < shapeVertex.Length; i++)
        {
            Vector3 v1 = p - shapeVertex[i];
            Vector3 v2 = p - shapeVertex[(i + 1) % shapeVertex.Length];
            
            float a = atan2(v2.z,v2.x) -atan2(v1.z,v1.x);

            if (a >= Mathf.PI)
                a -= 2 * Mathf.PI;
            else if (a <= -Mathf.PI)
                a += 2 * Mathf.PI;

            windingNumber += a;

            //Debug.Log($"windingNumber =={windingNumber}");
        }


        return Mathf.Round(windingNumber / Mathf.PI) == 0 ? false : true;
    }

    bool IsPointInShapeRayCast(Vector3 p, Vector3[] shapeVertex)
    {

        int Count = 0;

        for (int i = 0, j = shapeVertex.Length - 1; i < shapeVertex.Length; j = i++)
        {
            if ((shapeVertex[i].z > p.z) && (shapeVertex[j].z <= p.z) || (shapeVertex[i].z <= p.z) && (shapeVertex[j].z > p.z))
            {
                if (p.x < shapeVertex[i].x + (p.z - shapeVertex[i].z) / (shapeVertex[j].z - shapeVertex[i].z) * (shapeVertex[j].x - shapeVertex[i].x))
                {
                    Count += 1;
                }
            }

        }
        return Count % 2 == 0 ? false : true;
    }


}








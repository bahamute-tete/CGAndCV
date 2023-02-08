
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using UnityEngine.UIElements;

public class OrientationBoundingBox : MonoBehaviour
{
    public int pointCount = 10;
    public GameObject prefab;

    List<Vector3> positions = new List<Vector3>();
    List<GameObject> points = new List<GameObject>();

    public List<Vector3> testPointSet = new List<Vector3>();
    public Vector3 R, S, T;
    // Start is called before the first frame update
    void Start()
    {
        GameObject container = new GameObject("Container");
        positions = CreatePointsSet(pointCount);
        points = CreatePointsSet(container);

        CaculateEigenVector(testPointSet,out R,out S,out T);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<Vector3> CreatePointsSet(int count)
    {
        List<Vector3> pointSet = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            Vector3 position =new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            pointSet.Add(position);
        }

        return pointSet;
    }

    List<GameObject> CreatePointsSet(GameObject container)
    {
        List<Vector3> pos = CreatePointsSet(pointCount);
        List<GameObject> points = new List<GameObject>();
        for (int i = 0; i < pointCount; i++)
        {
            GameObject o = Instantiate(prefab);
            o.transform.localPosition = pos[i];
            o.transform.localRotation = Random.rotation;
            o.transform.localScale = Vector3.one * Random.Range(0.5f, 1);
            o.transform.SetParent(container.transform);
            points.Add(o);
        }
        return points;
    }

    Matrix<double> GetCovarianceMatrix(Matrix<double> matrix, bool bias)
    {
        var columnAverages = matrix.ColumnSums() / matrix.RowCount;
        var centeredColumns = matrix.EnumerateColumns().Zip(columnAverages, (col, avg) => col - avg);
        var centered = DenseMatrix.OfColumnVectors(centeredColumns);
        var normalizationFactor = bias ? matrix.RowCount : matrix.RowCount - 1;
        return centered.TransposeThisAndMultiply(centered) / normalizationFactor;
    }

    void CaculateEigenVector(List<Vector3> positions,out Vector3 R ,out Vector3 S,out Vector3 T)
    {
        R = S = T = Vector3.zero;
       
        double[,] pointSetArray = new double[positions.Count, 3]; 
        for (int i = 0; i < positions.Count; i++)
        {
            pointSetArray[i, 0] = positions[i].x;
            pointSetArray[i, 1] = positions[i].y;
            pointSetArray[i, 2] = positions[i].z;
        }

        Matrix<double> pointSetMatirx = DenseMatrix.OfArray(pointSetArray);

        var CovMatrix = GetCovarianceMatrix(pointSetMatirx, true);
        Evd<double> eigen = CovMatrix.Evd();
        var eigenValue = eigen.EigenValues.Real();
        var eigenVector = eigen.EigenVectors;

        //double min = DenseMatrix.op_DotMultiply(pointSetMatirx, eigenVector.Column(2));

        var s = pointSetMatirx * eigenVector;
        var min = s.Row(0).Enumerate().Min();
        var max = s.Row(0).Enumerate().Max();


        Debug.Log(s);

        Debug.Log($"min ={min}"+"\n"+$"max={max}");

        R = new Vector3((float)eigenVector.Column(2).At(0), 
                        (float)eigenVector.Column(2).At(1), 
                        (float)eigenVector.Column(2).At(2));

        S = new Vector3((float)eigenVector.Column(1).At(0),
                      (float)eigenVector.Column(1).At(1),
                      (float)eigenVector.Column(1).At(2));

        T = new Vector3((float)eigenVector.Column(0).At(0),
                      (float)eigenVector.Column(0).At(1),
                      (float)eigenVector.Column(0).At(2));


    }

    void GetOrientationBoundingBox(List<Vector3> positions)
    {
        Vector3 R, S, T = Vector3.zero;
        CaculateEigenVector(positions, out R, out S, out T);

        List<float> RDotP_Grp = new List<float>();
        List<float> SDotP_Grp = new List<float>();
        List<float> TDotP_Grp = new List<float>();
        for (int i = 0; i < positions.Count; i++)
        {
            RDotP_Grp.Add(Vector3.Dot(R, positions[i]));
            SDotP_Grp.Add(Vector3.Dot(S, positions[i]));
            TDotP_Grp.Add(Vector3.Dot(T, positions[i]));
        }


    }
}

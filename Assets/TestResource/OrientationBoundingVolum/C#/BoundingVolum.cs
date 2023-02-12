using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BoundingVolum 
{
    private static BoundingVolum instance;
    public static BoundingVolum Instance { 
        get {
            if (instance == null)
                {
                    instance = new BoundingVolum();
                }
                return instance; 
            } 
    }

    private BoundingVolum() { }


    static Matrix<double>  GetCovarianceMatrix(Matrix<double> matrix, bool bias)
{
    var columnAverages = matrix.ColumnSums() / matrix.RowCount;
    var centeredColumns = matrix.EnumerateColumns().Zip(columnAverages, (col, avg) => col - avg);
    var centered = DenseMatrix.OfColumnVectors(centeredColumns);
    var normalizationFactor = bias ? matrix.RowCount : matrix.RowCount - 1;
    return centered.TransposeThisAndMultiply(centered) / normalizationFactor;
}

    static Matrix<double> GetPointSetMatrix(List<Vector3> positions)
{
    double[,] pointSetMatrix = new double[positions.Count, 3];
    for (int i = 0; i < positions.Count; i++)
    {
        pointSetMatrix[i, 0] = positions[i].x;
        pointSetMatrix[i, 1] = positions[i].y;
        pointSetMatrix[i, 2] = positions[i].z;
    }
    return DenseMatrix.OfArray(pointSetMatrix);
}

    static Matrix<double> GetEigenVector(Matrix<double> pointSetMatirx)
{
    var CovMatrix = GetCovarianceMatrix(pointSetMatirx, true);
    Evd<double> eigen = CovMatrix.Evd();
    var eigenValue = eigen.EigenValues.Real();
    var eigenVectors = eigen.EigenVectors;

    return eigenVectors;
}

    static void GetPCA(Matrix<double> eigenVectors,out Vector3 R ,out Vector3 S ,out Vector3 T)
{
        R = S = T = Vector3.zero;

        R = new Vector3(   (float)eigenVectors.Column(2).At(0),
                        (float)eigenVectors.Column(2).At(1),
                        (float)eigenVectors.Column(2).At(2));

        S = new Vector3(   (float)eigenVectors.Column(1).At(0),
                        (float)eigenVectors.Column(1).At(1),
                        (float)eigenVectors.Column(1).At(2));

        T = new Vector3(   (float)eigenVectors.Column(0).At(0),
                        (float)eigenVectors.Column(0).At(1),
                        (float)eigenVectors.Column(0).At(2));

}

    static Dictionary<string,Vector2> GetPlaneDistanceMinMaxValue(Matrix<double> pointSetMatirx, Matrix<double> eigenVectors)
{
    Dictionary<string, Vector2> res = new Dictionary<string, Vector2>();
    var s = pointSetMatirx * eigenVectors;

    var minT =(float) s.Column(0).Enumerate().Min();
    var maxT =(float) s.Column(0).Enumerate().Max();

    var minS = (float)s.Column(1).Enumerate().Min();
    var maxS = (float)s.Column(1).Enumerate().Max();

    var minR = (float)s.Column(2).Enumerate().Min();
    var maxR = (float)s.Column(2).Enumerate().Max();

    res.Add("T", new Vector2(minT, maxT));
    res.Add("S", new Vector2(minS, maxS));
    res.Add("R", new Vector2(minR, maxR));

    return res;
}

    public static void GetOrientationBoundingBox(List<Vector3> positions, out Vector3 center, out Vector3 extents, out Vector3[] boundingBoxPoints)
    {
        center = extents = Vector3.zero;
        boundingBoxPoints = new Vector3[8];

        Matrix<double> pointSetMatirx = GetPointSetMatrix(positions);
        Matrix<double> eigenVectors = GetEigenVector(pointSetMatirx);

        Vector3 R, S, T;
        GetPCA(eigenVectors, out R, out S, out T);

        Dictionary<string, Vector2> pairs = GetPlaneDistanceMinMaxValue(pointSetMatirx, eigenVectors);

        var a = 0.5f * (pairs["R"].x + pairs["R"].y);
        var b = 0.5f * (pairs["S"].x + pairs["S"].y);
        var c = 0.5f * (pairs["T"].x + pairs["T"].y);
        var sa = 0.5f * (pairs["R"].x - pairs["R"].y);
        var sb = 0.5f * (pairs["S"].x - pairs["S"].y);
        var sc = 0.5f * (pairs["T"].x - pairs["T"].y);

        center = a * R + b * S + c * T;
        extents = new Vector3(Mathf.Abs(sa), Mathf.Abs(sb), Mathf.Abs(sc));

        Vector3 p0 = center + extents.x * R + extents.y * S + extents.z * T;
        Vector3 p1 = center + extents.x * R + extents.y * S - extents.z * T;

        Vector3 p2 = center + extents.x * R - extents.y * S + extents.z * T;
        Vector3 p3 = center + extents.x * R - extents.y * S - extents.z * T;

        Vector3 p4 = center - extents.x * R + extents.y * S + extents.z * T;
        Vector3 p5 = center - extents.x * R + extents.y * S - extents.z * T;

        Vector3 p6 = center - extents.x * R - extents.y * S + extents.z * T;
        Vector3 p7 = center - extents.x * R - extents.y * S - extents.z * T;


        boundingBoxPoints = new Vector3[8] { p0, p1, p2, p3, p4, p5, p6, p7 };
    }

  
    public static void GetOrientationBoundingSphere(List<Vector3> positions, out Vector3 center, out float radius)
    {
        center = Vector3.zero;
        radius = 0;
        Vector3 tangencyPos= Vector3.zero;

        Matrix<double> pointSetMatirx = GetPointSetMatrix(positions);
        Matrix<double> eigenVectors = GetEigenVector(pointSetMatirx);

        var s = pointSetMatirx * eigenVectors;

        var minR_Index = s.Column(2).MinimumIndex();
        var maxR_Index = s.Column(2).MaximumIndex();

        var pMin =pointSetMatirx.Row(minR_Index);
        var pMax = pointSetMatirx.Row(maxR_Index);

        var array_min = pMin.ToArray();
        Vector3 p_Min = new Vector3((float)array_min[0], (float)array_min[1], (float)array_min[2]);
        var array_max = pMax.ToArray();
        Vector3 p_Max = new Vector3((float)array_max[0], (float)array_max[1], (float)array_max[2]);

        center = 0.5f * (p_Min + p_Max);
        radius = Vector3.Distance(p_Max, center);

       ////includeTesting
        for (int i = 0; i < positions.Count; i++)
        {
            if (Vector3.SqrMagnitude(positions[i] - center) > radius * radius)
            {
                var dir = (positions[i] - center).normalized;
                tangencyPos = center - radius * dir;
                center = 0.5f * (positions[i] + tangencyPos);
                radius = Vector3.Distance(tangencyPos, center);
            }
        }

    }

}


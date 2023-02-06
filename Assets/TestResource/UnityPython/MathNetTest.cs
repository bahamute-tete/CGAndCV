using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

public class MathNetTest : MonoBehaviour
{

   
    // Start is called before the first frame update
    void Start()
    {
        Matrix<double> A = Matrix<double>.Build.Random(3, 4);
        

        var M = Matrix<float>.Build;
        var m =M.Random(3, 4);

        double[,] x = { { -1, -2, 1 }, { 1, 0, 2 }, { 2, -1, 3 }, { 2, -1, 2 } };
        var B = DenseMatrix.OfArray(x);

        Generate.LinearSpaced(11, 0.0, 1.0);

        var C = GetCovarianceMatrix(B,true);

        Evd<double> eigen = C.Evd();
        Debug.Log(B.Transpose());
        Debug.Log(C);

        var  eigenValue = eigen.EigenValues;
        var eigenVector = eigen.EigenVectors;

        Debug.Log(eigenValue);
        Debug.Log(eigenVector);

        Vector3 R = new Vector3( (float)eigenVector.Column(2).At(0),(float)eigenVector.Column(2).At(1),(float)eigenVector.Column(2).At(2));

        Debug.Log(R);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Matrix<double> GetCovarianceMatrix(Matrix<double> matrix,bool bias)
    {
        var columnAverages = matrix.ColumnSums() / matrix.RowCount;
        var centeredColumns = matrix.EnumerateColumns().Zip(columnAverages, (col, avg) => col - avg);
        var centered = DenseMatrix.OfColumnVectors(centeredColumns);
        var normalizationFactor =bias ? matrix.RowCount : matrix.RowCount - 1;
        return centered.TransposeThisAndMultiply(centered)/normalizationFactor;
    }
}

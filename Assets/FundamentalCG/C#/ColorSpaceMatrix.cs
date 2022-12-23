using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSpaceMatrix : MonoBehaviour
{

    Matrix4x4 M_sRGBToXYZ = new Matrix4x4();
    Matrix4x4 M_XYZTosRGB = new Matrix4x4();
    Matrix4x4 M_XYZToLSM = new Matrix4x4();
    Matrix4x4 M_LSMToXYZ = new Matrix4x4();

    const float FLT_EPSILON = 1.192092896e-7f;

    [SerializeField]
    Text mtr0;
    [SerializeField]
    Text mtr1;
    [SerializeField]
    Text mtr2;
    [SerializeField]
    Text mtr3;
    // Start is called before the first frame update
    void Start()
    {
        Vector4 s2x0 = new Vector4(0.4124f, 0.3576f, 0.1805f, 0.0000f);
        Vector4 s2x1 = new Vector4(0.2126f, 0.7152f, 0.0722f, 0.0000f);
        Vector4 s2x2 = new Vector4(0.0193f, 0.1192f, 0.9505f, 0.0000f);
        Vector4 s2x3 = new Vector4(0.0000f, 0.0000f, 0.0000f, 1.0000f);

        Vector4 x2s0 = new Vector4(3.2405f, -1.5371f, -0.4985f, 0.0000f);
        Vector4 x2s1 = new Vector4(-0.9693f, 1.8706f, 0.0416f, 0.0000f);
        Vector4 x2s2 = new Vector4(0.0556f, -0.2040f, 1.0572f, 0.0000f);
        Vector4 x2s3 = new Vector4(0.0000f, 0.0000f, 0.0000f, 1.0000f);

        Vector4 x2l0 = new Vector4(0.38971f, 0.68898f, -0.07868f, 0.0000f);
        Vector4 x2l1 = new Vector4(-0.22981f, 1.18340f, 0.04641f, 0.0000f);
        Vector4 x2l2 = new Vector4(0.00000f, 0.00000f, 1.00000f, 0.0000f);
        Vector4 x2l3 = new Vector4(0.00000f, 0.00000f, 0.00000f, 1.0000f);

        Vector4 l2x0 = new Vector4(1.91019f, -1.11214f, 0.20195f, 0.0000f);
        Vector4 l2x1 = new Vector4(0.37095f, 0.62905f, 0.00000f, 0.0000f);
        Vector4 l2x2 = new Vector4(0.00000f, 0.00000f, 1.00000f, 0.00000f);
        Vector4 l2x3 = new Vector4(0.00000f, 0.00000f, 0.00000f, 1.00000f);


        M_sRGBToXYZ.SetRow(0, s2x0);
        M_sRGBToXYZ.SetRow(1, s2x1);
        M_sRGBToXYZ.SetRow(2, s2x2);
        M_sRGBToXYZ.SetRow(3, s2x3);

        M_XYZTosRGB.SetRow(0, x2s0);
        M_XYZTosRGB.SetRow(1, x2s1);
        M_XYZTosRGB.SetRow(2, x2s2);
        M_XYZTosRGB.SetRow(3, x2s3);

        M_XYZToLSM.SetRow(0, x2l0);
        M_XYZToLSM.SetRow(1, x2l1);
        M_XYZToLSM.SetRow(2, x2l2);
        M_XYZToLSM.SetRow(3, x2l3);

        M_LSMToXYZ.SetRow(0, l2x0);
        M_LSMToXYZ.SetRow(1, l2x1);
        M_LSMToXYZ.SetRow(2, l2x2);
        M_LSMToXYZ.SetRow(3, l2x3);

        Matrix4x4 Color_LIN_2_LMS_MAT = M_XYZToLSM * M_sRGBToXYZ;

        mtr0.text = Color_LIN_2_LMS_MAT.GetRow(0).ToString("#.#####");
        mtr1.text = Color_LIN_2_LMS_MAT.GetRow(1).ToString("#.#####");
        mtr2.text = Color_LIN_2_LMS_MAT.GetRow(2).ToString("#.#####");
        mtr3.text = Color_LIN_2_LMS_MAT.GetRow(3).ToString("#.#####");

        //Debug.Log(3.0f / 0.0f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3 ColorSpacePositivePow(Vector3 x, float a)
    {
        float v1= Mathf.Pow(Mathf.Max(Mathf.Abs(x.x), FLT_EPSILON), a);
        float v2 = Mathf.Pow(Mathf.Max(Mathf.Abs(x.y), FLT_EPSILON), a);
        float v3 = Mathf.Pow(Mathf.Max(Mathf.Abs(x.z), FLT_EPSILON), a);

        return new Vector3(v1, v2, v3);

    }


    Vector3 ColorSpaceLinearToSRGB(Vector3 c)
    {

        Vector3 sRGBLo =new Vector3( c.x * 12.92f, c.y * 12.92f, c.z * 12.92f);
        float vv1 = (ColorSpacePositivePow(c, 1.0f / 2.4f).x * 1.055f) - 0.055f;
        float vv2 = (ColorSpacePositivePow(c, 1.0f / 2.4f).y * 1.055f) - 0.055f;
        float vv3 = (ColorSpacePositivePow(c, 1.0f / 2.4f).z * 1.055f) - 0.055f;
        Vector3 sRGBHi =new Vector3(vv1,vv2,vv3);

        float l1 = (c.x <= 0.0031308) ? sRGBLo.x : sRGBHi.x;
        float l2 = (c.y <= 0.0031308) ? sRGBLo.y : sRGBHi.y;
        float l3 = (c.z <= 0.0031308) ? sRGBLo.z : sRGBHi.z;
        Vector3 sRGB = new Vector3(l1,l2,l3);
        return sRGB;

    }


    Vector3 ColorSpaceSRGBToLinear(Vector3 c)
    {

        Vector3 linearRGBLo =new Vector3( c.x / 12.92f, c.y / 12.92f, c.z / 12.92f);
        float vv1 = ColorSpacePositivePow((c+new Vector3(0.055f, 0.055f, 0.055f))/1.055f,2.4f).x;
        float vv2 = ColorSpacePositivePow((c + new Vector3(0.055f, 0.055f, 0.055f)) / 1.055f, 2.4f).y;
        float vv3 = ColorSpacePositivePow((c + new Vector3(0.055f, 0.055f, 0.055f)) / 1.055f, 2.4f).z;
        Vector3 linearRGBHi = new Vector3(vv1, vv2, vv3);

        float l1 = (c.x <= 0.04045) ? linearRGBLo.x : linearRGBHi.x;
        float l2 = (c.y <= 0.04045) ? linearRGBLo.y : linearRGBHi.y;
        float l3 = (c.z <= 0.04045) ? linearRGBLo.z : linearRGBHi.z;

        Vector3 linearRGB = new Vector3(l1, l2, l3);
        return linearRGB;

    }
}

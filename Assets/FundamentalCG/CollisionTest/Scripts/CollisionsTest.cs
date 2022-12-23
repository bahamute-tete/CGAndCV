using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionsTest : MonoBehaviour
{
    [Header("TestSph")]
    [SerializeField] GameObject collisionSph;
    [SerializeField] Vector3 velocitySph = new Vector3(2.0f, 1.0f, 0);
    [SerializeField] private Vector4[] collisionPointSph;
    private float effective_R_Sph;
    private Vector3 startSphPos;
    private float t_CollisionSph;
    private float collisonD_Sph;
    Vector3 nextSphPos;

    [Header("TestCube")]
    [SerializeField] GameObject collisionCube;
    [SerializeField] Vector3 velocityCube = new Vector3(2.0f, 1.0f, 0);
    [SerializeField] private Vector4[] collisionPointCube;
    private float effective_R_Cube;
    private Vector3 startCubePos;
    private float t_CollisionCube;
    private float collisonD_Cube;
    Vector3 nextCubePos;

    [Header("CollisionPlane")]
    [SerializeField] GameObject collisionPlane;
    private Vector3 pN = Vector3.zero;
    private Vector4 L = Vector4.one;
    [SerializeField] Material groundMat;
    int typeIndex = 0;
    int ElementCount;

    [Header("UI")]
    [SerializeField] Text sc;
    [SerializeField] Text cc0;
    [SerializeField] Text cc1;
    [SerializeField] Text cc2;
    [SerializeField] Text cc3;

    [SerializeField] Text sct;
    [SerializeField] Text cct;



    void Start()
    {

        startSphPos = collisionSph.transform.position;
        startCubePos = collisionCube.transform.position;


        CollisionCaculate(collisionSph, collisionPlane, velocitySph,true,out pN, out t_CollisionSph, out collisionPointSph,out effective_R_Sph);

        CollisionCaculate(collisionCube, collisionPlane, velocityCube, false, out pN, out t_CollisionCube, out collisionPointCube,out effective_R_Cube);

        Vector3 originalNormal = collisionPlane.GetComponent<MeshFilter>().mesh.normals[0];
        L = TransformPlane(collisionPlane.transform.position, collisionPlane.transform.rotation, new Vector4(originalNormal.x, originalNormal.y, originalNormal.z, 0));

        //Debug.Log("t_Collision_Sph ==" + t_Collision);

        //foreach (var p in collisionPointSph)
        //{
        //    Debug.Log("collisionPos_Sph ==" + p);
        //}

        Matrix4x4 M_w2o= collisionPlane.transform.worldToLocalMatrix;




        sc.text = collisionPointSph[0].ToString();
        cc0.text = collisionPointCube[0].ToString();
        cc1.text = collisionPointCube[1].ToString();
        cc2.text = collisionPointCube[2].ToString();
        cc3.text = collisionPointCube[3].ToString();

        sct.text = "CollisionTime(Sphere) = " + t_CollisionSph.ToString();
        cct.text = "CollisionTime(Cube) = " + t_CollisionCube.ToString();


        foreach (var p in collisionPointCube)
        {
            //Debug.Log("collisionPos_Cube ==" + p);
            if (p != Vector4.zero)
                ElementCount++;
        }

      
        switch (ElementCount)
        {
            case 1:
                typeIndex = 0;//point
                cc1.enabled = false;
                cc2.enabled = false;
                cc3.enabled = false;
                break;

            case 2:
                typeIndex = 1;//line
                cc2.enabled = false;
                cc3.enabled = false;
                break;

            case 4:
                typeIndex = 2;//plane
                break;
        }
        Debug.Log("ElementCount==" + ElementCount);
        Debug.Log("_Type==" + typeIndex);
        int typeID = Shader.PropertyToID("_Type");
        groundMat.SetInt(typeID, typeIndex);


        int sphCP = Shader.PropertyToID("_CollisionPointSph");
        int cubeCP = Shader.PropertyToID("_CollisionPointCube");
        int matrixW2L = Shader.PropertyToID("_WorldToLocal");
        groundMat.SetVector(sphCP, collisionPointSph[0]);
        groundMat.SetVectorArray(cubeCP, collisionPointCube);
        groundMat.SetMatrix(matrixW2L, M_w2o);






    }

    //Update is called once per frame
    void Update()
    {


        sc.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, collisionPointSph[0]) + new Vector2(0, -70);

        cc0.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, collisionPointCube[0]) + new Vector2(-70, -70);
        cc1.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, collisionPointCube[1]) + new Vector2(70, -70);
        cc2.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, collisionPointCube[2]) + new Vector2(-70, 70);
        cc3.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, collisionPointCube[3]) + new Vector2(70, 70);


        sc.text = collisionPointSph[0].ToString();
        cc0.text = collisionPointCube[0].ToString();
        cc1.text = collisionPointCube[1].ToString();
        cc2.text = collisionPointCube[2].ToString();
        cc3.text = collisionPointCube[3].ToString();


        nextSphPos = startSphPos + velocitySph * Time.deltaTime;
        collisonD_Sph = Vector4.Dot(L, new Vector4(nextSphPos.x, nextSphPos.y, nextSphPos.z, 1));
        //Debug.Log("collisonD_Sph==" + collisonD_Sph);
        startSphPos = nextSphPos;
        collisionSph.transform.position = nextSphPos;
        if (collisonD_Sph <= effective_R_Sph)
        {
            velocitySph = Vector3.zero;
        }
        

        nextCubePos = startCubePos + velocityCube * Time.deltaTime;
        collisonD_Cube = Vector4.Dot(L, new Vector4(nextCubePos.x, nextCubePos.y, nextCubePos.z, 1));
        //Debug.Log("collisonD_Cube==" + collisonD_Cube);
        startCubePos = nextCubePos;
        collisionCube.transform.position = nextCubePos;
        if (collisonD_Cube <=effective_R_Cube)
        {
            velocityCube = Vector3.zero;
        }
       

    }

    Vector4 TransformPlane(Vector3 T_xyz, Quaternion q, Vector4 L)
    {

        Matrix4x4 Rot = Matrix4x4.Rotate(q);
        Matrix4x4 Trans = Matrix4x4.Translate(T_xyz);

        Matrix4x4 customTrans = new Matrix4x4();
        customTrans.SetColumn(0, new Vector4(T_xyz.x, T_xyz.y, T_xyz.z, 0));

        Matrix4x4 InverRot = Rot.inverse;

        Matrix4x4 M_Inver_T = InverRot * customTrans;

        Matrix4x4 IF = new Matrix4x4();
        IF.SetColumn(0, InverRot.GetColumn(0));
        IF.SetColumn(1, InverRot.GetColumn(1));
        IF.SetColumn(2, InverRot.GetColumn(2));
        IF.SetColumn(3, -M_Inver_T.GetColumn(0));
        IF.SetRow(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 ITF = Matrix4x4.Transpose(IF);

        Vector4 newP = ITF * L;
        return newP;
    }

    void CollisionCaculate(GameObject collisionObject, GameObject collisionPlane,Vector3 velocity,bool isSphere,out Vector3 collisionPlaneNormal, out float collisionTime,out Vector4[] contactPoint,out float effectiveR)
    {
        collisionTime = 0;

        if (isSphere)
        contactPoint = new Vector4[1];
        else
        contactPoint = new Vector4[4];

        collisionPlaneNormal = Vector3.one;
        effectiveR = 0;

        Vector3 originalNormal = collisionPlane.GetComponent<MeshFilter>().mesh.normals[0];
        Vector4 L = TransformPlane(collisionPlane.transform.position, collisionPlane.transform.rotation, new Vector4(originalNormal.x, originalNormal.y, originalNormal.z, 0));
        Vector3 normal_L = new Vector3(L.x, L.y, L.z);
        //Debug.Log("normal_L ==" + normal_L);
        float D = L.w;

        Vector3 center = collisionObject.transform.position;
       
        float RN, SN, TN;
        Vector3 R = collisionCube.transform.right;
        Vector3 S = collisionCube.transform.up;
        Vector3 T = collisionCube.transform.forward;


        RN = Vector3.Dot(collisionCube.transform.localScale.x * R, normal_L);
        SN = Vector3.Dot(collisionCube.transform.localScale.y * S, normal_L);
        TN = Vector3.Dot(collisionCube.transform.localScale.z * T, normal_L);

        effectiveR = (isSphere) ? collisionObject.transform.localScale.x * 0.5f: (Mathf.Abs(RN) + Mathf.Abs(SN) + Mathf.Abs( TN)) * 0.5f;

        //Debug.Log("effective_R ==" + effective_R);

        Vector4 L_prime = new Vector4(L.x, L.y, L.z, D - effectiveR);

        Vector3 centerPos = collisionObject.transform.position;
        Vector4 sphC_Homo = new Vector4(centerPos.x, centerPos.y, centerPos.z, 1.0f);

        collisionPlaneNormal = normal_L;
        collisionTime = -Vector4.Dot(L_prime, sphC_Homo) / Vector3.Dot(normal_L, velocity);


        if (isSphere)
        {
            Vector3 res = (center + velocity * collisionTime) - normal_L * effectiveR;
            contactPoint[0] = new Vector4(res.x, res.y, res.z, 1.0f);
        }
        else
        {

            Vector3 resRN = Mathf.Sign(RN) * R * collisionCube.transform.localScale.x;
            Vector3 resSN = Mathf.Sign(SN) * S * collisionCube.transform.localScale.y;
            Vector3 resTN = Mathf.Sign(TN) * T * collisionCube.transform.localScale.z;

            List<float> resDot = new List<float>();
            List<float> resNoneZero = new List<float>();

            resDot.Add(Mathf.Abs(RN)); resDot.Add(Mathf.Abs(SN)); resDot.Add(Mathf.Abs(TN));

            resNoneZero.Add(0); resNoneZero.Add(0); resNoneZero.Add(0);

            int zeroIndex = 0;

            for (int i = 0; i < resDot.Count; i++)
            {
                //Debug.Log("resDot[" + i + "]==" + resDot[i]);
                if (resDot[i] - 0.0001f < 0)
                {
                    //Debug.Log("i====" + i);
                    switch (i)
                    {
                        case 0:
                            resRN = R * collisionCube.transform.localScale.x;
                            break;

                        case 1:
                            resSN = S * collisionCube.transform.localScale.y;
                            break;

                        case 2:
                            resTN = T * collisionCube.transform.localScale.z;
                            break;

                    }
                    zeroIndex++;
                }
                else
                {
                    resNoneZero[i] = resDot[i];
                }
            }

            if (zeroIndex ==0)
            {
                Debug.Log("Point");
                contactPoint[0] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN + resTN);
            }

            if (zeroIndex == 1)
            {
                Debug.Log("Line");


                if (resNoneZero[0]==0)
                {
                    contactPoint[0] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN + resTN);
                    contactPoint[1] = (center + velocity * collisionTime) - 0.5f * (-resRN + resSN + resTN);
                }
                if (resNoneZero[1] == 0)
                {
                    contactPoint[0] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN + resTN);
                    contactPoint[1] = (center + velocity * collisionTime) - 0.5f * (resRN - resSN + resTN);
                }
                if (resNoneZero[2] == 0)
                {
                    contactPoint[0] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN + resTN);
                    contactPoint[1] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN - resTN);
                }

            }

            if (zeroIndex == 2)
            {
                Debug.Log("Plane");


                if (resNoneZero[0] != 0)
                {
                    contactPoint[0] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN + resTN);
                    contactPoint[1] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN - resTN);
                    contactPoint[2] = (center + velocity * collisionTime) - 0.5f * (resRN - resSN - resTN);
                    contactPoint[3] = (center + velocity * collisionTime) - 0.5f * (resRN - resSN + resTN);
                }

                if (resNoneZero[1] != 0)
                {
                    contactPoint[0] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN + resTN);
                    contactPoint[1] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN - resTN);
                    contactPoint[2] = (center + velocity * collisionTime) - 0.5f * (-resRN + resSN - resTN);
                    contactPoint[3] = (center + velocity * collisionTime) - 0.5f * (-resRN + resSN + resTN);
                }

                if (resNoneZero[2] != 0)
                {
                    contactPoint[0] = (center + velocity * collisionTime) - 0.5f * (resRN + resSN + resTN);
                    contactPoint[1] = (center + velocity * collisionTime) - 0.5f * (-resRN + resSN + resTN);
                    contactPoint[2] = (center + velocity * collisionTime) - 0.5f * (-resRN - resSN + resTN);
                    contactPoint[3] = (center + velocity * collisionTime) - 0.5f * (resRN - resSN + resTN);
                }

            }

        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodriguesRotation : MonoBehaviour
{

    [SerializeField] GameObject cube0;
    [SerializeField] GameObject cube1;
    Vector3 v0;
    Vector3 v1;
    [SerializeField] Vector3 u;//rotatingAxis
    [Range(0,6.28f)]
    [SerializeField] float theta;


    GameObject lineContent;
    LineRenderer lr;
    float t;

    // Start is called before the first frame update
    void Start()
    {
        //lineContent = new GameObject("LineContent");
        //lr = lineContent.AddComponent<LineRenderer>();
        //lr.positionCount = 2;
        //lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        //lr.startColor = lr.endColor = Color.red;
        //lr.startWidth = lr.endWidth = 0.05f;


        v0 = cube0.transform.forward;
        v1 = cube1.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {

        //lr.SetPosition(0, - Vector3.Normalize( u)*2);
        //lr.SetPosition(1,Vector3.Normalize( u)*2);
         t += Time.deltaTime * Mathf.Rad2Deg*0.1f;
        cube0.transform.localPosition = new Vector3(0, 0, 0);

        cube0.transform.forward = RodriguesMatrix(u, t) * v0;

        cube0.transform.localPosition = new Vector3(0, 0, 1);
        //cube1.transform.forward = RodriguesFomular(v1, u, t);

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(Vector3.Normalize(u) * 2, -Vector3.Normalize(u) * 2);

        //for(int i = 0;i <10;i)
    }

    Vector3 RodriguesFomular(Vector3 v, Vector3 u, float theta)
    {
        u = Vector3.Normalize(u);
        v = Vector3.Normalize(v);
        Vector3 part1 = v * Mathf.Cos(theta);
        Vector3 part2 = u * (1 - Mathf.Cos(theta)) * Vector3.Dot(u, v);
        Vector3 part3 = Mathf.Sin(theta) * Vector3.Cross(u, v);
        Vector3 res = part1 + part2 + part3;
        return res;
    }

    Matrix4x4 RodriguesMatrix(Vector3 u, float theta)
    {
        u = Vector3.Normalize(u);
        Matrix4x4 R = new Matrix4x4();

        R.m00 = Mathf.Cos(theta) + u.x * u.x * (1 - Mathf.Cos(theta));
        R.m01 = u.x * u.y * (1 - Mathf.Cos(theta)) - u.z * Mathf.Sin(theta);
        R.m02 = u.y * Mathf.Sin(theta) + u.x * u.z * (1 - Mathf.Cos(theta));
        R.m10 = u.z * Mathf.Sin(theta) + u.x * u.y * (1 - Mathf.Cos(theta));
        R.m11 = Mathf.Cos(theta) + u.y * u.y * (1 - Mathf.Cos(theta));
        R.m12 = -u.x * Mathf.Sin(theta) + u.y * u.z * (1 - Mathf.Cos(theta));
        R.m20 = -u.y * Mathf.Sin(theta) + u.x * u.z * (1 - Mathf.Cos(theta));
        R.m21 = u.x * Mathf.Sin(theta) + u.y * u.z*(1 - Mathf.Cos(theta));
        R.m22 = Mathf.Cos(theta) + u.z * u.z * (1 - Mathf.Cos(theta));
        R.m33 = 1.0f;

        return R;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}

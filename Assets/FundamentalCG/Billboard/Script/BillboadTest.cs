using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillboadTest : MonoBehaviour
{
    [SerializeField] float W;
    [SerializeField] float H;
    [SerializeField] Vector3 R;
    [SerializeField] Vector3 U;
    [SerializeField] Vector3 P;

    Vector3[] pos = new Vector3[5];

    [SerializeField] GameObject refrenceObj;
    GameObject LineRenderContent;
    LineRenderer lr;
    [SerializeField]private Material lineMat;
    [SerializeField]private Color lineColor = new Vector4(1, 1, 1, 1);
    [Range(0,6.28f)]
    [SerializeField] private float angle;
    [SerializeField] bool isFaceAlign;
    [SerializeField] bool isConstrainY;

    [SerializeField] Toggle toggleVertical;
    [SerializeField] Toggle toggleFacePlane;
    [SerializeField] Slider particleAngle;

    // Start is called before the first frame update
    void Start()
    {
       

        LineRenderContent = new GameObject("Content");

        lr = LineRenderContent.AddComponent<LineRenderer>();
        lr.material = lineMat;
        isFaceAlign = true;
        isConstrainY = true;


        toggleVertical.onValueChanged.AddListener(delegate
        {
            isConstrainY = !isConstrainY;
        });

        toggleFacePlane.onValueChanged.AddListener(delegate
        {
            isFaceAlign = !isFaceAlign;
        });

        particleAngle.value = angle;
        particleAngle.onValueChanged.AddListener(delegate {
           
            angle = particleAngle.value;

        });

        refrenceObj = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isConstrainY)
        {
            toggleFacePlane.transform.gameObject.SetActive(false);
        }
        else
        {
            toggleFacePlane.transform.gameObject.SetActive(true);
        }



        BillboardAlignCamera(refrenceObj, P, new Vector2(W, H), angle, isFaceAlign, isConstrainY, out pos);
        if (!isFaceAlign)
        {
            Vector3 C = Camera.main.transform.position;
            Vector3 Z = Vector3.Normalize(C - P);
            Camera.main.transform.forward = -Z;
        }


        lr.positionCount = pos.Length;
        lr.startWidth = lr.endWidth = 0.05f;
        lr.startColor = lr.endColor = lineColor;
        lr.SetPositions(pos);


    }

    void BillboardAlignCamera(GameObject c,Vector3 P, Vector2 size,float angle,bool isFaceAlign,bool isConstrainY, out Vector3[] pos)
    {
        pos = new Vector3[5];
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = Vector3.zero;
        }

        Vector3 R = c.transform.right;
        Vector3 U = c.transform.up;

        Vector3 C = c.transform.position;//CameraPos In WorldSpace
        Vector3 Z = Vector3.Normalize(C - P);

        Vector3 B, A;
        Vector3 X, Y;

        Vector3 Q1 ;
        Vector3 Q2 ;
        Vector3 Q3 ;
        Vector3 Q4 ;

        if (isConstrainY)
        {
            Vector3 constrainX = new Vector3(P.z-C.z,0, C.x-P.x);
           

            Q1 = P + 0.5f * size.x * Vector3.Normalize(constrainX) + new Vector3(0, 0.5f * size.y, 0 );
            Q2 = P - 0.5f * size.x * Vector3.Normalize(constrainX) + new Vector3(0, 0.5f * size.y, 0);
            Q3 = P - 0.5f * size.x * Vector3.Normalize(constrainX) - new Vector3(0, 0.5f * size.y, 0);
            Q4 = P + 0.5f * size.x * Vector3.Normalize(constrainX) - new Vector3(0, 0.5f * size.y, 0);

        }
        else
        {
            if (Vector3.Magnitude(Vector3.Cross(U, Z)) - Mathf.Epsilon < 0)
            {
                B = Vector3.Normalize(Vector3.Cross(Z, R));
                A = Vector3.Cross(B, Z);
            }
            else
            {
                A = Vector3.Normalize(Vector3.Cross(U, Z));
                B = Vector3.Cross(Z, A);
            }


            if (isFaceAlign)
            {
                //Align with camera face plane ==> effettive with large amount particles rendering
                X = size.x * 0.5f * Mathf.Cos(angle) * R + size.x * 0.5f * Mathf.Sin(angle) * U;
                Y = -size.y * 0.5f * Mathf.Sin(angle) * R + size.y * 0.5f * Mathf.Cos(angle) * U;
            }
            else
            {
                X = size.x * 0.5f * Mathf.Cos(angle) * A + size.x * 0.5f * Mathf.Sin(angle) * B;
                Y = -size.y * 0.5f * Mathf.Sin(angle) * A + size.y * 0.5f * Mathf.Cos(angle) * B;
            }

            Q1 = P + X + Y;
            Q2 = P - X + Y;
            Q3 = P - X - Y;
            Q4 = P + X - Y;

        }

        pos[0] = Q1; pos[1] = Q2; pos[2] = Q3; pos[3] = Q4; pos[4] = Q1;

    }
}

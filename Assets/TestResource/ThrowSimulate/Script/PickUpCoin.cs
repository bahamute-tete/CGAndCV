using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

public class PickUpCoin : MonoBehaviour
{

    [SerializeField] Camera camera;
    float nearPlane;
    [SerializeField]float offsetZ=1f;
    float depth;
    bool isPick = false;
    [SerializeField] GameObject pickUpObj;
    Vector3 startPos, endPos;
    float t;
    [SerializeField] float duration = 1f;
    [SerializeField]bool isAttract = false;

    Rigidbody rg;

    GameObject gizmoContent;
 
    LineRenderer lr;
    [SerializeField] Material mat;

    //public delegate void AttractThrowCoin();
    //public static event  AttractThrowCoin OnAttractThrowCoin;

    //public delegate void ThrowCoin();
    //public static event ThrowCoin OnyThrowCoin;

    Vector3 readyPos = Vector3.zero; // MMB pressed
    Vector3 releasePos = Vector3.zero; //MMB release
    Vector3 readyMousePos;
    Vector3 releaseDir;


    [SerializeField]Slider forceSlider;


    GameObject pathContent;
    GameObject pathContent2;
    GameObject pathContent3;
    LineRenderer lrpath;
    LineRenderer lrpath2;
    LineRenderer lrpath3;
    [SerializeField]int pathCount = 100;
    [SerializeField] GameObject endPot;
    [SerializeField]List<Vector3> pathPoint = new List<Vector3>();

    private void Awake()
    {
        nearPlane = camera.nearClipPlane;
        depth = nearPlane + offsetZ + camera.transform.position.z;

       

        gizmoContent = new GameObject("DirGizContent");
        lr =gizmoContent.AddComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr.material = mat;
        lr.startColor = lr.endColor = Color.yellow;
        lr.startWidth = lr.endWidth =0.05f;
        lr.textureMode = LineTextureMode.Tile;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;



        pathContent = new GameObject("PathContent");
        lrpath = pathContent.AddComponent<LineRenderer>();
        //lrpath.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lrpath.material = mat;
        lrpath.startColor = lrpath.endColor = Color.yellow;
        lrpath.startWidth = lrpath.endWidth = 0.01f;
        lrpath.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        pathContent2 = new GameObject("PathContent2");
        lrpath2 = pathContent2.AddComponent<LineRenderer>();
        lrpath2.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lrpath2.startColor = lrpath2.endColor = Color.cyan;
        lrpath2.startWidth = lrpath2.endWidth = 0.01f;
        lrpath2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        pathContent3 = new GameObject("PathContent3");
        lrpath3 = pathContent3.AddComponent<LineRenderer>();
        //lrpath3.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lrpath3.material = mat;
        lrpath3.startColor = lrpath3.endColor = Color.white;
        lrpath3.startWidth = lrpath3.endWidth = 0.05f;
        lrpath3.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        forceSlider.value = 1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 v = new Vector3(0, 6.0f, 6.0f);
        Vector3 g = new Vector3(0, -9.81f, 0);

        lrpath.positionCount = pathCount;
        lrpath2.positionCount = pathCount;
        lrpath3.positionCount = pathCount;
    }

    // Update is called once per frame
    void Update()
    {
        forceSlider.value += Input.mouseScrollDelta.y*0.1f;

        depth = nearPlane + offsetZ+camera.transform.position.z;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Vector3 m = ray.origin;
        RaycastHit hit;

       
        if (pickUpObj != null)
        {
            rg = pickUpObj.GetComponent<Rigidbody>();
           
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Equals("Bullet"))
                {
                    Vector3 contactPos = hit.point;
                    Vector3 nearPlanePos = PointOnNearPlane(contactPos, nearPlane, depth);
                    pickUpObj = hit.transform.gameObject;
                    startPos = pickUpObj.transform.position;
                    endPos = nearPlanePos;
                    isPick = true;
                }
            }
        }

        if (isPick)
        {
            t += Time.deltaTime / duration;
            float ct =Clamp01 (EasingFunLibarary.GetFunction(EasingFunLibarary.FunctionName.SmoothStart)(t));
            pickUpObj.transform.position = Vector3.LerpUnclamped(startPos, endPos, ct);

            if (ct == 1)
            {
                isPick = false;
                isAttract = true;
                t = 0f;
            }
        }

        if (isAttract)
        {
            pickUpObj.transform.position = ray.GetPoint(depth);

            Rigidbody rg = pickUpObj.GetComponent<Rigidbody>();
            rg.useGravity = false;
            rg.velocity = Vector3.zero;
            rg.angularVelocity = Vector3.zero;

            Vector3 force = Vector3.zero;
            Vector3 velocity = Vector3.zero;

            if (Input.GetMouseButtonDown(2))
            {
                readyPos = pickUpObj.transform.position;
                readyMousePos = Input.mousePosition;
            }

            if (Input.GetMouseButton(2))
            {
                pickUpObj.transform.position = readyPos;


                releasePos = ray.GetPoint(depth + 1.0f);

                releaseDir = Vector3.Normalize(releasePos - readyPos);
                force = forceSlider.value * 10f * releaseDir;
                //velocity = forceSlider.value * 10f * releaseDir;
                velocity = force / rg.mass;

                CanculatePath(velocity, pickUpObj.transform.position);

                lr.positionCount = 2;
                lr.SetPosition(0, readyPos);
                lr.SetPosition(1, releasePos);

               

            }


            if (Input.GetMouseButtonUp(2))
            {
                rg.useGravity = true;
                isAttract = false;
                releaseDir = Vector3.Normalize(releasePos - readyPos);
                force = forceSlider.value*10f * releaseDir;
                

                

                if (rg != null)
                {
                    rg.angularVelocity += new Vector3(1f, 1f, 10f);
                    rg.AddForceAtPosition(force, pickUpObj.transform.TransformPoint(new Vector3(1f, 1f, 1f)), ForceMode.Impulse);

                    //rg.velocity = forceSlider.value * 10f * releaseDir;
                }

                pickUpObj = null;

                readyPos = Vector3.zero;
                releasePos = Vector3.zero;
                lr.SetPosition(0, readyPos);
                lr.SetPosition(1, releasePos);


                lrpath.positionCount = pathPoint.Count;
                lrpath.SetPositions(pathPoint.ToArray());

                lrpath3.positionCount = 0;

            }

        }
    }


    void CanculatePath(Vector3 velocity ,Vector3 startPos)
    {
        Vector3 g = Physics.gravity; 
        Vector3 nextPos = Vector3.zero;
        Vector3 res = Vector3.zero;
        float h = 0.02f;
        lrpath2.positionCount = pathCount;
        lrpath3.positionCount = pathCount;
        #region real phyisc
        //for (int i = 0; i < pathCount; i++)
        //{
        //    float t = 2f * i / (float)pathCount;

        //    Vector3 pos = startPos + velocity * t + 0.5f * g * Pow(t, 2);
        //    lrpath2.SetPosition(i, pos);

        //}
        #endregion

        #region Euler Method
        //for (int i = 0; i < pathCount; i++)
        //{
        //    float t = 2f * i / (float)pathCount;

        //    nextPos = startPos + h * (velocity + g * t);
        //    startPos = nextPos;
        //    lrpath3.SetPosition(i, nextPos);
        //}
        #endregion

        #region improv Euler Method
        for (int i = 0; i < pathCount; i++)
        {
            float t = 2f * i / (float)pathCount;

            nextPos = startPos + h * (velocity + g * t);
            res = startPos + 0.5f * h * (nextPos + startPos);
            
            startPos = nextPos;

            lrpath3.SetPosition(i, res);
        }
        #endregion

        endPot.transform.position = lrpath3.GetPosition(pathCount - 1);
    }

    private void DisplayPath()
    {

        if (Input.GetMouseButtonDown(2))
        {
            readyPos = pickUpObj.transform.position;
            readyMousePos = Input.mousePosition;
        }

            if (Input.GetMouseButton(2))
        {
            pickUpObj.transform.position = readyPos;
           
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            releasePos = ray.GetPoint(depth + 1.0f);

            lr.positionCount = 2;
            lr.SetPosition(0, readyPos);
            lr.SetPosition(1, releasePos);
        }


        if (Input.GetMouseButtonUp(2))
        {
         
            //Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            //pickUpObj.transform.position = ray.GetPoint(depth);

            Vector3 releaseDir = Vector3.Normalize(releasePos - readyPos);
            Vector3 force = 10f * releaseDir;
            if (rg != null)
            {
                rg.AddForce(force, ForceMode.Acceleration);
            }

            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, Vector3.zero);
        }






    }

   

    private Vector3 PointOnNearPlane( Vector3 input,float n,float depth)
    {
        Vector3 output = new Vector3(input.x * n / input.z, input.y * n / input.z, depth);
        return output;
    }

    private void ThrowCoin()
    {
       
    }

    private void OnDrawGizmos()
    {

        depth = nearPlane + offsetZ + camera.transform.position.z;
        Vector3 center = new Vector3(0f, 0f, depth + 1.0f);
        Gizmos.DrawWireCube(center, new Vector3(3f,2f,0f));
    }
}

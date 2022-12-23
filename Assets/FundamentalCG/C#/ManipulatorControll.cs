using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulatorControll : MonoBehaviour
{


    private GameObject[] pointProxyes;
    [SerializeField]
    private Material xaixeMat;
    private Color xc;
    [SerializeField]
    private Material yaixeMat;
    private Color yc;
    [SerializeField]
    private Material zaixeMat;
    private Color zc;
    [SerializeField]
    private float colorMul = 2.0f;

    [SerializeField]
    private Transform selectedObj =null;
    private bool selectedX=false;
    private bool selectedY = false;
    private bool selectedZ = false;
    [SerializeField]
    private Vector3 currentPointerPos= new Vector3(0,0,0);
    RaycastHit hit;

    private float scrSide;//-1 ,1 ==> left or right

    // Start is called before the first frame update
    void Start()
    {
         pointProxyes = GameObject.FindGameObjectsWithTag("Manipulator");
        xc = xaixeMat.color;
        yc = yaixeMat.color;
        zc = zaixeMat.color;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 carFoward = Camera.main.transform.forward;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
           // RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Equals("Manipulator"))
                {
                   // Debug.Log("hit"+ hit.transform.name);
                    foreach (var obj in pointProxyes)
                    {
                        obj.GetComponent<ControllerXYZ>().isSelected = false;
                        currentPointerPos = new Vector3(0, 0, 0);
                    }
                    hit.transform.gameObject.GetComponent<ControllerXYZ>().isSelected = true;
                }
                else if (hit.transform.tag.Equals("ManipulatorX"))
                {
                    //Debug.Log("HitX");
                    selectedObj = hit.transform.parent;
                   
                    currentPointerPos = hit.transform.position;
                    selectedX = true;
                    selectedY = false;
                    selectedZ = false;
                    Vector3 ccmV3 = Vector3.Cross( currentPointerPos- selectedObj.transform.position, carFoward);
                    scrSide =Mathf.Sign(Vector3.Dot(ccmV3, new Vector3(0, 1, 0)));
                    hit.transform.GetComponent<MeshRenderer>().sharedMaterial.color = xc * colorMul;

                }
                else if (hit.transform.tag.Equals("ManipulatorY"))
                {
                   // Debug.Log("HitY");
                    selectedObj = hit.transform.parent;
                    
                    currentPointerPos = hit.transform.position;
                    selectedX = false;
                    selectedY = true;
                    selectedZ = false;
                    hit.transform.GetComponent<MeshRenderer>().sharedMaterial.color = yc * colorMul;
                }
                else if (hit.transform.tag.Equals("ManipulatorZ"))
                {
                   // Debug.Log("HitZ");
                    selectedObj = hit.transform.parent;
                    
                    currentPointerPos = hit.transform.position;
                    selectedX = false;
                    selectedY = false;
                    selectedZ = true;
                    Vector3 ccmV3 = Vector3.Cross(currentPointerPos - selectedObj.transform.position, carFoward);
                    scrSide = Mathf.Sign(Vector3.Dot(ccmV3, new Vector3(0, 1, 0)));
                    hit.transform.GetComponent<MeshRenderer>().sharedMaterial.color = zc * colorMul;
                }
                else
                {
                    //Debug.Log("11111");
                    foreach (var obj in pointProxyes)
                    {
                        obj.GetComponent<ControllerXYZ>().isSelected = false;
                        currentPointerPos =new Vector3(0,0,0);
                        selectedObj = null;
                        selectedX = false;
                        selectedY = false;
                        selectedZ = false;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (selectedObj)
            {   
                
               
                Vector3 hitScr = Camera.main.WorldToScreenPoint(currentPointerPos);
                //Debug.Log("hitScr" + hitScr);

                Vector3 rdVector = Input.mousePosition -new Vector3( hitScr.x, hitScr.y,0);
                //Debug.Log("inputeMouse" + Input.mousePosition);
                //Debug.Log("rdVector" + rdVector);

                Vector2 rd = new Vector2(rdVector.x / Mathf.Sqrt(rdVector.x * rdVector.x + rdVector.y * rdVector.y), rdVector.y / Mathf.Sqrt(rdVector.x * rdVector.x + rdVector.y * rdVector.y));
                //Debug.Log("RD ===" + rd);


                float dirSignH = Mathf.Sign(rd.x);
                float dirSignV = Mathf.Sign(rd.y);
               // Debug.Log("dirSignV ===" + dirSignV);




                
                float vec_Magnitude = Vector3.Magnitude(rdVector);
                float w = 0.0004f;
                //Debug.Log(vec_Magnitude);
                if (vec_Magnitude > 10)
                {
                    if (selectedX )//&& Mathf.Abs(rd.x) > Mathf.Abs(rd.y))
                    {

                        Vector3 offsetX = new Vector3(vec_Magnitude * w * dirSignH* -scrSide, 0, 0);
                        selectedObj.transform.position += offsetX;
                        currentPointerPos = selectedObj.GetChild(0).transform.position;

                    }
                    else if (selectedY) //&& Mathf.Abs(rd.y) > Mathf.Abs(rd.x))
                    {
                        
                        Vector3 offsetY = new Vector3(0, vec_Magnitude * w * dirSignV, 0);
                        selectedObj.transform.position += offsetY;
                        currentPointerPos = selectedObj.GetChild(1).transform.position;
                    }
                    else if (selectedZ) //&&Mathf.Abs(rd.x) > Mathf.Abs(rd.y))
                    {
                        Vector3 offsetZ = new Vector3(0,0,vec_Magnitude * w * dirSignH * -scrSide);
                        selectedObj.transform.position += offsetZ;
                        currentPointerPos = selectedObj.GetChild(2).transform.position;
                    }
                }
   
            }

        }

        if (Input.GetMouseButtonUp(0))
        {
            currentPointerPos = new Vector3(0, 0, 0);
            selectedObj = null;
            foreach (var obj in pointProxyes)
            {

                obj.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.color = xc;
                obj.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial.color = yc;
                obj.transform.GetChild(2).GetComponent<MeshRenderer>().sharedMaterial.color = zc;

            }
        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestPick : MonoBehaviour
{
    [SerializeField]
    private GameObject gameObject;
  
    Vector3 intercetionPoint;
    
    public float sphW = 9.0f;

    private RaycastHit hit;
    [SerializeField]
    private GameObject uiGameObj;

    private Vector3 mouseDownPos;
    private Vector3 currentPos;
    private float curretnPosphi;
    private float curretnPostheta;
    private float theta;
    private float phi;


    [SerializeField]
    Slider rx;
    [SerializeField]
    Slider ry;
    [SerializeField]
    Slider rz;
    Vector3 currentAn;
    Vector3 resAn;

    Vector3 newAngel = new Vector3(0, 0, 0);
    Quaternion uiObjQuaternion;

    [SerializeField]
    Text rtx;
    [SerializeField]
    Text rty;
    [SerializeField]
    Text rtz;


    [SerializeField]
    Slider nfslider;



    Vector3 ro;

    void UIControl()
    {

        rx.value = 0;
        ry.value =0;
        rz.value = 0;
       

        rx.onValueChanged.AddListener(delegate {
            newAngel.x = rx.value;
        });

        ry.onValueChanged.AddListener(delegate {
            newAngel.y = ry.value;
        });

        rz.onValueChanged.AddListener(delegate {
            newAngel.z = rz.value;
        });

        sphW = 9.0f;
        nfslider.onValueChanged.AddListener(delegate {
            sphW = nfslider.value;
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        UIControl();
        ro = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Equals("UIGameObject")  )
                {
                    uiGameObj = hit.transform.gameObject;
                    mouseDownPos = Input.mousePosition;

                    currentPos = hit.transform.position;
                    curretnPostheta = Mathf.Acos(currentPos.y / sphW);
                    curretnPosphi = Mathf.Atan2(currentPos.z, currentPos.x);

                    //uiGameObj.transform.rotation = Quaternion.Euler(resAn);

                }
            }
            else
            {

                if (EventSystem.current.IsPointerOverGameObject() == false)
                {
                    uiGameObj = null;

                    Vector3 wp = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane));
                    Vector3 rd = Vector3.Normalize(wp - ro);

                    //float t = SphIntersect(ro, rd, new Vector4(0, 0, 0, sphW));
                    Vector3 intercetionPoint = ro + rd * sphW;
                    Vector3 nor = Vector3.Normalize(new Vector3(0, 0, 0) - intercetionPoint);

                    GameObject temp = Instantiate(gameObject, intercetionPoint, Quaternion.identity);
                    temp.transform.forward = -nor;
                    //resAn = temp.transform.rotation.eulerAngles;


                }
                
            }
        }


        if (Input.GetMouseButton(0)&& uiGameObj && EventSystem.current.IsPointerOverGameObject()==false)
        {
            //RotateWithAngle();
            RotateWithPickCoord();
        }

        if (Input.GetMouseButtonUp(0) && uiGameObj)
        {
            
            theta = 0;
            phi = 0;

            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                currentAn = uiGameObj.transform.rotation.eulerAngles;
                //Debug.Log("currentAn =====" + currentAn);
                rx.value = 0;
                ry.value = 0;
                rz.value = 0;

            }
        }


        if (uiGameObj )
        {

            Vector3 currentPos = uiGameObj.transform.position;
            Vector3 rd = Vector3.Normalize(currentPos - ro);
            uiGameObj.transform.position = ro + rd * sphW;

            newAngel = currentAn + new Vector3(rx.value, ry.value, rz.value);
            uiGameObj.transform.rotation = Quaternion.Euler(newAngel);
            resAn = newAngel;
            //Debug.Log("resAn =====  " + resAn);
           
           


        }
    }


    float  SphIntersect(Vector3 ro, Vector3 rd, Vector4 sph)
    {
        Vector3 oc = ro - new Vector3(sph.x,sph.y,sph.z);
        float b =Vector3.Dot(oc, rd);
        float c = Vector3.Dot(oc, oc) - sph.w * sph.w;
        float d = b * b - c;

        if (d < 0) return 0;
        d =Mathf.Sqrt(d);
        return -b +d;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    void RotateWithAngle()
    {
        currentAn = uiGameObj.transform.rotation.eulerAngles;
        float ax = ClampAngle(currentAn.x, 0, 360f);
        float ay = ClampAngle(currentAn.y, 0, 360f);
        float az = ClampAngle(currentAn.z, 0, 360f);

        newAngel = new Vector3(ax, ay, az);

        Vector3 mouseOffset = Input.mousePosition - mouseDownPos;
        float w = 0.045f;

        float dis = Vector3.Magnitude(mouseOffset);
        if (dis > 5)
        {

            phi = -mouseOffset.x * Mathf.Deg2Rad * w;
            theta = -mouseOffset.y * Mathf.Deg2Rad * w;

            float thetaNew = curretnPostheta + theta;
            float phiNew = curretnPosphi + phi;

            float x = sphW * Mathf.Sin(thetaNew) * Mathf.Cos(phiNew);
            float y = sphW * Mathf.Cos(thetaNew);
            float z = sphW * Mathf.Sin(thetaNew) * Mathf.Sin(phiNew);

            uiGameObj.transform.position = new Vector3(x, y, z);
            Vector3 nor = Vector3.Normalize(new Vector3(0, 0, 0) - uiGameObj.transform.position);
            uiGameObj.transform.forward = -nor;
        }
    }

    void RotateWithPickCoord()
    {

        Vector3 mouseOffset = Input.mousePosition - mouseDownPos;
       

        float dis = Vector3.Magnitude(mouseOffset);
        if (dis > 5)
        {

            Vector3 wp = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane));
            Vector3 rd = Vector3.Normalize(wp - ro);
            uiGameObj.transform.position = ro + rd * sphW;
            Vector3 nor = Vector3.Normalize(new Vector3(0, 0, 0) - uiGameObj.transform.position);
            uiGameObj.transform.forward = -nor;
            currentAn = uiGameObj.transform.rotation.eulerAngles;
            resAn = currentAn;
        }
       
    }

}

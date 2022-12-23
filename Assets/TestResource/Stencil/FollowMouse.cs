using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FollowMouse : MonoBehaviour
{
    Camera camera;

    Vector3 startPosOnNear;
    Vector3 startPosOnNear2D;
    float deltaDisX;
    Vector3 startAngle;
    Quaternion targetAngle;

    Material material;
    int sID;

    [SerializeField] Toggle toggle;


    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
       

        toggle.isOn = false;
       

        sID = Shader.PropertyToID("_scale");
        material.SetFloat(sID, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (toggle.isOn)
        {
            material.SetFloat(sID, 1.0f);
        }
        else
        {
            material.SetFloat(sID, 0.0f);
        }

        if (Input.GetMouseButtonDown(0))
        {
            startPosOnNear = RectTransformUtility.ScreenPointToRay(camera, Input.mousePosition).origin;
            startPosOnNear2D = Input.mousePosition;
            startAngle = gameObject.transform.localRotation.eulerAngles;

        }

        if (Input.GetMouseButton(0))
        {
            Vector3 mouseOnNear = RectTransformUtility.ScreenPointToRay(camera, Input.mousePosition).origin;
            Vector3 mouseOnNear2D = Input.mousePosition;
            float dir = (Mathf.Sign(mouseOnNear2D.x - startPosOnNear2D.x) > 0) ? 1.0f : -1.0f;
            deltaDisX = dir * Vector3.Magnitude(mouseOnNear - startPosOnNear);


            float angle = Mathf.Atan(deltaDisX / camera.nearClipPlane) * Mathf.Rad2Deg;
            float newAngleY = startAngle.y - angle;

            targetAngle = Quaternion.Euler(startAngle.x, newAngleY, startAngle.z);
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetAngle, Time.deltaTime * 6);
        }

        if (Input.GetMouseButtonUp(0))
        {

            startPosOnNear = Vector3.zero;
            startAngle = Vector3.zero;

        }

    }
}

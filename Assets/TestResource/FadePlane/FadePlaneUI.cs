using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FadePlaneUI : MonoBehaviour
{

    [SerializeField] List<GameObject> items = new List<GameObject>();

     List<GameObject> displayitems = new List<GameObject>();
     GameObject centerItem;

    [SerializeField] Camera camera;
     Vector3[] conners= new Vector3[4];

     [SerializeField]Rect cameraRec;

     List<Vector3> itemStartPos = new List<Vector3>();

    float deltaDisX;
    float zUI2Camera;
    Vector3 screenPosStart;
    [SerializeField] float gaps;


    


    // Start is called before the first frame update

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject temp = transform.GetChild(i).gameObject;
            //temp.transform.position = new Vector3(-24, temp.transform.position.y, temp.transform.position.z) + new Vector3(12 * i, 0, 0);
            float x = (i % 2 == 0) ? gaps * i : -gaps * (i + 1);
            temp.transform.position = new Vector3(x, temp.transform.position.y, temp.transform.position.z);

            items.Add(temp);
        }
        centerItem = items[0];

        items.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));

        zUI2Camera = centerItem.transform.position.z - camera.transform.position.z;

        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), items[0].transform.position.z - camera.transform.position.z, Camera.MonoOrStereoscopicEye.Mono, conners);

        float minX = items[0].transform.position.x - gaps;
        float maxX = items[items.Count - 1].transform.position.x + gaps;

        //cameraRec = new Rect(new Vector2(conners[0].x, conners[0].y), new Vector2(conners[3].x - conners[0].x, conners[2].y - conners[0].y));
        cameraRec = new Rect(new Vector2(minX, conners[0].y), new Vector2(maxX - minX, conners[2].y - conners[0].y));
    }

    private void OnDrawGizmos()
    {
        if (items.Count != 0)
        {
            zUI2Camera = centerItem.transform.position.z - camera.transform.position.z;
            Gizmos.DrawWireCube(camera.transform.position + new Vector3(0, 0, zUI2Camera), new Vector3(cameraRec.width, cameraRec.height, 0.1f));
        }

        
    }
    // Update is called once per frame
    void Update()
    {
        foreach (var o in items)
        {
            Vector3 itemRd = Vector3.Normalize(o.transform.position-camera.transform.position);
            
            float leftOrRight = Mathf.Sign(Vector3.Cross(itemRd, camera.transform.forward).y);

            float ratioForAngle = o.transform.position.x / 15.0f;
            float ratioForDis = o.transform.position.x / 15.0f;

            float angle = (leftOrRight > 0) ? Mathf.Clamp(45.0f * ratioForAngle, -90, 0) : Mathf.Clamp(45.0f * ratioForAngle, 0, 90);
            float scalor = Mathf.Lerp(1.0f, 0.6f, Mathf.Abs(ratioForDis));

            o.transform.localEulerAngles = new Vector3(0, 0, -angle);
            o.transform.localScale = new Vector3(scalor, scalor, scalor);
        }


        if (Input.GetMouseButtonDown(0))
        {
            screenPosStart = Input.mousePosition;

            foreach (var p in items)
            {
               Vector3 pInScr =RectTransformUtility.WorldToScreenPoint(camera, p.transform.position);
                itemStartPos.Add(pInScr);
            }
        }


        if (Input.GetMouseButton(0))
        { 
            deltaDisX = Input.mousePosition.x - screenPosStart.x;
            Vector3 offsetX = new Vector3(deltaDisX, 0, 0);
            for (int i = 0; i < items.Count; i++)
            {
                if (Mathf.Abs(deltaDisX) > 5.0f)
                {
                    Vector3 newPos = itemStartPos[i] + offsetX;
                    Ray ray = RectTransformUtility.ScreenPointToRay(camera, newPos);

                    Vector3 wordPosOnNear = ray.origin;
                    float realx = wordPosOnNear.x * zUI2Camera / camera.nearClipPlane;
                    items[i].transform.position = new Vector3(realx, items[i].transform.position.y, items[i].transform.position.z);
                }

                if (!cameraRec.Contains(items[i].transform.position))
                {
                    if (items[i].transform.position.x <= 0)
                    {

                        items[i].transform.position = items[items.Count - 1].transform.position + new Vector3(12.0f, 0, 0);
                        items[i].transform.SetAsLastSibling();

                        GameObject temp = items[i];
                        items.RemoveAt(i);
                        items.Insert(items.Count, temp);
                    }
                    else
                    {
                        items[i].transform.position = items[0].transform.position - new Vector3(12.0f, 0, 0);
                        items[i].transform.SetAsFirstSibling();

                        GameObject temp = items[i];
                        items.RemoveAt(i);
                        items.Insert(0, temp);
                        
                    }

                    screenPosStart = Input.mousePosition;

                    itemStartPos.Clear();
                    foreach (var p in items)
                    {
                        Vector3 pInScr = RectTransformUtility.WorldToScreenPoint(camera, p.transform.position);
                        itemStartPos.Add(pInScr);
                    }
                }     
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            screenPosStart = Vector3.zero;
            itemStartPos.Clear();
        }

    }


}

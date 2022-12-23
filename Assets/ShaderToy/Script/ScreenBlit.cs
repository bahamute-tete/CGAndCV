using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenBlit : MonoBehaviour
{
    [SerializeField]
    private Material material;
    private Vector2 mousePos = new Vector2(0,0);
    private int _MousePosID;
    [Range(0.1f, 1)]
    public float kochPattenShape = 0.1f;
    private int kochPattenShapeID = 0;
    private Vector4 mdControl = new Vector4(0, 0, 0, 0);
    Vector2 mdPos = new Vector2(0,0);
    float mdScale = 1f;
    Vector2 smoothPos;
    float smoothScale;

    float mdAngle = 0;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //RenderTexture scrRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        if (material)
        {
           
            Graphics.Blit(source, destination, material);

        }else
        {
            Graphics.Blit(source, destination);
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        _MousePosID = Shader.PropertyToID("_MousePos");
        mousePos = new Vector2(0,0);
        kochPattenShapeID = Shader.PropertyToID("_KochPattenShape");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mousePos = Input.mousePosition;
            mousePos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            material.SetVector(_MousePosID, new Vector4(mousePos.x, mousePos.y, 0, 0));
        }

        material.SetFloat(kochPattenShapeID, kochPattenShape);



        if (Input.GetKey(KeyCode.Z))
        {
            mdScale *= 0.99f;
        }

        if (Input.GetKey(KeyCode.X))
        {
            mdScale *= 1.01f;
        }


        if (Input.GetKey(KeyCode.Q))
        {
            mdAngle -= 0.01f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            mdAngle += 0.01f;
        }


        Vector2 dir = new Vector2(0.01f * mdScale, 0);

        float s = Mathf.Sin(mdAngle);
        float c = Mathf.Cos(mdAngle);

        dir = new Vector2(dir.x * c - dir.y * s, dir.x * s + dir.y * c);

   
        if (Input.GetKey(KeyCode.D))
        {
            mdPos += dir;
        }
        if (Input.GetKey(KeyCode.A))
        {
            mdPos -= dir;
        }

        dir = new Vector2(-dir.y,dir.x);
        if (Input.GetKey(KeyCode.S))
        {
            mdPos-= dir;
        }

        if (Input.GetKey(KeyCode.W))
        {
            mdPos += dir;
        }

        smoothPos = Vector2.Lerp(smoothPos, mdPos, 0.01f);
        smoothScale = Mathf.Lerp(smoothScale, mdScale, 0.01f);

       //mdControl = new Vector4(smoothPos.x, smoothPos.y, smoothScale, smoothScale);
       mdControl = new Vector4(mdPos.x, mdPos.y, mdScale, mdScale);
        material.SetVector("_Pos", mdControl);
        material.SetFloat("_Angle", mdAngle);
        //Debug.Log("mousePos" + mousePos);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RayMarchingCameraSetup : MonoBehaviour
{   
    [SerializeField]
    Shader rayMarchingShader = null;
    [Space(15)]
    [Range(0, 1)]
    public float smoothValue = 0.5f;

    [SerializeField]
    private Texture2D colorRamp;
    [Range(0, 1.0f)]
    public float colorRange = 0;
    private float gyroidColor = 0;
    private int colorRangeID = 0;
    [Space(15)]
    [Range(1,100)]
    public float gyroidScale = 30.0f;
    [Range(0,0.1f)]
    public float thickness = 0.01f;
    [Range(0,1.5f)]
    public float bias = 0.5f;
    [Range(0,5)]
    public float animationSpeed = 0.0f;

    private int gyroidDataID = 0;
    private Vector4 gyroidData = new Vector4(0, 0, 0, 0);

    private int svalueID = 0;
    private Material _MatForScreen;
    public Material MatForScreen
    {
        get
        {
            if (!_MatForScreen)
            {
                if (rayMarchingShader)
                {
                    _MatForScreen = new Material(rayMarchingShader);
                    _MatForScreen.hideFlags = HideFlags.HideAndDontSave;
                }
                else
                {
                    throw new MissingReferenceException("Miss RayMarchinScreenShader");
                }
            }
            return _MatForScreen;
        }

        set => _MatForScreen = value;
    }

    public Camera CurrentCamera
    {
        get
        { if (!_CurrentCamera)
            {
                _CurrentCamera = GetComponent<Camera>();
                _CurrentCamera.depthTextureMode = DepthTextureMode.Depth;
            }

            return _CurrentCamera;
        }
        set => _CurrentCamera = value;
    }
    private Camera _CurrentCamera;


    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!MatForScreen)
        { 
            Graphics.Blit(source, destination);
        }

            GLScreenBlit(source, destination ,MatForScreen);
            //Graphics.Blit(source, destination, MatForScreen);
            MatForScreen.SetMatrix("_CameraConnerContent", GetCameraFrustumConner(CurrentCamera));
            MatForScreen.SetMatrix("_MatrixCameraViewToWorld", CurrentCamera.cameraToWorldMatrix);
            MatForScreen.SetVector("_CameraWPos", CurrentCamera.transform.position);
            MatForScreen.SetTexture("_ColorRamp", colorRamp);
    }

    private Matrix4x4 GetCameraFrustumConner(Camera camera)
    {
        float cameraFov = camera.fieldOfView;
        float cameraAspect = camera.aspect;
        float cameraFarClipPlane = camera.farClipPlane;
        Matrix4x4 cameraConnerContent = Matrix4x4.identity;

        float tan_Fov = Mathf.Tan(0.5f* cameraFov * Mathf.Deg2Rad);
        Vector3 distoUpDir = Vector3.up * tan_Fov * cameraFarClipPlane;
        Vector3 distoRight = Vector3.right * tan_Fov * cameraFarClipPlane * cameraAspect;

        Vector3 connerTopLeft = -cameraFarClipPlane * Vector3.forward - distoRight + distoUpDir;
        Vector3 connerTopRight = -cameraFarClipPlane * Vector3.forward + distoRight + distoUpDir;
        Vector3 connerBottomRight = -cameraFarClipPlane * Vector3.forward + distoRight - distoUpDir;
        Vector3 connerBottomLeft = -cameraFarClipPlane * Vector3.forward - distoRight - distoUpDir;

        cameraConnerContent.SetRow(0, connerTopLeft);
        cameraConnerContent.SetRow(1, connerTopRight);
        cameraConnerContent.SetRow(2, connerBottomRight);
        cameraConnerContent.SetRow(3, connerBottomLeft);
        return cameraConnerContent;  
    }

    private void GLScreenBlit(RenderTexture source, RenderTexture destination,Material material)
    {
        RenderTexture.active = destination;

        material.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();
        material.SetPass(0);

        GL.Begin(GL.QUADS);
        GL.Color(Color.red);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR


        GL.End();
        GL.PopMatrix();
    }

    void Start()
    {
        svalueID = Shader.PropertyToID("_SmoothValue");
        gyroidDataID = Shader.PropertyToID("_GyroidData");
        colorRangeID = Shader.PropertyToID("_ColorRange");
        
    }

    // Update is called once per frame
    void Update()
    {
        gyroidData = new Vector4(gyroidScale, thickness, bias, animationSpeed);
        gyroidColor = colorRange;
        MatForScreen.SetFloat(svalueID, smoothValue);
        MatForScreen.SetVector(gyroidDataID, gyroidData);
        MatForScreen.SetFloat(colorRangeID, gyroidColor);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenImageBlit : SceneViewFilter
{

    public Shader shader;
    public Camera camera;


    public Material mat;

    private void Awake()
    {
        camera.depthTextureMode = DepthTextureMode.Depth;
        if (!mat)
        {
            mat = new Material(shader);
        }

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mat)
        {
            Graphics.Blit(source, destination, mat);
            mat.SetTexture("_MainTex", source);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

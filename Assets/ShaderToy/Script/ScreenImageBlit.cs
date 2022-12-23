using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenImageBlit : MonoBehaviour
{

    public Shader shader;

    Material mat;

    private void Awake()
    {
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

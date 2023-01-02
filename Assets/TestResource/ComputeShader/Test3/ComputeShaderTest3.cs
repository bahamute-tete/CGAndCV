using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest3 : MonoBehaviour
{
    public ComputeShader cs;
    public Texture2D texture;

    ComputeBuffer _CBuffer;

    public RenderTexture rt;
    // Start is called before the first frame update
    void Start()
    {
        rt = new RenderTexture(Screen.width, Screen.height, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        int kernalIndex = cs.FindKernel("CSMain");
        cs.SetTexture(0, "_InputTex", texture);
        cs.SetTexture(0, "_OutTex", rt);

        cs.Dispatch(0, texture.width / 8, texture.height / 8, 1);


    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(rt, destination);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

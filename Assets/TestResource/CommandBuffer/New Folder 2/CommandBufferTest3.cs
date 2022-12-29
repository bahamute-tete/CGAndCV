using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

public class CommandBufferTest3 : MonoBehaviour
{
    private CommandBuffer cmd;


    public Material _OverrideMat;
    static int nameID = Shader.PropertyToID("_MainTex");

    public GameObject renderTarget0;
    public GameObject renderTarget1;
    public GameObject renderTarget2;
    private Renderer renderer0;
    private Renderer renderer1;
    private Renderer renderer2;


    public GameObject _Screen;
    private Renderer renderer3;

   
    RenderTexture _OrginRT;

    public Material processMat;
    static int texID = Shader.PropertyToID("_OrginalTex");
    static int commandTexID = Shader.PropertyToID("_RenderTex");
    static int lineID = Shader.PropertyToID("_OutlineTex");

    private void OnEnable()
    {
     
    }

    private void OnDisable()
    {
        if (cmd != null)
        {
            cmd.Clear();
            _OrginRT.Release();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        renderer0 = renderTarget0.GetComponent<Renderer>();
        renderer1 = renderTarget1.GetComponent<Renderer>();
        renderer2 = renderTarget2.GetComponent<Renderer>();

     

        cmd = new CommandBuffer() { name="CommandBufferTest"};

        _OrginRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);

        RenderTexture rt0 = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        RenderTexture rt1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);


        cmd.SetRenderTarget(_OrginRT);
        cmd.ClearRenderTarget(true, true, Color.black);

        cmd.DrawRenderer(renderer0, _OverrideMat);
        cmd.DrawRenderer(renderer1, _OverrideMat);
        cmd.DrawRenderer(renderer2, _OverrideMat);

        processMat.SetTexture(texID, _OrginRT);
        //cmd.Blit(_OrginRT, texID, processMat);

        cmd.Blit(_OrginRT, rt0, processMat, 0);
        cmd.Blit(rt0, rt1, processMat, 1);


        for (int i = 0; i < 3; i++)
        {
            cmd.Blit(rt1, rt0, processMat, 0);
            cmd.Blit(rt0, rt1, processMat, 1);
        }

        processMat.SetTexture(commandTexID, rt1);
        //cmd.Blit(rt1, commandTexID, processMat);

        cmd.Blit(rt1, rt0, processMat, 2);


        processMat.SetTexture(lineID, rt0);
        cmd.Blit(rt0, rt1, processMat, 3);



        if (_Screen != null)
        {

            _Screen.GetComponent<Renderer>().material.mainTexture = rt1;
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmd);
        }

        rt0.Release();
        rt1.Release();
    }





    // Update is called once per frame
    void Update()
    {
        
    }
}

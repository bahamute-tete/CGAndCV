using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandBufferTest2 : MonoBehaviour
{
    CommandBuffer buffer ;
    [SerializeField] GameObject renderTarget;
    [SerializeField] Material replaceMat;
    private RenderTexture rt;

    private void Awake()
    {
        buffer = new CommandBuffer() { name = "TestBuffer" };
    }
    private void OnEnable()
    {
        rt = new RenderTexture(Screen.width, Screen.height, 16);

        buffer.SetRenderTarget(rt);
        buffer.ClearRenderTarget(true, true, Color.black);

        Renderer rd = renderTarget.GetComponent<Renderer>();
        Material mat = replaceMat == null ? rd.sharedMaterial : replaceMat;

        buffer.DrawRenderer(rd, mat);

        GetComponent<Renderer>().sharedMaterial.mainTexture = rt;

        Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);
    }

    private void OnDisable()
    {
        if (buffer!=null)
        {
            buffer.Clear();
            rt.Release();
        }
        
    }





}

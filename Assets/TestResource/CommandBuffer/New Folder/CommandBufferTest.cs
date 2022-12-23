using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CommandBufferTest : MonoBehaviour
{

    
    [SerializeField] Shader  shader;

    private void OnEnable()
    {
        CommandBuffer buf = new CommandBuffer();

        buf.DrawRenderer(GetComponent<Renderer>(), new Material(shader),0,-1);

        Camera.main.AddCommandBuffer(CameraEvent.AfterGBuffer, buf);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
